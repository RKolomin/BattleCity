using BattleCity.GameObjects;

namespace BattleCity.Handlers.PowerUp
{
    public class WeaponUpgradePowerUp : IPowerUpHandler
    {
        private readonly BattleGround battleGround;

        public WeaponUpgradePowerUp(BattleGround battleGround)
        {
            this.battleGround = battleGround;
        }

        public void Handle(BattleUnit unit, GameFieldObject powerUpObj)
        {
            if (unit.IsUser)
            {
                battleGround?.UpgradePlayerUnit(unit);
            }
            else
            {
                if (battleGround.Config.EnemyPowerUpHasEffect)
                    battleGround?.UpgradeEnemyUnit(unit);
            }
        }
    }
}
