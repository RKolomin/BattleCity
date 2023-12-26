using BattleCity.Common;
using BattleCity.Enums;

namespace BattleCity.GameObjects
{
    /// <summary>
    /// Базовое представление игрового объекта
    /// </summary>
    public class BaseGameObject : IResxId
    {
        /// <summary>
        /// Идентификатор объекта
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Название объекта
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// X - координата в условных единицах
        /// </summary>
        public int X { get; set; }

        /// <summary>
        /// Y - координата в условных единицах
        /// </summary>
        public int Y { get; set; }

        /// <summary>
        /// Координата X, Субпиксель в условных единицах
        /// </summary>
        public int SubPixelX { get; set; }

        /// <summary>
        /// Координата Y, Субпиксель в условных единицах
        /// </summary>
        public int SubPixelY { get; set; }

        /// <summary>
        /// Направление движения (в какую сторону направлен объект)
        /// </summary>
        public MoveDirection Direction { get; set; }

        /// <summary>
        /// Установить позицию из указанного объекта
        /// </summary>
        /// <param name="gameObject"></param>
        public void SetPositionFromObject(BaseGameObject gameObject)
        {
            X = gameObject.X;
            Y = gameObject.Y;
            SubPixelX = gameObject.SubPixelX;
            SubPixelY = gameObject.SubPixelY;
        }

        /// <summary>
        /// Задать позицию (x,y)
        /// </summary>
        public void SetPosition(int x, int y)
        {
            X = x;
            Y = y;
        }

        /// <summary>
        /// Задать позицию
        /// </summary>
        public void SetPosition(int x, int y, int subPixelX, int subPixelY)
        {
            X = x;
            Y = y;
            SubPixelX = subPixelX;
            SubPixelY = subPixelY;
        }

        /// <summary>
        /// Получить строковое представление объекта
        /// </summary>
        /// <returns></returns>
        public override string ToString() { return $"{Name} {X} {Y}"; }
    }
}
