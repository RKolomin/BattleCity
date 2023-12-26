using System;

namespace BattleCity.InputControllers
{
    public interface IControllerHub : IDisposable
    {
        IKeyboardController Keyboard { get; }

        void AddController(IInputController controller);

        void RemoveController(IInputController controller);

        void AddButtonsMap(IButtonsMap map);

        void RemoveButtonsMap(IButtonsMap map);

        void Update();

        bool IsKeyPressed(int playerId, string buttonId, bool currentFrameOnly);

        bool IsKeyReleased(int playerId, string buttonId);

        bool IsLongPressed(int playerId, string buttonId);
    }
}
