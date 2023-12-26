using BattleCity.GameObjects;

namespace BattleCity.Handlers.PowerUp
{
    public class FreezeEnemyPowerUp : IPowerUpHandler
    {
        private readonly BattleGround battleGround;

        public FreezeEnemyPowerUp(BattleGround battleGround)
        {
            this.battleGround = battleGround;
        }

        public void Handle(BattleUnit unit, GameFieldObject powerUpObj)
        {
            if (unit.IsUser)
            {
                battleGround?.FreezeAllActiveEnemies();
            }
            else
            {
                if (battleGround.Config.EnemyPowerUpHasEffect)
                    battleGround?.FreezeAllActivePlayers();
            }
        }
    }
}
