using System;

namespace BattleCity
{
    static class MathF
    {
        public static readonly float D3DX_PI = (float)Math.PI;

        /// <summary>
        /// Lerp
        /// </summary>
        /// <param name="firstFloat">minimum</param>
        /// <param name="secondFloat">maximum</param>
        /// <param name="by">current value</param>
        /// <returns></returns>
        public static float Lerp(float firstFloat, float secondFloat, float by)
        {
            return firstFloat * (1 - by) + secondFloat * by;
        }

        /// <summary>
        /// Lerp
        /// </summary>
        /// <param name="firstFloat">minimum</param>
        /// <param name="secondFloat">maximum</param>
        /// <param name="by">current value</param>
        /// <returns></returns>
        public static double Lerp(double firstFloat, double secondFloat, double by)
        {
            return firstFloat * (1 - by) + secondFloat * by;
        }

    }
}
