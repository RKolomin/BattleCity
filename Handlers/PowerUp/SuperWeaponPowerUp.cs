using BattleCity.GameObjects;

namespace BattleCity.Handlers.PowerUp
{
    public class SuperWeaponPowerUp : IPowerUpHandler
    {
        private readonly BattleGround battleGround;

        public SuperWeaponPowerUp(BattleGround battleGround)
        {
            this.battleGround = battleGround;
        }

        public void Handle(BattleUnit unit, GameFieldObject powerUpObj)
        {
            if (unit.IsUser)
            {
                battleGround?.UpgradePlayerUnit(unit, 3);
            }
            else
            {
                if (battleGround.Config.EnemyPowerUpHasEffect)
                    battleGround?.UpgradeEnemyUnit(unit, 3);
            }
        }
    }
}
