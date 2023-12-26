using BattleCity.GameObjects;
using BattleCity.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BattleCity.Repositories
{
    /// <summary>
    /// Репозиторий игровых объектов
    /// </summary>
    public class GameObjectRepository : BaseRepository<GameFieldObject>
    {
        /// <summary>
        /// Конструктор
        /// </summary>
        public GameObjectRepository(ILogger logger, string directory, string filename, int capacity)
            : base(logger, directory, filename, capacity) { }

        public void Load()
        {
            Deserialize();
        }

        /// <summary>
        /// Получить игровой объект по имени
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public GameFieldObject GetByName(string name)
        {
            if (string.IsNullOrEmpty(name))
                return null;

            return array.FirstOrDefault(x => x != null && x.Name != null && x.Name.ToUpper() == name.ToUpper());
        }

        public GameFieldObject GetById(int id)
        {
            return array.FirstOrDefault(x => x != null && x.Id == id);
        }

        /// <summary>
        /// Получить все <see cref="GameFieldObject"/>'s по предикату
        /// </summary>
        /// <param name="func"></param>
        /// <returns></returns>
        public IEnumerable<GameFieldObject> GetAll(Func<GameFieldObject, bool> func)
        {
            return array.Where(x => x != null && func(x));
        }

        /// <summary>
        /// Добавить игровой объект
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public bool Add(GameFieldObject item)
        {
            for (int i = 0; i < array.Length; i++)
                if (array[i] == null)
                {
                    item.Id = i;
                    array[i] = item;
                    return true;
                }

            return false;
        }
    }
}