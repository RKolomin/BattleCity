using BattleCity.Enums;
using SlimDX.Direct3D9;
using Rectangle = System.Drawing.Rectangle;
using GdiFont = System.Drawing.Font;

namespace BattleCity.Video
{
    public class GameFont : IGameFont
    {
        private IDeviceContext deviceContext;
        private Font font;

        private GdiFont gdiFont;

        /// <inheritdoc/>
        public string Name { get; }

        /// <inheritdoc/>
        public float Size { get; }

        public GameFont(IDeviceContext deviceContext, GdiFont gdiFont)
        {
            this.deviceContext = deviceContext;
            this.gdiFont = gdiFont;
            font = new Font(deviceContext.Device, gdiFont);
            Name = gdiFont.Name;
            Size = gdiFont.Size;

            deviceContext.DeviceLost += DeviceContext_DeviceLost;
            deviceContext.DeviceRestored += DeviceContext_DeviceRestored;
        }

        private void DeviceContext_DeviceRestored()
        {
            font?.OnResetDevice();
        }

        private void DeviceContext_DeviceLost()
        {
            font?.OnLostDevice();
        }

        /// <inheritdoc/>
        public Rectangle MeasureString(string text)
        {
            return font.MeasureString(null, text, 0);
        }

        /// <inheritdoc/>
        public void DrawString(string text, int x, int y, int color)
        {
            font.DrawString(null, text, x, y, color);
        }

        /// <inheritdoc/>
        public void DrawString(string text, int x, int y, int width, int height, DrawStringFormat format, int color)
        {
            DrawString(text, new Rectangle(x, y, width, height), format, color);
        }

        /// <inheritdoc/>
        public void DrawString(string text, Rectangle rect, DrawStringFormat format, int color)
        {
            DrawTextFormat textFormat = DrawTextFormat.Left;

            if (format.HasFlag(DrawStringFormat.Left))
                textFormat |= DrawTextFormat.Left;

            if (format.HasFlag(DrawStringFormat.Top))
                textFormat |= DrawTextFormat.Top;

            if (format.HasFlag(DrawStringFormat.Right))
                textFormat |= DrawTextFormat.Right;

            if (format.HasFlag(DrawStringFormat.Bottom))
                textFormat |= DrawTextFormat.Bottom;

            if (format.HasFlag(DrawStringFormat.Center))
                textFormat |= DrawTextFormat.Center;

            if (format.HasFlag(DrawStringFormat.VerticalCenter))
                textFormat |= DrawTextFormat.VerticalCenter;

            if (format.HasFlag(DrawStringFormat.SingleLine))
                textFormat |= DrawTextFormat.SingleLine;

            if (format.HasFlag(DrawStringFormat.ExpandTabs))
                textFormat |= DrawTextFormat.ExpandTabs;

            if (format.HasFlag(DrawStringFormat.WordBreak))
                textFormat |= DrawTextFormat.WordBreak;

            if (format.HasFlag(DrawStringFormat.NoClip))
                textFormat |= DrawTextFormat.NoClip;

            font.DrawString(null, text, rect, textFormat, color);
        }

        public void Dispose()
        {
            if (deviceContext != null)
            {
                deviceContext = null;
            }

            if (font != null)
            {
                font.Dispose();
                font = null;
            }

            if (gdiFont != null)
            {
                gdiFont.Dispose();
                gdiFont = null;
            }
        }
    }
}
