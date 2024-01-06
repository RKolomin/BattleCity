using BattleCity.Common;
using BattleCity.GameObjects;

namespace BattleCity.Handlers.PowerUp
{
    public class LifeUpPowerUp : IPowerUpHandler
    {
        private readonly BattleGround battleGround;

        public LifeUpPowerUp(BattleGround battleGround)
        {
            this.battleGround = battleGround;
        }

        public void Handle(BattleUnit unit, GameFieldObject powerUpObj)
        {
            if (unit.IsUser)
            {
                Player player = battleGround.GetPlayerByUnit(unit);
                if (player != null)
                {
                    battleGround.AddPlayerLifeUp(player, 1, true);
                }
            }
            else if (unit is EnemyUnit enemyUnit)
            {
                if (battleGround.Config.EnemyPowerUpHasEffect)
                    battleGround?.UpgradeEnemyToBonusUnit(enemyUnit);
            }
        }
    }
}
