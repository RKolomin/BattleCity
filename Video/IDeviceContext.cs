using SlimDX.Direct3D9;
using System;

namespace BattleCity.Video
{
    public interface IDeviceContext
    {
        /// <summary>
        /// Событие при потери устройства
        /// </summary>
        event Action DeviceLost;

        /// <summary>
        /// Событие при восстановлении устройства
        /// </summary>
        event Action DeviceRestored;

        /// <summary>
        /// Событие при изменении размера ширины / высоты в параметрах устройства
        /// </summary>
        event Action DeviceResize;

        /// <summary>
        /// Графическое устройство
        /// </summary>
        Device Device { get; }

        /// <summary>
        /// Ширина игрового пространства
        /// </summary>
        int DeviceWidth { get; }

        /// <summary>
        /// Высота игрового пространства
        /// </summary>
        int DeviceHeight { get; }

        /// <summary>
        /// Опрелелить, что устройство потеряно
        /// </summary>
        bool IsLost();
    }
}
