using BattleCity.Common;
using BattleCity.Enums;
using System.Collections.Generic;
using System.Linq;

namespace BattleCity.InputControllers
{
    public class ControllerHub : IControllerHub
    {
        readonly List<IButtonsMap> buttonsMaps = new List<IButtonsMap>();
        readonly Dictionary<string, IInputController> controllers = new Dictionary<string, IInputController>();

        public IKeyboardController Keyboard { get; }

        /// <summary>
        /// Спустя сколько кадров нажатие кнопки будет считаться долгим нажатием
        /// </summary>
        const int longPressFramesDelay = 30;

        /// <summary>
        /// Частота воспроизведения долгого нажатия кнопки (т.е. каждые N-кадров)
        /// </summary>
        const int longPressFramesRepeat = 10;

        public ControllerHub(IGameApplication gameApplication)
        {
            Keyboard = new KeyboardController(gameApplication, longPressFramesDelay, longPressFramesRepeat);
        }

        public void AddController(IInputController controller)
        {
            controllers.Add(controller.Id, controller);
        }

        public void RemoveController(IInputController controller)
        {
            if (controllers.ContainsKey(controller.Id))
                controllers.Remove(controller.Id);
        }

        public void AddButtonsMap(IButtonsMap map)
        {
            buttonsMaps.Add(map);
        }

        public void RemoveButtonsMap(IButtonsMap map)
        {
            buttonsMaps.Remove(map);
        }

        public void Update()
        {
            Keyboard.Update();
            foreach (var controller in controllers)
            {
                controller.Value.Update();
            }
        }

        private IInputController GetControllerById(string id)
        {
            if (string.IsNullOrEmpty(id)) return null;
            controllers.TryGetValue(id, out var controller);
            return controller;
        }

        public bool IsKeyPressed(int playerId, string buttonId, bool currentFrameOnly)
        {
            var collection = playerId == 0
                ? buttonsMaps
                : buttonsMaps.Where(p => p.Player == playerId);

            foreach (var map in collection)
            {
                var button = map.Get(buttonId);

                if (button == null)
                    continue;

                var controller = map.DeviceType == InputDeviceType.Keyboard
                ? Keyboard
                : GetControllerById(map.DeviceId);

                if (controller != null)
                {
                    if (currentFrameOnly)
                    {
                        if (button.KeyCodePrimary.HasValue && controller.IsDown(button.KeyCodePrimary.Value))
                            return true;

                        if (button.KeyCodeSecondary.HasValue && controller.IsDown(button.KeyCodeSecondary.Value))
                            return true;
                    }
                    else
                    {
                        if (button.KeyCodePrimary.HasValue && controller.IsPressed(button.KeyCodePrimary.Value))
                            return true;

                        if (button.KeyCodeSecondary.HasValue && controller.IsPressed(button.KeyCodeSecondary.Value))
                            return true;
                    }
                }
            }

            return false;
        }

        public bool IsKeyReleased(int playerId, string buttonId)
        {
            var collection = playerId == 0
                 ? buttonsMaps
                 : buttonsMaps.Where(p => p.Player == playerId);

            foreach (var map in collection)
            {
                var button = map.Get(buttonId);

                if (button == null)
                    continue;

                var controller = map.DeviceType == InputDeviceType.Keyboard
                    ? Keyboard
                    : GetControllerById(map.DeviceId);

                if (controller != null)
                {
                    if (button.KeyCodePrimary.HasValue && controller.IsReleased(button.KeyCodePrimary.Value))
                        return true;

                    if (button.KeyCodeSecondary.HasValue && controller.IsReleased(button.KeyCodeSecondary.Value))
                        return true;
                }
            }

            return false;
        }

        public bool IsLongPressed(int playerId, string buttonId)
        {
            var collection = playerId == 0
                ? buttonsMaps
                : buttonsMaps.Where(p => p.Player == playerId);

            foreach (var map in collection)
            {
                var button = map.Get(buttonId);

                if (button == null)
                    continue;

                var controller = map.DeviceType == InputDeviceType.Keyboard
                    ? Keyboard
                    : GetControllerById(map.DeviceId);

                if (controller != null)
                {
                    if (button.KeyCodePrimary.HasValue &&
                        controller.IsLongPress(button.KeyCodePrimary.Value, longPressFramesDelay, longPressFramesRepeat))
                        return true;

                    if (button.KeyCodeSecondary.HasValue &&
                        controller.IsLongPress(button.KeyCodeSecondary.Value, longPressFramesDelay, longPressFramesRepeat))
                        return true;
                }
            }

            return false;
        }

        public void Dispose()
        {
            Keyboard?.Dispose();
            if (controllers != null)
            {
                foreach (var controller in controllers)
                {
                    controller.Value?.Dispose();
                }
            }
        }
    }
}
