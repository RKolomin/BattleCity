using SlimDX.XInput;

namespace BattleCity.InputControllers
{
    /// <summary>
    /// Перечисление кнопок XInput (Xbox360)
    /// </summary>
    public sealed class XInputKeys
    {
        public bool IsGamepadButtonFlags(int key)
        {
            return key >= 0 && key <= short.MaxValue;
        }

        public const int
        /// <summary>
        /// Стрелка Up
        /// </summary>
        Up = (int)GamepadButtonFlags.DPadUp,

        /// <summary>
        /// Стрелка Down
        /// </summary>
        Down = (int)GamepadButtonFlags.DPadDown,

        /// <summary>
        /// Стрелка Left
        /// </summary>
        Left = (int)GamepadButtonFlags.DPadLeft,

        /// <summary>
        /// Стрелка Right
        /// </summary>
        Right = (int)GamepadButtonFlags.DPadRight,

        /// <summary>
        /// Кнопка Back (Select)
        /// </summary>
        Back = (int)GamepadButtonFlags.Back,

        /// <summary>
        /// Кнопка Start
        /// </summary>
        Start = (int)GamepadButtonFlags.Start,

        /// <summary>
        /// Кнопка A
        /// </summary>
        A = (int)GamepadButtonFlags.A,

        /// <summary>
        /// Кнопка B
        /// </summary>
        B = (int)GamepadButtonFlags.B,

        /// <summary>
        /// Кнопка Y
        /// </summary>
        Y = (int)GamepadButtonFlags.Y,

        /// <summary>
        /// Кнопка X
        /// </summary>
        X = (int)GamepadButtonFlags.X,

        /// <summary>
        /// Кнопка L1
        /// </summary>
        L1 = (int)GamepadButtonFlags.LeftShoulder,

        /// <summary>
        /// Кнопка L2
        /// </summary>
        L2 = short.MaxValue + 1,

        /// <summary>
        /// Кнопка L3
        /// </summary>
        L3 = (int)GamepadButtonFlags.LeftThumb,

        /// <summary>
        /// Кнопка R1
        /// </summary>
        R1 = (int)GamepadButtonFlags.RightShoulder,

        /// <summary>
        /// Кнопка R2
        /// </summary>
        R2 = short.MaxValue + 2,

        /// <summary>
        /// Кнопка R3
        /// </summary>
        R3 = (int)GamepadButtonFlags.RightThumb;
    }
}
