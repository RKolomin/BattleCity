using BattleCity.Extensions;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Text;

namespace BattleCity.Common
{
    /// <summary>
    /// Игровые достижения
    /// </summary>
    public class GameAchievements
    {
        /// <summary>
        /// Наименование файла
        /// </summary>
        const string FILENAME = "achievements.json";

        /// <summary>
        /// Рекорд первого игрока
        /// </summary>
        public UserRecord Player1Record { get; set; } = new UserRecord(0);

        /// <summary>
        /// Рекорд второго игрока
        /// </summary>
        public UserRecord Player2Record { get; set; } = new UserRecord(0);

        /// <summary>
        /// Маскимально набранные очки среди игроков
        /// </summary>
        public int? HiScoreValue { get; set; }

        /// <summary>
        /// Получить наивысшее значение рекорда
        /// </summary>
        /// <param name="defaultValue">Значение по умолчанию</param>
        /// <returns></returns>
        public int GetHiScoreValue(int defaultValue)
        {
            int value = defaultValue;
            if (Player1Record != null && Player1Record.HiScoreValue > value)
                value = Player1Record.HiScoreValue;

            if (Player2Record != null && Player2Record.HiScoreValue > value)
                value = Player2Record.HiScoreValue;

            return value;
        }

        /// <summary>
        /// Загрузить рекорд
        /// </summary>
        /// <returns></returns>
        public static GameAchievements Load()
        {
            try
            {
                var data = File.ReadAllText(FILENAME, Encoding.UTF8);
                var result = JsonConvert.DeserializeObject<GameAchievements>(data);
                if (result.Player1Record == null)
                    result.Player1Record = new UserRecord(0);
                if (result.Player2Record == null)
                    result.Player2Record = new UserRecord(0);
                return result;
            }
            catch
            {
                return new GameAchievements();
            }
        }

        /// <summary>
        /// Сохранить рекорд
        /// </summary>
        public void Save()
        {
            try
            {
                File.WriteAllText(FILENAME, this.ToJson(), Encoding.UTF8);
            }
            catch { }
        }
    }

    /// <summary>
    /// Рекорд игрока
    /// </summary>
    public class UserRecord
    {
        /// <summary>
        /// Наивысший результат по очкам
        /// </summary>
        [JsonProperty]
        public int HiScoreValue { get; private set; }

        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="hiScoreValue"></param>
        [JsonConstructor]
        public UserRecord(int hiScoreValue)
        {
            HiScoreValue = hiScoreValue;
        }

        /// <summary>
        /// Обновить наивысший результат по очкам
        /// </summary>
        /// <param name="newValue"></param>
        public void UpdateHiScoreValue(int newValue)
        {
            HiScoreValue = Math.Max(HiScoreValue, newValue);
        }
    }
}
