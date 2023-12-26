using BattleCity.GameObjects;

namespace BattleCity.Handlers.PowerUp
{
    public class ShieldPowerUp : IPowerUpHandler
    {
        private readonly BattleGround battleGround;

        public ShieldPowerUp(BattleGround battleGround)
        {
            this.battleGround = battleGround;
        }

        public void Handle(BattleUnit unit, GameFieldObject powerUpObj)
        {
            if (unit.IsUser)
            {
                battleGround?.AddExtraShield(unit);
            }
            else if (unit is EnemyUnit enemyUnit)
            {
                if (battleGround.Config.EnemyPowerUpHasEffect)
                    battleGround.UpgradeEnemyUnitHealth(enemyUnit);
            }
        }
    }
}
