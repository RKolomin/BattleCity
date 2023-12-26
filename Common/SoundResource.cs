using BattleCity.Enums;
using Newtonsoft.Json;
using SlimDX.Multimedia;

namespace BattleCity.Common
{
    /// <summary>
    /// Звуковой ресурс
    /// </summary>
    public class SoundResource : IResxId
    {
        /// <summary>
        /// <inheritdoc/>
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Имя файла
        /// </summary>
        public string File { get; set; }

        /// <summary>
        /// Название ресурса
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Авто повтор (цикличность воспроизведения)
        /// </summary>
        public bool AutoRepeat { get; set; }

        /// <summary>
        /// Тип данных
        /// </summary>
        public SoundType Type { get; set; }

        /// <summary>
        /// PCM данные
        /// </summary>
        [JsonIgnore]
        public byte[] Data { get; set; }

        /// <summary>
        /// Формат PCM данных
        /// </summary>
        [JsonIgnore]
        public WaveFormat Format { get; set; }

        /// <summary>
        /// Признак существования файла
        /// </summary>
        [JsonIgnore]
        public bool FileExists { get; set; }
    }
}
