using BattleCity.GameObjects;
using System.Collections.Generic;

namespace BattleCity.Common
{
    /// <summary>
    /// Игровой уровень (Stage)
    /// </summary>
    public class BattleStage : IResxId
    {
        /// <summary>
        /// Порядковый номер уровня
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Список (очередь) вражеских юнитов
        /// </summary>
        public List<SpawnQueueBattleUnit> Enemies { get; set; } = new List<SpawnQueueBattleUnit>(100);

        /// <summary>
        /// Объекты поля
        /// </summary>
        public List<BaseGameObject> FieldObjects { get; set; } = new List<BaseGameObject>(256);
    }
}