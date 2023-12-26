using BattleCity.GameObjects;

namespace BattleCity.Handlers.PowerUp
{
    public class ShipPowerUp : IPowerUpHandler
    {
        private readonly BattleGround battleGround;

        public ShipPowerUp(BattleGround battleGround)
        {
            this.battleGround = battleGround;
        }

        public void Handle(BattleUnit unit, GameFieldObject powerUpObj)
        {
            if (unit.IsUser)
            {
                battleGround?.AddShipToUnit(unit);
            }
            else
            {
                if (battleGround.Config.EnemyPowerUpHasEffect)
                    battleGround?.AddShipToUnit(unit);
            }

        }
    }
}
