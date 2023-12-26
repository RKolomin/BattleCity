using BattleCity.Extensions;
using BattleCity.GameObjects;

namespace BattleCity.Common
{
    /// <summary>
    /// Строительный объект
    /// </summary>
    public class ConstructionObject
    {
        /// <summary>
        /// Связка блоков по горизонтали
        /// </summary>
        public int BunchBlocksHorizontanlly { get; set; } = 1;

        /// <summary>
        /// Связка блоков по вертикали
        /// </summary>
        public int BunchBlocksVertically { get; set; } = 1;

        /// <summary>
        /// Объект поля
        /// </summary>
        public GameFieldObject Item { get; private set; }

        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="item"></param>
        /// <param name="bunchBlocksHorizontanlly"></param>
        /// <param name="bunchBlocksVertically"></param>
        public ConstructionObject(GameFieldObject item, int bunchBlocksHorizontanlly, int bunchBlocksVertically)
        {
            BunchBlocksHorizontanlly = bunchBlocksHorizontanlly;
            BunchBlocksVertically = bunchBlocksVertically;
            Item = item.Clone();
        }
    }
}
