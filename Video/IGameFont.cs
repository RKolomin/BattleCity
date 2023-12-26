using BattleCity.Enums;
using System;
using Rectangle = System.Drawing.Rectangle;

namespace BattleCity.Video
{
    public interface IGameFont : IDisposable
    {
        string Name { get; }
        float Size { get; }

        void DrawString(string text, int x, int y, int color);
        void DrawString(string text, Rectangle rect, DrawStringFormat format, int color);
        void DrawString(string text, int x, int y, int width, int height, DrawStringFormat format, int color);
        Rectangle MeasureString(string text);
    }
}
