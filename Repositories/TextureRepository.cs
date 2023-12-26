using BattleCity.Common;
using BattleCity.Logging;
using SlimDX.Direct3D9;
using System.IO;
using System.Linq;

namespace BattleCity.Repositories
{
    /// <summary>
    /// Репозиторий текстурных ресурсов
    /// </summary>
    public class TextureRepository : BaseRepository<TextureResource>
    {
        Device device;
        Texture whitePixel;
        Texture redPixel;

        /// <summary>
        /// Альтернативная контент директория
        /// </summary>
        public string AlterContentDirectory { get; set; }

        /// <summary>
        /// Конструктор
        /// </summary>
        public TextureRepository(ILogger logger, string directory, string filename, int capacity)
            : base(logger, directory, filename, capacity) { }

        /// <summary>
        /// Загрузить индекс-таблицу
        /// </summary>
        /// <param name="device"></param>
        public void Load(Device device)
        {
            this.device = device;

            whitePixel = CreateColoredTexture(ColorConverter.ToInt32("#FFFFFFFF"), 1, 1);
            redPixel = CreateColoredTexture(ColorConverter.ToInt32("#FFFF0000"), 1, 1);

            Deserialize();

            foreach (var item in array)
            {
                if (item == null)
                    continue;

                if (string.IsNullOrEmpty(item.File))
                {
                    logger?.WriteLine($"tex_id {item.Id}: empty file name", LogLevel.Warning);
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
                        logger?.WriteLine($"tex_id {item.Id}: file not exists", LogLevel.Warning);
                    }
                }
            }
        }

        private Texture CreateColoredTexture(int color, int width, int height)
        {
            if (device == null)
                return null;

            var texture = new Texture(device, width, height, 1, 0, Format.A8R8G8B8, Pool.Managed);
            var dataRect = texture.LockRectangle(0, 0);
            dataRect.Data.Position = 0;

            for (int n = 0; n < width * height; n++)
            {
                dataRect.Data.Write(color);
            }

            texture.UnlockRectangle(0);

            return texture;
        }

        private string GetFileFullpath(string contentDirectory, string fileName)
        {
            return Path.Combine(contentDirectory, "Textures", fileName);
        }

        protected override void OnDeserializeItem(int index, TextureResource t)
        {
            t.File = t.File.Trim().ToUpper();
            array[t.Id] = t;
        }

        /// <summary>
        /// Добавить текстурный ресурс
        /// </summary>
        /// <param name="file"></param>
        /// <param name="colorKey"></param>
        /// <returns></returns>
        public bool Add(string file, int? colorKey = null)
        {
            if (Path.IsPathRooted(file))
                file = Path.GetFileName(file);

            file = file.Trim().ToUpper();

            foreach (var t in array)
            {
                if (t == null) continue;
                if (t.File == file) return true;
            }

            for (int i = 1; i < array.Length; i++)
            {
                if (array[i] == null)
                {
                    array[i] = new TextureResource(i, file);
                    if (colorKey.HasValue)
                        array[i].ColorKey = colorKey.Value;
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Получить текстурный ресурс по имени
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public TextureResource GetByName(string name)
        {
            if (string.IsNullOrEmpty(name))
                return null;

            return array.FirstOrDefault(p => p != null && p.Name != null && p.Name.ToUpper() == name.ToUpper());
        }

        /// <summary>
        /// Получить или загрузить <see cref="Texture"/> по индентификатру ресурса
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public Texture GetOrCreateTexture(int id)
        {
            if (id < 0)
                return null;

            if (id == 0)
                return whitePixel;

            var item = array[id];

            if (item == null || !item.FileExists)
                return redPixel;

            if (item.Texture == null)
            {
                var fullPath = GetFileFullpath(directory, item.File);
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
                    return redPixel;
                }

                item.Load(device, fullPath);
            }

            return item.Texture;
        }

        /// <summary>
        /// Удаление всех используемых объектов, освобождение памяти
        /// </summary>
        public override void Dispose()
        {
            if (whitePixel != null)
            {
                whitePixel.Dispose();
                whitePixel = null;
            }

            if (redPixel != null)
            {
                redPixel.Dispose();
                redPixel = null;
            }

            if (array != null)
            {
                foreach (var item in array)
                {
                    if (item == null || item.Texture == null)
                        continue;

                    item.Texture.Dispose();
                    item.Texture = null;
                }
            }

            device = null;

            base.Dispose();
        }
    }
}