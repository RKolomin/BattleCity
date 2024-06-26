using BattleCity.Audio;
using BattleCity.Audio.Decoders;
using BattleCity.Common;
using BattleCity.Enums;
using BattleCity.Logging;
using System.IO;
using System.Linq;

namespace BattleCity.Repositories
{
    /// <summary>
    /// Репозиторий звуковых ресурсов
    /// </summary>
    public class SoundRepository : BaseRepository<SoundResource>
    {
        /// <summary>
        /// Альтернативная контент директория
        /// </summary>
        public string AlterContentDirectory { get; set; }

        /// <summary>
        /// Конструктор
        /// </summary>
        public SoundRepository(ILogger logger, string directory, string filename, int capacity)
            : base(logger, directory, filename, capacity) { }

        /// <summary>
        /// Загрузить индекс-таблицу
        /// </summary>
        /// <param name="log"></param>
        public void Load()
        {
            Deserialize();

            foreach (var item in array)
            {
                if (item == null)
                    continue;

                if (string.IsNullOrEmpty(item.File))
                {
                    logger?.WriteLine($"snd_id {item.Id}: empty file name", LogLevel.Warning);
                }
                else
                {
                    if (File.Exists(GetFileFullpath(directory, item.File)))
                    {
                        item.FileExists = true;
                    }
                    else if (!string.IsNullOrEmpty(AlterContentDirectory) && File.Exists(GetFileFullpath(AlterContentDirectory, item.File)))
                    {
                        item.FileExists = true;
                    }
                    else
                    {
                        logger?.WriteLine($"snd_id {item.Id}: \"{item.File}\" not exists", LogLevel.Warning);
                    }
                }
            }
        }

        private string GetFileFullpath(string contentDirectory, string fileName)
        {
            return Path.Combine(contentDirectory, "Sounds", fileName);
        }

        protected override void OnDeserializeItem(int index, SoundResource t)
        {
            t.File = t.File.Trim().ToUpper();
            array[t.Id] = t;
        }

        /// <summary>
        /// Добавить звуковой ресурс
        /// </summary>
        /// <param name="file"></param>
        /// <param name="repeat"></param>
        /// <param name="soundType"></param>
        /// <returns></returns>
        public bool Add(string file, bool repeat, SoundType soundType = SoundType.Sound)
        {
            if (Path.IsPathRooted(file))
                file = Path.GetFileName(file);

            var name = Path.GetFileNameWithoutExtension(file);
            file = file.Trim().ToUpper();

            foreach (var t in array)
            {
                if (t == null)
                    continue;
                if (t.Type == soundType && string.Compare(t.File, file, true) == 0)
                    return true;
            }

            for (int i = 1; i < array.Length; i++)
                if (array[i] == null)
                {
                    array[i] = new SoundResource()
                    {
                        Id = i,
                        File = file,
                        Name = name,
                        Type = soundType,
                        AutoRepeat = repeat
                    };
                    return true;
                }

            return false;
        }

        /// <summary>
        /// Получить звуковой ресурс по имени
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public SoundResource GetByName(string name, SoundType type = SoundType.Sound)
        {
            if (string.IsNullOrEmpty(name))
                return null;

            return array.FirstOrDefault(p =>
                p != null && p.Type == type && p.Name != null && p.Name.ToUpper() == name.ToUpper());
        }

        /// <summary>
        /// Получить или создать <see cref="IAudioReader"/> по имени ресурса
        /// </summary>
        /// <param name="sndName"></param>
        /// <returns></returns>
        public IAudioReader GetOrCreateAudioReader(string sndName, SoundType type)
        {
            if (string.IsNullOrEmpty(sndName))
                return null;

            var item = GetByName(sndName, type);
            return GetOrCreateAudioReader(item?.Id ?? 0);
        }

        /// <summary>
        /// Получить или создать <see cref="IAudioReader"/> по идентификатору ресурса
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public IAudioReader GetOrCreateAudioReader(int id)
        {
            if (id == 0)
                return null;

            var item = array[id];

            if (item == null)
                return null;

            if (!item.FileExists)
                return null;

            if (item.Data == null)
            {
                string fullPath = GetFileFullpath(directory, item.File);
                bool fileExists = true;

                if (!File.Exists(fullPath))
                {
                    fileExists = false;
                    if (AlterContentDirectory != null)
                    {
                        fullPath = GetFileFullpath(AlterContentDirectory, item.File);
                        fileExists = File.Exists(fullPath);
                    }
                }

                if (!fileExists)
                {
                    item.FileExists = false;
                    return null;
                }

                using (var stream = new WaveFileReader(fullPath, 0, true, true, true))
                {
                    item.Data = new byte[stream.Length];
                    stream.Read(item.Data, 0, item.Data.Length);

                    if (stream.WaveFormat.SamplesPerSecond != 44100 || stream.WaveFormat.Channels != 2)
                    {
                        item.Data = Resampler.Resample(item.Data, stream.WaveFormat.SamplesPerSecond, stream.WaveFormat.Channels);
                    }

                    item.Format = new SlimDX.Multimedia.WaveFormat()
                    {
                        AverageBytesPerSecond = 176400,
                        BitsPerSample = 16,
                        BlockAlignment = 4,
                        Channels = 2,
                        FormatTag = SlimDX.Multimedia.WaveFormatTag.Pcm,
                        SamplesPerSecond = 44100
                    };
                }
            }

            return new PcmAudioReader(item.Format, item.Type, item.Name, item.Data);
        }

        /// <summary>
        /// Удаление всех используемых объектов, освобождение памяти
        /// </summary>
        public override void Dispose()
        {
            base.Dispose();

            if (array != null)
            {
                foreach (var item in array)
                {
                    if (item != null)
                        item.Data = null;
                }
            }
        }

    }
}