using BattleCity.Common;
using System.Reflection;

namespace BattleCity.Extensions
{
    public static class GameConfigExtensions
    {
        /// <summary>
        /// Скопировать конфигурации
        /// </summary>
        /// <param name="currentConfig"></param>
        /// <param name="config"></param>
        public static void CopyFrom(this GameConfig currentConfig, GameConfig config)
        {
            foreach (var p in typeof(GameConfig).GetProperties(
                BindingFlags.Public |
                BindingFlags.SetProperty |
                BindingFlags.Instance))
            {
                if (!p.CanRead || !p.CanWrite) continue;
                if (p.PropertyType.IsArray)
                    continue;
                var value = p.GetValue(config, null);
                p.SetValue(currentConfig, value, null);
            }
        }
    }
}
