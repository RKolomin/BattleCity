using BattleCity.GameObjects;
using System.Collections.Generic;

namespace BattleCity.Common
{
    /// <summary>
    /// Игрок
    /// </summary>
    public class Player
    {
        /// <summary>
        /// Идентификатор игрока (1, 2, и т.д.)
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        /// Набранные очки
        /// </summary>
        public int Score { get; set; }

        /// <summary>
        /// Всего уничтоженных врагов за всю игру
        /// </summary>
        public int TotalEnemiesDestoyed { get; set; }

        /// <summary>
        /// Количество жизней
        /// </summary>
        public int Lifes { get; set; }

        /// <summary>
        /// Боевой юнит игрока
        /// </summary>
        public UserBattleUnit Unit { get; set; }

        /// <summary>
        /// Признак замороки / блокировки игрока (игрок не может двигаться по полю)
        /// </summary>
        public bool IsFrozen => Unit != null && Unit.Freeze > 0;

        /// <summary>
        /// Признак, что игрок жив (существует на поле)
        /// </summary>
        public bool IsAlive => Lifes > 0;

        /// <summary>
        /// Имя игрока
        /// </summary>
        public string PlayerName { get; set; }

        /// <summary>
        /// Уровень прокачки / модернизация / апгрейды
        /// </summary>
        public int UpgradeLevel { get; set; }

        /// <summary>
        /// Список уничтоженных врагов (должен очищаться на каждом уровне)
        /// </summary>
        public List<DestroyedEnemyInfo> DestroyedEnemies { get; set; } = new List<DestroyedEnemyInfo>();

        /// <summary>
        /// Создать игрока
        /// </summary>
        /// <param name="config">Игровые конфигурации</param>
        /// <param name="id">Идентификатор игрока</param>
        /// <param name="lifes">Начальное количество жизней</param>
        /// <returns></returns>
        public static Player Create(GameConfig config, int id, int lifes = 1)
        {
            var player = new Player()
            {
                Id = id,
                Unit = new UserBattleUnit(config),
                Lifes = lifes,
                PlayerName = $"Player_{id}",
            };

            return player;
        }
    }
}
