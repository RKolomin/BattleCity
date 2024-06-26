using Device = SlimDX.Direct3D9.Device;
using System;
using System.Drawing;
using System.Drawing.Text;
using BattleCity.Repositories;
using BattleCity.Logging;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using GdiFont = System.Drawing.Font;

namespace BattleCity.Common
{
    /// <summary>
    /// Игровой контент
    /// </summary>
    public class GameContent : IDisposable
    {
        #region members

        /// <summary>
        /// Директория контента по умолчанию
        /// </summary>
        public const string DefaultContentDirectory = "Data";

        /// <summary>
        /// Наименование файла конфигураций по умолчанию
        /// </summary>
        public const string GameConfigFileName = "game_config.json";

        /// <summary>
        /// Наименование файла уровней (stages)
        /// </summary>
        private const string StagesFileName = "stages.json";

        /// <summary>
        /// Наименование файла таблицы звуков
        /// </summary>
        private const string SoundsFileName = "sounds.json";

        /// <summary>
        /// Наименование файла таблицы текстур
        /// </summary>
        private const string TexturesFileName = "textures.json";

        /// <summary>
        /// Наименование файла таблицы игровых объектов
        /// </summary>
        private const string GameObjectsFileName = "game_objects.json";

        /// <summary>
        /// Размер репозитория (он же размер таблицы)
        /// </summary>
        private const int RepositoryCapacity = 256;

        /// <summary>
        /// Признак контент директории по умолчанию
        /// </summary>
        public bool IsDefaultContentDirectory => string.Compare(ContentDirectory, DefaultContentDirectory, true) == 0;

        private ILogger logger;
        private PrivateFontCollection fontCollection = new PrivateFontCollection();
        private readonly Dictionary<string, string> fontDictionary = new Dictionary<string, string>();

        #endregion

        #region Properties

        /// <summary>
        /// Диктория контента
        /// </summary>
        public string ContentDirectory { get; private set; }

        /// <summary>
        /// Основные конфигурации
        /// </summary>
        public CommonConfig CommonConfig { get; set; } = new CommonConfig();

        /// <summary>
        /// Игровые конфигурация
        /// </summary>
        public GameConfig GameConfig { get; set; }

        /// <summary>
        /// Звуки
        /// </summary>
        public SoundRepository Sounds { get; set; }

        /// <summary>
        /// Текстуры
        /// </summary>
        public TextureRepository Textures { get; set; }

        /// <summary>
        /// Игровые объекты
        /// </summary>
        public GameObjectRepository GameObjects { get; set; }

        /// <summary>
        /// Уровни (stages)
        /// </summary>
        public StageRepository Stages { get; set; }

        #endregion

        #region Constructor

        /// <summary>
        /// Конструктор
        /// </summary>
        public GameContent(string contentDirectory, ILogger logger)
        {
            this.logger = logger;
            ContentDirectory = string.IsNullOrEmpty(contentDirectory) ? DefaultContentDirectory : contentDirectory;

            string gameConfigDirectory = ContentDirectory;
            string soundsDirectory = ContentDirectory;
            string texturesDirectory = ContentDirectory;
            string gameObjectsDirectory = ContentDirectory;
            string stagesDirectory = ContentDirectory;

            #region Game config

            if (!IsDefaultDirectory(gameConfigDirectory) && !FileExists(Path.Combine(gameConfigDirectory, GameConfigFileName)))
            {
                gameConfigDirectory = DefaultContentDirectory;
            }

            if (IsDefaultDirectory(gameConfigDirectory) && !FileExists(Path.Combine(gameConfigDirectory, GameConfigFileName)))
            {
                GameContentGenerator.CreateDefaultGameConfig(contentDirectory).Save();
            }

            GameConfig = GameConfig.Load(gameConfigDirectory, GameConfigFileName, logger)
                      ?? GameContentGenerator.CreateDefaultGameConfig(DefaultContentDirectory);

            #endregion

            #region Sounds

            if (!IsDefaultDirectory(soundsDirectory) && !FileExists(Path.Combine(soundsDirectory, SoundsFileName)))
            {
                soundsDirectory = DefaultContentDirectory;
            }
            if (!FileExists(Path.Combine(soundsDirectory, SoundsFileName)))
            {
                if (!IsDefaultDirectory(soundsDirectory))
                {
                    soundsDirectory = DefaultContentDirectory;
                }
                if (IsDefaultDirectory(soundsDirectory))
                {
                    using (var repo = new SoundRepository(logger, soundsDirectory, TexturesFileName, RepositoryCapacity))
                    {
                        GameContentGenerator.CreateDefaultSoundResources(repo, CommonConfig);
                        repo.Save();
                    }
                }
            }

            Sounds = new SoundRepository(logger, soundsDirectory, SoundsFileName, RepositoryCapacity);
            if (!IsDefaultDirectory(soundsDirectory))
                Sounds.AlterContentDirectory = DefaultContentDirectory;

            #endregion


            #region Textures

            if (!IsDefaultDirectory(texturesDirectory) && !FileExists(Path.Combine(texturesDirectory, TexturesFileName)))
            {
                texturesDirectory = DefaultContentDirectory;
            }
            if (IsDefaultDirectory(texturesDirectory) && !FileExists(Path.Combine(texturesDirectory, TexturesFileName)))
            {
                if (!IsDefaultDirectory(texturesDirectory))
                {
                    texturesDirectory = DefaultContentDirectory;
                }
                if (IsDefaultDirectory(texturesDirectory))
                {
                    using (var repo = new TextureRepository(logger, texturesDirectory, TexturesFileName, RepositoryCapacity))
                    {
                        GameContentGenerator.CreateDefaultTextureResources(repo);
                        repo.Save();
                    }
                }
            }

            Textures = new TextureRepository(logger, texturesDirectory, TexturesFileName, RepositoryCapacity);
            if (!IsDefaultDirectory(texturesDirectory))
                Textures.AlterContentDirectory = DefaultContentDirectory;

            #endregion


            #region Game objects

            if (!IsDefaultDirectory(gameObjectsDirectory) && !FileExists(Path.Combine(gameObjectsDirectory, GameObjectsFileName)))
            {
                gameObjectsDirectory = DefaultContentDirectory;
            }
            if (IsDefaultDirectory(gameObjectsDirectory) && !FileExists(Path.Combine(gameObjectsDirectory, GameObjectsFileName)))
            {
                if (!IsDefaultDirectory(gameObjectsDirectory))
                {
                    gameObjectsDirectory = DefaultContentDirectory;
                }
                if (IsDefaultDirectory(gameObjectsDirectory))
                {
                    using (var repo = new GameObjectRepository(logger, gameObjectsDirectory, GameObjectsFileName, RepositoryCapacity))
                    {
                        Textures.Load(null);
                        Sounds.Load();

                        GameContentGenerator.CreateDefaultGameObjects(this, repo);
                        repo.Save();

                        Textures.Dispose();
                        Sounds.Dispose();

                        Sounds = new SoundRepository(logger, soundsDirectory, SoundsFileName, RepositoryCapacity);
                        if (!IsDefaultDirectory(soundsDirectory))
                            Sounds.AlterContentDirectory = DefaultContentDirectory;

                        Textures = new TextureRepository(logger, texturesDirectory, TexturesFileName, RepositoryCapacity);
                        if (!IsDefaultDirectory(texturesDirectory))
                            Textures.AlterContentDirectory = DefaultContentDirectory;
                    }
                }
            }

            GameObjects = new GameObjectRepository(logger, gameObjectsDirectory, GameObjectsFileName, RepositoryCapacity);

            #endregion


            #region Stages

            if (!IsDefaultDirectory(stagesDirectory) && !FileExists(Path.Combine(stagesDirectory, StagesFileName)))
            {
                stagesDirectory = DefaultContentDirectory;
            }

            Stages = new StageRepository(logger, stagesDirectory, StagesFileName, RepositoryCapacity);

            #endregion
        }

        private bool IsDefaultDirectory(string directory)
        {
            return string.Compare(directory, DefaultContentDirectory, true) == 0;
        }

        private bool FileExists(string filePath)
        {
            return File.Exists(filePath);
        }

        #endregion

        #region Methods

        /// <summary>
        /// Получить максимальный номер уровня, который предлагается выбрать при новой игре
        /// </summary>
        public int GetMaxStageNumber()
        {
            return Stages.LastIndexOf(p => p != null) + 1;
        }

        /// <summary>
        /// Очистить данные
        /// </summary>
        public void Clear()
        {
            Textures.Clear();
            GameObjects.Clear();
            Sounds.Clear();
            fontCollection?.Dispose();
            fontCollection = new PrivateFontCollection();
            fontDictionary.Clear();
        }

        /// <summary>
        /// Загрузить данные
        /// </summary>
        /// <param name="device"></param>
        public void Load(Device device)
        {
            Textures.Load(device);
            GameObjects.Load();
            Sounds.Load();
            Stages.Load();
            LoadFonts();
        }

        /// <summary>
        /// Освободить ресурсы, очистить память
        /// </summary>
        public void Dispose()
        {
            logger = null;

            if (Textures != null)
            {
                Textures.Dispose();
                Textures = null;
            }

            if (GameObjects != null)
            {
                GameObjects.Dispose();
                GameObjects = null;
            }

            if (Sounds != null)
            {
                Sounds.Dispose();
                Sounds = null;
            }

            if (Stages != null)
            {
                Stages.Dispose();
                Stages = null;
            }

            if (fontCollection != null)
            {
                fontCollection.Dispose();
                fontCollection = null;
            }
        }

        /// <summary>
        /// Создать шрифт заданного размера
        /// </summary>
        /// <param name="size">Размер шрифта</param>
        /// <param name="fontFileName">Имя файла шрифта или null (шрифт по умолчанию)</param>
        /// <param name="bold">Задать жирный стиль</param>
        /// <returns></returns>
        public GdiFont GetFont(float size, string fontFileName = null, bool bold = false)
        {
            string fontFamily = null;
            if (fontFileName == null)
            {
                fontDictionary.TryGetValue(CommonConfig.DefaultFontFileName, out fontFamily);
            }
            if (fontFamily == null && !fontDictionary.TryGetValue(fontFileName, out fontFamily))
                fontFamily = null;

            if (fontCollection.Families.Length == 0 || fontFamily == null)
                return new GdiFont(SystemFonts.DefaultFont.FontFamily, size, bold ? FontStyle.Bold : FontStyle.Regular);

            return new GdiFont(fontCollection.Families.First(p => p.Name == fontFamily), size, bold ? FontStyle.Bold : FontStyle.Regular);
        }

        private void LoadFonts()
        {
            string fontDirectory = Path.Combine(ContentDirectory, "Fonts");
            string defaultFontDirectory = Path.Combine(DefaultContentDirectory, "Fonts");

            string fontPath = Path.Combine(fontDirectory, CommonConfig.LogoFontFileName);
            string defaultFontPath = Path.Combine(defaultFontDirectory, CommonConfig.LogoFontFileName);

            if (File.Exists(fontPath))
            {
                AddFontFile(fontPath);
            }
            else if (fontDirectory != defaultFontDirectory && File.Exists(defaultFontPath))
            {
                AddFontFile(defaultFontPath);
            }
            else
            {
                logger?.WriteLine($"Default font {CommonConfig.DefaultFontFileName} not found. {SystemFonts.DefaultFont.FontFamily} will be used", LogLevel.Warning);
            }

            DirectoryInfo fontDir = new DirectoryInfo(fontDirectory);

            try
            {
                fontDir = new DirectoryInfo(fontDirectory);
                if (!fontDir.Exists)
                {
                    logger?.WriteLine($"{nameof(LoadFonts)}: directory \"{fontDirectory}\" not exists", LogLevel.Warning);
                    fontDir = null;
                }
            }
            catch
            {
                logger?.WriteLine($"{nameof(LoadFonts)}: wrong font directory \"{fontDirectory}\"", LogLevel.Warning);
            }

            if (fontDir == null)
            {
                if (fontDirectory != defaultFontDirectory)
                {
                    fontDir = new DirectoryInfo(defaultFontDirectory);
                }
            }

            if (fontDir != null)
            {
                foreach (var file in fontDir.GetFiles())
                {
                    AddFontFile(file.FullName);
                }
            }
        }

        private void AddFontFile(string fontPath)
        {
            string fontFile = fontPath;
            try
            {
                fontFile = Path.GetFileName(fontPath);
                if (fontDictionary.ContainsKey(fontFile))
                    return;
                string fontFamily = null;
                using (var tmpPrivateFontCollection = new PrivateFontCollection())
                {
                    tmpPrivateFontCollection.AddFontFile(fontPath);
                    fontFamily = tmpPrivateFontCollection.Families[0].Name;
                }
                fontCollection.AddFontFile(fontPath);
                fontDictionary.Add(fontFile, fontFamily);
            }
            catch
            {
                logger?.WriteLine($"Error load font file {fontFile}", LogLevel.Error);
            }
        }

        #endregion
    }
}