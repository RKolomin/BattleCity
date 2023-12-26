using BattleCity.Common;
using BattleCity.Enums;
using SlimDX.DirectInput;
using System.Collections.Concurrent;

namespace BattleCity.InputControllers
{
    public class KeyboardController : IKeyboardController
    {
        IGameApplication gameApplication;
        Keyboard keyboard;
        DirectInput di = new DirectInput();
        KeyboardState keyboardCurrentState = new KeyboardState();
        KeyboardState keyboardLastState = new KeyboardState();
        ConcurrentDictionary<Key, int> longPressKeys = new ConcurrentDictionary<Key, int>();

        /// <summary>
        /// Спустя сколько кадров нажатие кнопки будет считаться долгим нажатием
        /// </summary>
        readonly int longPressFramesDelay = 30;

        /// <summary>
        /// Частота воспроизведения долгого нажатия кнопки (т.е. каждые N-кадров)
        /// </summary>
        readonly int longPressFramesRepeat = 10;

        public string Id { get; }
        public string Name { get; }

        public KeyboardController(IGameApplication gameApplication, int longPressFramesDelay, int longPressFramesRepeat)
        {
            this.longPressFramesDelay = longPressFramesDelay;
            this.longPressFramesRepeat = longPressFramesRepeat;
            this.gameApplication = gameApplication;
            Id = nameof(InputDeviceType.Keyboard);
            Name = nameof(InputDeviceType.Keyboard);
            keyboard = new Keyboard(di);
            keyboard.Acquire();
        }

        public void Update()
        {
            if (!gameApplication.IsActive)
            {
                keyboardLastState = new KeyboardState();
                keyboardCurrentState = new KeyboardState();
                return;
            }

            keyboardLastState = keyboardCurrentState;
            keyboard.Poll();
            keyboardCurrentState = keyboard.GetCurrentState();

            foreach (var keybKey in keyboardCurrentState.PressedKeys)
            {
                longPressKeys.AddOrUpdate(keybKey, 0, (dkey, oldVal) => oldVal + 1);
            }

            foreach (var keybKey in keyboardLastState.PressedKeys)
            {
                if (!keyboardCurrentState.IsPressed(keybKey))
                    longPressKeys.TryRemove(keybKey, out _);
            }
        }

        public void Dispose()
        {
            if (keyboard != null)
            {
                if (!keyboard.Disposed)
                    keyboard.Unacquire();
                keyboard.Dispose();
                keyboard = null;
            }
            if (di != null)
            {
                di.Dispose();
                di = null;
            }
            gameApplication = null;
            longPressKeys = null;
            keyboardCurrentState = null;
            keyboardLastState = null;

            Disposed = true;
        }

        public KeyboardState CurrentState => keyboardCurrentState;
        public KeyboardState PreviousState => keyboardLastState;

        public bool Disposed { get; private set; }

        public bool IsReleased(int key) => gameApplication.IsActive && PreviousState.IsPressed((Key)key) && !CurrentState.IsPressed((Key)key);
        public bool IsPressed(int key) => gameApplication.IsActive && CurrentState.IsPressed((Key)key);
        public bool IsDown(int key) => gameApplication.IsActive && !PreviousState.IsPressed((Key)key) && CurrentState.IsPressed((Key)key);

        public bool IsLongPress(int key, int period, int repeatPeriod)
        {
            if (gameApplication.IsActive && longPressKeys.TryGetValue((Key)key, out int times))
            {
                return /*times == 0 || */(times >= period && times % repeatPeriod == 0);
            }
            return false;
        }

        public bool IsLongPress(int key)
        {
            if (gameApplication.IsActive && longPressKeys.TryGetValue((Key)key, out int times))
            {
                return /*times == 0 || */(times >= longPressFramesDelay && times % longPressFramesRepeat == 0);
            }
            return false;
        }
    }
}
