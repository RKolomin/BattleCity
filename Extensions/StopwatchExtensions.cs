#if NET35
using System.Diagnostics;

namespace BattleCity.Extensions
{
    public static class StopwatchExtensions
    {
        public static void Restart(this Stopwatch stopwatch)
        {
            stopwatch.Reset();
            stopwatch.Start();
        }
    }
}
#endif