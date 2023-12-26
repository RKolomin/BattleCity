using BattleCity.GameObjects;

namespace BattleCity.Common
{
    /// <summary>
    /// Соприкосновение с объектом
    /// </summary>
    public class ContactObject
    {
        /// <summary>
        /// Объект, с кем соприкасаемся
        /// </summary>
        public GameFieldObject Object { get; set; }

        /// <summary>
        /// Дистанция соприкосновения
        /// </summary>
        public double Distance { get; set; }
    }
}
