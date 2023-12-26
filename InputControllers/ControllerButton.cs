namespace BattleCity.InputControllers
{
    /// <summary>
    /// Кнопка
    /// </summary>
    public class ControllerButton
    {
        /// <summary>
        /// Идентификатор кнопки
        /// </summary>
        public string ButtonId { get; }

        /// <summary>
        /// Первичный код
        /// </summary>
        public int? KeyCodePrimary { get; set; }

        /// <summary>
        /// Вторичный код
        /// </summary>
        public int? KeyCodeSecondary { get; set; }

        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="buttonId">Идентификатор кнопки</param>
        /// <param name="keyCodePrimary">Первичный код</param>
        /// <param name="keyCodeSecondary">Вторичный код</param>
        public ControllerButton(string buttonId, int? keyCodePrimary = null, int? keyCodeSecondary = null)
        {
            ButtonId = buttonId;
            KeyCodePrimary = keyCodePrimary;
            KeyCodeSecondary = keyCodeSecondary;
        }
    }
}
