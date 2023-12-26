using System;
using System.Runtime.InteropServices;

namespace BattleCity.Audio.Decoders
{
    /// <summary>
    /// A FourCC descriptor.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Size = 4)]
    internal readonly struct FourCC : IEquatable<FourCC>
    {
        private readonly uint value;

        public static uint Get(char ch0, char ch1, char ch2, char ch3)
        {
            uint Temp; // The temporary variable.
            // Convert each of the input characters and
            // bit shift as needed.
            Temp = Convert.ToUInt32(ch0);
            Temp += Convert.ToUInt32(ch1) * 0x100;
            Temp += Convert.ToUInt32(ch2) * 0x10000;
            Temp += Convert.ToUInt32(ch3) * 0x1000000;
            return Temp;
        }

        public static bool IsMatch(byte[] source, byte[] dest)
        {
            int i = dest.Length;
            while (-1 < --i)
                if (dest[i] != source[i]) return false;
            return true;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FourCC" /> struct.
        /// </summary>
        /// <param name="fourCC">The fourCC value as a string .</param>
        public FourCC(string fourCC)
        {
            if (fourCC.Length != 4)
                throw new ArgumentException(string.Format(System.Globalization.CultureInfo.InvariantCulture, "Invalid length for FourCC(\"{0}\". Must be be 4 characters long ", fourCC), "fourCC");
            this.value = ((uint)fourCC[3]) << 24 | ((uint)fourCC[2]) << 16 | ((uint)fourCC[1]) << 8 | ((uint)fourCC[0]);
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="FourCC" /> struct.
        /// </summary>
        /// <param name="byte1">The byte1.</param>
        /// <param name="byte2">The byte2.</param>
        /// <param name="byte3">The byte3.</param>
        /// <param name="byte4">The byte4.</param>
        public FourCC(char byte1, char byte2, char byte3, char byte4)
        {
            this.value = ((uint)byte4) << 24 | ((uint)byte3) << 16 | ((uint)byte2) << 8 | ((uint)byte1);
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="FourCC" /> struct.
        /// </summary>
        /// <param name="fourCC">The fourCC value as an uint.</param>
        public FourCC(uint fourCC)
        {
            this.value = fourCC;
        }


        /// <summary>
        /// Initializes a new instance of the <see cref="FourCC" /> struct.
        /// </summary>
        /// <param name="fourCC">The fourCC value as an int.</param>
        public FourCC(int fourCC)
        {
            this.value = unchecked((uint)fourCC);
        }


        /// <summary>
        /// Performs an implicit conversion from <see cref="SharpDX.Multimedia.FourCC"/> to <see cref="System.Int32"/>.
        /// </summary>
        /// <param name="d">The d.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        public static implicit operator uint(FourCC d)
        {
            return d.value;
        }


        /// <summary>
        /// Performs an implicit conversion from <see cref="SharpDX.Multimedia.FourCC"/> to <see cref="System.Int32"/>.
        /// </summary>
        /// <param name="d">The d.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        public static implicit operator int(FourCC d)
        {
            return unchecked((int)d.value);
        }


        /// <summary>
        /// Performs an implicit conversion from <see cref="System.Int32"/> to <see cref="SharpDX.Multimedia.FourCC"/>.
        /// </summary>
        /// <param name="d">The d.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        public static implicit operator FourCC(uint d)
        {
            return new FourCC(d);
        }


        /// <summary>
        /// Performs an implicit conversion from <see cref="System.Int32"/> to <see cref="SharpDX.Multimedia.FourCC"/>.
        /// </summary>
        /// <param name="d">The d.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        public static implicit operator FourCC(int d)
        {
            return new FourCC(d);
        }


        /// <summary>
        /// Performs an implicit conversion from <see cref="SharpDX.Multimedia.FourCC"/> to <see cref="System.String"/>.
        /// </summary>
        /// <param name="d">The d.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        public static implicit operator string(FourCC d)
        {
            return d.ToString();
        }


        /// <summary>
        /// Performs an implicit conversion from <see cref="System.String"/> to <see cref="SharpDX.Multimedia.FourCC"/>.
        /// </summary>
        /// <param name="d">The d.</param>
        /// <returns>
        /// The result of the conversion.
        /// </returns>
        public static implicit operator FourCC(string d)
        {
            return new FourCC(d);
        }


        public override string ToString()
        {
            return string.Format("{0}(0x{1:X})", new string(new[]
                                  {
                                      (char) (value & 0xFF),
                                      (char) ((value >> 8) & 0xFF),
                                      (char) ((value >> 16) & 0xFF),
                                      (char) ((value >> 24) & 0xFF),
                                  }), value);
        }


        public bool Equals(FourCC other)
        {
            return value == other.value;
        }


        public override bool Equals(object obj)
        {
            if (obj is null) return false;
            return obj is FourCC fourCC && Equals(fourCC);
        }


        public override int GetHashCode()
        {
            return (int)value;
        }


        public static bool operator ==(FourCC left, FourCC right)
        {
            return left.Equals(right);
        }


        public static bool operator !=(FourCC left, FourCC right)
        {
            return !left.Equals(right);
        }
    }
}
