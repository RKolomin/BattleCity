using BattleCity.Common;
using BattleCity.GameObjects;
using BattleCity.Logging;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace BattleCity.Repositories
{
    /// <summary>
    /// Репозиторий игровых уровней
    /// </summary>
    public class StageRepository : BaseRepository<BattleStage>, IEnumerable<BattleStage>
    {
        /// <summary>
        /// Конструктор
        /// </summary>
        public StageRepository(ILogger logger, string directory, string filename, int capacity)
            : base(logger, directory, filename, capacity)
        {

        }

        /// <summary>
        /// Загрузить индекс-таблицу
        /// </summary>
        public void Load()
        {
            Deserialize();

            int stageNumber = 0;

            foreach (var item in array)
            {
                stageNumber++;

                if (item == null)
                    continue;

                item.Id = stageNumber;

                if (item.FieldObjects == null)
                {
                    item.FieldObjects = new List<BaseGameObject>();
                    logger?.WriteLine($"stage_{stageNumber}: contains no field objects", LogLevel.Warning);
                }
            }
        }

        public IEnumerator<BattleStage> GetEnumerator()
        {
            return array.AsEnumerable().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        public int LastIndexOf(Func<BattleStage, bool> predicate)
        {
            for (int i = array.Length - 1; i >= 0; i--)
            {
                if (predicate(array[i]))
                    return i;
            }

            return -1;
        }

        /// <summary>
        /// Получение <see cref="BattleStage"/> по индексу (не путать с порядковым номером, он начинается с 1)
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public new BattleStage this[int index]
        {
            get { return base[index]; }
            set { base[index] = value; }
        }
    }
}
