using BattleCity.Enums;
using BattleCity.Video;

namespace BattleCity.VisualComponents
{
    /// <summary>
    /// Меню опции настроек
    /// </summary>
    public class SettingMenuOption : MenuOption
    {
        public string DisplayValue { get; set; }
        public SettingSectionEnum Section { get; set; }

        public override void Draw(IGameFont font)
        {
            font.DrawString($"{Text} {DisplayValue}", X, Y, Color);
        }
    }
}
