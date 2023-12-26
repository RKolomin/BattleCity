using System;

namespace BattleCity.Enums
{
    /// <summary>
    /// Тип действия
    /// </summary>
    [Flags]
    public enum UnitAction
    {
        /// <summary>
        /// Простой (ничего не делать)
        /// </summary>
        Idle = 0,

        /// <summary>
        /// Двигаться
        /// </summary>
        Move = 1,

        /// <summary>
        /// Стрелять
        /// </summary>
        Attack = 2,
    }
}
