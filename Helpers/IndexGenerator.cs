using System.Threading;

namespace BattleCity.Helpers
{
    class IndexGenerator
    {
        private int index;

        public void Reset(int initialIndex = 0)
        {
            Interlocked.Exchange(ref index, initialIndex);
        }

        public int Next()
        {
            return Interlocked.Add(ref index, 1);
        }
    }
}
