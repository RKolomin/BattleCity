using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace BattleCity.Extensions
{
    static class TypeExtensions
    {
        public static IEnumerable<Type> GetAllAssignableFroms(this Type baseType)
        {
            foreach (var type in Assembly.GetAssembly(baseType).GetTypes().Where(x => baseType.IsAssignableFrom(x) && !x.IsAbstract))
            {
                yield return type;
                //if (type.IsInterface || type.IsAbstract) continue;
            }

            yield break;
        }
    }
}
