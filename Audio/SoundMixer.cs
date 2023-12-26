using BattleCity.Enums;
using BattleCity.Logging;
using SlimDX.Multimedia;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BattleCity.Audio
{
    /// <summary>
    /// Звуковой микшер
    /// </summary>
    public class SoundMixer : ISoundMixer, IPcmOutputStream
    {
        private readonly WaveFormat defaultWaveFmt;
        private ILogger log;
        private readonly object syncObject = new object();
        private readonly int maxStreamCount;
        private int activeStreamsCount = 0;
        private readonly float[] sndLevel;
        private List<IAudioReader>[] audioStreams = new List<IAudioReader>[2];

        public WaveFormat WaveFormat => defaultWaveFmt;

        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="log">Сервис логирования</param>
        /// <param name="maxStreamCount">Максимальное количество аудио потоков</param>
        public SoundMixer(ILogger log, int maxStreamCount)
        {
            this.log = log;
            this.maxStreamCount = maxStreamCount;
            int numSoundTypes = Enum.GetValues(typeof(SoundType)).Length;
            audioStreams = new List<IAudioReader>[numSoundTypes];
            sndLevel = new float[numSoundTypes];

            for (int i = 0; i < numSoundTypes; i++)
            {
                audioStreams[i] = new List<IAudioReader>(maxStreamCount);
                sndLevel[i] = 1;
            }

            defaultWaveFmt = new WaveFormat()
            {
                BitsPerSample = 16,
                Channels = 2,
                SamplesPerSecond = 44100,
                FormatTag = WaveFormatTag.Pcm,
                BlockAlignment = 4,
                AverageBytesPerSecond = 176400
            };
        }

        /// <inheritdoc/>
        public void SetSoundLevel(SoundType type, float level)
        {
            if (!Enum.IsDefined(typeof(SoundType), type))
                return;

            sndLevel[(int)type] = Math.Max(0, Math.Min(1, level));
        }

        /// <inheritdoc/>
        public float GetSoundLevel(SoundType type)
        {
            if (!Enum.IsDefined(typeof(SoundType), type))
                return 0;

            return sndLevel[(int)type];
        }

        /// <inheritdoc/>
        public bool Contains(string sndName, SoundType type)
        {
            lock (syncObject)
            {
                int idx = (int)type;
                if (idx < 0 || idx >= audioStreams.Length)
                {
                    log?.WriteLine($"Invalid audio stream id: {type}", LogLevel.Warning);
                    return false;
                }

                var list = audioStreams[idx];
                return list.Any(p => p.Name == sndName);
            }
        }

        /// <inheritdoc/>
        public void Add(IAudioReader audioReader, bool reuseExistSlot = false, bool restartExistSlot = false)
        {
            lock (syncObject)
            {
                int idx = (int)audioReader.AudioStreamType;
                if (idx < 0 || idx >= audioStreams.Length)
                {
                    log?.WriteLine($"Invalid audio stream id: {audioReader.AudioStreamType}", LogLevel.Warning);
                    return;
                }

                var list = audioStreams[idx];
                bool addNew = true;

                if (reuseExistSlot)
                {
                    var slot = list.FirstOrDefault(x => x.Name == audioReader.Name);
                    if (slot != null)
                    {
                        if (restartExistSlot)
                            slot.ResetPosition();
                        else
                            slot.RepeatOnce = true;
                        addNew = false;
                    }
                }
                if (addNew)
                {
                    //if (list.Count >= maxStreamCount)
                    if (activeStreamsCount >= maxStreamCount)
                    {
                        log?.WriteLine("Max audio streams exceeded", LogLevel.Warning);
                        return;
                    }
                    list.Add(audioReader);
                }
            }
        }

        /// <inheritdoc/>
        public void Remove(IAudioReader stream)
        {
            if (stream == null)
                return;
            lock (syncObject)
            {
                int idx = (int)stream.AudioStreamType;
                if (idx < 0 || idx >= audioStreams.Length)
                {
                    log?.WriteLine($"Invalid audio stream id: {stream.AudioStreamType}", LogLevel.Warning);
                    return;
                }

                var list = audioStreams[idx];
                list.Remove(stream);
                activeStreamsCount--;
            }
        }

        /// <inheritdoc/>
        public void Clear(SoundType? streamId = null)
        {
            lock (syncObject)
            {
                if (streamId == null)
                {
                    audioStreams[0].Clear();
                    audioStreams[1].Clear();
                    activeStreamsCount = 0;
                }
                else
                {
                    int idx = (int)streamId;
                    if (idx < 0 || idx >= audioStreams.Length)
                    {
                        log?.WriteLine($"Invalid audio stream id: {streamId}", LogLevel.Warning);
                        return;
                    }
                    var list = audioStreams[idx];
                    int numItems = list.Count;
                    list.Clear();
                    activeStreamsCount = Math.Max(0, activeStreamsCount - numItems);
                }
            }
        }

        private AudioChunk ReadChunk(int byteCount)
        {
            byte[] data = new byte[byteCount];
            AudioChunk resultChunk = new AudioChunk(data, 0, byteCount / (double)defaultWaveFmt.AverageBytesPerSecond, (SoundType)999);

            List<AudioChunk> chunks;

            lock (syncObject)
            {
                var activeStreamCount = audioStreams.Sum(x => x.Count);
                if (activeStreamCount > 0)
                {
                    #region way 1. Parallel 

                    //chunks = audioStreams.AsParallel().Select(x => x.Read(byteCount)).ToList();

                    #endregion

                    #region way 2. Parallel tasks

                    var tasks = new List<Task<AudioChunk>>(activeStreamCount);
                    for (int i = 0; i < audioStreams.Length; i++)
                    {
                        foreach (var stream in audioStreams[i].Where(p => !p.Paused))
                        {
                            tasks.Add(Task.Factory.StartNew(() =>
                            {
                                return stream.Read(byteCount);
                            }));
                        }
                    }

                    Task.WaitAll(tasks.ToArray());
                    chunks = tasks.Select(x => x.Result).ToList();

                    #endregion

                    foreach (var list in audioStreams)
                    {
                        int removedCount = list.RemoveAll(x => x.IsEof && !x.RepeatOnce && !x.Paused);
                        activeStreamsCount = Math.Max(0, activeStreamsCount - removedCount);
                    }
                }
                else chunks = new List<AudioChunk>(0);
            }

            double fadeDuration = Math.Min(0.004d, resultChunk.Duration);
            double sampleDuration = defaultWaveFmt.BlockAlignment / (double)defaultWaveFmt.AverageBytesPerSecond;
            int chunkSamplesPerChannelCount = resultChunk.ShortCount / defaultWaveFmt.Channels;

            Parallel.For(0, resultChunk.ShortCount, i =>
            {
                int sum = chunks.Sum(x =>
                {
                    int sampleNum = i / defaultWaveFmt.Channels;
                    double sampleTime = sampleNum * sampleDuration;
                    double fade = 1;

                    //if (x.TimePosition < fadeDuration)
                    //{
                    //    fade = MathF.Lerp(0, fadeDuration, x.TimePosition + sampleTime) * 1000;
                    //    fade = Math.Round(fade, 4);
                    //}
                    //if (x.TimePosition > Math.Round(x.Duration, 4) - fadeDuration)
                    //{
                    //    var sampleTime2 = (chunkSamplesPerChannelCount - sampleNum) * sampleDuration;
                    //    fade = MathF.Lerp(0, fadeDuration, sampleTime2) * 1000;
                    //    fade = Math.Round(fade, 4);
                    //}

                    return (int)(x.Data.Shorts[i] * fade * sndLevel[(int)x.AudioStreamType]);
                });
                if (sum > short.MaxValue)
                    sum = short.MaxValue;
                else if (sum < short.MinValue)
                    sum = short.MinValue;

                resultChunk.Data.Shorts[i] = (short)sum;
            });

            return resultChunk;
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            lock (syncObject)
            {
                log = null;
                audioStreams = null;
                activeStreamsCount = 0;
            }
        }

        #region Stream

        /// <inheritdoc/>
        public int Read(byte[] buffer, int offset, int count)
        {
            try
            {
                var data = ReadChunk(count);
                Array.Copy(data.Data.Bytes, 0, buffer, offset, count);
                return count;
            }
            catch
            {
                return 0;
            }
        }

        #endregion
    }
}
