using BattleCity.GameObjects;

namespace BattleCity.Handlers.PowerUp
{
    public class KillEnemyPowerUp : IPowerUpHandler
    {
        private readonly BattleGround battleGround;

        public KillEnemyPowerUp(BattleGround battleGround)
        {
            this.battleGround = battleGround;
        }

        public void Handle(BattleUnit unit, GameFieldObject powerUpObj)
        {
            if (unit.IsUser)
            {
                battleGround?.DestroyAllActiveEnemies();
            }
            else
            {
                if (battleGround.Config.EnemyPowerUpHasEffect)
                    battleGround?.DestroyAllPlayers();
            }
        }
    }
}
