using BattleCity.Video;
using SlimDX;

namespace BattleCity.Common
{
    /// <summary>
    /// Объект строительства
    /// </summary>
    public class BlockPlaceholder
    {
        /// <summary>
        /// X - координата в условных единицах
        /// </summary>
        public int X { get; set; }

        /// <summary>
        /// Y - координата в условных единицах
        /// </summary>
        public int Y { get; set; }

        /// <summary>
        /// Шинина в условных единицах
        /// </summary>
        public int Width { get { return CreatableObject == null ? 2 : CreatableObject.Item.Width; } }

        /// <summary>
        /// Длина в условных единицах
        /// </summary>
        public int Height { get { return CreatableObject == null ? 2 : CreatableObject.Item.Height; } }

        /// <summary>
        /// Связка блоков по горизонтали
        /// </summary>
        public int BunchBlocksHorizontanlly { get; private set; } = 1;

        /// <summary>
        /// Связка блоков по вертикали
        /// </summary>
        public int BunchBlocksVertically { get; private set; } = 1;

        /// <summary>
        /// Иекущий объект конструктора
        /// </summary>
        public ConstructionObject CreatableObject { get; private set; }

        /// <summary>
        /// Название текущего объекта
        /// </summary>
        public string Name => CreatableObject == null ? "<NULL>" : CreatableObject.Item.Name;

        /// <summary>
        /// Задать объект конструктора
        /// </summary>
        /// <param name="creatableObject"></param>
        public void Set(ConstructionObject creatableObject)
        {
            if (creatableObject != null)
            {
                CreatableObject = creatableObject;
                if (CreatableObject.Item.Width > 0 && CreatableObject.Item.Width % 2 == 0)
                {
                    X -= X % 2;
                    CreatableObject.Item.X -= CreatableObject.Item.X % 2;
                }
                if (CreatableObject.Item.Height > 0 && CreatableObject.Item.Height % 2 == 0)
                {
                    Y -= Y % 2;
                    CreatableObject.Item.Y -= CreatableObject.Item.Y % 2;
                }

                BunchBlocksHorizontanlly = creatableObject.BunchBlocksHorizontanlly;
                BunchBlocksVertically = creatableObject.BunchBlocksVertically;
            }
            else
            {
                CreatableObject = null;
                BunchBlocksHorizontanlly = 1;
                BunchBlocksVertically = 1;
            }
        }

        public void Draw(IGameGraphics graphics, int left, int top, int cellSize, int frameNumber, int flickerFrames)
        {
            bool draw = flickerFrames == 0 || (frameNumber / flickerFrames) % 2 == 0;

            if (CreatableObject != null && draw)
            {
                for (int m = 0; m < BunchBlocksHorizontanlly; m++)
                {
                    for (int n = 0; n < BunchBlocksVertically; n++)
                    {
                        CreatableObject.Item.X = X + m * CreatableObject.Item.Width;
                        CreatableObject.Item.Y = Y + n * CreatableObject.Item.Height;
                        graphics.BeginDrawGameObjects();
                        graphics.DrawGameObject(left, top, CreatableObject.Item, 0, cellSize);
                        graphics.EndDrawGameObjects();
                    }
                }
            }

            // draw placeholder borders

            var x = left + X * cellSize;
            var y = top + Y * cellSize;
            var w = Width * BunchBlocksHorizontanlly * cellSize;
            var h = Height * BunchBlocksVertically * cellSize;
            var c = new Color4(1, 1, 1, 1).ToArgb();
            graphics.DrawBorderRect(x, y, w, h, c);
        }
    }
}