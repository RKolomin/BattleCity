using Newtonsoft.Json;
using SlimDX.Direct3D9;
using System;
using System.IO;

namespace BattleCity.Common
{
    /// <summary>
    /// Текстурный ресурс
    /// </summary>
    public class TextureResource : IResxId, IDisposable
    {
        /// <inheritdoc/>
        public int Id { get; set; }

        /// <summary>
        /// Имя файла текстуры
        /// </summary>
        public string File { get; set; }

        /// <summary>
        /// Цвет-маска
        /// </summary>
        public int ColorKey { get; set; } = ColorConverter.ToInt32("#FF408000");

        /// <summary>
        /// Признак того, что файл текстуры существует
        /// </summary>
        [JsonIgnore]
        public bool FileExists { get; set; }

        /// <summary>
        /// Названиие текстуры
        /// </summary>
        [JsonIgnore]
        public string Name { get; private set; }

        /// <summary>
        /// Текстура
        /// </summary>
        [JsonIgnore]
        public Texture Texture { get; set; }

        /// <summary>
        /// Формат загружаемой текстуры
        /// </summary>
        [JsonIgnore]
        public Format TextureFormat { get; set; } = Format.A4R4G4B4;

        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="id"></param>
        /// <param name="file"></param>
        [JsonConstructor]
        public TextureResource(int id, string file)
        {
            Id = id;
            File = file;
            Name = Path.GetFileNameWithoutExtension(File).Trim().ToUpper();
        }

        /// <summary>
        /// Загрузить текстуру из файла
        /// </summary>
        /// <param name="device"></param>
        /// <param name="file"></param>
        public void Load(Device device, string file)
        {
            Texture = Texture.FromFile(device, file, 0, 0, 1, 0, TextureFormat, Pool.Managed, Filter.None, Filter.None, ColorKey);
            //Texture = Texture.FromFile(device, file, 0, 0, 0, 0, TexutureLoadFormat, Pool.Default, Filter.Linear, Filter.Linear, ColorKey);
        }

        /// <summary>
        /// Очистить ресурс, освободить память
        /// </summary>
        public void Dispose()
        {
            if (Texture != null)
            {
                Texture.Dispose();
                Texture = null;
            }
        }
    }
}
