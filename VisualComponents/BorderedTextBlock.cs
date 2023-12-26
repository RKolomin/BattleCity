using BattleCity.Enums;
using BattleCity.Video;

namespace BattleCity.VisualComponents
{
    /// <summary>
    /// Блок текста с рамкой
    /// </summary>
    public class BorderedTextBlock : TextBlock
    {
        private IGameGraphics graphics;

        #region Constructor

        public BorderedTextBlock(IGameGraphics graphics, IGameFont font, int x, int y, int width, int height, int textColor, string text = "")
            : base(font, x, y, width, height, textColor, text)
        {
            this.graphics = graphics;
            BorderColor = textColor;
        }

        #endregion

        #region Properties

        /// <summary>
        /// Отступ сверху
        /// </summary>
        public int MarginTop { get; set; }

        /// <summary>
        /// Отступ слева
        /// </summary>
        public int MarginLeft { get; set; }

        /// <summary>
        /// Ширина текстуры
        /// </summary>
        public float TextureWidth { get; set; } = 48;

        /// <summary>
        /// Высота текстуры
        /// </summary>
        public float TextureHeight { get; set; } = 48;

        /// <summary>
        /// Цвет рамки
        /// </summary>
        public int BorderColor { get; set; }

        /// <summary>
        /// Размер рамки
        /// </summary>
        public int BorderSize { get; set; } = 4;

        #endregion

        #region public methods

        /// <summary>
        /// Отрисовать
        /// </summary>
        public void Draw()
        {
            if (string.IsNullOrEmpty(Text)) 
                return;
            Font.DrawString(Text, X + MarginLeft, Y + MarginTop * 2, TextColor);
            graphics.DrawBorderRect(X, Y, Width, Height, TextColor);
        }

        /// <summary>
        /// Отрисовать текст с заданным форматом
        /// </summary>
        public override void Draw(DrawStringFormat textFormat)
        {
            if (string.IsNullOrEmpty(Text))
                return;
            Font.DrawString(
                Text,
                X + MarginLeft, Y + MarginTop * 2, Width - 2 * MarginLeft, Height - 2 * MarginTop,
                textFormat,
                TextColor);

            graphics.DrawBorderRect(X, Y, Width, Height, TextColor);
        }

        ~BorderedTextBlock()
        {
            graphics = null;
        }

        #endregion

    }
}
