using System;

namespace BattleCity.InputControllers
{
    /// <summary>
    /// Интерфейс контроллера устройства ввода
    /// </summary>
    public interface IInputController : IDisposable
    {
        /// <summary>
        /// Идентификатор устройства
        /// </summary>
        string Id { get; }

        /// <summary>
        /// Название устройства
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Обновить состояния устройства
        /// </summary>
        void Update();

        /// <summary>
        /// Определить нажатие/удержание кнопки
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        bool IsPressed(int key);

        /// <summary>
        /// Определить нажатие кнопки
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        bool IsDown(int key);

        /// <summary>
        /// Определить была ли отпущена кнопка
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        bool IsReleased(int key);

        /// <summary>
        /// Определить долгое нажатие кнопки
        /// </summary>
        /// <param name="key"></param>
        /// <param name="period"></param>
        /// <param name="repeatPeriod"></param>
        /// <returns></returns>
        bool IsLongPress(int key, int period, int repeatPeriod);
    }
}
