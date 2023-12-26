using BattleCity.Enums;

namespace BattleCity.InputControllers
{
    public interface IButtonsMap
    {
        InputDeviceType DeviceType { get; }

        ControllerButton Get(string buttonId);

        int Player { get; }

        string DeviceName { get; }

        string DeviceId { get; }
    }
}
