using System;

namespace BattleCity.Common
{
    /// <summary>
    /// Интерфейс игрового приложения
    /// </summary>
    public interface IGameApplication
    {
        /// <summary>
        /// Признак того, что приложение активно (на переднем плане)
        /// </summary>
        bool IsActive { get; }

        /// <summary>
        /// Дескриптор игрового окна
        /// </summary>
        IntPtr Handle { get; }

        /// <summary>
        /// Непрерывные выстрел при зажатой кнопки
        /// </summary>
        bool ContinuousFire { get; set; }

        /// <summary>
        /// Признак полноэкранного режима
        /// </summary>
        bool IsFullScreen { get; }

        /// <summary>
        /// Признак сохранения соотношения сторон
        /// </summary>
        bool SaveAspectRatio { get; }

        /// <summary>
        /// Значение эффекта Scanlines
        /// </summary>
        int ScanlinesFxLevel { get; set; }

        /// <summary>
        /// Переключить полноэкранный режим
        /// </summary>
        void SwitchFullScreenMode();

        /// <summary>
        /// Переключить состояние сохранения соотношения сторон
        /// </summary>
        void SwitchAspectRatio();

        /// <summary>
        /// Задать текущую контент директорию
        /// </summary>
        void SetContentDirectory(string contentDirectoryName);

        /// <summary>
        /// Ширина клиентской области формы
        /// </summary>
        int ClientSizeWidth { get; }

        /// <summary>
        /// Высота клиентской области формы
        /// </summary>
        int ClientSizeHeight { get; }
    }
}
