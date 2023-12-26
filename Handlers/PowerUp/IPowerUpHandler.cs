using BattleCity.GameObjects;

namespace BattleCity.Handlers.PowerUp
{
    public interface IPowerUpHandler
    {
        void Handle(BattleUnit unit, GameFieldObject powerUpObj);
    }
}
