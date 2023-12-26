using SlimDX;
using System;
using System.Globalization;

namespace BattleCity
{
    public static class ColorConverter
    {
        private static byte[] GetArgb(string hexValue)
        {
            hexValue = hexValue.TrimStart('#');
            byte[] values = new byte[4];
            if (hexValue.Length == 8)
            {
                values[3] = byte.Parse(hexValue.Substring(0, 2), NumberStyles.HexNumber);
                values[2] = byte.Parse(hexValue.Substring(2, 2), NumberStyles.HexNumber);
                values[1] = byte.Parse(hexValue.Substring(4, 2), NumberStyles.HexNumber);
                values[0] = byte.Parse(hexValue.Substring(6, 2), NumberStyles.HexNumber);
            }
            else
            {
                values[3] = 255;
                values[2] = byte.Parse(hexValue.Substring(0, 2), NumberStyles.HexNumber);
                values[1] = byte.Parse(hexValue.Substring(2, 2), NumberStyles.HexNumber);
                values[0] = byte.Parse(hexValue.Substring(4, 2), NumberStyles.HexNumber);
            }

            return values;
        }

        public static int ToInt32(string hexValue)
        {
            if (string.IsNullOrEmpty(hexValue))
                return int.Parse("FFFFFFFF", NumberStyles.HexNumber);

            byte[] values = GetArgb(hexValue);
            return BitConverter.ToInt32(values, 0);
        }

        public static Color4 ToColor4(string hexValue)
        {
            return new Color4(ToInt32(hexValue));
        }

        public static Color3 ToColor3(string hexValue)
        {
            return new Color4(ToInt32(hexValue)).ToColor3();
        }

        public static Vector3 ToVector3(string hexValue)
        {
            return new Color4(ToInt32(hexValue)).ToVector3();
        }

        public static Vector4 ToVector4(string hexValue)
        {
            return new Color4(ToInt32(hexValue)).ToVector4();
        }
    }
}