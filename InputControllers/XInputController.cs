using BattleCity.Common;
using SlimDX.XInput;
using System;
using System.Collections.Concurrent;
using System.Linq;

namespace BattleCity.InputControllers
{
    public class XInputController : IInputController
    {
        Controller controller;
        Gamepad currentState;
        Gamepad lastState;
        IGameApplication gameApplication;
        ConcurrentDictionary<int, int> longPressKeys = new ConcurrentDictionary<int, int>();

        public string Id { get; }
        public string Name { get; }

        public XInputController(int playerNumber, IGameApplication gameApplication)
        {
            this.gameApplication = gameApplication;
            controller = new Controller((UserIndex)(playerNumber - 1));
            Id = playerNumber.ToString();
            Name = $"{nameof(XInputController)}: {playerNumber}";
        }

        public void Update()
        {
            if (!gameApplication.IsActive || !controller.IsConnected)
            {
                currentState = new Gamepad();
                lastState = new Gamepad();
                longPressKeys.Clear();
                return;
            }

            lastState = currentState;
            var state = controller.GetState();
            currentState = state.Gamepad;

            var buttons = Enum.GetValues(typeof(GamepadButtonFlags))
                .Cast<GamepadButtonFlags>()
                .Select(s => (int)s)
                .ToArray();

            foreach (var button in buttons)
            {
                if (currentState.Buttons.HasFlag((GamepadButtonFlags)button))
                {
                    longPressKeys.AddOrUpdate(button, 0, (dkey, oldVal) => oldVal + 1);
                }
            }

            foreach (var button in buttons)
            {
                if (!lastState.Buttons.HasFlag((GamepadButtonFlags)button))
                {
                    longPressKeys.TryRemove(button, out _);
                }
            }
        }

        public bool IsPressed(int key)
        {
            return currentState.Buttons.HasFlag((GamepadButtonFlags)key);
        }

        public bool IsDown(int key)
        {
            return currentState.Buttons.HasFlag((GamepadButtonFlags)key) && !lastState.Buttons.HasFlag((GamepadButtonFlags)key);
        }

        public bool IsReleased(int key)
        {
            return !currentState.Buttons.HasFlag((GamepadButtonFlags)key) && lastState.Buttons.HasFlag((GamepadButtonFlags)key);
        }

        public bool IsLongPress(int key, int period, int repeatPeriod)
        {
            if (longPressKeys.TryGetValue(key, out int times))
            {
                return /*times == 0 || */(times >= period && times % repeatPeriod == 0);
            }
            return false;
        }

        public void Dispose()
        {
            controller = null;
            gameApplication = null;
            longPressKeys = null;
        }
    }
}
