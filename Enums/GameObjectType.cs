using System;

namespace BattleCity.Enums
{
    /// <summary>
    /// Тип игрового объекта
    /// </summary>
    [Flags]
    public enum GameObjectType
    {
        /// <summary>
        /// Не задано
        /// </summary>
        None = 0,

        /// <summary>
        /// Объект прокачки (бонус)
        /// </summary>
        PowerUp = 1,

        /// <summary>
        /// Уничтожаемый
        /// </summary>
        Destroyable = 2,

        /// <summary>
        /// Структура (кирпичи, блоки, лес и т.п.)
        /// </summary>
        //Structure = 4,

        /// <summary>
        /// Анимация
        /// </summary>
        Animation = 8,

        /// <summary>
        /// Препятсвие, блокирующее движение.
        /// При исключении по этому типу стоит игнорировать Water, т.к. юнит может имеет тип Ship.
        /// Forest и Ice не являются преградой.
        /// </summary>
        Barrier = 16,

        /// <summary>
        /// Стратегический объект
        /// </summary>
        Tower = 32,

        /// <summary>
        /// Снаряд
        /// </summary>
        Projectile = 64,

        /// <summary>
        /// Корабль (хождение по воде)
        /// </summary>
        Ship = 128,

        /// <summary>
        /// Вода (Море)
        /// </summary>
        Water = 256,

        /// <summary>
        /// Лёд
        /// </summary>
        Ice = 512,

        /// <summary>
        /// Лес / деревья
        /// </summary>
        Forest = 1024,

        /// <summary>
        /// Игрок
        /// </summary>
        Player = 2048,

        /// <summary>
        /// Враг
        /// </summary>
        Enemy = 4096,

        /// <summary>
        /// Позиция появления
        /// </summary>
        SpawPosition = 8192,

        /// <summary>
        /// Юнит
        /// </summary>
        Unit = 16384,

        /// <summary>
        /// Разрушенный стратегический объект
        /// </summary>
        DestoyedTower = 32768,
    }
}
