using BattleCity.Enums;
using System.Collections.Generic;
using System.Linq;

namespace BattleCity.InputControllers
{
    public class ButtonsMap : IButtonsMap
    {
        private readonly List<ControllerButton> buttons = new List<ControllerButton>();

        public InputDeviceType DeviceType { get; }

        public int Player { get; }

        public string DeviceName { get; private set; }

        public string DeviceId { get; private set; }

        public ButtonsMap(int player, InputDeviceType deviceType, List<ControllerButton> buttons,
            string deviceId = null, string deviceName = null)
        {
            Player = player;
            DeviceType = deviceType;
            this.buttons = buttons;
            SetDevice(deviceId, deviceName);
        }

        public ControllerButton Get(string buttonId)
        {
            return buttons.FirstOrDefault(p => p.ButtonId == buttonId);
        }

        public void SetDevice(string deviceId, string deviceName)
        {
            DeviceId = deviceId;
            DeviceName = deviceName;
        }
    }
}
