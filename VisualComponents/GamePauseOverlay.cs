using BattleCity.Audio;
using BattleCity.Common;
using BattleCity.Enums;
using BattleCity.Video;
using System;
using Rectangle = System.Drawing.Rectangle;

namespace BattleCity.VisualComponents
{
    public class GamePauseOverlay : IDisposable
    {
        private IGameFont font;
        private ISoundEngine soundEngine;
        private readonly Rectangle boundingBox;
        private readonly int textHeight;
        private readonly string text = "PAUSE";
        private readonly int textColor = Colors.Red;
        private int frameNumber = 0;
        public bool IsVisible { get; private set; }

        public GamePauseOverlay(
            IGameGraphics graphics,
            GameContent content,
            Rectangle boundingBox,
            ISoundEngine soundEngine)
        {
            this.soundEngine = soundEngine;
            this.boundingBox = boundingBox;
            font = graphics.CreateFont(content.GetFont(content.CommonConfig.DefaultFontSize));
            textHeight = (font.MeasureString("L").Height) * 2;
        }

        public void Show()
        {
            IsVisible = true;
            soundEngine.PlaySound("pause");
        }

        public void Hide()
        {
            IsVisible = false;
        }

        public void Draw()
        {
            var y = (boundingBox.Height - textHeight) / 2;

            var rect = new Rectangle()
            {
                X = boundingBox.X,
                Y = boundingBox.Y + y,
                Width = boundingBox.Width,
                Height = textHeight
            };

            if ((frameNumber / 16) % 2 == 0)
                font.DrawString(text, rect, DrawStringFormat.Center, textColor);
            frameNumber++;
        }

        public void Dispose()
        {
            if (font != null)
            {
                font.Dispose();
                font = null;
            }

            soundEngine = null;
        }
    }
}
