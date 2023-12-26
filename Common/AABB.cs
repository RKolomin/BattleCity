using BattleCity.Enums;
using BattleCity.GameObjects;
using SlimDX;
using System;

namespace BattleCity.Common
{
    /// <summary>
    /// Axis aligned bounding box
    /// </summary>
    public static class AABB
    {
        /// <summary>
        /// Create an axis aligned bounding box
        /// </summary>
        /// <param name="gameObject">Объект для которого создается <see cref="BoundingBox"/></param>
        /// <param name="scale">Масштабирование</param>
        /// <returns></returns>
        public static BoundingBox Create(GameFieldObject gameObject, int scale)
        {
            int width, height;

            if (gameObject.Direction == MoveDirection.Right || gameObject.Direction == MoveDirection.Left)
            {
                width = gameObject.Height;
                height = gameObject.Width;
            }
            else
            {
                width = gameObject.Width;
                height = gameObject.Height;
            }

            Vector3 leftTop = new Vector3(
                    gameObject.X * scale + gameObject.SubPixelX,
                    gameObject.Y * scale + gameObject.SubPixelY,
                    0);

            return new BoundingBox(
                leftTop,
                leftTop + new Vector3(width * scale, height * scale, 0)
                );
        }

        /// <summary>
        /// Create an axis aligned bounding box
        /// </summary>
        /// <param name="scale">Масштабирование</param>
        public static BoundingBox Create(int x, int y, int width, int height, int scale = 1)
        {
            Vector3 leftTop = new Vector3(
                    x * scale,
                    y * scale,
                    0);

            return new BoundingBox(
                leftTop,
                leftTop + new Vector3(width * scale, height * scale, 0)
                );
        }

        /// <summary>
        /// Определить дистанцию между областями по указанной границе примыкания
        /// </summary>
        /// <param name="aabb1"></param>
        /// <param name="aabb2"></param>
        /// <param name="axisAlignment"></param>
        /// <returns></returns>
        public static float Distance(BoundingBox aabb1, BoundingBox aabb2, MoveDirection axisAlignment)
        {
            switch (axisAlignment)
            {
                case MoveDirection.Up:
                    return Math.Abs(aabb1.Maximum.Y - aabb2.Maximum.Y);
                case MoveDirection.Down:
                    return Math.Abs(aabb1.Minimum.Y - aabb2.Minimum.Y);
                case MoveDirection.Right:
                    return Math.Abs(aabb1.Maximum.X - aabb2.Maximum.X);
                case MoveDirection.Left:
                    return Math.Abs(aabb1.Minimum.X - aabb2.Minimum.X);
            }

            return 0;
        }
    }
}
