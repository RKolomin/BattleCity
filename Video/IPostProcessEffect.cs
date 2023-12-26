using System;

namespace BattleCity.Video
{
    /// <summary>
    /// Интерфейс пост обработки
    /// </summary>
    public interface IPostProcessEffect : IDisposable
    {
        /// <summary>
        /// Отрисовать
        /// </summary>
        void Draw();
    }
}
