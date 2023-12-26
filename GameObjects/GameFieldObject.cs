using BattleCity.Enums;
using Newtonsoft.Json;
using System.Collections.Generic;

namespace BattleCity.GameObjects
{
    /// <summary>
    /// Объект игрового поля
    /// </summary>
    public class GameFieldObject : BaseGameObject
    {
        // цвет по умолчанию
        private string hexColor = "#FFFFFFFF";

        /// <summary>
        /// Шинина в условных единицах
        /// </summary>
        public int Width { get; set; }

        /// <summary>
        /// Длина в условных единицах
        /// </summary>
        public int Height { get; set; }

        /// <summary>
        /// Порядок отрисовки (Z-Order)
        /// </summary>
        public int DrawOrder { get; set; }

        /// <summary>
        /// Шаг повторения текстуры по оси X
        /// </summary>
        public float UVTileX { get; set; }

        /// <summary>
        /// Шаг повторения текстуры по оси Y
        /// </summary>
        public float UVTileY { get; set; }

        /// <summary>
        /// Находится в состоянии появления (ожидает появления на поле)
        /// </summary>
        [JsonIgnore]
        public bool IsSpawn { get; set; }

        /// <summary>
        /// Признак видимости
        /// </summary>
        public bool IsVisible { get; set; } = true;

        /// <summary>
        /// Время бездействия (в кадрах)
        /// </summary>
        [JsonIgnore]
        public int InactiveTime { get; set; }

        /// <summary>
        /// Первичный (основной) цвет (hex значение)
        /// </summary>
        public string HexColor
        {
            get { return hexColor; }
            set
            {
                hexColor = value;
                Color = string.IsNullOrEmpty(HexColor)
                    ? ColorConverter.ToInt32("#FFFFFFFF")
                    : ColorConverter.ToInt32(HexColor);
            }
        }

        /// <summary>
        /// Дополнительные цвета (hex значения)
        /// </summary>
        public string[] FlashHexColors { get; set; }

        /// <summary>
        /// Первичный (основной) цвет
        /// </summary>
        [JsonIgnore]
        public int Color { get; private set; }

        /// <summary>
        /// Прочность брони
        /// </summary>
        public int Armor { get; set; }

        /// <summary>
        /// Значение здоровья
        /// </summary>
        public int Health { get; set; }

        /// <summary>
        /// Скорость движения в условных единицах
        /// </summary>
        public decimal MoveSpeed { get; set; }

        /// <summary>
        /// Список идентификатор текстур. Используется для анимации
        /// </summary>
        public List<int> TextureIdList { get; set; } = new List<int>(0);

        /// <summary>
        /// Длительность анимации (сколько кадров отображается текстура)
        /// </summary>
        public int TextureAnimationTime { get; set; }

        /// <summary>
        /// Бонусные очки за уничтожение / подбирание, которые зачисляются игроку
        /// </summary>
        public int BonusPoints { get; set; }

        /// <summary>
        /// Уровень прокачки / модернизация / апгрейд
        /// </summary>
        public int UpgradeLevel { get; set; }

        /// <summary>
        /// Идентификатор звука при уничтожении этого объекта
        /// </summary>
        public int DestroySndId { get; set; }

        /// <summary>
        /// Идентификатор звука при появлении на поле
        /// </summary>
        public int AppearSndId { get; set; }

        /// <summary>
        /// Идентификатор звука при соприкосновении
        /// </summary>
        public int CollideSndId { get; set; }

        /// <summary>
        /// Пушка
        /// </summary>
        public Gun Gun { get; set; }

        /// <summary>
        /// Тип объекта (комбинируются)
        /// </summary>
        public GameObjectType Type { get; set; }

        /// <summary>
        /// Получить следующий идентификатор текстуры
        /// </summary>
        /// <returns></returns>
        public virtual int NextTextureId(int gameTime)
        {
            if (TextureIdList.Count == 0) return 0;
            if (TextureAnimationTime <= 0 || TextureIdList.Count == 1) return TextureIdList[0];
            int index = (gameTime / TextureAnimationTime) % TextureIdList.Count;
            return TextureIdList[index];
        }
    }
}