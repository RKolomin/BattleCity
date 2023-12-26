namespace BattleCity.InputControllers
{
    public interface IKeyboardController : IInputController
    {
        /// <summary>
        /// Определить долгое нажатие кнопки
        /// </summary>
        /// <param name="key"></param>
        bool IsLongPress(int key);
    }
}
