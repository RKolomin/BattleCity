using SlimDX.Multimedia;

namespace BattleCity.Extensions
{
    public static class WaveFormatExtensions
    {
        /// <summary>
        /// Gets the size of a wave buffer equivalent to the latency in milliseconds.
        /// </summary>
        /// <param name="milliseconds">The milliseconds.</param>
        /// <returns></returns>
        public static int ConvertLatencyToByteSize(this WaveFormat waveFormat, long milliseconds)
        {
            int bytes = (int)((waveFormat.AverageBytesPerSecond / 1000.0) * milliseconds);
            if ((bytes % waveFormat.BlockAlignment) != 0)
            {
                // Return the upper BlockAligned
                bytes = bytes + waveFormat.BlockAlignment - (bytes % waveFormat.BlockAlignment);
            }
            return bytes;
        }
    }
}
