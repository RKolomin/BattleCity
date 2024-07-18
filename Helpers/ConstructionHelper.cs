using BattleCity.Common;
using BattleCity.Enums;
using BattleCity.Extensions;
using BattleCity.GameObjects;
using BattleCity.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BattleCity.Helpers
{
    /// <summary>
    /// Вспомогательный класс конструктора объектов
    /// </summary>
    public class ConstructionHelper : IDisposable
    {
        #region members

        public int DefaultBlockSize { get; } = 2;

        private const string BrickObjName = "brick";
        private const string IronObjName = "iron";
        private const string IceObjName = "ice";
        private const string WaterObjName = "water";
        private const string ForestObjName = "forest";

        private ILogger logger;
        private GameContent content;
        private GameConfig Config { get; }

        #endregion


        #region Constructor

        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="content"></param>
        /// <param name="logger"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public ConstructionHelper(GameContent content, ILogger logger)
        {
            this.content = content ?? throw new ArgumentNullException("content");

            Config = content.GameConfig;
            if (Config == null)
                throw new ArgumentNullException("config");

            this.logger = logger;
        }

        #endregion


        #region methods

        /// <summary>
        /// Удалить блоки вокруг стратегических объектов
        /// </summary>
        /// <param name="gameObjects">Коллекция хранимых объектов</param>
        public void RemoveBlocksAroundTowers(List<GameFieldObject> gameObjects)
        {
            // находим стратегические объекты на поле
            var strategicObjectList = gameObjects
                .Where(x => x.IsVisible && x.Type.HasFlag(GameObjectType.Tower))
                .ToList();

            if (strategicObjectList.Count == 0)
            {
                // список пуст, дальнейшие действия не требуются
                return;
            }

            int blockWidth = DefaultBlockSize;
            int blockHeight = DefaultBlockSize;

            foreach (var eagle in strategicObjectList)
            {
                // удаляем блоки над базой и под базой
                for (int x = eagle.X - blockWidth; x <= eagle.X + eagle.Width + blockWidth; x++)
                {
                    RemoveOverlapsOrContainsObjects(gameObjects, x, eagle.Y - blockHeight, blockWidth, blockHeight);
                    RemoveOverlapsOrContainsObjects(gameObjects, x, eagle.Y + eagle.Height, blockWidth, blockHeight);
                }

                // удаляем блоки слева и справа от базы
                for (int y = eagle.Y - blockHeight; y <= eagle.Y + eagle.Height + blockHeight; y++)
                {
                    RemoveOverlapsOrContainsObjects(gameObjects, eagle.X - blockWidth, y, blockWidth, blockHeight);
                    RemoveOverlapsOrContainsObjects(gameObjects, eagle.X + eagle.Width, y, blockWidth, blockHeight);
                }
            }
        }

        /// <summary>
        /// Удалить объекты с которыми найдено наложение
        /// </summary>
        /// <param name="gameObjects">Коллекция хранимых объектов</param>
        public void RemoveOverlapsOrContainsObjects(List<GameFieldObject> gameObjects, int x, int y, int width, int height)
        {
            var aabb = AABB.Create(x, y, width, height, Config.SubPixelSize);

            gameObjects.RemoveAll(p =>
               p.IsVisible &&
               !p.Type.HasFlag(GameObjectType.Enemy) &&
               !p.Type.HasFlag(GameObjectType.Player) &&
               !p.Type.HasFlag(GameObjectType.Tower) &&
               !p.Type.HasFlag(GameObjectType.Ship) &&
               p.GetAABB(Config.SubPixelSize).OverlapsOrContains(aabb));
        }

        /// <summary>
        /// Добавить стратегические объекты
        /// </summary>
        /// <param name="gameObjects">Коллекция хранимых объектов</param>
        public void AddTowerObjects(List<GameFieldObject> gameObjects)
        {
            var eagle = content.GameObjects.GetAll(p => p.Type.HasFlag(GameObjectType.Tower)).FirstOrDefault();

            if (eagle == null)
            {
                logger?.WriteLine($"{nameof(AddTowerObjects)}: {nameof(GameObjectType.Tower)} not defined", LogLevel.Warning);
                return;
            }
            if (Config.TowerLocations == null || Config.TowerLocations.Length == 0)
            {
                logger?.WriteLine($"{nameof(AddTowerObjects)}: {nameof(Config.TowerLocations)} not present", LogLevel.Warning);
                return;
            }
            foreach (var location in Config.TowerLocations)
            {
                // добавляем базу на поле
                AddObjectById(gameObjects, eagle.Id, location.X, location.Y);
            }
        }

        /// <summary>
        /// Добавить объекты из коллекции <paramref name="objectsToAdd"/> в коллекцию <paramref name="gameObjects"/>
        /// </summary>
        /// <param name="gameObjects">Коллекция хранимых объектов</param>
        /// <param name="objectsToAdd">Коллекция объектов для добавления</param>
        /// <param name="ingoreOverlaps">Признак игноривания перекрытия объектов</param>
        public void AddObjects(List<GameFieldObject> gameObjects, List<GameFieldObject> objectsToAdd, bool ingoreOverlaps = false)
        {
            if (objectsToAdd == null)
                return;

            foreach (var block in objectsToAdd)
            {
                if (ingoreOverlaps || !gameObjects.Any(p => p != block && p.OverlapsOrContains(block, Config.SubPixelSize)))
                {
                    gameObjects.Add(block);
                }
            }
        }

        /// <summary>
        /// Добавить кирпичные блоки вокруг стратегических объектов
        /// </summary>
        /// <param name="gameObjects">Коллекция хранимых объектов</param>
        public void AddBricksAroundTowers(List<GameFieldObject> gameObjects)
        {
            RemoveBlocksAroundTowers(gameObjects);
            var defenseObjList = CreateBricksAroundTowers(gameObjects);
            gameObjects.AddRange(defenseObjList);
        }

        /// <summary>
        /// Добавить объекты в коллекцию <paramref name="gameObjects"/>
        /// </summary>
        /// <param name="containerList">Коллекция хранимых объектов</param>
        /// <param name="preset">Шаблон объекта</param>
        /// <param name="x">X - координата в условных единицах</param>
        /// <param name="y">Y - координата в условных единицах</param>
        /// <param name="columns">Количество колонок</param>
        /// <param name="rows">Количество строк</param>
        private void AddObjects(
            List<GameFieldObject> containerList, GameFieldObject preset,
            int x, int y, int columns, int rows)
        {
            int objX, objY;

            for (int i = 0; i < columns; i++)
            {
                for (int j = 0; j < rows; j++)
                {
                    objX = x + i * preset.Width;
                    objY = y + j * preset.Height;

                    if (objX < 0 || objY < 0 || objX >= Config.FieldWidth || objY >= Config.FieldHeight)
                        continue;

                    var b = new GameFieldObject().CopyFrom(preset);
                    b.X = objX;
                    b.Y = objY;

                    if (!containerList.Any(p => p.X == b.X && p.Y == b.Y))
                        containerList.Add(b);
                }
            }
        }

        /// <summary>
        /// Создать iron блоки вокруг стратегических объектов
        /// </summary>
        /// <param name="gameObjects">Коллекция хранимых объектов</param>
        /// <returns></returns>
        public List<GameFieldObject> CreateIronBlocksAroundTowers(List<GameFieldObject> gameObjects)
        {
            // определяем список стратегических объектов
            var strategicObjectList = gameObjects
                .Where(p => p.IsVisible && p.Type.HasFlag(GameObjectType.Tower))
                .ToList();
            if (gameObjects.Count == 0)
            {
                // список пуст, дальнешие действия не требуются
                return new List<GameFieldObject>(0);
            }

            // получаем блок из контента
            var blockPreset = content.GameObjects.GetByName(IronObjName);
            if (blockPreset == null || blockPreset.Id == 0)
            {
                // блок не найден, дальнейшие действия не требуются
                return new List<GameFieldObject>(0);
            }

            int blockWidth = blockPreset.Width;
            int blockHeight = blockPreset.Height;

            if (blockWidth <= 0 || blockHeight <= 0)
            {
                // некорректная размерность блока, дальнейшие действия не требуются
                return new List<GameFieldObject>(0);
            }

            // результруеющий список блоков
            List<GameFieldObject> resultList = new List<GameFieldObject>();

            int x, y;

            // добавляем iron блоки вокруг каждого стратегического объекта
            foreach (var eagle in strategicObjectList)
            {
                if (eagle.Width < 1 || eagle.Height < 1)
                    continue;

                x = eagle.X - blockWidth;
                int columns = Convert.ToInt32(eagle.Width / (double)blockWidth) + 2;
                int rows = Convert.ToInt32(eagle.Height / (double)blockHeight) + 2;

                y = eagle.Y + eagle.Height;
                AddObjects(resultList, blockPreset, x, y, columns, 1);
                y = eagle.Y - blockHeight;
                AddObjects(resultList, blockPreset, x, y, columns, 1);

                x = eagle.X - blockWidth;
                AddObjects(resultList, blockPreset, x, y, 1, rows);
                x = eagle.X + eagle.Width;
                AddObjects(resultList, blockPreset, x, y, 1, rows);
            }

            return resultList;
        }

        /// <summary>
        /// Создать кирпичные блоки вокруг стратегических объектов
        /// <param name="gameObjects">Коллекция хранимых объектов</param>
        /// </summary>
        public List<GameFieldObject> CreateBricksAroundTowers(List<GameFieldObject> gameObjects)
        {
            // определяем список стратегических объектов
            var strategicObjectList = gameObjects
                .Where(p => p.IsVisible && p.Type.HasFlag(GameObjectType.Tower))
                .ToList();
            if (gameObjects.Count == 0)
            {
                // список пуст, дальнешие действия не требуются
                return new List<GameFieldObject>(0);
            }

            // получаем блок из контента
            var blockPreset = content.GameObjects.GetByName(BrickObjName);

            if (blockPreset == null)
            {
                // блок не найден, дальнейшие действия не требуются
                return new List<GameFieldObject>();
            }

            int blockWidth = DefaultBlockSize;      // brickBlockPreset.Width
            int blockHeight = DefaultBlockSize;     // brickBlockPreset.Height

            // результруеющий список блоков
            List<GameFieldObject> resultList = new List<GameFieldObject>();

            int x, y;

            int subWidth = Math.Max(1, blockWidth / blockPreset.Width);
            int subHeight = Math.Max(1, blockHeight / blockPreset.Height);

            // добавляем iron блоки вокруг каждого стратегического объекта
            foreach (var eagle in strategicObjectList)
            {
                if (eagle.Width < 1 || eagle.Height < 1)
                    continue;

                x = eagle.X - blockWidth;
                int columns = Convert.ToInt32(eagle.Width / (double)Math.Min(blockPreset.Width, blockWidth)) + 2 * subWidth;
                int rows = Convert.ToInt32(eagle.Height / (double)Math.Min(blockPreset.Height, blockHeight)) + 2 * subHeight;

                y = eagle.Y + eagle.Height;
                AddObjects(resultList, blockPreset, x, y, columns, subHeight);
                y = eagle.Y - blockHeight;
                AddObjects(resultList, blockPreset, x, y, columns, subWidth);

                x = eagle.X - blockWidth;
                AddObjects(resultList, blockPreset, x, y, subWidth, rows);
                x = eagle.X + eagle.Width;
                AddObjects(resultList, blockPreset, x, y, subHeight, rows);
            }

            return resultList;
        }

        /// <summary>
        /// Добавить объект в коллекцию <paramref name="gameObjects"/> по заданному идентификатору объекта
        /// </summary>
        /// <param name="gameObjects">Коллекция хранимых объектов</param>
        /// <param name="id">Идентификатор объекта</param>
        /// <param name="x">X - координата в условных единицах</param>
        /// <param name="y">Y - координата в условных единицах</param>
        public void AddObjectById(List<GameFieldObject> gameObjects, int id, int x, int y)
        {
            var gameObject = CreateObject(id, x, y);
            if (gameObject != null)
                gameObjects.Add(gameObject);
        }

        /// <summary>
        /// Создать объект по заданным координатам и идентификатору объекта
        /// </summary>
        /// <param name="id">Идентификатор объекта</param>
        /// <param name="x">X - координата в условных единицах</param>
        /// <param name="y">Y - координата в условных единицах</param>
        /// <returns>Созданный объект или <see cref="null"/>, если не удалось создать объект</returns>
        private GameFieldObject CreateObject(int id, int x, int y)
        {
            if (x < 0 || y < 0 || x >= Config.FieldWidth || y >= Config.FieldHeight)
                return null;
            GameFieldObject gameObject = new GameFieldObject();
            var baseItem = content.GameObjects[id];
            if (baseItem == null)
                return null;
            gameObject.CopyFrom(baseItem);
            gameObject.X = x;
            gameObject.Y = y;
            gameObject.IsVisible = true;

            return gameObject;
        }

        /// <summary>
        /// Освобождение ресурсов
        /// </summary>
        public void Dispose()
        {
            logger = null;
            content = null;
        }

        #endregion

    }
}
