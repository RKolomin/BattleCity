using System.Collections.Generic;

namespace BattleCity.Common
{
    /// <summary>
    /// Итог завершения уровня (stage)
    /// </summary>
    public class StageResult
    {
        /// <summary>
        /// Признак конца игры
        /// </summary>
        public bool IsGameOver { get; set; }

        /// <summary>
        /// Текущий номер уровня
        /// </summary>
        public int StageNumber { get; set; }

        /// <summary>
        /// Результаты битвы по каждому игроку
        /// </summary>
        public List<Player> PlayersResults { get; set; } = new List<Player>(10);
    }    
}
