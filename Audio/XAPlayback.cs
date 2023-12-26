using BattleCity.Extensions;
using SlimDX.XAudio2;
using System;
using System.Collections.Generic;
using System.Threading;

namespace BattleCity.Audio
{
    /// <summary>
    /// XAudio2 Движок воспроизведения аудио
    /// </summary>
    public class XAPlayback : IAudioPlayback, IDisposable
    {
        private XAudio2 xaudio;
        private MasteringVoice masteringVoice;
        private SourceVoice sourceVoice;
        private readonly object mutex = new object();
        private List<AudioBuffer> streamBuffers;
        private System.Collections.BitArray bufferStatus;
        private EventHandler<ContextEventArgs> bufferEndCallback;
        private List<byte[]> byteBuffers;
        private readonly int NumStreamingBuffers = 2;
        private int StreamingBufferSize = 0;
        private readonly int bufDuration;

        private bool mTerminate;
        private Thread playThread;
        private IPcmOutputStream playStream;

        /// <summary>
        /// Признак выполненной инициализации
        /// </summary>
        public bool IsInitialized => masteringVoice != null;

        /// <summary>
        /// Мастер уровень звука
        /// </summary>
        public float MasterLevel
        {
            get { return sourceVoice?.Volume ?? 1; }
            set
            {
                if (sourceVoice != null)
                    sourceVoice.Volume = value;
            }
        }

        /// <summary>
        /// Соотношение частоты
        /// </summary>
        public float FrequencyRatio
        {
            get { return sourceVoice?.FrequencyRatio ?? 1; }
            set
            {
                if (sourceVoice != null)
                    sourceVoice.FrequencyRatio = value;
            }
        }

        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="soundMixer">Микшер</param>
        /// <param name="bufDuration">Размер буффера</param>
        /// <param name="bufCount">Количество буфферов</param>
        public XAPlayback(IPcmOutputStream soundMixer, int bufDuration = 20, int bufCount = 2)
        {
            playStream = soundMixer;
            this.bufDuration = Math.Max(20, bufDuration);
            NumStreamingBuffers = Math.Max(2, bufCount);
            Initialize();
            CreateBuffers();
        }

        /// <summary>
        /// Инициализация
        /// </summary>
        private void Initialize()
        {
            lock (mutex)
            {
                bufferEndCallback = new EventHandler<ContextEventArgs>(Streaming_BufferEnd);
                streamBuffers = new List<AudioBuffer>(NumStreamingBuffers);
                byteBuffers = new List<byte[]>(NumStreamingBuffers);
                bufferStatus = new System.Collections.BitArray(NumStreamingBuffers, false);

                try
                {
                    xaudio = new XAudio2();
                    masteringVoice = new MasteringVoice(xaudio);
                }
                catch
                {
                    // xaudio.DeviceCount == 0 ?
                }
            }
        }

        /// <summary>
        /// Создать буфферы
        /// </summary>
        private void CreateBuffers()
        {
            StreamingBufferSize = playStream.WaveFormat.ConvertLatencyToByteSize(bufDuration);
            
            for (var i = 0; i < NumStreamingBuffers; i++)
            {
                byte[] byteBuff = new byte[StreamingBufferSize];
                byteBuffers.Add(byteBuff);
                AudioBuffer audioBuff = new AudioBuffer
                {
                    AudioData = new SlimDX.DataStream(byteBuff, true, true)
                };
                streamBuffers.Add(audioBuff);
            }
        }

        /// <summary>
        /// Метод обработки завершения воспроизвидения буффера
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Streaming_BufferEnd(object sender, ContextEventArgs e)
        {
            lock (mutex)
            {
                // Unset the bit for the completed buffer
                bufferStatus.Set((int)e.Context, false);

                // Signal to the PlayThread
                Monitor.Pulse(mutex);
            }
        }

        /// <summary>
        /// Заполнить буффер по указанному индексу
        /// </summary>
        /// <param name="bufferIdx"></param>
        private void FillBuffer(int bufferIdx)
        {
            AudioBuffer buffer = streamBuffers[bufferIdx];
            byte[] byteBuffer = byteBuffers[bufferIdx];

            var dataRead = playStream.Read(byteBuffer, 0, StreamingBufferSize);
            if (dataRead > 0)
            {
                //bandEq.Process(byteBuffer, dataRead);
                buffer.Flags = BufferFlags.EndOfStream;
                buffer.AudioBytes = dataRead;
                buffer.Context = (IntPtr)bufferIdx;
                sourceVoice.SubmitSourceBuffer(buffer);
                bufferStatus.Set(bufferIdx, true);
            }
        }

        /// <summary>
        /// Метод цикла воспроизведения
        /// </summary>
        /// <param name="_"></param>
        void PlayThread(object _)
        {
            lock (mutex)
            {
                while (true)
                {
                    if (mTerminate)
                    {
                        return;
                    }

                    // Fill any unfilled buffers
                    for (var i = 0; i < bufferStatus.Count; i++)
                    {
                        if (bufferStatus.Get(i) == false)
                        {
                            FillBuffer(i);
                        }
                    }

                    // Wait to be signalled that a buffer is empty
                    Monitor.Wait(mutex);
                }
            }
        }

        /// <summary>
        /// Выходной формат аудио данных
        /// </summary>
        private SlimDX.Multimedia.WaveFormat OutputWaveFormat
        {
            get
            {
                var fmt = playStream.WaveFormat; //defaultWaveFmt.AverageBytesPerSecond
                return new SlimDX.Multimedia.WaveFormat()
                {
                    AverageBytesPerSecond = fmt.AverageBytesPerSecond,
                    BitsPerSample = fmt.BitsPerSample,
                    BlockAlignment = fmt.BlockAlignment,
                    Channels = fmt.Channels,
                    FormatTag = fmt.FormatTag,
                    SamplesPerSecond = fmt.SamplesPerSecond
                };
            }
        }

        /// <summary>
        /// Начать воспроизведение
        /// </summary>
        public void Start()
        {
            sourceVoice = new SourceVoice(xaudio, OutputWaveFormat)
            {
                Volume = 1
            };
            sourceVoice.BufferEnd += bufferEndCallback;

            // Fill buffers initially
            bool isDone = false;
            for (var i = 0; i < NumStreamingBuffers; i++)
            {
                FillBuffer(i);
            }

            sourceVoice.Start();

            if (!isDone)
            {
                ParameterizedThreadStart threadProc = PlayThread;
                playThread = new Thread(threadProc)
                {
                    Name = "Audio playback Thread"
                };
                playThread.Start();
            }
        }

        /// <summary>
        /// Очистка ресурсов, освобождение памяти
        /// </summary>
        public void Dispose()
        {
            lock (mutex)
            {
                mTerminate = true;
                if (playThread != null)
                {
                    Monitor.Pulse(mutex);
                    //playThread.Abort();
                    playThread = null;
                }

                if (streamBuffers != null)
                {
                    foreach (AudioBuffer buffer in streamBuffers)
                    {
                        buffer.AudioData.Dispose();
                        buffer.Dispose();
                    }
                    byteBuffers = null;
                    streamBuffers = null;
                }

                if (masteringVoice != null)
                {
                    masteringVoice.Dispose();
                    masteringVoice = null;
                }

                if (xaudio != null)
                {
                    xaudio.Dispose();
                    xaudio = null;
                }

                playStream = null;
            }
        }
    }
}
