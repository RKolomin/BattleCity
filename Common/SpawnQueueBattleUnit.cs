using System;

namespace BattleCity.Common
{
    /// <summary>
    /// Очередной юнит для появления
    /// </summary>
    public class SpawnQueueBattleUnit
    {
        /// <summary>
        /// Имя объекта
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Количество бонусов при попадении в юнита
        /// </summary>
        public int ExtraBonus { get; set; }

        /// <summary>
        /// Здоровье
        /// </summary>
        public int Health { get; set; }

        /// <summary>
        /// Конструктор по умолчанию
        /// </summary>
        public SpawnQueueBattleUnit()
        {

        }

        /// <summary>
        /// Параметризированный конструктор
        /// </summary>
        /// <param name="name">Имя объекта</param>
        /// <param name="extraBonus">Доп. бонусы</param>
        /// <param name="health">Здоровье</param>
        public SpawnQueueBattleUnit(string name, int extraBonus, int health)
        {
            Name = name;
            ExtraBonus = Math.Max(0, extraBonus);
            Health = Math.Max(ExtraBonus > 0 ? 0 : 1, health);
        }
    }
}
