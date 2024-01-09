using BattleCity.Common;
using BattleCity.VisualComponents;
using BattleCity.GameObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using BattleCity.Enums;
using BattleCity.Helpers;
using BattleCity.Video;
using BattleCity.Logging;
using BattleCity.Extensions;
using BattleCity.InputControllers;

namespace BattleCity
{
    /// <summary>
    /// Дизайнер (редактор) уровней
    /// </summary>
    public class LevelEditor : IDisposable
    {
        #region events

        /// <summary>
        /// Событие выхода из редактора уровней
        /// </summary>
        public event Action Exit;

        #endregion

        public bool Disposed { get; private set; }

        #region members

        // шрифты
        IGameFont font, smallFont;

        // контекст графического устройства
        IDeviceContext deviceContext;

        // хаб игровых устройств
        IControllerHub controllerHub;

        // сервсис логирования
        ILogger logger;

        // игровой контент
        GameContent content;

        // графика
        IGameGraphics graphics;

        // игровые конфигурации
        GameConfig Config => content?.GameConfig;

        // помощник конструктора объектов
        ConstructionHelper constructionHelper;

        // список созданных объектов на поле
        List<GameFieldObject> gameObjects = new List<GameFieldObject>();

        // список идентификаторов объектов для создания
        List<ConstructionObject> placeholderObjectList = new List<ConstructionObject>();

        // визуальный блок создания объекта
        BlockPlaceholder block = new BlockPlaceholder();

        // текст в рамке
        BorderedTextBlock stringBox;

        // загруженный уровень (stage)
        BattleStage currentStage;

        // игровое время / номер кадра
        int gameTime = 0;

        // размер шаматной клетки
        int chessCellSize = 2;

        // признак отображения настройки вражеских юнитов
        bool showConfigEnemies;

        // признак отображения шахматки
        bool showChessboard = true;

        // признак отображения сетки
        bool showGridLines;

        // текущий индекс объектов для создания
        int currentCreatableObjectIndex = -1;

        // порядковый номер уровня (stage)
        int stageNumber = 0;

        // признак наличия изменений
        bool hasChanges;

        // признак отображения запроса на подтвержение
        bool showConfirmDialog;

        #region enemy editor

        int selectedEnemyBlock;
        int enemyBlockWidth;
        int enemyBlockHeight;
        int vertBlocksCount;
        const int enemyBlocksPerRow = 3;
        bool showEnemyColorAnimation = true;
        const int MAX_UNIT_HEALTH = 4;
        SpawnQueueBattleUnit[] enemies;
        List<GameFieldObject> definedEnemyList = new List<GameFieldObject>();

        #endregion

        readonly int confirmTextColor = Colors.Red;

        // действие, для которого запрашивается подтверждение
        ConfirmActionEnum confirmAction;

        enum ConfirmActionEnum
        {
            Exit,
            NextLevel,
            PrevLevel,
            ClearStage
        }

        #endregion


        #region Constructor

        /// <summary>
        /// Конструктор
        /// </summary>
        public LevelEditor(
            ILogger logger,
            IDeviceContext deviceContext,
            GameContent content,
            IGameGraphics graphics,
            IControllerHub controllerHub)
        {
            this.controllerHub = controllerHub;
            this.logger = logger;
            this.content = content;
            this.deviceContext = deviceContext;
            this.graphics = graphics;
            constructionHelper = new ConstructionHelper(content, logger);
            font = graphics.CreateFont(content.GetFont(content.CommonConfig.LevelEditorFontSize, content.CommonConfig.DefaultFontFileName));
            smallFont = graphics.CreateFont(content.GetFont(Math.Max(7, content.CommonConfig.LevelEditorFontSize / 2), content.CommonConfig.DefaultFontFileName));
        }

        #endregion


        #region methods

        /// <summary>
        /// Подготовить список объектов для использования в конструкторе
        /// </summary>
        private void InitCreatableObjectList()
        {
            // формируем список идентификаторов возможных объектов для создания
            var list = content.GameObjects.GetAll(
                p =>
                // исключаем объекты чистой анимации
                p.Type != GameObjectType.Animation
                // исключаем объекты без типа
                && p.Type != GameObjectType.None
                // исключаем объекты типа снаряд
                && !p.Type.HasFlag(GameObjectType.Projectile)
                // исключаем объекты типа вр. юнит
                && !p.Type.HasFlag(GameObjectType.Enemy)
                // исключаем объекты типа игрок
                && !p.Type.HasFlag(GameObjectType.Player)
                // исключаем объекты прокачки
                && !p.Type.HasFlag(GameObjectType.PowerUp)
                // исключаем объекты типа кораблик
                && !p.Type.HasFlag(GameObjectType.Ship)
                // исключаем объекты типа позиции появления
                && !p.Type.HasFlag(GameObjectType.SpawPosition)
                );

            placeholderObjectList = list
                .Select(s => new ConstructionObject(s, 1, 1))
                .ToList();

            if (placeholderObjectList.Count > 0)
            {
                foreach (var gameObject in list.Where(p => p.Width == p.Height && (p.Width == 1 || p.Width == 2)))
                {
                    placeholderObjectList.Add(new ConstructionObject(gameObject, 2, 1));
                    placeholderObjectList.Add(new ConstructionObject(gameObject, 1, 2));
                    placeholderObjectList.Add(new ConstructionObject(gameObject, 2, 2));

                    if (gameObject.Width == 1)
                    {
                        placeholderObjectList.Add(new ConstructionObject(gameObject, 2, 4));
                        placeholderObjectList.Add(new ConstructionObject(gameObject, 4, 2));
                        placeholderObjectList.Add(new ConstructionObject(gameObject, 4, 4));
                    }
                }

                placeholderObjectList = placeholderObjectList
                    .OrderBy(o => o.BunchBlocksHorizontanlly)
                    .OrderBy(o => o.BunchBlocksVertically)
                    .OrderBy(o => o.Item.Name ?? "")
                    .ToList();
            }

            placeholderObjectList.Add(null);
            currentCreatableObjectIndex = placeholderObjectList.Count - 1;
        }

        /// <summary>
        /// Очистить уровень
        /// </summary>
        private void Reset()
        {
            definedEnemyList = content.GameObjects
                .GetAll(p => p.Type.HasFlag(GameObjectType.Enemy))
                .OrderBy(p => p.Id)
                .Select(p => p.Clone())
                .ToList();

            currentCreatableObjectIndex = placeholderObjectList.Count - 1;
            block.Set(null);
            UpdatePlaceholderPosition();
            ClearAll();
            gameTime = 0;
            hasChanges = false;

            selectedEnemyBlock = 0;
            enemyBlockWidth = Config.SubPixelSize * 16;
            enemyBlockHeight = Config.SubPixelSize * 8;
            vertBlocksCount = deviceContext.DeviceHeight / enemyBlockHeight;
            if (deviceContext.DeviceHeight % enemyBlockHeight > 0)
                vertBlocksCount++;
        }

        /// <summary>
        /// Инициализаци редактора уровней
        /// </summary>
        public void Initialize()
        {
            currentStage = null;
            Reset();
            InitCreatableObjectList();
            LoadStage(1);
            hasChanges = false;
        }

        /// <summary>
        /// Добавить объект поля
        /// </summary>
        /// <param name="creatableObject"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        private void AddGameObject(ConstructionObject creatableObject, int x, int y)
        {
            for (int m = 0; m < creatableObject.BunchBlocksHorizontanlly; m++)
            {
                for (int n = 0; n < creatableObject.BunchBlocksVertically; n++)
                {
                    int itemPosX = x + m * creatableObject.Item.Width;
                    int itemPosY = y + n * creatableObject.Item.Height;

                    constructionHelper.RemoveOverlapsOrContainsObjects(
                        gameObjects,
                        itemPosX,
                        itemPosY,
                        creatableObject.Item.Width,
                        creatableObject.Item.Height);

                    constructionHelper.AddObjectById(gameObjects, creatableObject.Item.Id, itemPosX, itemPosY);
                    hasChanges = true;
                }
            }
        }

        /// <summary>
        /// Удалить объект поля
        /// </summary>
        /// <param name="gameObject"></param>
        private void RemoveGameObject(GameFieldObject gameObject)
        {
            if (gameObjects.Remove(gameObject))
                hasChanges = true;
        }

        /// <summary>
        /// Подготовить объекты поля по умолчанию
        /// </summary>
        private void CreateDefaultLevel()
        {
            constructionHelper.AddTowerObjects(gameObjects);
            constructionHelper.AddBricksAroundTowers(gameObjects);
            hasChanges = true;
        }

        /// <summary>
        /// Создать объект поля
        /// </summary>
        /// <param name="id"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        private GameFieldObject CreateGameObject(int id, int x, int y)
        {
            if (x < 0 || y < 0 || x >= Config.FieldWidth || y >= Config.FieldHeight)
                return null;
            GameFieldObject gameObject = new GameFieldObject();
            var baseItem = content.GameObjects[id];
            if (baseItem == null) return null;
            gameObject.CopyFrom(baseItem);
            gameObject.X = x;
            gameObject.Y = y;
            gameObject.IsVisible = true;

            return gameObject;
        }

        /// <summary>
        /// Загрузить уровень (stage)
        /// </summary>
        /// <param name="stageNumber"></param>
        private void LoadStage(int stageNumber)
        {
            Reset();
            currentStage = null;

            this.stageNumber = stageNumber;

            if (stageNumber > content.Stages.Capacity)
            {
                logger?.WriteLine($"{nameof(LoadStage)} failed: wrong stage number {stageNumber}", LogLevel.Warning);
                return;
            }

            var stage = content.Stages[stageNumber - 1];

            if (stage == null || stage.FieldObjects == null || stage.FieldObjects.Count == 0)
            {
                currentStage = content.Stages[stageNumber - 1] = new BattleStage();
                CreateDefaultLevel();
            }
            else
            {
                currentStage = stage;
                stage.FieldObjects.ForEach(fieldObj =>
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
                });
            }

            // Времено для переноса списка юнитов
            if (currentStage.Enemies == null || currentStage.Enemies.Count == 0)
                currentStage.Enemies = CreateInitialEnemyQueue();

            if (currentStage.Enemies != null && currentStage.Enemies.Count > 0)
            {
                for (int i = 0; i < Math.Min(enemies.Length, currentStage.Enemies.Count); i++)
                {
                    enemies[i] = currentStage.Enemies[i]?.Clone();
                }
            }

            if (definedEnemyList != null && definedEnemyList.Count > 0)
            {
                for (int i = 0; i < enemies.Length; i++)
                {
                    if (enemies[i] == null)
                        enemies[i] = CreateDefaultEnemy();
                }
            }

            hasChanges = false;
        }

        private List<SpawnQueueBattleUnit> CreateInitialEnemyQueue()
        {
            var allEnemyPresets = content.GameObjects
                            .GetAll(p => p.Type.HasFlag(GameObjectType.Enemy))
                            .ToList();

            List<SpawnQueueBattleUnit> list = new List<SpawnQueueBattleUnit>();

            // создаём очередь вражеских юнитов, которые будут появляться на поле
            for (int i = 0; i < Config.MaxEnemy; i++)
            {
                int enemyNameIndex = Config.Random.Next(0, allEnemyPresets.Count);
                var unit = allEnemyPresets[enemyNameIndex];

                var enemy = new SpawnQueueBattleUnit()
                {
                    Name = allEnemyPresets[enemyNameIndex].Name,
                    Health = 1,
                    ExtraBonus = 0
                };

                if (Config.MaxBonusedUnitsOnField == -1 || Config.MaxBonusedUnitsOnField > gameObjects.Count(p => p is EnemyUnit bn && bn.ExtraBonus > 0))
                {
                    // определяем шанс появления бонусного юнита
                    var chanceBonusPowerUp = 100 - Math.Min(100, Math.Max(0, Config.BonusedEnemySpawnChance));
                    if (Config.Random.Next(0, 101) >= chanceBonusPowerUp)
                    {
                        enemy.ExtraBonus = Config.Random.Next(1, Config.MaxExtraBonusPerUnit + 1);
                    }
                }

                // начиная с 4го уровня добавляем сложность
                const int addDifficultFromStageNumber = 4;
                if (stageNumber > addDifficultFromStageNumber && unit.FlashHexColors != null && unit.FlashHexColors.Length > 1)
                {
                    int d = (stageNumber + 1) - addDifficultFromStageNumber;
                    if (Config.Random.Next(0, 101) < d * 10)
                        enemy.Health = Config.Random.Next(enemy.ExtraBonus > 0 ? 0 : 1, unit.FlashHexColors.Length + 1);
                }

                list.Add(enemy);
            }

            return list;
        }

        /// <summary>
        /// Сохранить уровень (stage)
        /// </summary>
        private void SaveStage()
        {
            if (stageNumber == 0 || currentStage == null)
                return;

            currentStage.FieldObjects = gameObjects.Select(s => s.ToBaseGameObject()).ToList();
            if (definedEnemyList == null || definedEnemyList.Count == 0)
            {
                currentStage.Enemies = new List<SpawnQueueBattleUnit>();
                logger?.WriteLine($"{nameof(SaveStage)}: no enemies defined", LogLevel.Warning);
            }
            else
            {
                var stageEnemies = new SpawnQueueBattleUnit[Config.MaxEnemy];
                for (int i = 0; i < stageEnemies.Length; i++)
                {
                    stageEnemies[i] = enemies[i] == null ? CreateDefaultEnemy() : enemies[i].Clone();
                }
                currentStage.Enemies = new List<SpawnQueueBattleUnit>(stageEnemies);
            }

            // выставим нумерацию по порядку
            int n = 1;

            foreach (var stage in content.Stages)
            {
                if (stage != null)
                    stage.Id = n;
                n++;
            }

            content.Stages.Save();
            DrawText("STAGE SAVED");
        }

        private SpawnQueueBattleUnit CreateDefaultEnemy()
        {
            return new SpawnQueueBattleUnit(definedEnemyList[0].Name, 0, 1);
        }

        private void DrawText(string text)
        {
            int maxTextLength = 20;
            var boxSize = font.MeasureString(new string('W', maxTextLength));
            var boxHeight = (int)(boxSize.Height * 1.2f);
            var boxWidth = boxSize.Width + 8;

            if (text.Length < maxTextLength)
                text = new string(' ', (maxTextLength - text.Length) / 2) + text;
            stringBox = new BorderedTextBlock(graphics, font, 100, 100, boxWidth, boxHeight, Colors.White, text)
            {
                MarginLeft = 4,
                MarginTop = (boxHeight - boxSize.Height) / 2
            };
        }

        /// <summary>
        /// Удалить все объекты с поля
        /// </summary>
        private void ClearAll()
        {
            gameObjects = new List<GameFieldObject>();
            enemies = new SpawnQueueBattleUnit[Config.MaxEnemy];
            for (int i = 0; i < enemies.Length; i++)
            {
                enemies[i] = CreateDefaultEnemy();
            }
            hasChanges = true;
        }

        /// <summary>
        /// Обработать нажатие клавиш
        /// </summary>
        private void ProcessInputKeys()
        {
            if (showConfirmDialog)
            {
                if (controllerHub.Keyboard.IsDown(KeyboardKey.Enter) || controllerHub.IsKeyPressed(1, ButtonNames.CreateObject, true))
                {
                    showConfirmDialog = false;
                    hasChanges = false;
                    HandleAction(confirmAction);
                }
                else if (controllerHub.Keyboard.IsDown(KeyboardKey.Escape) || controllerHub.IsKeyPressed(1, ButtonNames.Cancel, true))
                {
                    showConfirmDialog = false;
                }
                return;
            }

            if (showConfigEnemies)
                ProcessEnemyConfigInputKeys();
            else
                ProcessStageInputKeys();
        }

        private void ProcessStageInputKeys()
        {
            bool shiftKeyPressed =
                controllerHub.Keyboard.IsPressed(KeyboardKey.LeftShift) ||
                controllerHub.Keyboard.IsPressed(KeyboardKey.RightShift);

            int blockMoveOffset = new int[] { 2, block.Height, block.Width }.Min();
            if (shiftKeyPressed) blockMoveOffset *= 2;

            if (controllerHub.IsKeyPressed(1, ButtonNames.Up, true) || controllerHub.IsLongPressed(1, ButtonNames.Up))
            {
                block.Y = Math.Max(0, block.Y - blockMoveOffset);
                UpdatePlaceholderPosition();
            }
            else if (controllerHub.IsKeyPressed(1, ButtonNames.Down, true) || controllerHub.IsLongPressed(1, ButtonNames.Down))
            {
                block.Y = Math.Min(
                    Config.FieldHeight - blockMoveOffset,
                    //block.Y + Math.Max(1, block.Height * block.BunchBlocksVertically));
                    block.Y + blockMoveOffset);
                UpdatePlaceholderPosition();
            }
            else if (controllerHub.IsKeyPressed(1, ButtonNames.Left, true) || controllerHub.IsLongPressed(1, ButtonNames.Left))
            {
                //block.X = Math.Max(0, block.X - Math.Max(1, block.Width * block.BunchBlocksHorizontanlly));
                block.X = Math.Max(0, block.X - blockMoveOffset);
                UpdatePlaceholderPosition();
            }
            else if (controllerHub.IsKeyPressed(1, ButtonNames.Right, true) || controllerHub.IsLongPressed(1, ButtonNames.Right))
            {
                block.X = Math.Min(
                    Config.FieldWidth - blockMoveOffset,
                    //block.X + Math.Max(1, block.Width * block.BunchBlocksHorizontanlly));
                    block.X + blockMoveOffset);
                UpdatePlaceholderPosition();
            }
            else if (controllerHub.Keyboard.IsDown(KeyboardKey.F1) || controllerHub.IsKeyPressed(1, ButtonNames.LoadPrevLevel, true))
            {
                if (stageNumber <= 1)
                    return;
                HandleAction(ConfirmActionEnum.PrevLevel);
            }
            else if (controllerHub.Keyboard.IsDown(KeyboardKey.F2) || controllerHub.IsKeyPressed(1, ButtonNames.LoadNextLevel, true))
            {
                if (stageNumber == 0 || stageNumber >= content.Stages.Capacity)
                    return;
                HandleAction(ConfirmActionEnum.NextLevel);
            }

            else if (controllerHub.Keyboard.IsDown(KeyboardKey.F9))
            {
                showConfirmDialog = true;
                confirmAction = ConfirmActionEnum.ClearStage;
            }
            else if (controllerHub.Keyboard.IsDown(KeyboardKey.F4))
            {
                SaveStage();
                hasChanges = false;
            }

            else if (controllerHub.Keyboard.IsDown(KeyboardKey.F5))
            {
                ChangeChessCellSize();
            }
            else if (controllerHub.Keyboard.IsDown(KeyboardKey.F6))
            {
                currentCreatableObjectIndex = (currentCreatableObjectIndex + 1) % placeholderObjectList.Count;
                var creatableObject = placeholderObjectList[currentCreatableObjectIndex];
                block.Set(creatableObject);
                UpdatePlaceholderPosition();
            }

            else if (controllerHub.Keyboard.IsDown(KeyboardKey.F7))
            {
                currentCreatableObjectIndex = currentCreatableObjectIndex == 0 ? placeholderObjectList.Count - 1 : currentCreatableObjectIndex - 1;
                var creatableObject = placeholderObjectList[currentCreatableObjectIndex];
                block.Set(creatableObject);
                UpdatePlaceholderPosition();
            }

            else if (controllerHub.Keyboard.IsDown(KeyboardKey.F8))
            {
                showChessboard = !showChessboard;
            }

            else if (controllerHub.Keyboard.IsDown(KeyboardKey.G))
            {
                showGridLines = !showGridLines;
            }

            else if (controllerHub.Keyboard.IsDown(KeyboardKey.F11))
            {
                if (definedEnemyList != null && definedEnemyList.Count > 0)
                    showConfigEnemies = true;
            }
            else if (controllerHub.Keyboard.IsDown(KeyboardKey.F12))
            {
                HandleAction(ConfirmActionEnum.Exit);
            }
            else if (controllerHub.Keyboard.IsDown(KeyboardKey.Escape) || controllerHub.IsKeyPressed(1, ButtonNames.Cancel, true))
            {
                block.Set(null);
            }
            else if (controllerHub.Keyboard.IsDown(KeyboardKey.Space) || controllerHub.IsKeyPressed(1, ButtonNames.Attack, true))
            {
                if (block.CreatableObject != null)
                {
                    AddGameObject(block.CreatableObject, block.X, block.Y);
                }
            }
            else if (controllerHub.Keyboard.IsDown(KeyboardKey.LeftControl) || controllerHub.Keyboard.IsDown(KeyboardKey.RightControl))
            {
                var aabb = AABB.Create(block.X, block.Y, block.Width * block.BunchBlocksHorizontanlly, block.Height * block.BunchBlocksVertically);
                var gameObject = gameObjects.Where(p => aabb.OverlapsOrContains(p.GetAABB(1))).OrderBy(p => p.X).ThenBy(p => p.Y).FirstOrDefault();

                if (gameObject != null)
                {
                    var creatableObject = placeholderObjectList.FirstOrDefault(p => p.Item.Id == gameObject.Id);
                    if (creatableObject != null)
                    {
                        block.Set(creatableObject);
                    }
                }
            }

            else if (controllerHub.Keyboard.IsDown(KeyboardKey.Delete))
            {
                constructionHelper.RemoveOverlapsOrContainsObjects(
                        gameObjects,
                        block.X,
                        block.Y,
                        block.Width * block.BunchBlocksHorizontanlly,
                        block.Height * block.BunchBlocksVertically);
            }
        }

        private void ProcessEnemyConfigInputKeys()
        {
            if (controllerHub.IsKeyPressed(1, ButtonNames.Up, true) || controllerHub.IsLongPressed(1, ButtonNames.Up) ||
                controllerHub.Keyboard.IsDown(KeyboardKey.UpArrow) || controllerHub.Keyboard.IsLongPress(KeyboardKey.UpArrow))
            {
                selectedEnemyBlock -= enemyBlocksPerRow;
                selectedEnemyBlock = Math.Max(0, selectedEnemyBlock);
            }
            else if (controllerHub.IsKeyPressed(1, ButtonNames.Down, true) || controllerHub.IsLongPressed(1, ButtonNames.Down) ||
                controllerHub.Keyboard.IsDown(KeyboardKey.DownArrow) || controllerHub.Keyboard.IsLongPress(KeyboardKey.DownArrow))
            {
                selectedEnemyBlock += enemyBlocksPerRow;
                selectedEnemyBlock = Math.Min(selectedEnemyBlock, Config.MaxEnemy - 1);
            }
            else if (controllerHub.IsKeyPressed(1, ButtonNames.Left, true) || controllerHub.IsLongPressed(1, ButtonNames.Left) ||
                controllerHub.Keyboard.IsDown(KeyboardKey.LeftArrow) || controllerHub.Keyboard.IsLongPress(KeyboardKey.LeftArrow))
            {
                selectedEnemyBlock--;
                selectedEnemyBlock = Math.Max(0, selectedEnemyBlock);
            }
            else if (controllerHub.IsKeyPressed(1, ButtonNames.Right, true) || controllerHub.IsLongPressed(1, ButtonNames.Right) ||
                controllerHub.Keyboard.IsDown(KeyboardKey.RightArrow) || controllerHub.Keyboard.IsLongPress(KeyboardKey.RightArrow))
            {
                selectedEnemyBlock++;
                selectedEnemyBlock = Math.Min(selectedEnemyBlock, Config.MaxEnemy - 1);
            }
            else if (controllerHub.Keyboard.IsDown(KeyboardKey.F1))
            {
                if (enemies[selectedEnemyBlock] == null)
                    return;
                enemies[selectedEnemyBlock].Health = Math.Max(enemies[selectedEnemyBlock].ExtraBonus > 0 ? 0 : 1, enemies[selectedEnemyBlock].Health - 1);
                hasChanges = true;
            }
            else if (controllerHub.Keyboard.IsDown(KeyboardKey.F2))
            {
                if (enemies[selectedEnemyBlock] == null)
                    return;
                enemies[selectedEnemyBlock].Health = Math.Min(MAX_UNIT_HEALTH, enemies[selectedEnemyBlock].Health + 1);
                hasChanges = true;
            }
            else if (controllerHub.Keyboard.IsDown(KeyboardKey.F3))
            {
                if (enemies[selectedEnemyBlock] == null || enemies[selectedEnemyBlock].ExtraBonus == 0)
                    return;
                if (enemies[selectedEnemyBlock].ExtraBonus == 1)
                {
                    enemies[selectedEnemyBlock].ExtraBonus = 0;
                    enemies[selectedEnemyBlock].Health = Math.Max(1, enemies[selectedEnemyBlock].Health);
                }
                else
                {
                    enemies[selectedEnemyBlock].ExtraBonus--;
                }
                hasChanges = true;
            }
            else if (controllerHub.Keyboard.IsDown(KeyboardKey.F4))
            {
                if (enemies[selectedEnemyBlock] == null)
                    return;

                enemies[selectedEnemyBlock].ExtraBonus = Math.Min(Config.MaxExtraBonusPerUnit, enemies[selectedEnemyBlock].ExtraBonus + 1);
                hasChanges = true;
            }
            else if (controllerHub.Keyboard.IsDown(KeyboardKey.F5))
            {
                showEnemyColorAnimation = !showEnemyColorAnimation;
            }
            else if (controllerHub.Keyboard.IsDown(KeyboardKey.F6))
            {
                if (enemies[selectedEnemyBlock] == null)
                    enemies[selectedEnemyBlock] = CreateDefaultEnemy();
                else
                {
                    int index = definedEnemyList.FindIndex(p => p.Name == enemies[selectedEnemyBlock].Name);
                    if (index == -1)
                        index = 0;
                    else if (index == 0)
                        index = definedEnemyList.Count - 1;
                    else
                        index--;
                    enemies[selectedEnemyBlock] = new SpawnQueueBattleUnit(definedEnemyList[index].Name, 0, 1);
                }
                hasChanges = true;
            }
            else if (controllerHub.Keyboard.IsDown(KeyboardKey.F7))
            {
                if (enemies[selectedEnemyBlock] == null)
                    enemies[selectedEnemyBlock] = CreateDefaultEnemy();
                else
                {
                    int index = definedEnemyList.FindIndex(p => p.Name == enemies[selectedEnemyBlock].Name) + 1;
                    if (index >= definedEnemyList.Count)
                        index = 0;
                    enemies[selectedEnemyBlock] = new SpawnQueueBattleUnit(definedEnemyList[index].Name, 0, 1);
                }
                hasChanges = true;
            }
            else if (controllerHub.Keyboard.IsDown(KeyboardKey.F11))
            {
                showConfigEnemies = false;
            }
            else if (controllerHub.Keyboard.IsDown(KeyboardKey.D1))
            {
                var unit = definedEnemyList.FirstOrDefault(p => p.Name?.ToUpper() == "BASIC_TANK");
                if (unit != null)
                    enemies[selectedEnemyBlock] = new SpawnQueueBattleUnit(unit.Name, 0, 1);
            }
            else if (controllerHub.Keyboard.IsDown(KeyboardKey.D2))
            {
                var unit = definedEnemyList.FirstOrDefault(p => p.Name?.ToUpper() == "POWER_TANK");
                if (unit != null)
                    enemies[selectedEnemyBlock] = new SpawnQueueBattleUnit(unit.Name, 0, 1);
            }
            else if (controllerHub.Keyboard.IsDown(KeyboardKey.D3))
            {
                var unit = definedEnemyList.FirstOrDefault(p => p.Name?.ToUpper() == "FAST_TANK");
                if (unit != null)
                    enemies[selectedEnemyBlock] = new SpawnQueueBattleUnit(unit.Name, 0, 1);
            }
            else if (controllerHub.Keyboard.IsDown(KeyboardKey.D4))
            {
                var unit = definedEnemyList.FirstOrDefault(p => p.Name?.ToUpper() == "HEAVY_TANK");
                if (unit != null)
                    enemies[selectedEnemyBlock] = new SpawnQueueBattleUnit(unit.Name, 0, MAX_UNIT_HEALTH);
            }
            else if (controllerHub.Keyboard.IsDown(KeyboardKey.B))
            {
                if (enemies[selectedEnemyBlock] == null)
                    return;
                if (enemies[selectedEnemyBlock].ExtraBonus > 0)
                {
                    enemies[selectedEnemyBlock].ExtraBonus = 0;
                    enemies[selectedEnemyBlock].Health = Math.Max(1, enemies[selectedEnemyBlock].Health);
                }
                else
                {
                    enemies[selectedEnemyBlock].ExtraBonus = 1;
                    if (enemies[selectedEnemyBlock].Health == 1)
                        enemies[selectedEnemyBlock].Health = 0;
                }
            }
        }

        private void HandleAction(ConfirmActionEnum action)
        {
            switch (action)
            {
                case ConfirmActionEnum.Exit:
                    {
                        if (hasChanges)
                        {
                            showConfirmDialog = true;
                            confirmAction = action;
                        }
                        else
                            Exit?.Invoke();
                        break;
                    }
                case ConfirmActionEnum.PrevLevel:
                    {
                        if (hasChanges)
                        {
                            showConfirmDialog = true;
                            confirmAction = action;
                        }
                        else
                            LoadStage(stageNumber - 1);
                        break;
                    }
                case ConfirmActionEnum.NextLevel:
                    {
                        if (hasChanges)
                        {
                            showConfirmDialog = true;
                            confirmAction = action;
                        }
                        else
                            LoadStage(stageNumber + 1);
                        break;
                    }
                case ConfirmActionEnum.ClearStage:
                    {
                        ClearAll();
                        CreateDefaultLevel();
                        break;
                    }
            }
        }

        /// <summary>
        /// Обновить позицию блока создания объектов поля
        /// </summary>
        private void UpdatePlaceholderPosition()
        {
            block.X = Math.Min(
                Math.Max(0, block.X),
                Config.FieldWidth - block.Width * block.BunchBlocksHorizontanlly);

            block.Y = Math.Min(
                Math.Max(0, block.Y),
                Config.FieldHeight - block.Height * block.BunchBlocksVertically);
        }

        /// <summary>
        /// Отрисовать объекты поля
        /// </summary>
        private void DrawStageObjects(int left, int top)
        {
            graphics.BeginDrawGameObjects();

            foreach (var gameObject in gameObjects.OrderBy(p => p.DrawOrder))
            {
                if (!gameObject.IsVisible) continue;
                graphics.DrawGameObject(left, top, gameObject, gameTime, Config.SubPixelSize);
            }

            graphics.EndDrawGameObjects();
        }

        /// <summary>
        /// Отрисовать текстовые подсказки
        /// </summary>
        private void DrawStageHints(int left, int top)
        {
            var x = left + 8 + (Config.SubPixelSize * Config.FieldWidth);
            var y = top + 8;
            var h = (int)(font.MeasureString("L").Height * 1.2f);
            var textColor = Config.TextColor;

            if (stageNumber > 0)
                font.DrawString($"STAGE: {stageNumber}", x, y, textColor);
            else
                font.DrawString($"NEW STAGE", x, y, textColor);

            y += h * 2;

            font.DrawString("F1  PREV STAGE", x, y, textColor); y += h;
            font.DrawString("F2  NEXT STAGE", x, y, textColor); y += h;

            font.DrawString("F4  SAVE STAGE", x, y, textColor);
            y += h * 2;

            if (showGridLines)
                font.DrawString(" G  HIDE GRID", x, y, textColor);
            else
                font.DrawString(" G  SHOW GRID", x, y, textColor);
            y += h;

            font.DrawString("F5  GRID SIZE " + chessCellSize, x, y, textColor); y += h;
            font.DrawString("F6  NEXT TYPE", x, y, textColor); y += h;
            font.DrawString("F7  PREV TYPE", x, y, textColor); y += h;
            if (showChessboard)
                font.DrawString("F8  HIDE CHESS", x, y, textColor);
            else
                font.DrawString("F8  SHOW CHESS", x, y, textColor);
            y += h * 2;

            font.DrawString("F9  CLEAR STAGE", x, y, textColor); y += h;

            font.DrawString("F11 STAGE ENEMIES", x, y, textColor); y += h;

            font.DrawString("F12 EXIT", x, y, textColor); y += 2 * h;

            font.DrawString("CTRL  PICK OBJ", x, y, textColor); y += h;
            font.DrawString("SPACE ADD OBJ", x, y, textColor); y += h;
            font.DrawString("DEL   REMOVE OBJ", x, y, textColor); y += 2 * h;

            font.DrawString($"MAP SIZE: {Config.FieldWidth}x{Config.FieldHeight}", x, y, textColor); y += h;
            font.DrawString($"CELL SIZE:{Config.SubPixelSize}", x, y, textColor); y += 2 * h;
            font.DrawString("CURRENT BLOCK", x, y, textColor); y += h;
            font.DrawString($"{block.Name}", x, y, textColor); y += h;
            font.DrawString($"POSITION:{block.X}x{block.Y}", x, y, textColor);
        }

        /// <summary>
        /// Изменить размер шахматной клетки
        /// </summary>
        private void ChangeChessCellSize()
        {
            switch (chessCellSize)
            {
                case 1:
                    chessCellSize = 2;
                    break;
                case 2:
                    chessCellSize = 4;
                    break;
                default:
                    chessCellSize = 1;
                    break;
            }
        }

        /// <summary>
        /// Отрисовка
        /// </summary>
        public void Render()
        {
            ProcessInputKeys();

            if (Disposed)
                return;

            gameTime++;
            graphics.SetDefaultRenderStates();

            if (showConfigEnemies)
                DrawEnemyConfig();
            else
                DrawStage();

            if (showConfirmDialog)
                DrawConfirmDialog();

            if (stringBox != null)
            {
                stringBox.FrameNumber++;
                if (stringBox.FrameNumber < 120)
                    stringBox.Draw();
            }
        }

        private void DrawStage()
        {
            graphics.Clear(Config.BackgroundColor);

            int left = 2 * Config.SubPixelSize;
            int top = 4 * Config.SubPixelSize;
            int width = Config.FieldWidth * Config.SubPixelSize;
            int height = Config.FieldHeight * Config.SubPixelSize;

            graphics.FillRect(
                left, top, width, height,
                Config.BattleGroundColor);

            if (showChessboard)
                graphics.DrawChessboard(left, top, width, height,
                    Config.SubPixelSize * chessCellSize,
                    ColorConverter.ToInt32(Config.ChessCellHexColor1),
                    ColorConverter.ToInt32(Config.ChessCellHexColor2));

            DrawStageObjects(left, top);

            if (showGridLines)
                graphics.DrawGridLines(left, top, width, height,
                    Config.SubPixelSize * chessCellSize * 2,
                    Colors.White);

            block.Draw(graphics, left, top, Config.SubPixelSize, gameTime, Config.PlaceholderFlickerFrames);
            DrawStageHints(left, top);
        }

        private void DrawEnemyConfig()
        {
            graphics.Clear(Config.BattleGroundColor);

            int left = 2 * Config.SubPixelSize;
            int top = 2 * Config.SubPixelSize;

            int startIndex = selectedEnemyBlock / (vertBlocksCount * enemyBlocksPerRow);
            startIndex *= vertBlocksCount * enemyBlocksPerRow;

            List<EnemyDrawBlock> enemyDrawBlocks = new List<EnemyDrawBlock>();
            int enemyDrawBlockPadding = Math.Max(2, enemyBlockHeight / 8);

            List<TextBlock> enemyStatTextBlocks = new List<TextBlock>();

            for (int i = startIndex; i < Config.MaxEnemy; i++)
            {
                int rowIndex = (i - startIndex) / enemyBlocksPerRow;
                int columnIndex = i % enemyBlocksPerRow;

                int x = columnIndex * enemyBlockWidth;
                int y = rowIndex * enemyBlockHeight;
                var borderColor = i == selectedEnemyBlock ? Colors.Tomato : Colors.DimBlack;

                if (i == selectedEnemyBlock)
                {
                    graphics.FillRect(left + x, top + y, enemyBlockWidth, enemyBlockHeight, Colors.DarkDimBlack);
                }

                graphics.DrawBorderRect(left + x, top + y, enemyBlockHeight, enemyBlockHeight, borderColor);
                graphics.DrawBorderRect(left + x, top + y, enemyBlockWidth, enemyBlockHeight, borderColor);

                var enemyBlockObj = i >= enemies.Length ? null : enemies[i];

                var textColor = enemyBlockObj == null ? Colors.Gray : Colors.Green;
                font.DrawString(
                    (i + 1).ToString(),
                    left + x, top + y, enemyBlockHeight, enemyBlockHeight,
                    DrawStringFormat.VerticalCenter | DrawStringFormat.Center, textColor);

                if (enemyBlockObj != null)
                {
                    if (enemyBlockObj.ExtraBonus > 0)
                    {
                        enemyStatTextBlocks.Add(new TextBlock(
                            smallFont,
                            left + x + enemyBlockHeight + 2,
                            top + y + 2,
                            enemyBlockHeight - 4,
                            enemyBlockHeight - 4,
                            Colors.DodgerBlue,
                            "B:" + enemyBlockObj.ExtraBonus.ToString())
                        { Tag = 1 });
                    }

                    enemyStatTextBlocks.Add(new TextBlock(
                        smallFont,
                        left + x + enemyBlockHeight + 2,
                        top + y + 2,
                        enemyBlockHeight - 4,
                        enemyBlockHeight - 4,
                        Colors.DarkOliveGreen,
                        "H:" + enemyBlockObj.Health.ToString())
                    { Tag = 0 });

                    var gameObject = definedEnemyList.FirstOrDefault(p => p.Name == enemyBlockObj.Name);
                    if (gameObject != null)
                    {
                        gameObject.Width = enemyBlockHeight - 2 * enemyDrawBlockPadding;
                        gameObject.Height = enemyBlockHeight - 2 * enemyDrawBlockPadding;

                        enemyDrawBlocks.Add(new EnemyDrawBlock()
                        {
                            DrawObject = gameObject,
                            HexColor = GetEnemyHexColor(gameObject, enemyBlockObj, "#FFFFFF"),
                            X = left + x + enemyBlockHeight + enemyDrawBlockPadding,
                            Y = top + y + 2 * enemyDrawBlockPadding,
                        });
                    }
                }
            }

            graphics.BeginDrawGameObjects();
            foreach (var enemyDrawBlock in enemyDrawBlocks)
            {
                enemyDrawBlock.DrawObject.HexColor = enemyDrawBlock.HexColor;
                graphics.DrawGameObject(enemyDrawBlock.X, enemyDrawBlock.Y, enemyDrawBlock.DrawObject, 0, 1);
            }
            graphics.EndDrawGameObjects();

            foreach (var textBlock in enemyStatTextBlocks)
            {
                if ((int)textBlock.Tag == 1)
                    textBlock.Draw(DrawStringFormat.Right | DrawStringFormat.Top);
                else
                    textBlock.Draw(DrawStringFormat.Left | DrawStringFormat.Top);
            }

            DrawEnemyConfigHints(left, top);
        }

        private string GetEnemyHexColor(GameFieldObject gameObject, SpawnQueueBattleUnit unit, string hexColorOriginal)
        {
            if (unit.ExtraBonus > 0)
            {
                if (!showEnemyColorAnimation)
                    return Config?.EnemyFlashHexColor ?? hexColorOriginal;

                if (Config.PowerUpBonusFlashColorDuration > 0)
                {
                    if (gameObject.FlashHexColors == null || gameObject.FlashHexColors.Length == 0)
                    {
                        return gameTime % Config.PowerUpBonusFlashColorDuration == 0
                            ? hexColorOriginal
                            : Config?.EnemyFlashHexColor ?? hexColorOriginal;
                    }
                    else
                    {
                        return (gameTime % (Config.PowerUpBonusFlashColorDuration * 2)) >= Config.PowerUpBonusFlashColorDuration
                            ? gameObject.FlashHexColors[0]
                            : (Config?.EnemyFlashHexColor ?? hexColorOriginal);
                    }
                }
            }
            else
            {
                if (!showEnemyColorAnimation)
                    return hexColorOriginal;

                if (Config.UnitFlashColorDuration > 0 && gameObject.FlashHexColors != null && gameObject.FlashHexColors.Length > 1)
                {
                    int n = gameTime % (Config.UnitFlashColorDuration * gameObject.FlashHexColors.Length);
                    n %= gameObject.FlashHexColors.Length;
                    int flashColorIndex = Math.Min(Math.Max(0, unit.Health - 1), n);

                    if (Config.UnitFlashColorDuration < 2 || gameTime % Config.UnitFlashColorDuration == 0)
                        return gameObject.FlashHexColors[flashColorIndex];
                    return hexColorOriginal;
                }
            }

            return hexColorOriginal;
        }

        private void DrawEnemyConfigHints(int left, int top)
        {
            var x = left + 8 + (Config.SubPixelSize * Config.FieldWidth);
            var y = top + 8;
            var h = (int)(font.MeasureString("L").Height * 1.2f);
            var textColor = Config.TextColor;

            if (stageNumber > 0)
                font.DrawString($"STAGE: {stageNumber}", x, y, textColor);
            else
                font.DrawString($"NEW STAGE", x, y, textColor);

            y += h * 2;

            font.DrawString("F1  DECREASE HP", x, y, textColor); y += h;
            font.DrawString("F2  INCREASE HP", x, y, textColor); y += h;
            y += h;
            font.DrawString("F3  DECREASE BONUS", x, y, textColor); y += h;
            font.DrawString("F4  INCREASE BONUS", x, y, textColor); y += h;
            y += h;
            font.DrawString("F5  ANIMATION", x, y, textColor); y += h;
            y += h;
            font.DrawString("F6  NEXT TYPE", x, y, textColor); y += h;
            font.DrawString("F7  PREV TYPE", x, y, textColor); y += h;
            y += h;
            font.DrawString("F11 STAGE EDITOR", x, y, textColor);
            y += h;
            y += h;
            for (int i = 0; i < definedEnemyList.Count; i++)
            {
                string count = enemies.Count(p => p.Name == definedEnemyList[i].Name).ToString("00");
                font.DrawString($"{count} x {definedEnemyList[i].Name}", x, y, textColor);
                y += h;
            }
        }

        /// <summary>
        /// Отрисовать диалог подтверждения
        /// </summary>
        private void DrawConfirmDialog()
        {
            string text;

            switch (confirmAction)
            {
                case ConfirmActionEnum.ClearStage:
                    text =
                    "CLEAR STAGE ?" + Environment.NewLine + Environment.NewLine +
                    "PRESS ENTER TO CONTINUE" + Environment.NewLine + Environment.NewLine +
                    "OR PRESS ESC TO CANCEL";
                    break;
                default:
                    text =
                    "YOU HAVE UNSAVED CHANGES" + Environment.NewLine + Environment.NewLine +
                    "PRESS ENTER TO IGNORE CHANGES" + Environment.NewLine + Environment.NewLine +
                    "OR PRESS ESC TO CANCEL";
                    break;
            }

            var boxSize = font.MeasureString(new string('W', 31));
            var boxHeight = (int)(boxSize.Height * 1.2f);
            var boxWidth = boxSize.Width + 8;

            var stringBox = new BorderedTextBlock(graphics, font, 100, 100, boxWidth, boxHeight * 7, confirmTextColor, text)
            {
                MarginLeft = 4,
                MarginTop = 4
            };

            stringBox.Draw(DrawStringFormat.Center | DrawStringFormat.VerticalCenter);
        }

        /// <summary>
        /// Удаление всех используемых объектов, освобождение памяти
        /// </summary>
        public void Dispose()
        {
            stringBox = null;
            gameObjects = null;
            enemies = null;
            content = null;
            block = null;
            graphics = null;
            controllerHub = null;
            logger = null;
            constructionHelper = null;
            deviceContext = null;

            if (font != null)
            {
                font.Dispose();
                font = null;
            }

            if (smallFont != null)
            {
                smallFont.Dispose();
                smallFont = null;
            }

            Disposed = true;
        }

        #endregion

    }
}