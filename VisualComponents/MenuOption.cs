using BattleCity.Video;

namespace BattleCity.VisualComponents
{
    /// <summary>
    /// Опция меню
    /// </summary>
    public class MenuOption
    {
        /// <summary>
        /// Отображаемый текст
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// X - координата
        /// </summary>
        public int X { get; set; }

        /// <summary>
        /// Y - координата
        /// </summary>
        public int Y { get; set; }

        /// <summary>
        /// Цвет текста
        /// </summary>
        public int Color { get; set; }

        /// <summary>
        /// Признак достуности выбора опции
        /// </summary>
        public bool Selectable { get; set; } = true;

        /// <summary>
        /// Тэг
        /// </summary>
        public string Tag { get; set; }

        /// <summary>
        /// Отрисовка
        /// </summary>
        /// <param name="font"></param>
        public virtual void Draw(IGameFont font)
        {
            font.DrawString(Text, X, Y, Color);
        }
    }
}
