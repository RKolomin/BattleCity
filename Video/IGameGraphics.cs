using BattleCity.GameObjects;
using System;
using GdiFont = System.Drawing.Font;

namespace BattleCity.Video
{
    public interface IGameGraphics : IDisposable
    {
        /// <summary>
        /// Очистка экрана
        /// </summary>
        /// <param name="fillColor">Цвет заливки</param>
        void Clear(int fillColor);

        /// <summary>
        /// Установка состояний отрисовки по умолчанию
        /// </summary>
        void SetDefaultRenderStates();

        /// <summary>
        /// Отрисовать шахматку
        /// </summary>
        void DrawChessboard(int x, int y, int w, int h, int cellSize, int cellColor1, int cellColor2);

        /// <summary>
        /// Отрисовать сетку
        /// </summary>
        void DrawGridLines(int x, int y, int w, int h, int cellSize, int color);

        /// <summary>
        /// Отрисовать кирпичный оверлей
        /// </summary>
        void DrawBrickWallOverlay(float x, float y, float width, float height, float zoomX, float zoomY, int color);

        /// <summary>
        /// Отрисовать прямоугольник с заданным цветом
        /// </summary>
        /// <param name="x">Абсолютное значение X координаты</param>
        /// <param name="y">Абсолютное значение Y координаты</param>
        /// <param name="w">Абсолютное значение ширины</param>
        /// <param name="h">Абсолютное значение высоты</param>
        /// <param name="color">Цвет заливки</param>
        void FillRect(int x, int y, int w, int h, int fillColor);

        /// <summary>
        /// Отрисовать прямоугольник с заданным цветом граней
        /// </summary>
        /// <param name="x">Абсолютное значение X координаты</param>
        /// <param name="y">Абсолютное значение Y координаты</param>
        /// <param name="w">Абсолютное значение ширины</param>
        /// <param name="h">Абсолютное значение высоты</param>
        /// <param name="color">Цвет граней</param>
        void DrawBorderRect(int x, int y, int w, int h, int borderColor);

        /// <summary>
        /// Начать отрисовску игровых объектов
        /// </summary>
        void BeginDrawGameObjects();

        /// <summary>
        /// Отрисовать игровой объект
        /// </summary>
        /// <param name="left">Абсолютное смещение по оси X</param>
        /// <param name="top">Абсолютное смещение по оси Y</param>
        /// <param name="gameObject"></param>
        /// <param name="gameTime">Игрвое время (номер кадра)</param>
        /// <param name="cellSize">Размер условных субпикселей</param>
        void DrawGameObject(int left, int top, GameFieldObject gameObject, int gameTime, int cellSize);

        /// <summary>
        /// Завершить отрисовку игровых объектов
        /// </summary>
        void EndDrawGameObjects();

        /// <summary>
        /// Создать графических шрифт
        /// </summary>
        /// <param name="gdiFont">Шрифт</param>
        /// <returns></returns>
        IGameFont CreateFont(GdiFont gdiFont);
    }
}
