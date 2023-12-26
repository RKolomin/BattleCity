using BattleCity.GameObjects;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace BattleCity.Extensions
{
    public static class GameFieldObjectExtensions
    {
        public static BaseGameObject ToBaseGameObject(this GameFieldObject obj)
        {
            return new BaseGameObject()
            {
                Direction = obj.Direction,
                Id = obj.Id,
                Name = obj.Name,
                SubPixelX = obj.SubPixelX,
                SubPixelY = obj.SubPixelY,
                X = obj.X,
                Y = obj.Y,
            };
        }

        /// <summary>
        /// Копировать значения свойств из указанного объекта
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="V"></typeparam>
        /// <param name="item">Объект куда копировать</param>
        /// <param name="baseItem">Объект откуда копировать</param>
        /// <returns></returns>
        public static T CopyFrom<T, V>(this T item, V baseItem) where T : GameFieldObject where V : GameFieldObject
        {
            if (baseItem == null) return item;

            foreach (var p in typeof(V).GetProperties(
                BindingFlags.Public |
                BindingFlags.SetProperty |
                BindingFlags.Instance))
            {
                if (!p.CanRead || !p.CanWrite) continue;
                if (p.PropertyType.IsArray)
                    continue;
                var value = p.GetValue(baseItem, null);
                p.SetValue(item, value, null);
            }

            if (baseItem.FlashHexColors != null)
            {
                item.FlashHexColors = baseItem.FlashHexColors.ToArray();
            }

            if (baseItem.Gun != null)
            {
                item.Gun = new Gun()
                {
                    ShotSndId = baseItem.Gun.ShotSndId,
                    BulletSpeed = baseItem.Gun.BulletSpeed,
                    BulletHeight = baseItem.Gun.BulletHeight,
                    BulletPower = baseItem.Gun.BulletPower,
                    BulletWidth = baseItem.Gun.BulletWidth,
                    GunReloadDelay = baseItem.Gun.GunReloadDelay,
                    InitialCapacity = baseItem.Gun.InitialCapacity,
                    Capacity = baseItem.Gun.Capacity,
                };
            }
            item.TextureIdList = new List<int>(baseItem.TextureIdList.ToArray());
            item.IsVisible = true;

            return item;
        }

    }
}
