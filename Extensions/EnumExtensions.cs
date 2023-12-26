#if NET35

using System;

namespace BattleCity.Extensions
{
    public static class EnumExtensions
    {
        /// <summary>
        /// Check to see if a flags enumeration has a specific flag set.
        /// </summary>
        /// <param name="variable">Flags enumeration to check</param>
        /// <param name="value">Flag to check for</param>
        /// <returns></returns>
        public static bool HasFlag(this Enum variable, Enum value)
        {
            if (variable == null)
                return false;

            if (value == null)
                throw new ArgumentNullException("value");

            // Not as good as the .NET 4 version of this function, but should be good enough
            if (!Enum.IsDefined(variable.GetType(), value))
            {
                throw new ArgumentException(string.Format(
                    "Enumeration type mismatch.  The flag is of type '{0}', was expecting '{1}'.",
                    value.GetType(), variable.GetType()));
            }

            //ulong num = Convert.ToUInt64(value);
            ulong num = (ulong)typeof(Enum)
                .GetMethod("ToUInt64", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)
                .Invoke(null, new object[] { value });
            ulong variableUI64 = (ulong)typeof(Enum)
                .GetMethod("ToUInt64", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static)
                .Invoke(null, new object[] { variable });
            //ulong variableUI64 = Convert.ToUInt64(variable);
            return (variableUI64 & num) == num;

        }
    }
}

#endif