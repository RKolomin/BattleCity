using BattleCity.Common;
using BattleCity.Enums;
using BattleCity.GameObjects;
using SlimDX;
using Rectangle = System.Drawing.Rectangle;

namespace BattleCity.Extensions
{
    public static class ColliderExtensions
    {
        /// <summary>
        /// Получить <see cref="Rectangle"/> для указанного объекта
        /// </summary>
        /// <param name="obj">Объект для получения <see cref="Rectangle"/></param>
        /// <param name="subPixelSize">Масштабирование</param>
        /// <param name="axisAligned">Привязка к <see cref="MoveDirection"/></param>
        /// <returns></returns>
        public static Rectangle GetBounds(this GameFieldObject obj, int subPixelSize = 1, bool axisAligned = true)
        {
            Rectangle rect1 = new Rectangle(
                obj.X * subPixelSize + obj.SubPixelX,
                obj.Y * subPixelSize + obj.SubPixelY,
                obj.Width * subPixelSize,
                obj.Height * subPixelSize
            );
            if (axisAligned)
            {
                if (obj.Direction == MoveDirection.Right || obj.Direction == MoveDirection.Left)
                {
                    rect1.Width = obj.Height * subPixelSize;
                    rect1.Height = obj.Width * subPixelSize;
                }
            }

            return rect1;
        }

        /// <summary>
        /// Получить <see cref="BoundingBox"/> для указанного объекта
        /// </summary>
        /// <param name="obj">Объект для получения <see cref="BoundingBox"/></param>
        /// <param name="subPixelSize">Масштабирование</param>
        /// <returns></returns>
        public static BoundingBox GetAABB(this GameFieldObject obj, int subPixelSize = 1)
        {
            return AABB.Create(obj, subPixelSize);
        }

        /// <summary>
        /// Проверить перекрытие текущего объекта с указанным объектом
        /// </summary>
        /// <param name="thisObject">Текущий объект, для которого проводится проверка перекрытия с другим объектом</param>
        /// <param name="targetObject">Объект, перекрытие с которым проверяется</param>
        /// <param name="subPixelSize">Масштабирование</param>
        /// <returns></returns>
        public static bool Overlaps(this GameFieldObject thisObject, GameFieldObject targetObject, int subPixelSize = 1)
        {
            var aabb1 = AABB.Create(thisObject, subPixelSize);
            var aabb2 = AABB.Create(targetObject, subPixelSize);
            return Overlaps(aabb1, aabb2);
        }

        /// <summary>
        /// Определить перекрытие или вложенность одного объекта с другим
        /// </summary>
        public static bool OverlapsOrContains(this GameFieldObject cur, GameFieldObject item, int subPixelSize = 1)
        {
            var aabb1 = AABB.Create(cur, subPixelSize);
            var aabb2 = AABB.Create(item, subPixelSize);
            return BoundingBox.Contains(aabb1, aabb2) == ContainmentType.Contains ||
                Overlaps(aabb1, aabb2);
        }

        /// <summary>
        /// Определить перекрытие указанных областей
        /// </summary>
        public static bool Overlaps(this BoundingBox aabb1, BoundingBox aabb2)
        {
            if (aabb1.Minimum.X >= aabb2.Maximum.X || aabb2.Minimum.X >= aabb1.Maximum.X)
                return false;
            if (aabb1.Minimum.Y >= aabb2.Maximum.Y || aabb2.Minimum.Y >= aabb1.Maximum.Y)
                return false;
            return true;
        }

        /// <summary>
        /// Определить, содержится ли указанный <see cref="BoundingBox"/> в текущем <see cref="BoundingBox"/>
        /// </summary>
        /// <param name="aabb1"></param>
        /// <param name="aabb2"></param>
        /// <returns></returns>
        public static bool Contains(this BoundingBox aabb1, BoundingBox aabb2)
        {
            return BoundingBox.Contains(aabb1, aabb2) == ContainmentType.Contains;
        }

        /// <summary>
        /// Определить признак наложения или вложенности одного <see cref="BoundingBox"/> в другом <see cref="BoundingBox"/>
        /// </summary>
        /// <param name="aabb1"></param>
        /// <param name="aabb2"></param>
        /// <returns></returns>
        public static bool OverlapsOrContains(this BoundingBox aabb1, BoundingBox aabb2)
        {
            return BoundingBox.Contains(aabb1, aabb2) == ContainmentType.Contains ||
                Overlaps(aabb1, aabb2);
        }

        /// <summary>
        /// Определить, что <paramref name="aabb1"/> и <paramref name="aabb2"/> не пересекаются
        /// </summary>
        /// <returns></returns>
        public static bool DisjointTo(this BoundingBox aabb1, BoundingBox aabb2)
        {
            return BoundingBox.Contains(aabb1, aabb2) == ContainmentType.Disjoint;
        }

        ///// <summary>
        ///// Определить дистанцию между двумя <see cref="BoundingBox"/>
        ///// </summary>
        ///// <param name="box1">The first box to test.</param>
        ///// <param name="box2">The second box to test.</param>
        ///// <returns>The distance between the two objects.</returns>
        //public static float Distance(this BoundingBox box1, BoundingBox box2)
        //{
        //    float distance = 0f;

        //    // Distance for X.
        //    if (box1.Minimum.X > box2.Maximum.X)
        //    {
        //        float delta = box2.Maximum.X - box1.Minimum.X;
        //        distance += delta * delta;
        //    }
        //    else if (box2.Minimum.X > box1.Maximum.X)
        //    {
        //        float delta = box1.Maximum.X - box2.Minimum.X;
        //        distance += delta * delta;
        //    }

        //    // Distance for Y.
        //    if (box1.Minimum.Y > box2.Maximum.Y)
        //    {
        //        float delta = box2.Maximum.Y - box1.Minimum.Y;
        //        distance += delta * delta;
        //    }
        //    else if (box2.Minimum.Y > box1.Maximum.Y)
        //    {
        //        float delta = box1.Maximum.Y - box2.Minimum.Y;
        //        distance += delta * delta;
        //    }

        //    //// Distance for Z.
        //    //if (box1.Minimum.Z > box2.Maximum.Z)
        //    //{
        //    //    float delta = box2.Maximum.Z - box1.Minimum.Z;
        //    //    distance += delta * delta;
        //    //}
        //    //else if (box2.Minimum.Z > box1.Maximum.Z)
        //    //{
        //    //    float delta = box1.Maximum.Z - box2.Minimum.Z;
        //    //    distance += delta * delta;
        //    //}

        //    return distance == 0 ? 0f : (float)Math.Sqrt(distance);
        //}
    }
}
