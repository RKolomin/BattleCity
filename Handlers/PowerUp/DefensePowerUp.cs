using BattleCity.GameObjects;

namespace BattleCity.Handlers.PowerUp
{
    public class DefensePowerUp : IPowerUpHandler
    {
        private readonly BattleGround battleGround;

        public DefensePowerUp(BattleGround battleGround)
        {
            this.battleGround = battleGround;
        }

        public void Handle(BattleUnit unit, GameFieldObject powerUpObj)
        {
            if (unit.IsUser)
            {
                battleGround?.CreateTowerDefense();
            }
            else
            {
                if (battleGround.Config.EnemyPowerUpHasEffect)
                {
                    battleGround?.ResetTowerDefense();
                    battleGround?.RemoveBlocksAroundTowers();
                }
            }            
        }
    }
}
