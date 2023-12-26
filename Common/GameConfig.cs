using BattleCity.Extensions;
using BattleCity.Logging;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Text;
using Point = System.Drawing.Point;

namespace BattleCity.Common
{
    /// <summary>
    /// Конфигурации игры
    /// </summary>
    public class GameConfig
    {
        string fileName;
        string bgrHexColor;
        string battleGndHexColor;
        string transitionScreenBgrHexColor;
        string textHexColor;
        int randomSeed;

        /// <summary>
        /// Генератор случайных чисел
        /// </summary>
        [JsonIgnore]
        public Random Random { get; private set; } = new Random();

        /// <summary>
        /// Включить читы на кнопки F..
        /// </summary>
        [JsonIgnore]
        public bool CheatsEnabled { get; set; }

        /// <summary>
        /// Директория конфигураций
        /// </summary>
        [JsonIgnore]
        public string DirectoryPath { get; private set; }

        /// <summary>
        /// Имя файла конфигураций
        /// </summary>
        [JsonIgnore]
        public string FileName { get; private set; }

        /// <summary>
        /// Название конфигурации
        /// </summary>
        public string Name { get; set; } = "";

        /// <summary>
        /// Начальное значение последовательности псевдослучаных чисел
        /// </summary>
        public int RandomSeed
        {
            get { return randomSeed; }
            set
            {
                randomSeed = value;
                Random = new Random(randomSeed);
            }
        }

        /// <summary>
        /// Стандартный Рекорд по очкам
        /// </summary>
        public int HiScoreValue { get; set; } = 20000;

        /// <summary>
        /// Шанс (%) появления бонуснуго вражесного юнита
        /// </summary>
        public int BonusedEnemySpawnChance { get; set; }

        /// <summary>
        /// Максимальное количество бонусных юнитов единовременно на поле.
        /// -1=не ограничено, 0=отключено
        /// </summary>
        public int MaxBonusedUnitsOnField { get; set; }

        /// <summary>
        /// Продолжительность временной защиты базы (Количество секунд)
        /// </summary>
        public int TowerTempDefenseDuration { get; set; }

        /// <summary>
        /// Максиальное количество возможных бонусов от вражеских юнитов
        /// </summary>
        public int MaxExtraBonusPerUnit { get; set; }

        /// <summary>
        /// Агрессивность (%) вражеских юнитов
        /// </summary>
        public int EnemyAgressivity { get; set; }

        /// <summary>
        /// Цвет бонусного юнита (для анимации переливающегося юнита)
        /// </summary>
        public string EnemyFlashHexColor { get; set; }

        /// <summary>
        /// Показывать шахматку
        /// </summary>
        public bool ShowChessboard { get; set; }

        /// <summary>
        /// За какое набранное количество очков начисляется бонусная жизнь.
        /// 0 = отключено.
        /// </summary>
        public int RewardsExtraLifeAnEvery { get; set; }

        /// <summary>
        /// Бонус очков игроку, который уничтожил больше всего вражеских юнитов.
        /// Начисляется при подведении итогов прохождения уровня (stage)
        /// </summary>
        public int ChallengeBonusPoints { get; set; }

        /// <summary>
        /// Бонус очков за успешное прохождение уровня (stage).
        /// Начисляется при подведении итогов прохождения уровня (stage)
        /// </summary>
        public int StageBonusPoints { get; set; }

        /// <summary>
        /// Максимальное количество объектов прокачки на поле.
        /// -1=неограничено, 0=объекты никогда не будут появляться.
        /// </summary>
        public int MaxActivePowerUpsOnField { get; set; }

        /// <summary>
        /// Скрывать с поля бонусы, если появился бонусный юнит
        /// </summary>
        public bool HidePowerUpsIfBonusedEnemySpawned { get; set; }

        /// <summary>
        /// Глобальный множитель скорости вражеских юнитов
        /// </summary>
        public int EnemyMoveSpeedMultiply { get; set; } = 1;

        /// <summary>
        /// Глобальный множитель скорости юнитов игроков
        /// </summary>
        public int PlayerMoveSpeedMultiply { get; set; } = 1;

        /// <summary>
        /// Разрешить игроку уничтожать свою базу
        /// </summary>
        public bool PlayerDestroyBaseAllowed { get; set; }

        /// <summary>
        /// Признак того, что вражеские юниты могут подбирать объекты прокачки
        /// </summary>
        public bool EnemyPowerUpAllowed { get; set; }

        /// <summary>
        /// Признак того, что юниты игроков могут подбирать объекты прокачки
        /// </summary>
        public bool PlayerPowerUpAllowed { get; set; }

        /// <summary>
        /// Признак того, что для вражеских юнитов действует эффект прокачки
        /// </summary>
        public bool EnemyPowerUpHasEffect { get; set; }

        /// <summary>
        /// Продолжительность отображения объекта прокачки (в кадрах).
        /// 0=неограничено
        /// </summary>
        public int PowerUpLifetimeDuration { get; set; }

        /// <summary>
        /// Длительность анимации заморозки / блокировки юнита
        /// </summary>
        public int UnitFreezeAnimationFrames { get; set; }

        /// <summary>
        /// Длительность отображения одного цвета в режиме переливания цветов
        /// </summary>
        public int UnitFlashColorDuration { get; set; } = 2;

        /// <summary>
        /// Длительность отображения одного цвета в режиме переливания цветов у бонусного юнита
        /// </summary>
        public int PowerUpBonusFlashColorDuration { get; set; }

        /// <summary>
        /// Сбрасывать прокачку юнитов при запуске уровня (stage)
        /// </summary>
        public bool ResetUnitUpgradesOnStageStart { get; set; }

        /// <summary>
        /// Разрешить присоединяться к битке неактивным игрокам
        /// </summary>
        public bool AllowPlayerJoin { get; set; }

        /// <summary>
        /// Признак получения урона игроком при дружественной аттаке игроков
        /// </summary>
        public bool PlayerFriendlyFire { get; set; }

        /// <summary>
        /// Признак получения урона врага при дружественной аттаке врагов
        /// </summary>
        public bool EnemyFriendlyFire { get; set; }

        /// <summary>
        /// Продолжительность движения по инерции (в кадрах)
        /// </summary>
        public int MoveInertionDuration { get; set; }

        /// <summary>
        /// Продолжительность заморозки юнитов (в кадрах)
        /// </summary>
        public int EnemyFreezeDuration { get; set; }

        /// <summary>
        /// Продолжительность заморозки юнитов игроков (в кадрах)
        /// </summary>
        public int PlayerFreezeDuration { get; set; }

        /// <summary>
        /// Продолжительность действия щита у игрока при появлении (Shield) (в кадрах)
        /// </summary>
        public int PlayerSpawnShieldDuration { get; set; }

        /// <summary>
        /// Продолжительность действия щита у вржеского юнита при появлении (Shield) (в кадрах)
        /// </summary>
        public int EnemySpawnShieldDuration { get; set; }

        /// <summary>
        /// Продолжительность действия бонусного щита (Shield) (в кадрах)
        /// </summary>
        public int ExtraShieldDuration { get; set; }

        /// <summary>
        /// Продолжительность отображение текста значения очков (в кадрах)
        /// </summary>
        public int PointsTextShowDuration { get; set; }

        /// <summary>
        /// Бонусные очки за сокровища
        /// </summary>
        public int TreasureBonusPoints { get; set; }

        /// <summary>
        /// Задержка в кадрах перед последующим появлением вражеского юнита
        /// </summary>
        public int EmenySpawnDelay { get; set; }

        /// <summary>
        /// Продолжительность анимации появления юнита (в кадрах)
        /// </summary>
        public int SpawnAnimationDuration { get; set; }

        /// <summary>
        /// Время ожидания в секундах после победы на уровне (stage)
        /// </summary>
        public int StageCompleteDelayTime { get; set; }

        /// <summary>
        /// Максимально дозволенное количество вражеских юнитов на уровне (stage)
        /// </summary>
        public int MaxEnemiesPerStage { get; set; }

        /// <summary>
        /// Максимальное количество активных вражеских юнитов на поле
        /// </summary>
        public int MaxActiveEnemy { get; set; }

        /// <summary>
        /// Количество позиций появления вражеских юнитов
        /// </summary>
        public int EnemySpawnPositionCount { get; set; }

        /// <summary>
        /// Координаты появления вражеских юнитов
        /// </summary>
        public Point[] EnemySpawnLocations { get; set; } = new Point[3]
        {
            new Point(0, 0),
            new Point(24, 0),
            new Point(48, 0)
        };

        /// <summary>
        /// Координаты появления игроков
        /// </summary>
        public Point[] PlayerSpawnLocations { get; set; } = new Point[2]
        {
            new Point(16, 48),
            new Point(32, 48)
        };

        /// <summary>
        /// Координаты размещения стратегических объектов
        /// </summary>
        public Point[] TowerLocations { get; set; } = new Point[]
        {
            new Point(24, 48)
        };

        /// <summary>
        /// Отличительные цвета у игроков
        /// </summary>
        public string[] PlayerColors { get; set; } =
        {
            "#FFFFBE30",
            "#FF0DA657"
        };

        /// <summary>
        /// Индекс позиции, откуда будет появляться самый первый вражеский юнит
        /// </summary>
        public int EnemyFirstSpawnPositionIndex { get; set; }

        /// <summary>
        /// Максимальное количество вражеских юнитов за игру (stage)
        /// </summary>
        public int MaxEnemy { get; set; }

        /// <summary>
        /// Начальное количество жизней у игроков
        /// </summary>
        public int StartLifes { get; set; }

        /// <summary>
        /// Начальный уровень прокачки юнита у игроков
        /// </summary>
        public int PlayerDefaultUpgradeLevel { get; set; }

        /// <summary>
        /// Максимальный уровень прокачки юнита.
        /// -1=не ограничего, 0=без прокачки, >= 1.. макс значение прокачки.
        /// </summary>
        public int UnitMaxUpgradeLevel { get; set; }

        /// <summary>
        /// Показывать экран GAME OVER
        /// </summary>
        public bool ShowGameOverScreen { get; set; }

        /// <summary>
        /// Показывать экран HISCORE
        /// </summary>
        public bool ShowHiScoreScreen { get; set; }

        /// <summary>
        /// Цвет заднего плана
        /// </summary>
        public string BackgroundHexColor
        {
            get { return bgrHexColor; }
            set
            {
                bgrHexColor = value;
                BackgroundColor = ColorConverter.ToInt32(value);
            }
        }

        /// <summary>
        /// Цвет заднего плана экрана перехода
        /// </summary>
        public string TransitionScreenBackgroundHexColor
        {
            get { return transitionScreenBgrHexColor; }
            set
            {
                transitionScreenBgrHexColor = value;
                TransitionScreenBackgroundColor = ColorConverter.ToInt32(value);
            }
        }

        /// <summary>
        /// Цвет поля битвы
        /// </summary>
        public string BattleGroundHexColor
        {
            get { return battleGndHexColor; }
            set
            {
                battleGndHexColor = value;
                BattleGroundColor = ColorConverter.ToInt32(value);
            }
        }

        /// <summary>
        /// Цвет текста
        /// </summary>
        public string TextHexColor
        {
            get { return textHexColor; }
            set
            {
                textHexColor = value;
                TextColor = ColorConverter.ToInt32(value);
            }
        }

        /// <summary>
        /// Цвет чётной шахматной клетки
        /// </summary>
        public string ChessCellHexColor1 { get; set; }

        /// <summary>
        /// Цвет нечётной шахматной клетки
        /// </summary>
        public string ChessCellHexColor2 { get; set; }

        [JsonIgnore]
        public int TransitionScreenBackgroundColor { get; private set; }

        [JsonIgnore] // 0xFF1F1F1F
        public int BackgroundColor { get; private set; }

        [JsonIgnore]
        public int BattleGroundColor { get; private set; }

        [JsonIgnore]
        public int TextColor { get; set; }

        /// <summary>
        /// Ширина игрового поля в условных пикселях
        /// </summary>
        public int FieldWidth { get; set; }

        /// <summary>
        /// Высота игрового поля в условных пикселях
        /// </summary>
        public int FieldHeight { get; set; }

        /// <summary>
        /// Глобальная настройка! Размер условных субпикселей (масштабирование размера клетки).
        /// Это значение используется для домножение условных единиц для перевода в реальные координаты / размеры.
        /// Значение должно быть > 0
        /// </summary>
        public int SubPixelSize { get; set; } = 1;

        /// <summary>
        /// Продолжительность мерцания placeholder'а в режиме конструктора
        /// </summary>
        public int PlaceholderFlickerFrames { get; set; } = 15;

        /// <summary>
        /// Конструктор без параметров
        /// </summary>
        private GameConfig() { }

        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="directoryPath">Путь к директории с конфигурациями</param>
        public GameConfig(string directoryPath)
        {
            DirectoryPath = directoryPath;
        }

        /// <summary>
        /// Загрузить конфигурации из файла
        /// </summary>
        /// <returns></returns>
        public static GameConfig Load(string directoryPath, string fileName, ILogger logger)
        {
            string filePath = directoryPath + "\\" + fileName;
            if (!File.Exists(filePath))
                return null;
            try
            {
                var data = File.ReadAllText(filePath, Encoding.UTF8);
                var config = JsonConvert.DeserializeObject<GameConfig>(data);
                config.DirectoryPath = directoryPath;
                config.fileName = fileName;
                return config;
            }
            catch (Exception ex)
            {
                logger?.WriteLine($"Load {nameof(GameConfig)} error: " + ex);
                return null;
            }
        }

        /// <summary>
        /// Сохранить конфигурации в исходный файл
        /// </summary>
        public void Save()
        {
            var data = this.ToJson();
            string filePath = DirectoryPath + "\\" + fileName;
            File.Delete(filePath);
            using (StreamWriter stream = new StreamWriter(filePath, false, Encoding.UTF8))
            {
                stream.Write(data);
                stream.Close();
            }
        }
    }
}