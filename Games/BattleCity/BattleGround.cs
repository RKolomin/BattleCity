using BattleCity.Audio;
using BattleCity.Common;
using BattleCity.InputControllers;
using BattleCity.Enums;
using BattleCity.Extensions;
using BattleCity.GameObjects;
using BattleCity.Handlers.PowerUp;
using BattleCity.Helpers;
using BattleCity.Logging;
using BattleCity.Video;
using BattleCity.VisualComponents;
using System;
using System.Collections.Generic;
using System.Linq;
using Rectangle = System.Drawing.Rectangle;

namespace BattleCity
{
    /// <summary>
    /// Механика игры (gameplay)
    /// </summary>
    public class BattleGround : IDisposable
    {
        #region events

        public event Action Exit;
        public event Action<StageResult> EndGame;

        #endregion


        #region Properties

        /// <summary>
        /// Признак того, что ресурсы освобождены
        /// </summary>
        public bool IsDisposed { get; private set; }

        /// <summary>
        /// Признак, того, что игра на паузе
        /// </summary>
        public bool IsGamePaused => gamePauseOverlay.IsVisible;

        /// <summary>
        /// Конфигурации игры
        /// </summary>
        public GameConfig Config { get; }

        /// <summary>
        /// Количество активных игроков
        /// </summary>
        private int NumActivePlayers => players.Count(p => p != null && p.IsAlive);

        /// <summary>
        /// Область поля
        /// </summary>
        private Rectangle FieldBounds => new Rectangle(Left, Top, Width, Height);

        #endregion


        #region members

        int Left;
        int Top;
        int Width;
        int Height;

        List<GameFieldObject> gameObjects = new List<GameFieldObject>();
        List<EnemyUnit> battleUnits = new List<EnemyUnit>();
        List<Bullet> bullets = new List<Bullet>();
        List<RespawnPoint> respawnPoints = new List<RespawnPoint>();
        List<AnimationObject> animations = new List<AnimationObject>();
        List<IPowerUpHandler> powerUpHandlers = new List<IPowerUpHandler>();
        List<TextBlock> textBlocks = new List<TextBlock>();
        ConstructionHelper constructionHelper;
        IndexGenerator enemyIndexGenerator = new IndexGenerator();
        IAudioReader stageStartSnd;

        #region очередь появления вражеских юнитов, их активное количество и статусы

        Queue<SpawnQueueBattleUnit> enemyQueue = new Queue<SpawnQueueBattleUnit>();
        int enemySpawnPositionIndex;
        int enemiesCount;
        int stageMaxActiveEnemies;
        int activeEnemiesCount;
        int freezeEnemyTime;
        int spawnedEnemyCount;
        int spawnEnemyDelayFrames;
        bool enemyIsActive = true;

        #endregion

        #region защитные блоки, их состояние и продолжительность действия

        List<GameFieldObject> brickDefenseObjList = new List<GameFieldObject>();
        List<GameFieldObject> ironDefenseObjList = new List<GameFieldObject>();
        int defenseState;
        int tempTowerDefenseFrameNumber;

        #endregion

        ILogger logger;
        IControllerHub controllerHub;
        ISoundEngine soundEngine;
        IGameApplication gameApplication;
        IGameGraphics graphics;
        IDeviceContext deviceContext;
        GameContent content;
        // шрифт игровой (статусной) панели
        IGameFont gamePanelFont;
        // шрифт отображения очков
        IGameFont pointsTextFont;
        // текущий номер уровня
        int stageNumber;
        // начальное количество игроков
        readonly int initialNumPlayers;
        // текущее состояние уровня
        StageStateEnum stageState;
        // оверлей отображения статуса GameOver
        GameOverOverlay gameOverOverlay;
        // оверлей отображения статуса игры на паузе
        GamePauseOverlay gamePauseOverlay;
        // оверлей завершения уровня
        StageCompleteOverlay stageCompleteOverlay = new StageCompleteOverlay();
        // массив игроков
        Player[] players = new Player[MaxActivePlayers];
        // максимальное количество игроков
        const int MaxActivePlayers = 2;

        // игровое время (счётчик кадров)
        int gameTime = 0;

        #endregion


        #region Constructor

        /// <summary>
        /// Конструктор
        /// </summary>
        public BattleGround(
            IGameApplication gameApplication,
            ILogger logger,
            ISoundEngine soundEngine,
            IDeviceContext deviceContext,
            IControllerHub controllerHub,
            IGameGraphics graphics,
            GameContent content,
            int numPlayers)
        {
            this.deviceContext = deviceContext;
            this.gameApplication = gameApplication;
            this.controllerHub = controllerHub;
            this.logger = logger;
            this.content = content;
            this.soundEngine = soundEngine;
            Config = content.GameConfig.Clone();// content.GetGameConfig();
            constructionHelper = new ConstructionHelper(content, logger);
            initialNumPlayers = Math.Min(MaxActivePlayers, numPlayers);
            this.graphics = graphics;

            gamePanelFont = graphics.CreateFont(content.GetFont(content.CommonConfig.DefaultFontSize));
            pointsTextFont = graphics.CreateFont(content.GetFont(9f));

            powerUpHandlers = typeof(IPowerUpHandler)
                .GetAllAssignableFroms()
                .Select(s => (IPowerUpHandler)Activator.CreateInstance(s, this))
                .ToList();

            InitFieldBounds();

            gameOverOverlay = new GameOverOverlay(content, graphics, FieldBounds);
            gamePauseOverlay = new GamePauseOverlay(graphics, content, FieldBounds, soundEngine);
        }

        #endregion


        #region methods

        /// <summary>
        /// Начать игру
        /// </summary>
        public void Start()
        {
            stageState = StageStateEnum.Play;
            stageStartSnd?.ResetPosition();
            stageStartSnd = soundEngine.PlayMusic("level_start", true);
        }

        /// <summary>
        /// Получить игрока по указанному юниту
        /// </summary>
        /// <param name="unit"></param>
        /// <returns></returns>
        public Player GetPlayerByUnit(BattleUnit unit)
        {
            if (unit == null)
                return null;

            return players?.FirstOrDefault(x => x != null && x.Unit == unit);
        }

        /// <summary>
        /// Добавить жизни указанному игроку
        /// </summary>
        /// <param name="player">Игрок, которму добавляются жизник</param>
        /// <param name="count">Количество жизней добавляемых жизней</param>
        /// <param name="playSound">Воспроизвести звук</param>
        public void AddPlayerLifeUp(Player player, int count, bool playSound)
        {
            if (player == null)
                return;

            player.Lifes += count;
            if (playSound)
                soundEngine.PlaySound("extra_life");
        }

        /// <summary>
        /// Понизить уровень прокачки юнита у игрока
        /// </summary>
        /// <param name="unit"></param>
        public void DowngradePlayerUnit(BattleUnit unit)
        {
            if (unit.UpgradeLevel <= 0)
                return;

            UpgradePlayerUnit(unit, -1);
        }

        /// <summary>
        /// Изменить уровень прокачки юнита
        /// </summary>
        /// <param name="unit">Юнит</param>
        /// <param name="levelAdd">Значение для повышения или понижения текущего уровня прокачки</param>
        private void UpgrageUnitLevel(BattleUnit unit, int levelAdd)
        {
            if (levelAdd == 0)
                return;

            if (levelAdd > 0)
            {
                if (Config.UnitMaxUpgradeLevel == 0)
                    return;

                if (Config.UnitMaxUpgradeLevel > 0 && unit.UpgradeLevel >= Config.UnitMaxUpgradeLevel)
                    return;
            }

            GameObjectType targetType = unit.IsUser ? GameObjectType.Player : GameObjectType.Enemy;
            var allUnits = content.GameObjects.GetAll(p => p != null && p.Type.HasFlag(targetType));
            GameFieldObject unitUpgadePreset = null;

            if (levelAdd > 0)
            {
                int nextUpgradeLevel = Math.Min(Config.UnitMaxUpgradeLevel, unit.UpgradeLevel + levelAdd);

                unitUpgadePreset = allUnits.FirstOrDefault(p => p.UpgradeLevel == nextUpgradeLevel);
                if (unitUpgadePreset == null)
                    unitUpgadePreset = allUnits
                        .Where(p => p.UpgradeLevel > unit.UpgradeLevel && p.UpgradeLevel <= nextUpgradeLevel)
                        .OrderByDescending(p => p.UpgradeLevel)
                        .FirstOrDefault();
            }
            else
            {
                var defaultUpgradePreset = allUnits.OrderBy(p => p.UpgradeLevel).FirstOrDefault();

                if (defaultUpgradePreset != null && unit.UpgradeLevel <= defaultUpgradePreset.UpgradeLevel)
                    return;

                int nextUpgradeLevel = Math.Max(0, unit.UpgradeLevel + levelAdd);
                if (Config.UnitMaxUpgradeLevel > 0)
                    nextUpgradeLevel = Math.Min(nextUpgradeLevel, Config.UnitMaxUpgradeLevel);

                unitUpgadePreset = allUnits.FirstOrDefault(p => p.UpgradeLevel == nextUpgradeLevel);
                if (unitUpgadePreset == null)
                    unitUpgadePreset = allUnits.FirstOrDefault(p => p.UpgradeLevel < unit.UpgradeLevel);
            }

            // если не удалось найти прокачку или новая прокачка того же уровня, то ничего не делаем
            if (unitUpgadePreset == null || unitUpgadePreset.UpgradeLevel == unit.UpgradeLevel)
                return;

            // сохраняем текущие положение, направление движения и цвет
            var x = unit.X;
            var y = unit.Y;
            var subx = unit.SubPixelX;
            var suby = unit.SubPixelY;
            var direction = unit.Direction;
            var moveInertion = unit.MoveInertion;
            var forceMove = unit.ForceMoveInertion;
            var type = unit.Type;
            var hexColor = unit.HexColor;
            var health = unit.Health;

            // задаем апгрейд
            unit.CopyFrom(unitUpgadePreset);

            // восстанавливаем возможность ходить по воде
            if (type.HasFlag(GameObjectType.Ship) && !unit.Type.HasFlag(GameObjectType.Ship))
                unit.Type |= GameObjectType.Ship;

            // восстанавливаем положение, направление движения и цвет
            unit.Direction = direction;
            unit.X = x;
            unit.Y = y;
            unit.SubPixelX = subx;
            unit.SubPixelY = suby;
            unit.HexColor = hexColor;
            unit.ForceMoveInertion = forceMove;
            unit.MoveInertion = moveInertion;
            if (targetType == GameObjectType.Enemy)
            {
                unit.Health = health;
            }
        }

        /// <summary>
        /// Прокачать юнит игрока на заданное количество единиц прокачки
        /// </summary>
        /// <param name="unit"></param>
        /// <param name="levelAdd">Значение прокачки</param>
        public void UpgradePlayerUnit(BattleUnit unit, int levelAdd = 1)
        {
            if (levelAdd == 0)
                return;
            var player = GetPlayerByUnit(unit);

            if (player == null)
                return;

            UpgrageUnitLevel(unit, levelAdd);
            unit.Name = player.PlayerName;
            player.UpgradeLevel = unit.UpgradeLevel;
        }

        /// <summary>
        /// Прокачать вражеский юнит на заданное количество единиц прокачки
        /// </summary>
        /// <param name="unit"></param>
        /// <param name="levelAdd">Значение прокачки</param>
        public void UpgradeEnemyUnit(BattleUnit unit, int levelAdd = 1)
        {
            if (levelAdd == 0)
                return;

            UpgrageUnitLevel(unit, levelAdd);
        }

        /// <summary>
        /// Прокачать здоровье вражеского юнита (станет бронированным)
        /// </summary>
        /// <param name="unit"></param>
        public void UpgradeEnemyUnitHealth(EnemyUnit unit)
        {
            // при получении усиленной брони отнимаем бонус
            if (unit.ExtraBonus > 0)
                unit.ExtraBonus = 0;

            // если предусмотрено использовать отличительные цвета
            // то юнит получает максимальный уровень здоровья
            if (unit.FlashHexColors != null && unit.FlashHexColors.Length > 0)
                unit.Health = unit.FlashHexColors.Length;
        }

        /// <summary>
        /// Сделать бонусным юнитом
        /// </summary>
        /// <param name="unit"></param>
        public void UpgradeEnemyToBonusUnit(EnemyUnit unit)
        {
            if (Config.MaxExtraBonusPerUnit <= 0)
                return;

            unit.ExtraBonus = Math.Min(unit.ExtraBonus + 1, Config.MaxExtraBonusPerUnit);
        }

        /// <summary>
        /// Добавить возможность двигаться по воде
        /// </summary>
        /// <param name="unit"></param>
        public void AddShipToUnit(BattleUnit unit)
        {
            if (unit.Type.HasFlag(GameObjectType.Ship))
                return;

            unit.Type |= GameObjectType.Ship;
            CreateShip(unit);
        }

        private void CreateShip(BattleUnit unit)
        {
            var shipAnimation = new AnimationObject()
            {
                AutoRepeat = true,
                AttachedObject = unit,
            };
            shipAnimation.CopyFrom(content.GameObjects.GetByName("SHIP_SHAPE"));
            shipAnimation.HexColor = unit.HexColor;

            animations.Add(shipAnimation);
            gameObjects.Add(shipAnimation);
        }

        /// <summary>
        /// Снять возможность ходить по воде
        /// </summary>
        /// <param name="unit"></param>
        public void RemoveShipFromUnit(BattleUnit unit)
        {
            if (!unit.Type.HasFlag(GameObjectType.Ship))
                return;
            unit.Type ^= GameObjectType.Ship;

            var shipAnimation = animations.FirstOrDefault(p => p.AttachedObject == unit && p.Type.HasFlag(GameObjectType.Ship));
            if (shipAnimation != null)
            {
                animations.Remove(shipAnimation);
                gameObjects.Remove(shipAnimation);
            }
        }

        /// <summary>
        /// Обнулить защиту вокруг стратегических объектов
        /// </summary>
        public void ResetTowerDefense()
        {
            brickDefenseObjList?.Clear();
            ironDefenseObjList?.Clear();
            tempTowerDefenseFrameNumber = 0;
            defenseState = 0;
        }

        /// <summary>
        /// Удалить блоки вокруг стратегических объектов
        /// </summary>
        public void RemoveBlocksAroundTowers()
        {
            constructionHelper.RemoveBlocksAroundTowers(gameObjects);
        }

        /// <summary>
        /// Удалить бонус с поля
        /// </summary>
        private void RemovePowerUpFromField()
        {
            var tmpPowerup = gameObjects.FirstOrDefault(p => p.Type.HasFlag(GameObjectType.PowerUp));
            if (tmpPowerup != null && tmpPowerup is AnimationObject powerUpToRemove)
            {
                gameObjects.Remove(powerUpToRemove);
                animations.Remove(powerUpToRemove);
            }
        }

        /// <summary>
        /// Добавить бонус на поле
        /// </summary>
        private void AddPowerUpObject()
        {
            if (Config.MaxActivePowerUpsOnField == 0)
                return;
            int maxCount = Config.MaxActivePowerUpsOnField;// * Math.Max(1, NumActivePlayers);
            if (Config.MaxActivePowerUpsOnField > 0
                && gameObjects.Count(p => p.Type.HasFlag(GameObjectType.PowerUp)) >= maxCount)
            {
                RemovePowerUpFromField();
            }

            var powerups = content.GameObjects.GetAll(p => p != null && p.Type.HasFlag(GameObjectType.PowerUp)).ToList();
            if (powerups.Count == 0)
                return;

            int powerupIndex = 0;

            if (powerups.Count > 1)
            {
                powerupIndex = Config.Random.Next(powerups.Count);
                //powerupIndex = powerups.FindIndex(p => p.Name == "FREEZE_ENEMY_POWERUP");
            }

            var preset = powerups[powerupIndex];

            if (!CreateRandomObjectPosition(preset.Width, preset.Height, out int x, out int y))
                return;

            var animationObject = new AnimationObject();
            animationObject.CopyFrom(preset);

            if (Config.PowerUpLifetimeDuration > 0)
                animationObject.Duration = Config.PowerUpLifetimeDuration;
            else
                animationObject.Duration = int.MaxValue;

            animationObject.X = x;
            animationObject.Y = y;

            animations.Add(animationObject);
            gameObjects.Add(animationObject);

            soundEngine.PlaySound(animationObject.AppearSndId);
        }

        /// <summary>
        /// Создать случайную позицию объекта
        /// </summary>
        /// <param name="width">Ширина создаваемого объекта</param>
        /// <param name="height">Высота создаваемого объекта</param>
        /// <param name="x">Выходная X-координата объекта</param>
        /// <param name="y">Выходная Y-координата объекта</param>
        /// <returns></returns>
        private bool CreateRandomObjectPosition(int width, int height, out int x, out int y)
        {
            x = 0;
            y = 0;

            var excludeBoundsList = gameObjects
                .Where(p => p.IsVisible && p.Type.HasFlag(GameObjectType.Tower))
                .Select(p => p.GetAABB(Config.SubPixelSize))
                .ToList();

            excludeBoundsList.AddRange(
                respawnPoints.Select(p => p.GetAABB(Config.SubPixelSize))
            );

            // количество итераций для подбора возможной позиции объекта
            int retries = 100;

            while (retries-- > 0)
            {
                x = Config.Random.Next(Config.FieldWidth - width + 1);
                y = Config.Random.Next(Config.FieldHeight - height + 1);

                var aabb = AABB.Create(x, y, width, height, Config.SubPixelSize);

                // исключаем наложения
                if (excludeBoundsList.Any(p => p.OverlapsOrContains(aabb)))
                    continue;

                return true;
            }

            return false;
        }

        /// <summary>
        /// Проверить и дополнить очередь вражеских юнитов
        /// </summary>
        private void EnsureEnemyQueue()
        {
            // получаем все возможные типы вражеских юнитов
            var allEnemyPresets = content.GameObjects
                            .GetAll(p => p.Type.HasFlag(GameObjectType.Enemy) && p.UpgradeLevel == 0)
                            .ToList();

            // дополняем очередь вражеских юнитов, которые будут появляться на поле
            while (enemyQueue.Count < Config.MaxEnemy)
            {
                int enemyNameIndex = Config.Random.Next(0, allEnemyPresets.Count);
                var unit = allEnemyPresets[enemyNameIndex];

                var enemy = new SpawnQueueBattleUnit()
                {
                    Name = allEnemyPresets[enemyNameIndex].Name,
                    Health = Math.Max(1, unit.Health),
                    ExtraBonus = 0
                };

                // определяем носителей бонусов
                if (Config.MaxBonusedUnitsOnField == -1 || Config.MaxBonusedUnitsOnField > gameObjects.Count(p => p is EnemyUnit bn && bn.ExtraBonus > 0))
                {
                    // определяем шанс появления бонусного юнита
                    var chanceBonusPowerUp = 100 - Math.Min(100, Math.Max(0, Config.BonusedEnemySpawnChance));
                    if (Config.Random.Next(0, 101) >= chanceBonusPowerUp)
                    {
                        enemy.ExtraBonus = Config.Random.Next(1, Config.MaxExtraBonusPerUnit + 1);
                    }
                }

                // определяем количество жизней
                const int addDifficultFromStageNumber = 2;
                if (stageNumber > addDifficultFromStageNumber && unit.FlashHexColors != null && unit.FlashHexColors.Length > 1)
                {
                    int d = (stageNumber + 1) - addDifficultFromStageNumber;
                    if (Config.Random.Next(0, 101) < d * 10)
                        enemy.Health = Config.Random.Next(enemy.ExtraBonus > 0 ? 0 : 1, unit.FlashHexColors.Length + 1);
                }

                enemyQueue.Enqueue(enemy);
            }
        }

        /// <summary>
        /// Создать защитные блоки вокруг базы
        /// </summary>
        public void CreateTowerDefense()
        {
            // если продолжительность нулевая, то ничего не делаем
            if (Config.TowerTempDefenseDuration == 0)
                return;

            // устанавливаем продолжительность временной защиты
            tempTowerDefenseFrameNumber = Config.TowerTempDefenseDuration * 60;

            // обнуляем текущие защитные блоки
            brickDefenseObjList?.Clear();
            ironDefenseObjList?.Clear();

            constructionHelper.RemoveBlocksAroundTowers(gameObjects);

            // добавляем iron блоки вокруг базы
            ironDefenseObjList = constructionHelper.CreateIronBlocksAroundTowers(gameObjects);
            constructionHelper.AddObjects(gameObjects, ironDefenseObjList);
            defenseState = 1;

            if (Config.TowerTempDefenseDuration == -1)
            {
                ironDefenseObjList.Clear();
            }
            else
            {
                brickDefenseObjList = constructionHelper.CreateBricksAroundTowers(gameObjects);
            }

            if (ironDefenseObjList.Count == 0 || brickDefenseObjList.Count == 0)
                tempTowerDefenseFrameNumber = 0;
        }

        /// <summary>
        /// Обновить состояние защиты стратегических объектов
        /// </summary>
        private void UpdateTowerDefenseState()
        {
            if (Config.TowerTempDefenseDuration <= 0 || tempTowerDefenseFrameNumber <= 0)
            {
                return;
            }

            var time = Math.Min(4 * 60, (Config.TowerTempDefenseDuration * 60) / 2);
            if (tempTowerDefenseFrameNumber <= time)
            {
                bool state = tempTowerDefenseFrameNumber % 20 == 0;

                if (state)
                {
                    if (defenseState == 1)
                    {
                        gameObjects.RemoveAll(p => ironDefenseObjList.Contains(p));
                        brickDefenseObjList = constructionHelper.CreateBricksAroundTowers(gameObjects);
                        constructionHelper.AddObjects(gameObjects, brickDefenseObjList);
                        defenseState = 0;
                    }
                    else
                    {
                        gameObjects.RemoveAll(p => brickDefenseObjList.Contains(p));
                        ironDefenseObjList = constructionHelper.CreateIronBlocksAroundTowers(gameObjects);
                        constructionHelper.AddObjects(gameObjects, ironDefenseObjList);
                        defenseState = 1;
                    }
                }
            }

            tempTowerDefenseFrameNumber--;

            if (tempTowerDefenseFrameNumber == 0)
            {
                if (defenseState == 1)
                {
                    gameObjects.RemoveAll(p => ironDefenseObjList.Contains(p));
                    constructionHelper.AddObjects(gameObjects, brickDefenseObjList);
                    defenseState = 0;
                }
            }
        }

        /// <summary>
        /// Подготовить позиции появления юнитов
        /// </summary>
        private void InitSpawnPositions()
        {
            var spawnPoint = content.GameObjects.GetAll(p => p.Type.HasFlag(GameObjectType.SpawPosition)).First();

            enemySpawnPositionIndex = MaxActivePlayers;
            int spawnObjWidth = spawnPoint.Width;
            int spawnObjHeight = spawnPoint.Height;
            int i;

            if (Config.PlayerSpawnLocations == null || Config.PlayerSpawnLocations.Length == 0)
            {
                logger?.WriteLine($"{nameof(InitSpawnPositions)}: {nameof(Config.PlayerSpawnLocations)} not present", LogLevel.Warning);
                return;
            }

            if (Config.EnemySpawnLocations == null || Config.EnemySpawnLocations.Length == 0)
            {
                logger?.WriteLine($"{nameof(InitSpawnPositions)}: {nameof(Config.EnemySpawnLocations)} not present", LogLevel.Warning);
                return;
            }

            if (stageMaxActiveEnemies == 0)
            {
                logger?.WriteLine($"{nameof(InitSpawnPositions)}: {nameof(stageMaxActiveEnemies)} is zero", LogLevel.Warning);
                return;
            }

            for (i = 0; i < MaxActivePlayers; i++)
            {
                var location = Config.PlayerSpawnLocations[i];
                int x = Math.Max(0, Math.Min(Config.FieldWidth - spawnObjWidth, location.X));
                int y = Math.Max(0, Math.Min(Config.FieldHeight - spawnObjHeight, location.Y));

                // удаляем объекты с поля, которые пересекаются с позицией появления
                constructionHelper.RemoveOverlapsOrContainsObjects(gameObjects, x, y, spawnObjWidth, spawnObjHeight);

                // создаём позицию появления
                if (!respawnPoints.Any(p => p.X == x && p.Y == y))
                    AddSpawnPosition(x, y);

                var player = players[i];
                if (player != null && player.IsAlive)
                {
                    SpawnPlayer(player);
                }
            }

            enemySpawnPositionIndex += Config.EnemyFirstSpawnPositionIndex;

            for (i = 0; i < Config.EnemySpawnLocations.Length; i++)
            {
                int index = i % stageMaxActiveEnemies;
                var location = Config.EnemySpawnLocations[index];
                int x = Math.Max(0, Math.Min(Config.FieldWidth - spawnObjWidth, location.X));
                int y = Math.Max(0, Math.Min(Config.FieldHeight - spawnObjHeight, location.Y));

                // удаляем объекты с поля, которые пересекаются с позицией появления
                constructionHelper.RemoveOverlapsOrContainsObjects(gameObjects, x, y, spawnObjWidth, spawnObjHeight);

                // создаём позицию появления
                if (!respawnPoints.Any(p => p.X == x && p.Y == y))
                    AddSpawnPosition(x, y);
            }
        }

        /// <summary>
        /// Проверяем оставшееся количество врагов
        /// </summary>
        private void CheckRemainingEnemies()
        {
            if (enemiesCount == 0)
            {
                SetAllEnemiesDestroyed();
            }
        }

        /// <summary>
        /// Получить следующий вражеский юнит из очереди
        /// </summary>
        /// <returns></returns>
        private BattleUnit GetNextEnemy()
        {
            if (enemyQueue.Count == 0)
                return null;

            var queuedEnemy = enemyQueue.Dequeue();
            var preset = content.GameObjects.GetByName(queuedEnemy.Name);

            if (preset == null)
            {
                enemiesCount--;
                CheckRemainingEnemies();
                return null;
            }

            EnemyUnit enemy = new EnemyUnit(Config, enemyIndexGenerator.Next());
            enemy.CopyFrom(preset);

            if (!enemy.Type.HasFlag(GameObjectType.Enemy))
                enemy.Type |= GameObjectType.Enemy;

            enemy.Health = queuedEnemy.Health;
            enemy.ExtraBonus = queuedEnemy.ExtraBonus;
            enemy.Direction = MoveDirection.Down;

            return enemy;
        }

        /// <summary>
        /// Установить состояние активности вражеских юнитов
        /// </summary>
        /// <param name="enemyIsActive"></param>
        public void SetEnemyActiveState(bool enemyIsActive)
        {
            this.enemyIsActive = enemyIsActive;
        }

        /// <summary>
        /// Добавить позицию появления по указанным условным координатам
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        private void AddSpawnPosition(int x, int y)
        {
            var sp = new RespawnPoint();
            var spawnPreset = content.GameObjects.GetAll(p => p.Type.HasFlag(GameObjectType.SpawPosition)).First();

            if (spawnPreset == null)
            {
                logger?.WriteLine($"Object {nameof(GameObjectType.SpawPosition)} not found", LogLevel.Error);
                return;
            }

            sp.CopyFrom(spawnPreset);
            sp.Reset(Config.SpawnAnimationDuration);
            sp.SetPosition(x, y);
            respawnPoints.Add(sp);
        }

        /// <summary>
        /// Создать игроков
        /// </summary>
        private void CreatePlayers()
        {
            for (int i = 0; i < initialNumPlayers; i++)
            {
                players[i] = Player.Create(Config, i + 1, Config.StartLifes);
                players[i].UpgradeLevel = Config.PlayerDefaultUpgradeLevel;
                InitPlayerUnit(players[i], GetPlayerColor(i));
            }
        }

        /// <summary>
        /// Получить цвет игрока по индексу
        /// </summary>
        /// <param name="playerIndex">Индекс игрока</param>
        /// <returns></returns>
        private string GetPlayerColor(int playerIndex)
        {
            const string defaultColor = "#FFFFFFFF";
            if (Config.PlayerColors == null || Config.PlayerColors.Length == 0)
                return defaultColor;

            int colorIndex = playerIndex % Config.PlayerColors.Length;
            return Config.PlayerColors[colorIndex];
        }

        /// <summary>
        /// Загрузить и подготовить уровень (stage)
        /// </summary>
        /// <param name="stageNumber">Номер уровня, начиная с 1</param>
        public bool InitStage(int stageNumber)
        {
            if (stageState != StageStateEnum.None)
            {
                logger?.WriteLine($"{nameof(InitStage)} failed: state stage: {stageState}", LogLevel.Error);
                return false;
            }
            if (stageNumber <= 0)
            {
                logger?.WriteLine($"{nameof(InitStage)} failed: wrong stage number {stageNumber}", LogLevel.Error);
                return false;
            }

            if (stageStartSnd != null)
            {
                soundEngine.Stop(stageStartSnd);
                stageStartSnd.Reset();
            }

            BattleStage stage = null;

            if (content.Stages.Capacity > 0)
            {
                // определяем фактическое количество уровней
                int factStagesCount = content.GetMaxStageNumber();
                if (factStagesCount > 0)
                {
                    stage = content.Stages[(stageNumber - 1) % factStagesCount];
                }
            }

            // определяем границы поля битвы
            InitFieldBounds();

            this.stageNumber = stageNumber;
            stageMaxActiveEnemies = Config.MaxActiveEnemy;

            // задаем количество вражеских юнитов по умолчанию
            enemiesCount = Config.MaxEnemy;

            if (stage != null && stage.Enemies != null)
            {
                // задаем количество вражеских юнитов, определенных в уровне (stage)
                enemiesCount = stage.Enemies.Count;
            }

            if (stage != null && stage.Enemies != null && stage.Enemies.Count > 0 && !Config.ForceRandomEnemies)
            {
                // подготавливаем очередь вражеских юнитов,
                // определенных в уровне (stage)
                for (int i = 0; i < Math.Min(enemiesCount, stage.Enemies.Count); i++)
                {
                    var queuedEnemy = stage.Enemies[i];
                    var enemy = content.GameObjects.GetByName(queuedEnemy.Name);

                    if (enemy == null)
                    {
                        logger?.WriteLine($"{nameof(InitStage)}: enemy id:{stage.Enemies[i]} not exists", LogLevel.Warning);
                    }
                    else if (!enemy.Type.HasFlag(GameObjectType.Enemy))
                    {
                        logger?.WriteLine($"{nameof(InitStage)}: enemy id:{stage.Enemies[i]} is not enemy", LogLevel.Warning);
                    }
                    else
                    {
                        enemyQueue.Enqueue(queuedEnemy);
                    }
                }
            }

            // подготавливаем карту
            if (stage == null || stage.FieldObjects == null || stage.FieldObjects.Count == 0)
            {
                // объектов на карте нет, создами по умолчанию

                // добавляем стратегические объекты
                constructionHelper.AddTowerObjects(gameObjects);

                // добавляем кирпичные блоки вокруг стратегических объектов
                constructionHelper.AddBricksAroundTowers(gameObjects);

                //// добавляем тестовые блоки
                //constructionHelper.CreateTestBlocks();
            }
            else
            {
                // добавим объекты уровня
                foreach (var fieldObj in stage.FieldObjects)
                {
                    var id = fieldObj.Id;
                    GameFieldObject obj = new GameFieldObject();
                    obj.CopyFrom(content.GameObjects.GetById(id));
                    obj.X = fieldObj.X;
                    obj.Y = fieldObj.Y;
                    obj.SubPixelX = fieldObj.SubPixelX;
                    obj.SubPixelY = fieldObj.SubPixelY;
                    obj.Direction = fieldObj.Direction;
                    gameObjects.Add(obj);
                }
            }

            // Проверяем наличие достаточного количества вражеских юнитов
            if (stage == null || stage.Enemies == null || stage.Enemies.Count == 0 || enemyQueue.Count == 0)
            {
                EnsureEnemyQueue();
            }

            // подготовим позиции появления юнитов
            InitSpawnPositions();

            foreach (var player in players)
            {
                if (player != null && player.IsAlive && player.Unit != null &&
                    player.Unit.Type.HasFlag(GameObjectType.Ship))
                {
                    CreateShip(player.Unit);
                }
            }

            return true;
        }

        /// <summary>
        /// Настройка области поля битвы
        /// </summary>
        private void InitFieldBounds()
        {
            var rightOffset = Config.SubPixelSize * 5;
            Width = Config.SubPixelSize * Config.FieldWidth;
            Height = Config.SubPixelSize * Config.FieldHeight;
            int fontSize = Convert.ToInt32(gamePanelFont.MeasureString("W").Height * 1.2f);
            Left = Math.Max(0, (deviceContext.DeviceWidth - Width - rightOffset) / 2);
            Top = Math.Max(0, (deviceContext.DeviceHeight - (Height + fontSize)) / 2);
        }

        /// <summary>
        /// Сбросить все состояния
        /// </summary>
        /// <param name="hardReset">Полный сброс</param>
        public void Reset(bool hardReset)
        {
            if (hardReset)
            {
                CreatePlayers();
            }
            else
            {
                for (int i = 0; i < players.Length; i++)
                {
                    if (players[i] != null)
                    {
                        players[i].Unit.Freeze = 0;
                        players[i].DestroyedEnemies.Clear();
                        players[i].Unit.ResetShield();
                        if (players[i].Lifes > 0)
                        {
                            players[i].Unit.IsAlive = true;
                            players[i].Unit.Gun?.ReloadGun(true);
                            if (Config.ResetUnitUpgradesOnStageStart)
                                players[i].Unit.UpgradeLevel = Config.PlayerDefaultUpgradeLevel;

                            if (players[i].Unit.Type.HasFlag(GameObjectType.Ship))
                                CreateShip(players[i].Unit);
                        }
                    }
                }
            }

            enemyIndexGenerator.Reset(1);

            stageCompleteOverlay.Hide();
            gameOverOverlay.Hide();
            gamePauseOverlay.Hide();

            tempTowerDefenseFrameNumber = 0;
            brickDefenseObjList?.Clear();
            ironDefenseObjList?.Clear();

            spawnEnemyDelayFrames = 0;
            stageMaxActiveEnemies = 0;
            enemiesCount = 0;
            activeEnemiesCount = 0;
            spawnedEnemyCount = 0;
            gameTime = 0;
            freezeEnemyTime = 0;
            stageState = StageStateEnum.None;

            battleUnits.Clear();
            bullets.Clear();
            gameObjects.Clear();
            enemyQueue.Clear();
            respawnPoints.Clear();
            animations.Clear();
            textBlocks.Clear();
        }

        /// <summary>
        /// Проверить выхода снаряда за границы
        /// </summary>
        /// <returns></returns>
        private bool IsOutOfField(Bullet bullet)
        {
            return
                bullet.X < 0 ||
                bullet.Y < 0 ||
                bullet.Y + bullet.Height > Config.FieldHeight ||
                bullet.X + bullet.Width > Config.FieldWidth;
        }

        /// <summary>
        /// Выполнить проверку коллизий (столкновение снарядов)
        /// </summary>
        private void CheckBulletsCollision()
        {
            for (int i = 0; i < bullets.Count; i++)
            {
                var bullet = bullets[i];
                if (bullet.Power <= 0)
                    continue;

                // Проверяем выход за границы
                bool isOutOfField = IsOutOfField(bullet);
                if (!isOutOfField)
                {
                    var currentBulletPower = bullet.Power;
                    bool muteDestroyBrickSnd = false;

                    // получаем объекты пересечения с данным снарядом
                    if (CollideBullet(bullet, out List<GameFieldObject> objList))
                    {
                        bool isBulletAlive = true;
                        int bulletPower = bullet.Power;

                        foreach (var fieldObject in objList)
                        {
                            bullet.Power = bulletPower;

                            // проверяем условие попадения в самого себя
                            if (fieldObject == bullet.Owner)
                            {
                                if (fieldObject.GetAABB(Config.SubPixelSize).Overlaps(bullet.GetAABB()))
                                {
                                    isBulletAlive = false;
                                    bullet.Power = 0;
                                    break;
                                }
                                continue;
                            }

                            // если столкновение снаряда со снарядом (взаимоуничтожение)
                            if (fieldObject.Type.HasFlag(GameObjectType.Projectile))
                            {
                                isBulletAlive = false;
                                // уничтожаем снаряды
                                DestroyBullet(fieldObject);
                                DestroyBullet(bullet.BulletObject);
                                bullet.Owner.Gun?.ReloadGun(false);
                                bullet.BulletObject.Gun?.ReloadGun(false);
                                break;
                            }

                            AttackGameObject(bullet, fieldObject);

                            if (bullet.Power <= 0)
                                isBulletAlive = false;

                            if (fieldObject.Type.HasFlag(GameObjectType.Destroyable))
                            {
                                bool hasExtraBonus = fieldObject is EnemyUnit enemyUnit && enemyUnit.ExtraBonus > 0;
                                if (!hasExtraBonus && fieldObject.Armor <= 0 && fieldObject.Health <= 0)
                                {
                                    bool playSound = true;

                                    if (fieldObject.Name?.ToUpper() == "BRICK")
                                    {
                                        playSound = !muteDestroyBrickSnd;
                                        muteDestroyBrickSnd = true;
                                    }

                                    DestroyGameObject(bullet, fieldObject, playSound);

                                    if (stageState == StageStateEnum.GameOver)
                                    {
                                        bullet.Power = 0;
                                        break;
                                    }

                                    // исключаем возможность double kill (одни выстрелом двух сразу)
                                    if (fieldObject.Type.HasFlag(GameObjectType.Enemy) || fieldObject.Type.HasFlag(GameObjectType.Player))
                                    {
                                        bullet.Power = 0;
                                        break;
                                    }
                                }
                            }
                        }

                        if (!isBulletAlive)
                            bullet.Power = 0;
                    }

                    // Проверяем огневую мощь снаряда,
                    // если значение > 0, то снаряд двигается дальше
                    if (bullet.Power > 0)
                    {
                        bullet.Move();
                    }
                }

                // проверяем признак выхода за границы поля и оставшуюся мощь снаряда
                if (isOutOfField || bullet.Power <= 0)
                {
                    if (isOutOfField && bullet.Owner.IsUser)
                        soundEngine.PlaySound("hit_wall");
                    CreateBulletExplosion(bullet.BulletObject);
                    bullet.Destroy();
                    bullets.RemoveAt(i);
                    gameObjects.Remove(bullet.BulletObject);
                    i--;
                }
            }

            foreach (var bullet in bullets)
            {
                if (bullet.Power <= 0)
                    gameObjects.Remove(bullet.BulletObject);
            }
            bullets.RemoveAll(p => p.Power <= 0);
        }

        /// <summary>
        /// Атаковать указанный объект
        /// </summary>
        /// <param name="bullet">Атакующий снаряд</param>
        /// <param name="gameObject">Объект, подвергающийся атаке</param>
        private void AttackGameObject(Bullet bullet, GameFieldObject gameObject)
        {
            if (gameObject.Type == GameObjectType.None ||
                gameObject.Type == GameObjectType.Animation ||
                gameObject.Type == GameObjectType.DestoyedTower)
                return;

            if (!gameObject.Type.HasFlag(GameObjectType.Destroyable))
            {
                if (gameObject.Type.HasFlag(GameObjectType.Barrier) && !gameObject.Type.HasFlag(GameObjectType.Water))
                {
                    soundEngine.PlaySound("hit_brick");
                    bullet.Power = 0;
                }
                return;
            }

            // если атакуем стратегический объект
            if (gameObject.Type.HasFlag(GameObjectType.Tower))
            {
                // проверяем возможность атаковать игроком стратегический объект
                if (bullet.Owner.IsUser && !Config.PlayerDestroyBaseAllowed)
                {
                    bullet.Power = 0;
                    return;
                }
            }

            // если атакуем боевой юнит
            if (gameObject is BattleUnit unit)
            {
                // проверка на неуязвимость
                if (unit.Invincibility > 0)
                {
                    bullet.Power = 0;
                    return;
                }

                if (unit is UserBattleUnit userUnit)
                {
                    if (bullet.Owner.IsUser)
                    {
                        if (Config.PlayerFriendlyFire)
                        {
                            // Todo: дружественная аттака (игрок аттаковал игрока)
                            FreezePlayer(GetPlayerByUnit(userUnit));
                        }

                        bullet.Move(1);
                        bullet.Power = 0;
                        return;
                    }
                }
                else if (unit is EnemyUnit enemyUnit)
                {
                    if (!bullet.Owner.IsUser)
                    {
                        if (!Config.EnemyFriendlyFire)
                        {
                            // снаряд проходит насквозь
                            return;
                        }
                        // иначе враг аттаковал своего союзника
                    }

                    // если юнит является носителем бонусов
                    if (enemyUnit.ExtraBonus > 0)
                    {
                        bullet.Power = 0;
                        enemyUnit.ExtraBonus--;
                        // создаем на поле PowerUpBonus
                        AddPowerUpObject();
                        return;
                    }
                }

                // если боевой юнит имеет возможность ходить по воде
                if (unit.Type.HasFlag(GameObjectType.Ship))
                {
                    // убираем возможность ходить по воде
                    bullet.Power = 0;
                    RemoveShipFromUnit(unit);
                    return;
                }

                // проверяем уровень прокачки
                if (unit.UpgradeLevel == 0)
                {
                    // прокачки нет, отнимаем здоровье
                    gameObject.Health -= 1;
                    bullet.Power = 0;

                    // воспроизводим звук попадения по броне, если выстрел от игрока
                    if (bullet.Owner.IsUser && gameObject.Health > 0)
                        soundEngine.PlaySound("hit_armor");
                }
                else
                {
                    // понижаем уровень прокачки без уничтожения юнита
                    if (unit.IsUser)
                    {
                        DowngradePlayerUnit(unit);
                    }
                    else
                    {
                        gameObject.Health -= 1;
                    }
                    bullet.Power = 0;
                    soundEngine.PlaySound("hit_armor");
                }

                return;
            }


            // если атакуем лес
            if (gameObject.Type.HasFlag(GameObjectType.Forest))
            {
                // проверяем возможность нанесения урона снарядом
                if (bullet.Power >= gameObject.Armor)
                {
                    gameObject.Health = 0;
                    gameObject.Armor = 0;

                    // воспроизводим звук попадения, если выстрел от игрока
                    if (bullet.Owner.IsUser)
                        soundEngine.PlaySound(gameObject.DestroySndId);
                }

                return;
            }

            if (gameObject.Armor > 0)
            {
                if (bullet.Power >= gameObject.Armor)
                {
                    gameObject.Armor = 0;
                    bullet.Power = 0;
                }
                else
                {
                    bullet.Power = 0;
                    // воспроизводим звук попадения по броне, если выстрел от игрока
                    if (bullet.Owner.IsUser)
                        soundEngine.PlaySound("hit_armor");
                }
            }
            else
            {
                gameObject.Health = 0;
            }
        }

        /// <summary>
        /// Проверить столкновения снаряда с другими объектами
        /// </summary>
        /// <param name="bullet">Снаряд</param>
        /// <param name="result">Список объектов, с которыми пересекается снаряд</param>
        /// <returns><see cref="true"/>, если найдено одно или более пересечений со снарядом</returns>
        bool CollideBullet(Bullet bullet, out List<GameFieldObject> result)
        {
            var bulletRect = new Rectangle(bullet.X, bullet.Y, bullet.Width, bullet.Height);
            var bulletRoundedBounds = AABB.Create(bullet.X, bullet.Y, bullet.Width, bullet.Height, Config.SubPixelSize);

            var bulletBounds = bullet.GetAABB();

            // Список объектов для хранения найденных пересечений
            var contacts = new List<ContactObject>(10);

            var collection = gameObjects.Where(p =>
                p.IsVisible &&
                !p.IsSpawn &&
                p != bullet.BulletObject &&
                (p.Type.HasFlag(GameObjectType.Barrier) || p.Type.HasFlag(GameObjectType.Destroyable))
                );

            foreach (var gameObject in collection)
            {
                var bb2 = gameObject.GetAABB(Config.SubPixelSize);

                double? distance = null;
                if (!bulletBounds.DisjointTo(bb2) && !contacts.Any(p => p.Object == gameObject))
                {
                    distance = AABB.Distance(bulletRoundedBounds, bb2, bullet.Direction);
                    contacts.Add(new ContactObject() { Object = gameObject, Distance = distance.Value });
                }

                // подхватываем боковые объекты размерностью 1x1 (кроме снарядов)
                if (distance.HasValue && gameObject.Width == 1 && gameObject.Height == 1 && !gameObject.Type.HasFlag(GameObjectType.Projectile))
                {
                    if (bullet.Direction == MoveDirection.Up || bullet.Direction == MoveDirection.Down)
                    {
                        var itemLeft = gameObjects.FirstOrDefault(
                            p => p.Width == 1 && p.Height == 1 && p.Y == gameObject.Y &&
                            p.X == bulletRect.Left - 1 &&
                            p.Type == gameObject.Type && p.IsVisible && !p.IsSpawn);
                        if (itemLeft != null && !contacts.Any(p => p.Object == itemLeft))
                        {
                            contacts.Add(new ContactObject() { Object = itemLeft, Distance = distance.Value });
                        }

                        var itemRight = gameObjects.FirstOrDefault(
                            p => p.Width == 1 && p.Height == 1 && p.Y == gameObject.Y &&
                            p.X == bulletRect.Right &&
                            p.Type == gameObject.Type && p.IsVisible && !p.IsSpawn);
                        if (itemRight != null && !contacts.Any(p => p.Object == itemRight))
                        {
                            contacts.Add(new ContactObject() { Object = itemRight, Distance = distance.Value });
                        }
                    }
                    else
                    {
                        var itemTop = gameObjects.FirstOrDefault(
                            p => p.Width == 1 && p.Height == 1 && p.X == gameObject.X &&
                            p.Y == bulletRect.Top - 1 &&
                            p.Type == gameObject.Type && p.IsVisible && !p.IsSpawn);
                        if (itemTop != null && !contacts.Any(p => p.Object == itemTop))
                        {
                            contacts.Add(new ContactObject() { Object = itemTop, Distance = distance.Value });
                        }

                        var itemBottom = gameObjects.FirstOrDefault(
                            p => p.Width == 1 && p.Height == 1 && p.X == gameObject.X &&
                            p.Y == bulletRect.Bottom &&
                            p.Type == gameObject.Type && p.IsVisible && !p.IsSpawn);
                        if (itemBottom != null && !contacts.Any(p => p.Object == itemBottom))
                        {
                            contacts.Add(new ContactObject() { Object = itemBottom, Distance = distance.Value });
                        }
                    }
                }
            }

            #region доп проверка для объектов размерностью 1x1

            if (contacts.Count == 0)
            {
                collection = gameObjects.Where(p =>
                    p != bullet.BulletObject &&
                    p.Type != GameObjectType.None &&
                    p.Type != GameObjectType.DestoyedTower &&
                    p.Type != GameObjectType.Animation &&
                    !p.Type.HasFlag(GameObjectType.Projectile) &&
                    p.Width == 1 && p.Height == 1 &&
                    p.IsVisible);

                if (bullet.Direction == MoveDirection.Left || bullet.Direction == MoveDirection.Right)
                {
                    if (bulletRect.Y > 0)
                    {
                        bulletRect.Y--;
                        bulletRect.Height += constructionHelper.DefaultBlockSize;
                    }
                }
                else
                {
                    if (bulletRect.X > 0)
                    {
                        bulletRect.X--;
                        bulletRect.Width += constructionHelper.DefaultBlockSize;
                    }
                }

                foreach (var gameObject in collection)
                {
                    var bb2 = gameObject.GetAABB(Config.SubPixelSize);

                    double? distance = null;
                    if (!bulletRoundedBounds.DisjointTo(bb2) && !contacts.Any(p => p.Object == gameObject))
                    {
                        distance = AABB.Distance(bulletRoundedBounds, bb2, bullet.Direction);
                        contacts.Add(new ContactObject() { Object = gameObject, Distance = distance.Value });
                    }
                }
            }

            #endregion

            if (contacts.Count == 0)
            {
                // нет пересечений, возвращаем пустой список
                result = new List<GameFieldObject>(0);
            }
            else
            {
                // определяем список объектов по минимальной дистанции перечечения
                var minDist = contacts.Min(p => p.Distance);
                result = contacts.Where(p => p.Distance == minDist).Select(s => s.Object).ToList();
            }

            return result.Count > 0;
        }

        /// <summary>
        /// Обработка события, при котором стратегический объект уничтожен
        /// </summary>
        /// <param name="gameObject"></param>
        private void OnTowerDestroyed(GameFieldObject gameObject)
        {
            // Определяем объект разрушения
            int destroyedBaseId = content.GameObjects
                .GetAll(p => p.Type.HasFlag(GameObjectType.DestoyedTower))
                .Select(p => p.Id)
                .FirstOrDefault();

            if (destroyedBaseId != 0)
            {
                constructionHelper.AddObjectById(gameObjects, destroyedBaseId, gameObject.X, gameObject.Y);
            }

            if (stageState != StageStateEnum.GameOver)
            {
                // игра окончена
                SetGameOver();
            }
        }

        /// <summary>
        /// Игра окончена, вражеские юниты победили
        /// </summary>
        private void SetGameOver()
        {
            stageState = StageStateEnum.GameOver;
            gameOverOverlay.Show(60 * 5);
            // Game over!
        }

        /// <summary>
        /// Уничтожить объект поля
        /// </summary>
        /// <param name="bullet">Снаряд, которым уничтожается объект поля</param>
        /// <param name="gameObject">Объект уничтожения</param>
        /// <param name="playSound">Воспроизвести звук</param>
        private void DestroyGameObject(Bullet bullet, GameFieldObject gameObject, bool playSound = true)
        {
            var index = gameObjects.IndexOf(gameObject);
            if (index == -1) return;

            if (!gameObject.Type.HasFlag(GameObjectType.Destroyable))
            {
                bullet.Power = 0;
                return;
            }

            gameObjects.RemoveAt(index);

            BattleUnit whomDestroy = bullet.Owner;

            if (gameObject.Type.HasFlag(GameObjectType.Tower))
            {
                bullet.Power = 0;
                soundEngine.PlaySound(gameObject.DestroySndId);
                OnTowerDestroyed(gameObject);
            }

            else if (gameObject is BattleUnit unit)
            {
                bullet.Power = 0;
                if (unit.IsUser)
                {
                    var player = GetPlayerByUnit(unit);
                    DestroyPlayer(player);
                }
                else if (unit is EnemyUnit enemyUnit)
                {
                    battleUnits.Remove(enemyUnit);
                    DestroyEnemy(gameObject, true);

                    if (whomDestroy != null && whomDestroy.IsUser)
                    {
                        var player = GetPlayerByUnit(whomDestroy);
                        if (gameObject.BonusPoints != 0)
                        {
                            AddPoints(player, gameObject.BonusPoints);
                            DisplayPointsText(gameObject.BonusPoints.ToString(), gameObject.GetBounds(Config.SubPixelSize));
                        }

                        player?.DestroyedEnemies.Add(new DestroyedEnemyInfo()
                        {
                            Id = gameObject.Id,
                            TextureId = gameObject.TextureIdList.FirstOrDefault(),
                        });
                    }

                }
            }

            else if (gameObject.Health == 0)
            {
                if (playSound)
                    soundEngine.PlaySound(gameObject.DestroySndId);
            }
        }

        /// <summary>
        /// Уничтожить вражеский юнит
        /// </summary>
        /// <param name="enemyUnit">Вражеский юнит, которого уничтожаем</param>
        /// <param name="playSound">Воспроизводить звук уничтожения</param>
        private void DestroyEnemy(GameFieldObject enemyUnit, bool playSound)
        {
            CreateUnitExplosion(enemyUnit);
            if (playSound)
                soundEngine.PlaySound(enemyUnit.DestroySndId);
            enemiesCount--;
            activeEnemiesCount--;

            if (spawnEnemyDelayFrames <= 0)
                UpdateEnemySpawnDelay();

            if (enemiesCount == 0)
            {
                SetAllEnemiesDestroyed();
            }
        }

        /// <summary>
        /// Создать анимацию взрыва юнита
        /// </summary>
        /// <param name="fieldObject"></param>
        private void CreateUnitExplosion(GameFieldObject fieldObject)
        {
            CreateObjectExplosion(fieldObject, "UNIT_EXPLOSION");
        }

        /// <summary>
        /// Создать анимацию взрыва снаряда
        /// </summary>
        /// <param name="fieldObject"></param>
        private void CreateBulletExplosion(GameFieldObject fieldObject)
        {
            CreateExplosion(fieldObject, "BULLET_EXPLOSION");
        }

        /// <summary>
        /// Создать анимцию взрыва указанного объекта с заданным названием анимации
        /// </summary>
        /// <param name="fieldObject"></param>
        /// <param name="animationObjectName"></param>
        private void CreateExplosion(GameFieldObject fieldObject, string animationObjectName)
        {
            var gameObject = new AnimationObject();
            var preset = content.GameObjects.GetByName(animationObjectName);
            gameObject.CopyFrom(preset);

            int fieldObjectX = Math.Max(0, Math.Min(fieldObject.X, Config.FieldWidth));
            int fieldObjectY = Math.Max(0, Math.Min(fieldObject.Y, Config.FieldHeight));

            int fieldObjectWidth = fieldObject.Width;
            int fieldObjectHeight = fieldObject.Height;

            int x = Convert.ToInt32(((fieldObjectWidth - gameObject.Width) * Config.SubPixelSize) / 2.0);
            int y = Convert.ToInt32(((fieldObjectHeight - gameObject.Height) * Config.SubPixelSize) / 2.0);

            var subx = x + fieldObject.SubPixelX;
            var suby = y + fieldObject.SubPixelY;

            gameObject.X = fieldObjectX + subx / Config.SubPixelSize;
            gameObject.Y = fieldObjectY + suby / Config.SubPixelSize;

            gameObject.SubPixelX = subx % Config.SubPixelSize +
                (fieldObject.Direction == MoveDirection.Up || fieldObject.Direction == MoveDirection.Down ? Config.SubPixelSize / 2 : 0);
            gameObject.SubPixelY = suby % Config.SubPixelSize +
                (fieldObject.Direction == MoveDirection.Left || fieldObject.Direction == MoveDirection.Right ? Config.SubPixelSize / 2 : 0);

            animations.Add(gameObject);
            gameObjects.Add(gameObject);
        }

        /// <summary>
        /// Создать анимцию взрыва указанного объекта с заданным названием анимации
        /// </summary>
        /// <param name="fieldObject"></param>
        /// <param name="animationObjectName"></param>
        private void CreateObjectExplosion(GameFieldObject fieldObject, string animationObjectName)
        {
            var gameObject = new AnimationObject();
            var preset = content.GameObjects.GetByName(animationObjectName);
            gameObject.CopyFrom(preset);

            var xPos = (fieldObject.Width - gameObject.Width) / 2;
            var yPos = (fieldObject.Height - gameObject.Height) / 2;

            gameObject.X = fieldObject.X + xPos;
            gameObject.Y = fieldObject.Y + yPos;

            gameObject.SubPixelX = fieldObject.SubPixelX;
            gameObject.SubPixelY = fieldObject.SubPixelY;

            animations.Add(gameObject);
            gameObjects.Add(gameObject);
        }

        /// <summary>
        /// Начать появление вражеского юнита
        /// </summary>
        private void SpawnEnemy()
        {
            if (spawnEnemyDelayFrames > 0)
                return;

            int maxActiveEnemies = stageMaxActiveEnemies + (Math.Max(0, NumActivePlayers - 1) * Math.Max(1, Config.MaxActiveEnemy / 2));
            if (activeEnemiesCount >= maxActiveEnemies)
                return;

            if (enemyQueue.Count > 0)
            {
                if (enemySpawnPositionIndex >= respawnPoints.Count)
                    enemySpawnPositionIndex = MaxActivePlayers;

                var spawnPoint = respawnPoints[enemySpawnPositionIndex];

                // поиск свободной точки появления
                if (spawnPoint.SpawnObject != null)
                {
                    for (int i = 0; i < maxActiveEnemies; i++)
                    {
                        enemySpawnPositionIndex++;
                        if (enemySpawnPositionIndex >= respawnPoints.Count)
                            enemySpawnPositionIndex = MaxActivePlayers;

                        if (spawnPoint.SpawnObject == null)
                            break;
                    }

                    if (spawnPoint.SpawnObject != null)
                    {
                        // Нет свободной точки появления
                        // logger?.WriteLine($"{nameof(SpawnEnemy)}: no free point to spawn", LogLevel.Error);
                        return;
                    }
                }

                int index = enemySpawnPositionIndex;

                // поиск точки появления, которая не пересекатся с юнитом
                for (int i = 0; i < maxActiveEnemies; i++)
                {
                    spawnPoint = respawnPoints[index];
                    if (spawnPoint.SpawnObject == null &&
                        !battleUnits.Any(p => !p.IsSpawn && p.IsAlive && p.Overlaps(spawnPoint, Config.SubPixelSize)))
                    {
                        enemySpawnPositionIndex = index;
                        break;
                    }

                    index++;
                    if (index >= respawnPoints.Count)
                        index = MaxActivePlayers;
                }

                if (spawnPoint.SpawnObject != null)
                {
                    // Нет свободной точки появления
                    // logger?.WriteLine($"{nameof(SpawnEnemy)}: no free point to spawn", LogLevel.Error);
                    return;
                }

                var unit = GetNextEnemy();
                if (unit == null)
                    return;

                spawnPoint.Reset(Config.SpawnAnimationDuration);
                spawnPoint.IsVisible = true;
                unit.SetPositionFromObject(spawnPoint);
                unit.IsSpawn = true;

                spawnPoint.SpawnObject = unit;
                gameObjects.Add(spawnPoint);

                activeEnemiesCount++;
                enemySpawnPositionIndex++;
                UpdateEnemySpawnDelay();
            }
        }

        /// <summary>
        /// Определить время ожидания перед появлением следующего вражеского юнита
        /// </summary>
        private void UpdateEnemySpawnDelay()
        {
            #region original BC

            // 190 - level * 4 - (player_count - 1) * 20
            //int frames = 190 - (stageNumber -1 ) * 4 - (NumActivePlayers - 1) * 20;

            int frames = (Config.SpawnAnimationDuration + Config.EnemySpawnDelay) - (stageNumber - 1) * 4 - (NumActivePlayers - 1) * 20;
            spawnEnemyDelayFrames = Math.Max(0, frames);

            #endregion

            #region custom method

            //int maxActiveEnemies = stageMaxActiveEnemies + (Math.Max(0, NumActivePlayers - 1) * Math.Max(1, Config.MaxActiveEnemy / 2));
            //if (spawnedEnemyCount < maxActiveEnemies)
            //    spawnEnemyDelayFrames = Config.SpawnAnimationDuration + Config.EmenySpawnDelay;
            //else
            //    spawnEnemyDelayFrames = Config.EmenySpawnDelay;

            #endregion
        }

        /// <summary>
        /// Убрать все активные сокровища (бонусные объекты)
        /// </summary>
        private void RemovePowerUpObjects()
        {
            gameObjects.RemoveAll(p => p.Type.HasFlag(GameObjectType.PowerUp));
        }

        /// <summary>
        /// Установка состояния игры, при котором все враги считаются уничтоженными
        /// </summary>
        private void SetAllEnemiesDestroyed()
        {
            stageState = StageStateEnum.Complete;
            stageCompleteOverlay.Show(Math.Max(1, Config.StageCompleteDelayTime) * 60);
        }

        /// <summary>
        /// Уничтожить всех игроков
        /// </summary>
        public void DestroyAllPlayers()
        {
            foreach (var player in players)
            {
                DestroyPlayer(player);
            }
        }

        /// <summary>
        /// Уничтожеть пользовательский юнит
        /// </summary>
        /// <param name="player"></param>
        /// <param name="force">Принудительное уничтожение внезависимости от наличия неуязвимости</param>
        private void DestroyPlayer(Player player, bool force = false)
        {
            // если ссылна на объект игрока на задана или игрок не жив или ссылка на объект юнита не задана
            if (player == null || !player.IsAlive || player.Unit == null)
            {
                // ничего не далаем
                return;
            }

            // если игрок на возрождении (появляется)
            if (player.Unit.IsSpawn)
            {
                // ничего не далаем
                return;
            }

            // если активна неуязвимость
            if (player.Unit.Invincibility > 0 && !force)
            {
                // ничего не делаем
                return;
            }

            // обновляем признак жизни юнита
            player.Unit.IsAlive = false;
            // создаем анимацию уничтожения юнита
            CreateUnitExplosion(player.Unit);
            // воспроизводим звук уничтожения юнита
            soundEngine.PlaySound(player.Unit.DestroySndId);
            // уменьшаем количество жизней игрока
            player.Lifes--;
            // удаляем с поля юнит игрока
            gameObjects.Remove(player.Unit);

            // проверяем количество жизней игрока
            if (player.Lifes == 0)
            {
                player.Unit.IsAlive = false;
                //activePlayers = Math.Max(0, activePlayers - 1);
                // Игра завершена, если на поле нет живых игроков
                if (!players.Any(p => p != null && p.IsAlive && p.Lifes > 0))
                    SetGameOver();
            }
            else
            {
                player.UpgradeLevel = Config.PlayerDefaultUpgradeLevel;
                SpawnPlayer(player);
            }
        }

        /// <summary>
        /// Подгтовить игрока к появлению
        /// </summary>
        /// <param name="player"></param>
        private void SpawnPlayer(Player player)
        {
            int i = player.Id - 1;
            InitPlayerUnit(player, GetPlayerColor(i));

            var spawnPoint = respawnPoints[i];
            spawnPoint.Reset(Config.SpawnAnimationDuration);
            spawnPoint.IsVisible = true;
            spawnPoint.SpawnObject = player.Unit;

            player.Unit.IsAlive = true;
            player.Unit.MoveInertion = 0;
            player.Unit.SetPositionFromObject(respawnPoints[i]);
            player.Unit.Direction = MoveDirection.Up;
            player.Unit.IsSpawn = true;

            gameObjects.Add(spawnPoint);
        }

        /// <summary>
        /// Инициализация юнита игрока
        /// </summary>
        /// <param name="player">Ссылка на объект игрока</param>
        /// <param name="hexColor">Отличительный цвет юнита</param>
        private void InitPlayerUnit(Player player, string hexColor)
        {
            var allPlayerUnits = content.GameObjects.GetAll(p => p != null && p.Type.HasFlag(GameObjectType.Player));
            var maxUpradedUnit = allPlayerUnits.OrderByDescending(p => p.UpgradeLevel).FirstOrDefault();
            var unitPreset = allPlayerUnits.FirstOrDefault(p => p.UpgradeLevel == player.UpgradeLevel) ?? maxUpradedUnit;
            player.Unit.CopyFrom(unitPreset);
            player.Unit.Name = player.PlayerName;
            player.Unit.HexColor = hexColor;
            player.Unit.ForceMoveInertion = false;
        }

        /// <summary>
        /// Выполнить обновление всех объектов. Вызывается при каждом кадре
        /// </summary>
        private void Update()
        {
            if (IsDisposed)
                return;

            if (stageState == StageStateEnum.None)
                return;

            if (stageState == StageStateEnum.ExitToMainScreen)
            {
                ExitToMainScreen();
                return;
            }

            // инкрементируем номер кадра в качестве игрового времени
            gameTime++;

            if (stageState != StageStateEnum.GameOver)
            {
                if (stageCompleteOverlay.IsVisible)
                {
                    stageCompleteOverlay.Update();
                    if (stageCompleteOverlay.ElapsedFrames <= 0)
                    {
                        stageCompleteOverlay.Hide();
                        OnEndGame();
                        return;
                    }
                }

                ProcessControllerKeys();

                if (gamePauseOverlay.IsVisible)
                    return;
            }
            else if (gameOverOverlay.IsVisible)
            {
                gameOverOverlay.Update();
                if (gameOverOverlay.ElapsedFrames <= 0)
                {
                    gameOverOverlay.Hide();
                    OnEndGame();
                    return;
                }
            }

            UpdateSpawnPositions();
            CheckBulletsCollision();
            SpawnEnemy();
            ProcessEnemy();
            UpdatePlayers();
            UpdateTowerDefenseState();
            UpdateTextBlocks();
            UpdateAnimations();
        }

        /// <summary>
        /// Обновить состояния игроков
        /// </summary>
        private void UpdatePlayers()
        {
            foreach (var player in players)
            {
                if (player == null || player.Unit == null)
                    continue;

                if (player.IsFrozen)
                {
                    player.Unit.Freeze--;
                    player.Unit.UpdateAnimation(gameTime);
                }

                if (player.Unit.MoveInertion > 0)
                    MovePlayerUnit(player.Unit);
                else
                    player.Unit.StopMoving();
                UpdateUnitShieldState(player.Unit);
            }
        }

        /// <summary>
        /// Вызов события завершения игры
        /// </summary>
        private void OnEndGame()
        {
            EndGame?.Invoke(new StageResult()
            {
                StageNumber = stageNumber,
                IsGameOver = stageState == StageStateEnum.GameOver,
                PlayersResults = players.Where(p => p != null).ToList()
            });
        }

        /// <summary>
        /// Обновляем текстовые блоки
        /// </summary>
        private void UpdateTextBlocks()
        {
            foreach (var textBlock in textBlocks)
            {
                textBlock.FrameNumber++;
            }

            textBlocks.RemoveAll(p => p.FrameNumber >= Config.PointsTextShowDuration);
        }

        /// <summary>
        /// Выход на главный экран
        /// </summary>
        private void ExitToMainScreen()
        {
            soundEngine?.StopAll();
            Exit?.Invoke();
        }

        /// <summary>
        /// Обновить состояние щита для указанного юнита
        /// </summary>
        /// <param name="unit"></param>
        private void UpdateUnitShieldState(BattleUnit unit)
        {
            if (unit == null)
                return;

            if (unit.Invincibility > 0)
            {
                unit.Invincibility--;

                if (unit.Invincibility == 0)
                {
                    RemoveShield(unit);
                }
            }
        }

        /// <summary>
        /// Поставить игру на паузу
        /// </summary>
        private void PauseGame()
        {
            if (stageCompleteOverlay.IsVisible || gameOverOverlay.IsVisible)
                return;

            stageStartSnd?.Pause();
            gamePauseOverlay.Show();
        }

        /// <summary>
        /// Продолжить игру после паузы
        /// </summary>
        private void ResumeGame()
        {
            stageStartSnd?.Resume();
            gamePauseOverlay.Hide();
        }

        /// <summary>
        /// Выполнить обновление объектов анимации
        /// </summary>
        private void UpdateAnimations()
        {
            foreach (var animationObject in animations)
            {
                animationObject.Update();

                if (animationObject.ElapsedFrames <= 0 && !animationObject.AutoRepeat)
                    gameObjects.Remove(animationObject);
            }

            animations.RemoveAll(x => x.ElapsedFrames <= 0 && !x.AutoRepeat);
        }

        /// <summary>
        /// Создать щит указанному юниту
        /// </summary>
        /// <param name="unit">Юнит, которому создается щит</param>
        /// <param name="duration">Продолжительность действия щита</param>
        private void AddShield(BattleUnit unit, int duration)
        {
            if (duration <= 0)
                return;

            unit.Invincibility = duration;
            unit.Shield = new AnimationObject()
            {
                AutoRepeat = true,
                AttachedObject = unit
            };
            unit.Shield.CopyFrom(content.GameObjects.GetByName("SHIELD"));

            animations.Add(unit.Shield);
            gameObjects.Add(unit.Shield);
        }

        /// <summary>
        /// Добавить дополнительный щит указанному юниту
        /// </summary>
        /// <param name="unit">Юнит, которому создается щит</param>
        public void AddExtraShield(BattleUnit unit)
        {
            //ExtraShieldDuration
            AddShield(unit, Config.ExtraShieldDuration);
        }

        /// <summary>
        /// Снять щит с указанного юнита
        /// </summary>
        /// <param name="unit">Юнит, с которого снимается щит</param>
        private void RemoveShield(BattleUnit unit)
        {
            if (unit.Shield != null)
            {
                animations.Remove(unit.Shield);
                gameObjects.Remove(unit.Shield);
                unit.Shield = null;
            }
        }

        /// <summary>
        /// Обновить позиции появления
        /// </summary>
        private void UpdateSpawnPositions()
        {
            foreach (var sp in respawnPoints)
            {
                if (sp.SpawnObject == null)
                    continue;

                sp.Update();

                if (!sp.IsReady)
                    continue;

                if (sp.SpawnObject is BattleUnit unit)
                {
                    unit.IsSpawn = false;
                    unit.IsAlive = true;

                    if (unit.IsUser)
                    {
                        AddShield(unit, Config.PlayerSpawnShieldDuration);
                    }
                    else if (unit is EnemyUnit enemyUnit)
                    {
                        AddShield(unit, Config.EnemySpawnShieldDuration);
                        battleUnits.Add(enemyUnit);
                        spawnedEnemyCount++;

                        if (enemyUnit.ExtraBonus > 0 && Config.MaxBonusedUnitsOnField == 1 && Config.HidePowerUpsIfBonusedEnemySpawned)
                        {
                            RemovePowerUpObjects();
                        }
                    }
                }

                gameObjects.Add(sp.SpawnObject);
                gameObjects.Remove(sp);

                sp.Reset(Config.SpawnAnimationDuration);
                if (sp.SpawnObject != null && sp.SpawnObject.Type.HasFlag(GameObjectType.Enemy))
                    sp.ShowDelay = Config.EnemySpawnDelay;
            }
        }

        /// <summary>
        /// Обработать нажатия кнопок (клавиш)
        /// </summary>
        private void ProcessControllerKeys()
        {
            bool skipPauseBtn = false;
            if (!IsGamePaused)
            {
                var player1 = players.FirstOrDefault(x => x != null && x.Id == 1);
                var player2 = players.FirstOrDefault(x => x != null && x.Id == 2);

                if (player1 != null)
                {
                    if (!player1.IsAlive && !player1.Unit.IsSpawn)
                    {
                        if (Config.AllowPlayerJoin && player2 != null && player2.Lifes > 1 &&
                            controllerHub.IsKeyPressed(1, ButtonNames.Start, true))
                        {
                            skipPauseBtn = true;
                            JoinPlayer(1, player2);
                        }
                    }
                    else if (player1.IsAlive && !player1.Unit.IsSpawn)
                        ProcessInputPlayer1(player1);
                }
                else
                {
                    if (Config.AllowPlayerJoin && player2 != null && player2.Lifes > 1 &&
                           controllerHub.IsKeyPressed(1, ButtonNames.Start, true))
                    {
                        skipPauseBtn = true;
                        JoinPlayer(1, player2);
                    }
                }
                if (player2 != null)
                {
                    if (!player2.IsAlive && !player2.Unit.IsSpawn)
                    {
                        if (Config.AllowPlayerJoin && player1 != null && player1.Lifes > 1 &&
                            controllerHub.IsKeyPressed(2, ButtonNames.Start, true))
                        {
                            skipPauseBtn = true;
                            JoinPlayer(2, player1);
                        }
                    }
                    else if (player2.IsAlive && !player2.Unit.IsSpawn)
                        ProcessInputPlayer2(player2);
                }
                else
                {
                    if (Config.AllowPlayerJoin && player1 != null && player1.Lifes > 1 &&
                            controllerHub.IsKeyPressed(2, ButtonNames.Start, true))
                    {
                        skipPauseBtn = true;
                        JoinPlayer(2, player1);
                    }
                }
            }

            ProcessGlobalInput(skipPauseBtn);
        }

        /// <summary>
        /// Задействовать игрока
        /// </summary>
        /// <param name="playerId">Идентификатор (номер) игрока, который присоединяется к игре</param>
        /// <param name="doner">Игрок, который жертвует жизнью</param>
        private void JoinPlayer(int playerId, Player doner)
        {
            if (playerId <= 0 || playerId > MaxActivePlayers)
                return;

            int playerIndex = playerId - 1;
            var player = Player.Create(Config, playerId, Config.StartLifes);
            players[playerIndex] = player;
            InitPlayerUnit(players[playerIndex], GetPlayerColor(playerIndex));
            player.UpgradeLevel = Config.PlayerDefaultUpgradeLevel;
            player.Lifes = 1;
            doner.Lifes--;
            SpawnPlayer(player);
        }

        /// <summary>
        /// Выполнить движение юнита указанного игрока по заданному направлению
        /// </summary>
        /// <param name="player"></param>
        /// <param name="moveDirection">Направление движения</param>
        private void MovePlayer(Player player, MoveDirection moveDirection)
        {
            if (player.IsFrozen || !player.IsAlive || player.Unit == null)
                return;

            var unit = player.Unit;
            if (unit.MoveInertion > 0)
            {
                if (!unit.ForceMoveInertion)
                    return;
                if (!(unit.SubPixelX == 0 || unit.SubPixelY == 0))
                    return;
                unit.SetDirection(moveDirection);
                unit.MoveInertion = Convert.ToInt32(constructionHelper.DefaultBlockSize * Config.SubPixelSize * unit.MoveSpeed);
                return;
            }

            unit.SetDirection(moveDirection);
            unit.MoveInertion = Config.MoveInertionDuration;
        }

        /// <summary>
        /// Выполнить движение юнита
        /// </summary>
        /// <param name="unit"></param>
        private void MovePlayerUnit(BattleUnit unit)
        {
            if (MoveUnit(unit, Config.PlayerMoveSpeedMultiply))
            {
                unit.UpdateAnimation(gameTime);
                if (unit.MoveInertion > 0)
                    unit.MoveInertion--;
                soundEngine.PlaySound("move", true);
            }
            else
            {
                unit.StopMoving();
            }
        }

        /// <summary>
        /// Обработать нажатия ввода для первого игрока
        /// </summary>
        /// <param name="player"></param>
        private void ProcessInputPlayer1(Player player)
        {
            player.Unit.Update();

            if (controllerHub.IsKeyPressed(player.Id, ButtonNames.Up, false))
            {
                MovePlayer(player, MoveDirection.Up);
            }
            else if (controllerHub.IsKeyPressed(player.Id, ButtonNames.Down, false))
            {
                MovePlayer(player, MoveDirection.Down);
            }
            else if (controllerHub.IsKeyPressed(player.Id, ButtonNames.Left, false))
            {
                MovePlayer(player, MoveDirection.Left);
            }
            else if (controllerHub.IsKeyPressed(player.Id, ButtonNames.Right, false))
            {
                MovePlayer(player, MoveDirection.Right);
            }

            if (controllerHub.IsKeyPressed(player.Id, ButtonNames.Attack, !gameApplication.ContinuousFire))
            {
                PlayerShoot(player.Unit);
            }
        }

        /// <summary>
        /// Обработать нажатия ввода для второго игрока
        /// </summary>
        /// <param name="player"></param>
        /// <param name="input"></param>
        /// <param name="prevFrameInput"></param>
        private void ProcessInputPlayer2(Player player)
        {
            player.Unit.Update();

            if (controllerHub.IsKeyPressed(player.Id, ButtonNames.Up, false))
            {
                MovePlayer(player, MoveDirection.Up);
            }
            else if (controllerHub.IsKeyPressed(player.Id, ButtonNames.Down, false))
            {
                MovePlayer(player, MoveDirection.Down);
            }
            else if (controllerHub.IsKeyPressed(player.Id, ButtonNames.Left, false))
            {
                MovePlayer(player, MoveDirection.Left);
            }
            else if (controllerHub.IsKeyPressed(player.Id, ButtonNames.Right, false))
            {
                MovePlayer(player, MoveDirection.Right);
            }

            if (controllerHub.IsKeyPressed(player.Id, ButtonNames.Attack, !gameApplication.ContinuousFire))
            {
                PlayerShoot(player.Unit);
            }
        }

        /// <summary>
        /// Глобальная обработка нажатий ввода
        /// </summary>
        private void ProcessGlobalInput(bool skipPauseBtn)
        {
            if (gamePauseOverlay.IsVisible)
            {
                if (controllerHub.IsKeyPressed(0, ButtonNames.Pause, true))
                    ResumeGame();
            }

            else if (!skipPauseBtn && controllerHub.IsKeyPressed(0, ButtonNames.Pause, true))
            {
                PauseGame();
            }

            if (Config.CheatsEnabled)
            {
                if (controllerHub.Keyboard.IsDown(KeyboardKey.F1))
                {
                    enemyIsActive = !enemyIsActive;
                }

                else if (controllerHub.Keyboard.IsDown(KeyboardKey.F2))
                {
                    DestroyAllActiveEnemies();
                }

                else if (controllerHub.Keyboard.IsDown(KeyboardKey.F3))
                {
                    UpgradePlayerUnit(players[0].Unit);
                }

                else if (controllerHub.Keyboard.IsDown(KeyboardKey.F4))
                {
                    DestroyAllPlayers();
                }

                else if (controllerHub.Keyboard.IsDown(KeyboardKey.F5))
                {
                    Config.EnemyPowerUpAllowed = !Config.EnemyPowerUpAllowed;
                }

                else if (controllerHub.Keyboard.IsDown(KeyboardKey.F6))
                {
                    Config.PlayerPowerUpAllowed = !Config.PlayerPowerUpAllowed;
                }

                else if (controllerHub.Keyboard.IsDown(KeyboardKey.F7))
                {
                    Config.EnemyFriendlyFire = !Config.EnemyFriendlyFire;
                }

                else if (controllerHub.Keyboard.IsDown(KeyboardKey.F8))
                {
                    Config.ShowChessboard = !Config.ShowChessboard;
                }

                else if (controllerHub.Keyboard.IsDown(KeyboardKey.F9))
                {
                    CreateTowerDefense();
                    //DestroyPlayer(players.FirstOrDefault(p => p.Id == 1));
                }

                else if (controllerHub.Keyboard.IsDown(KeyboardKey.F10))
                {
                    AddPowerUpObject();
                }

                else if (controllerHub.Keyboard.IsDown(KeyboardKey.F11))
                {
                    //DestroyPlayer(players.FirstOrDefault(p => p.Id == 2));
                    //CreateTestBlocks();
                }
            }

            if (controllerHub.Keyboard.IsDown(KeyboardKey.F12))
            {
                stageState = StageStateEnum.ExitToMainScreen;
            }
        }

        /// <summary>
        /// Уничтожить все вражеские юниты, которые сейчас на поле
        /// </summary>
        public void DestroyAllActiveEnemies()
        {
            var tmpList = battleUnits.ToList();
            bool playSound = true;
            tmpList.ForEach(unit =>
            {
                battleUnits.Remove(unit);
                gameObjects.Remove(unit);
                DestroyEnemy(unit, playSound);
                playSound = false;
            });
        }

        /// <summary>
        /// Заморозить (заблокировать) все вражеские юниты, которые сейчас на поле
        /// </summary>
        public void FreezeAllActiveEnemies()
        {
            freezeEnemyTime = Config.EnemyFreezeDuration;
        }

        /// <summary>
        /// Заморозить (заблокировать движение) всех активных игроков
        /// </summary>
        public void FreezeAllActivePlayers()
        {
            foreach (var player in players)
            {
                FreezePlayer(player);
            }
        }

        /// <summary>
        /// Заморозить (заблокировать движение) указанного игрока
        /// </summary>
        public void FreezePlayer(Player player)
        {
            if (player == null || player.Unit == null)
                return;
            player.Unit.Freeze = Config.PlayerFreezeDuration;
        }

        /// <summary>
        /// Добавить снаряд
        /// </summary>
        private void AddBullet(Bullet bullet)
        {
            bullets.Add(bullet);
            var b = new GameFieldObject().CopyFrom(content.GameObjects.GetByName("bullet"));
            b.X = bullet.X;
            b.Y = bullet.Y;
            b.SubPixelX = bullet.SubPixelX;
            b.SubPixelY = bullet.SubPixelY;
            b.Direction = bullet.Direction;
            bullet.SetBulletObject(b);
            gameObjects.Add(b);
        }

        /// <summary>
        /// Удаляем снаряд с поля
        /// </summary>
        private void DestroyBullet(GameFieldObject bulletObject)
        {
            if (bulletObject == null)
                return;
            var bullet = bullets.FirstOrDefault(x => x.BulletObject == bulletObject);
            if (bullet != null)
            {
                bullet.Power = 0;
                bullet.Owner.Gun?.ReloadGun(false);
            }
        }

        /// <summary>
        /// Игрок выстрелил
        /// </summary>
        private void PlayerShoot(BattleUnit unit)
        {
            var bullet = unit.Gun?.Fire(unit, Config.SubPixelSize);
            if (bullet == null)
                return;
            AddBullet(bullet);
            soundEngine.PlaySound(unit.Gun.ShotSndId);
        }

        /// <summary>
        /// Выполнить обработку логики ИИ
        /// </summary>
        private void ProcessEnemy()
        {
            if (freezeEnemyTime <= 0 && enemyIsActive)
            {
                foreach (var unit in battleUnits)
                {
                    if (!unit.IsAlive)
                        continue;

                    var result = unit.Update();

                    if (result.HasFlag(UnitAction.Move))
                    {
                        bool successMove = MoveUnit(unit, Config.EnemyMoveSpeedMultiply);
                        if (unit.MoveInertion == 0) unit.StopMoving();

                        if (unit.SubPixelX == 0 && unit.SubPixelY == 0 && Config.Random.Next() % 16 == 0)
                        {
                            ChangeEnemyDirection(unit);
                        }

                        else if (!successMove && Config.Random.Next() % 4 == 0)
                        {
                            if (unit.SubPixelX != 0 || unit.SubPixelY != 0)
                            {
                                unit.InvertDirection();
                            }
                            else
                            {
                                unit.ChangeDirection();
                            }
                        }

                        unit.UpdateAnimation(gameTime);
                        if (!soundEngine.IsPlayingSound("move") && stageState == StageStateEnum.Play)
                            soundEngine.PlaySound("enemy_move", true);
                    }

                    if (result.HasFlag(UnitAction.Attack))
                    {
                        var bullet = unit.Gun?.Fire(unit, Config.SubPixelSize);
                        if (bullet != null)
                        {
                            AddBullet(bullet);
                            soundEngine.PlaySound(unit.Gun.ShotSndId);
                        }
                    }
                }
            }

            // обновляем значение времени заморозки
            if (enemyIsActive && freezeEnemyTime > 0)
                freezeEnemyTime--;

            // обновляем счётчик ожидания появления вражеских юнитов
            if (spawnEnemyDelayFrames > 0)
                spawnEnemyDelayFrames--;

            // Обновляем состояния щита у вражеских юнитов
            foreach (var unit in battleUnits)
            {
                UpdateUnitShieldState(unit);
            }
        }

        /// <summary>
        /// Выполнить движение указанного юнита
        /// </summary>
        /// <returns></returns>
        private bool MoveUnit(BattleUnit unit, int moveSpeedMultiply)
        {
            // запоминаем текущее положение юнита
            var x = unit.X;
            var y = unit.Y;
            var subx = unit.SubPixelX;
            var suby = unit.SubPixelY;

            // определяем текущие наложения юнита с объектами
            var unitOverlaps = gameObjects
                .Where(p =>
                    p.IsVisible &&
                    !p.IsSpawn &&
                    p != unit &&
                    p.Type.HasFlag(GameObjectType.Barrier) &&
                    !p.Type.HasFlag(GameObjectType.Water) &&
                    unit.OverlapsOrContains(p, Config.SubPixelSize))
                .ToList();

            // выполняем движение юнита
            var fieldBoundsCollision = unit.Move(
                Config.FieldWidth,
                Config.FieldHeight,
                Config.SubPixelSize,
                moveSpeedMultiply);

            // проверяем столкновение с границами игрового поля
            switch (fieldBoundsCollision)
            {
                case FieldBoundsCollision.Collided:
                    bool positionChanged = unit.X != x || unit.Y != y || subx != unit.SubPixelX || suby != unit.SubPixelY;
                    if (!unit.IsUser)
                    {
                        unit.MoveInertion = Math.Min(unit.MoveInertion, Config.SubPixelSize);
                    }
                    return positionChanged;
            }

            // проверка условия столкновения юнитов
            List<BattleUnit> overlapsAfterMove = battleUnits
                .Cast<BattleUnit>()
                .Where(p => p != unit && !p.IsSpawn && p.IsAlive && p.Overlaps(unit, Config.SubPixelSize))
                .ToList();

            overlapsAfterMove.AddRange(
                players.Where(pl => pl != null && pl.IsAlive && !pl.Unit.IsSpawn).Select(p => p.Unit)
                .Where(p => p != unit && p.Overlaps(unit, Config.SubPixelSize))
                );

            if (overlapsAfterMove.Count > 0)
            {
                // если это условие не выполняется, то ниже в коде всё равно отработает условие остановки юнита
                if (!overlapsAfterMove.All(p => unitOverlaps.Contains(p)))
                {
                    unit.StopMoving();
                    unit.SetPosition(x, y, subx, suby);
                    return false;
                }

                // дополнительно проверяем наложение с водой и структурами (brick, iron, ..)
                if (unitOverlaps.Any(p => p.Type.HasFlag(GameObjectType.Water) && !unit.Type.HasFlag(GameObjectType.Ship))
                   )
                {
                    unit.StopMoving();
                    unit.SetPosition(x, y, subx, suby);
                    return false;
                }

                //return true;
            }

            // список объектов для последующего удаления
            List<GameFieldObject> objectsToRemove = new List<GameFieldObject>();

            GameFieldObject iceFieldObj = null;

            // формируем копию списка объектов, для которых выполним проверку на пересечение (наложение)
            var gameObjectsCopy = gameObjects
                .Where(p =>
                    p != unit &&
                    p.IsVisible &&
                    !p.IsSpawn &&
                    !(p is BattleUnit)
                ).ToList();

            foreach (var gameObject in gameObjectsCopy)
            {
                // проверяем пересечение
                if (!unit.Overlaps(gameObject, Config.SubPixelSize))
                {
                    // нет пересечения
                    continue;
                }

                // проверяем признак пересечения с ледяной поверхностью
                if (gameObject.Type.HasFlag(GameObjectType.Ice))
                {
                    iceFieldObj = gameObject;
                    continue;
                }
                // проверяем признак пересечения с водой
                else if (gameObject.Type.HasFlag(GameObjectType.Water))
                {
                    // проверяем возможность хождения по воде
                    if (!unit.Type.HasFlag(GameObjectType.Ship))
                    {
                        unit.StopMoving();
                        unit.SetPosition(x, y, subx, suby);
                        return false;
                    }
                }
                // проверяем признак пересечения с объектом прокачки
                else if (gameObject.Type.HasFlag(GameObjectType.PowerUp))
                {
                    if (unit.IsUser)
                    {
                        if (Config.PlayerPowerUpAllowed)
                        {
                            objectsToRemove.Add(gameObject);
                            HandlePowerUp(unit, gameObject);
                        }
                    }
                    else
                    {
                        if (Config.EnemyPowerUpAllowed)
                        {
                            objectsToRemove.Add(gameObject);
                            HandlePowerUp(unit, gameObject);
                        }
                    }
                }
                else if (gameObject.Type.HasFlag(GameObjectType.Barrier))
                {
                    unit.StopMoving();
                    unit.SetPosition(x, y, subx, suby);
                    return false;
                }
            }

            // Если юнит не скользит по льду и задано форсированное движение
            if (iceFieldObj == null && unit.ForceMoveInertion)
            {
                // останавливаем движение
                unit.StopMoving();
            }
            // если юнит на льду на продолжает двигаться
            else if (iceFieldObj != null && unit.MoveInertion > 0)
            {
                if (!unit.ForceMoveInertion && unit.IsUser && unit.MoveInertion == Config.MoveInertionDuration)
                    soundEngine.PlaySound(iceFieldObj.CollideSndId);

                if (!unit.ForceMoveInertion)
                {
                    unit.ForceMoveInertion = true;
                    //unit.MoveInertion = Convert.ToInt32(iceFieldObj.Width * Config.SubPixelSize * unit.MoveSpeed);
                    unit.MoveInertion = Convert.ToInt32(constructionHelper.DefaultBlockSize * Config.SubPixelSize * unit.MoveSpeed);
                }
            }

            // удаляем объекты (подобранные объекты прокачки)
            foreach (var gameObject in objectsToRemove)
                gameObjects.Remove(gameObject);

            return true;
        }

        private void ChangeEnemyDirection(EnemyUnit unit)
        {
            if (Config.Random.Next() % 2 == 0)
            {
                unit.ChangeDirection();
            }
            else if (Config.Random.Next() % 2 == 0)
            {
                unit.RotateClockwise();
            }
            else
            {
                unit.RotateCounterClockwise();
            }
        }

        /// <summary>
        /// Обработка получение бонуса (прокачки)
        /// </summary>
        private void HandlePowerUp(BattleUnit unit, GameFieldObject powerUpObj)
        {
            // проверяем, что указанный объект является бонусом
            if (!powerUpObj.Type.HasFlag(GameObjectType.PowerUp))
                return;

            // находим обработчик подбора бонуса
            var handler = powerUpHandlers.FirstOrDefault(x =>
                x.GetType().Name.ToUpper()
                .StartsWith(powerUpObj.Name.Replace("_", ""))
                );

            int? currentUnitLifes = GetPlayerByUnit(unit)?.Lifes;

            // вызываем метод обработки подбора бонуса
            handler?.Handle(unit, powerUpObj);

            bool playSound = true;

            if (unit.IsUser)
            {
                var player = GetPlayerByUnit(unit);

                // Если у игрока изменилось количество жизней
                // то сработал бонус и звук уже теоретитчески воспроизводится
                if (player.Lifes != currentUnitLifes)
                {
                    // звук подбора бонуса не воспроизводим
                    playSound = false;
                }

                if (powerUpObj.BonusPoints != 0)
                {
                    int currentLifes = player.Lifes;
                    AddPoints(player, powerUpObj.BonusPoints);

                    if (currentLifes != player.Lifes)
                    {
                        // т.к. при начисление очков игроку может быть добавлена жизнь,
                        // которая сопровождается воспроизведение соответствующего звука, то
                        // звук подбора бонуса не воспроизводим
                        playSound = false;
                    }
                    DisplayPointsText(powerUpObj.BonusPoints.ToString(), powerUpObj.GetBounds(Config.SubPixelSize));
                }
            }

            if (playSound)
                soundEngine.PlaySound(powerUpObj.DestroySndId);
        }

        /// <summary>
        /// Начисление очков указанному игроку
        /// </summary>
        /// <param name="player">Игрок, которму начисляются очки</param>
        /// <param name="amount">Начисляемое значение очков</param>
        public void AddPoints(Player player, int amount)
        {
            if (player == null || amount <= 0)
                return;
            int oldScore = player.Score;
            player.Score += amount;

            if (Config.RewardsExtraLifeAnEvery > 0)
            {
                int oldValue = oldScore / Config.RewardsExtraLifeAnEvery;
                int newValue = player.Score / Config.RewardsExtraLifeAnEvery;
                if (newValue > oldValue)
                {
                    AddPlayerLifeUp(player, newValue - oldValue, true);
                }
            }
        }

        /// <summary>
        /// Отобразить текст в центре заданного прямоугольника
        /// </summary>
        private void DisplayPointsText(string text, Rectangle boundingBox)
        {
            if (Config.PointsTextShowDuration > 0)
            {
                int x = Left + boundingBox.Left;
                int y = Top + boundingBox.Top;
                int width = boundingBox.Width;
                int height = boundingBox.Height;

                textBlocks.Add(new TextBlock(
                    pointsTextFont, x, y, width, height,
                    Config.TextColor,
                    text));
            }
        }

        /// <summary>
        /// Отрисовать объекты поля
        /// </summary>
        private void DrawGameObjects()
        {
            graphics.BeginDrawGameObjects();
            var orderedList = gameObjects.Where(p => p.IsVisible).OrderBy(p => p.DrawOrder).ToList();

            foreach (var gameObject in orderedList)
            {
                if (gameObject is BattleUnit unit)
                {
                    if (!unit.IsAlive)
                        continue;
                }
                graphics.DrawGameObject(Left, Top, gameObject, gameTime, Config.SubPixelSize);
            }

            graphics.EndDrawGameObjects();
        }

        /// <summary>
        /// Отрисовать показатели игровых очков
        /// </summary>
        private void DrawPlayerScores()
        {
            int x = 0;
            int fontHeight = Convert.ToInt32(gamePanelFont.MeasureString("W").Height * 1.2f);
            int fontWidth = Convert.ToInt32(gamePanelFont.MeasureString("W").Height);
            int maxTextLength = "II - 9999999".Length * fontWidth;
            int paddingTop = Math.Max(4, Config.SubPixelSize / 3);

            for (int i = 0; i < MaxActivePlayers; i++)
            {
                var player = players[i];

                if (player != null)
                {
                    gamePanelFont.DrawString(
                    $"{ConvertToRomanicNumber(i + 1)} - {player.Score}",
                    new Rectangle(
                        Left + x,
                        Top + Height + paddingTop,
                        maxTextLength,
                        fontHeight),
                    DrawStringFormat.Top | DrawStringFormat.Left,
                    Config.TextColor);
                    x += maxTextLength + 3 * fontWidth;
                }
            }
        }

        /// <summary>
        /// Преобразовать число в эквивалент римского числа
        /// </summary>
        /// <returns>эквивалент римского числа</returns>
        private string ConvertToRomanicNumber(int value)
        {
            switch (value)
            {
                case 1: return "I";
                case 2: return "II";
                case 3: return "III";
                case 4: return "IV";
                case 5: return "V";
                case 6: return "VI";
                case 7: return "VII";
                case 8: return "VIII";
                case 9: return "IX";
                case 10: return "X";
                default:
                    return value.ToString();
            }
        }

        /// <summary>
        /// Отрисовать статусную панель
        /// </summary>
        private void DrawStatusPanel()
        {
            if (Config.SubPixelSize <= 0)
                return;

            graphics.BeginDrawGameObjects();

            #region Draw enemy status

            var icon = new GameFieldObject();
            var enemyStatusObj = content.GameObjects.GetByName("enemy_status");

            if (enemyStatusObj == null)
                return;

            icon.CopyFrom(enemyStatusObj);

            // смещение вправо от области поля битвы
            int x = Config.FieldWidth + 2;
            int y = 0;
            int suby = 0;


            // количество иконок в одной колонке (по вертикали)
            const int rowsCount = 10;
            // количество колонок
            int columnNum = (Config.MaxEnemiesPerStage / rowsCount) + (Config.MaxEnemiesPerStage % rowsCount > 0 ? 1 : 0);

            for (int i = 0; i < Math.Min(Config.MaxEnemiesPerStage, enemiesCount); i++)
            {
                int col = (i / rowsCount) % columnNum;
                int row = i % rowsCount;
                icon.X = x + col * icon.Width;
                icon.SubPixelX = col * 2;
                icon.SubPixelY = row * 2;
                icon.Y = y + row * icon.Height;
                graphics.DrawGameObject(Left, Top, icon, gameTime, Config.SubPixelSize);
            }


            #endregion

            #region Draw player status

            y = icon.Height * 11 + Convert.ToInt32((icon.Height * 11 * 2d) / Config.SubPixelSize);
            icon = new GameFieldObject();
            icon.CopyFrom(content.GameObjects.GetByName("player_status"));

            var player = players.FirstOrDefault(p => p != null && p.Id == 1);

            icon.HexColor = player?.Unit?.HexColor ?? Config.TextHexColor;
            icon.X = x;
            icon.Y = y;
            icon.SubPixelY = suby;
            gamePanelFont.DrawString(
                $"1P",
                new Rectangle(
                    Left + icon.X * Config.SubPixelSize,
                    Top + icon.Y * Config.SubPixelSize,
                    (6) * Config.SubPixelSize,
                    (2) * Config.SubPixelSize),
               DrawStringFormat.Top | DrawStringFormat.Left,
               Config.TextColor);

            icon.Y += icon.Height;
            graphics.DrawGameObject(Left, Top, icon, gameTime, Config.SubPixelSize);
            gamePanelFont.DrawString(
                (player?.Lifes ?? 0).ToString(),
                new Rectangle(
                    Left + (icon.X + icon.Width) * Config.SubPixelSize + Config.SubPixelSize / 4,
                    Top + (icon.Y) * Config.SubPixelSize,
                    (6) * Config.SubPixelSize,
                    (icon.Height) * Config.SubPixelSize),
               DrawStringFormat.Top | DrawStringFormat.Left,
               Config.TextColor);

            // player 2 status
            y += icon.Height + 4;

            player = players.FirstOrDefault(p => p != null && p.Id == 2);
            if (player != null)
            {
                icon.HexColor = player?.Unit?.HexColor ?? Config.TextHexColor;
                icon.X = x;
                icon.Y = y;
                icon.SubPixelY = suby;
                gamePanelFont.DrawString(
                    $"2P",
                    new Rectangle(
                        Left + icon.X * Config.SubPixelSize,
                        Top + icon.Y * Config.SubPixelSize,
                        (6) * Config.SubPixelSize,
                        (2) * Config.SubPixelSize),
                   DrawStringFormat.Top | DrawStringFormat.Left,
                   Config.TextColor);

                icon.Y += icon.Height;
                graphics.DrawGameObject(Left, Top, icon, gameTime, Config.SubPixelSize);
                gamePanelFont.DrawString(
                    (player?.Lifes ?? 0).ToString(),
                    new Rectangle(
                        Left + (icon.X + icon.Width) * Config.SubPixelSize + Config.SubPixelSize / 4,
                        Top + (icon.Y) * Config.SubPixelSize,
                        (6) * Config.SubPixelSize,
                        (icon.Height) * Config.SubPixelSize),
                   DrawStringFormat.Top | DrawStringFormat.Left,
                   Config.TextColor);
            }

            #endregion

            #region Draw stage status

            icon = new GameFieldObject();
            icon.CopyFrom(content.GameObjects.GetByName("stage_status"));
            icon.X = x;
            icon.Y = Config.FieldHeight - 2 * icon.Height;
            graphics.DrawGameObject(Left, Top, icon, gameTime, Config.SubPixelSize);
            gamePanelFont.DrawString(
                stageNumber.ToString(),
                new Rectangle(
                    Left + x * Config.SubPixelSize,
                    Top + (icon.Y + icon.Height) * Config.SubPixelSize,
                    (icon.Width + 1) * Config.SubPixelSize,
                    (icon.Height + 1) * Config.SubPixelSize),
               DrawStringFormat.Top | DrawStringFormat.Right,
               Config.TextColor);

            #endregion

            graphics.EndDrawGameObjects();
        }

        /// <summary>
        /// Отрисовать текстовые блоки
        /// </summary>
        private void DrawTextBlocks()
        {
            foreach (var textBlock in textBlocks)
            {
                textBlock.DrawInCenter();
            }
        }

        /// <summary>
        /// Отрисовать оверлеи
        /// </summary>
        private void DrawOverlays()
        {
            if (gamePauseOverlay.IsVisible)
                gamePauseOverlay.Draw();

            if (gameOverOverlay.IsVisible)
                gameOverOverlay.Draw();
        }

        /// <summary>
        /// Выполнить отрисовку
        /// </summary>
        public void Render()
        {
            Update();

            if (IsDisposed)
                return;

            graphics.Clear(Config.BackgroundColor);

            graphics.FillRect(
                Left, Top, Width, Height,
                Config.BattleGroundColor);

            if (Config.ShowChessboard)
                graphics.DrawChessboard(Left, Top, Width, Height,
                    Config.SubPixelSize * 2,
                    ColorConverter.ToInt32(Config.ChessCellHexColor1),
                    ColorConverter.ToInt32(Config.ChessCellHexColor2));

            graphics.SetDefaultRenderStates();
            DrawGameObjects();
            DrawStatusPanel();
            DrawPlayerScores();
            DrawTextBlocks();
            DrawOverlays();
        }

        /// <summary>
        /// Удаление всех используемых объектов, освобождение памяти
        /// </summary>
        public void Dispose()
        {
            if (IsDisposed) return;
            IsDisposed = true;

            if (deviceContext != null)
            {
                deviceContext = null;
            }

            if (gamePauseOverlay != null)
            {
                gamePauseOverlay.Dispose();
                gamePauseOverlay = null;
            }

            if (gameOverOverlay != null)
            {
                gameOverOverlay.Dispose();
                gameOverOverlay = null;
            }

            if (stageCompleteOverlay != null)
            {
                stageCompleteOverlay.Dispose();
                stageCompleteOverlay = null;
            }

            if (gamePanelFont != null)
            {
                gamePanelFont.Dispose();
                gamePanelFont = null;
            }

            if (pointsTextFont != null)
            {
                pointsTextFont.Dispose();
                pointsTextFont = null;
            }

            soundEngine?.StopAll();
            soundEngine = null;
            gameApplication = null;
            graphics = null;
            controllerHub = null;
            content = null;
            players = null;
            battleUnits = null;
            gameObjects = null;
            bullets = null;
            respawnPoints = null;
            animations = null;
            enemyQueue = null;
            powerUpHandlers = null;
            textBlocks = null;
            logger = null;
            constructionHelper = null;
        }

        #endregion

    }
}