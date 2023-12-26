using BattleCity.GameObjects;

namespace BattleCity.VisualComponents
{
    public class EnemyDrawBlock
    {
        public GameFieldObject DrawObject { get; set; }

        public string HexColor { get; set; }

        public int X { get; set; }

        public int Y { get; set; }
    }
}
