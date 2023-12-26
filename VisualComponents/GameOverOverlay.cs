using BattleCity.Common;
using BattleCity.Enums;
using BattleCity.Video;
using System;
using Rectangle = System.Drawing.Rectangle;

namespace BattleCity.VisualComponents
{
    public class GameOverOverlay : IDisposable
    {
        private IGameFont font;
        private readonly Rectangle boundingBox;
        private int y;
        private readonly int textMoveSpeed = 2;
        private readonly int textHeight;
        private readonly int textColor = Colors.Red;

        public bool IsVisible { get; private set; }
        public int ElapsedFrames { get; private set; }

        public GameOverOverlay(
            GameContent content,
            IGameGraphics graphics,
            Rectangle boundingBox)
        {
            this.boundingBox = boundingBox;
            font = graphics.CreateFont(content.GetFont(content.CommonConfig.DefaultFontSize));
            textHeight = (font.MeasureString("L").Height) * 2;
        }

        public void Show(int durationInFrames)
        {
            ElapsedFrames = durationInFrames;
            y = boundingBox.Height - textHeight;
            IsVisible = true;
        }

        public void Hide()
        {
            IsVisible = false;
        }

        public void Update()
        {
            if (ElapsedFrames > 0)
                ElapsedFrames--;

            int center = (boundingBox.Height - textHeight) / 2;
            y = Math.Max(center, y - textMoveSpeed);
        }

        public void Draw()
        {
            var rect = new Rectangle()
            {
                X = boundingBox.X,
                Y = boundingBox.Y + y,
                Width = boundingBox.Width,
                Height = textHeight
            };

            font.DrawString($"GAME{Environment.NewLine}OVER", rect, DrawStringFormat.Center, textColor);
        }

        public void Dispose()
        {
            if (font != null)
            {
                font.Dispose();
                font = null;
            }
        }
    }
}
