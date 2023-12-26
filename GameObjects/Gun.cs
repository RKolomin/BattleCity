using System;

namespace BattleCity.GameObjects
{
    /// <summary>
    /// Пушка
    /// </summary>
    public class Gun
    {
        /// <summary>
        /// Признак готовности выпустить снаряд
        /// </summary>
        public bool IsGunLoaded => Capacity > 0 && reloadGunFrames <= 0;

        /// <summary>
        /// Начальная (максимальная) ёмкость магазина
        /// </summary>
        public int InitialCapacity { get; set; } = 1;

        /// <summary>
        /// Количество снарядов в магазине
        /// </summary>
        public int Capacity { get; set; } = 1;

        /// <summary>
        /// Время перезарядки (время ожидания в кадрах)
        /// </summary>
        public int GunReloadDelay { get; set; } = 30;

        /// <summary>
        /// Скорость снаряда или 0, если требуется автоматическое определение
        /// </summary>
        public int BulletSpeed { get; set; } = 0;

        /// <summary>
        /// Мощность снаряда
        /// </summary>
        public int BulletPower { get; set; }

        /// <summary>
        /// Ширина выпускаемого снаряда в условных единицах
        /// </summary>
        public int BulletWidth { get; set; } = 2;

        /// <summary>
        /// Высота выпускаемого снаряда в условных единицах
        /// </summary>
        public int BulletHeight { get; set; } = 1;

        /// <summary>
        /// Идентификатор звука выстрела
        /// </summary>
        public int ShotSndId { get; set; }

        /// <summary>
        /// Оставшееся количество кадров до перезарядки
        /// </summary>
        protected int reloadGunFrames;

        /// <summary>
        /// Выпустить снаряд
        /// </summary>
        /// <param name="gunOwner">Юнит-владелец пушки</param>
        /// <param name="subPixelSize"></param>
        /// <returns>Ссылка на выпущенный снаряд или <see cref="null"/>, если пушка не заряжена</returns>
        public Bullet Fire(BattleUnit gunOwner, int subPixelSize)
        {
            if (!IsGunLoaded)
                return null;
            reloadGunFrames = GunReloadDelay;
            Capacity--;
            return new Bullet(this, gunOwner, subPixelSize);
        }

        /// <summary>
        /// Перезарядить пушку
        /// </summary>
        /// <param name="hardReset"></param>
        public void ReloadGun(bool hardReset)
        {
            if (hardReset)
            {
                Capacity = InitialCapacity;
                reloadGunFrames = 0;
            }
            else
            {
                Capacity = Math.Min(InitialCapacity, Capacity + 1);
            }
        }

        /// <summary>
        /// Обновить
        /// </summary>
        public void Update()
        {
            if (reloadGunFrames > 0)
            {
                reloadGunFrames--;
            }
        }
    }
}
