using BattleCity.Enums;
using BattleCity.Video;

namespace BattleCity.VisualComponents
{
    /// <summary>
    /// Текстовый блок
    /// </summary>
    public class TextBlock
    {
        #region Properties

        /// <summary>
        /// Графический шрифт
        /// </summary>
        public IGameFont Font { get; set; }

        /// <summary>
        /// Абсолютное значение X-координаты
        /// </summary>
        public int X { get; set; }

        /// <summary>
        /// Абсолютное значение Y-координаты
        /// </summary>
        public int Y { get; set; }

        /// <summary>
        /// Абсолютное значение ширины
        /// </summary>
        public int Width { get; set; }

        /// <summary>
        /// Абсолютное значение высоты
        /// </summary>
        public int Height { get; set; }

        /// <summary>
        /// Отображаемый текст
        /// </summary>
        public string Text { get; set; }

        /// <summary>
        /// Цвет текста
        /// </summary>
        public int TextColor { get; set; }

        /// <summary>
        /// Номер кадра
        /// </summary>
        public int FrameNumber { get; set; }

        /// <summary>
        /// Тэг
        /// </summary>
        public object Tag { get; set; }

        #endregion

        #region Constructor

        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="font">Шрифт</param>
        /// <param name="x">Абсолютное значение X-координаты блока</param>
        /// <param name="y">Абсолютное значение Y-координаты блока</param>
        /// <param name="width">Абсолютное значение ширины блока</param>
        /// <param name="height">Абсолютное значение высоты блока</param>
        /// <param name="textColor">Цвет текста</param>
        /// <param name="text">Текст для отрисовки</param>
        public TextBlock(IGameFont font, int x, int y, int width, int height, int textColor, string text)
        {
            Font = font;
            X = x;
            Y = y;
            Width = width;
            Height = height;
            TextColor = textColor;
            Text = text ?? "";
        }

        #endregion

        #region methods

        /// <summary>
        /// Отрисовать текст в центре блока
        /// </summary>
        public virtual void DrawInCenter()
        {
            Font.DrawString(Text, 
                X, Y, Width, Height, 
                DrawStringFormat.Center | DrawStringFormat.VerticalCenter | DrawStringFormat.NoClip, 
                TextColor);
        }

        /// <summary>
        /// Отрисовать текст с заданным форматом
        /// </summary>
        /// <param name="textFormat">Формат выводимого текста</param>
        public virtual void Draw(DrawStringFormat textFormat)
        {
            Font.DrawString(Text, X, Y, Width, Height, textFormat, TextColor);
        }

        ~TextBlock()
        {
            Font = null;
        }

        #endregion

    }
}
