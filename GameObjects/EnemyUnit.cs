using BattleCity.Common;
using BattleCity.Enums;
using System;
using System.Linq;

namespace BattleCity.GameObjects
{
    /// <summary>
    /// Вражеский юнит
    /// </summary>
    public class EnemyUnit : BattleUnit
    {
        /// <summary>
        /// Количество бонусов при попадении в юнита. По сути это дополнительные жизни.
        /// </summary>
        public int ExtraBonus { get; set; }

        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="config">Игровые конфигурации</param>
        public EnemyUnit(GameConfig config) : base(config)
        {
        }

        /// <inheritdoc/>
        public override int NextTextureId(int gameTime)
        {
            if (TextureIdList.Count == 0) return 0;
            if (TextureAnimationTime <= 0 || TextureIdList.Count == 1) return TextureIdList[0];
            return TextureIdList[textureIndex % TextureIdList.Count];
        }

        /// <summary>
        /// Обновить анимацию
        /// </summary>
        /// <param name="gameTime"></param>
        public override void UpdateAnimation(int gameTime)
        {
            if (hexColorOriginal == null)
                hexColorOriginal = HexColor;

            string nextHexColor = hexColorOriginal;

            if (ExtraBonus > 0)
            {
                if (config.PowerUpBonusFlashColorDuration > 0 &&
                    (config.PowerUpBonusFlashColorDuration < 2 || gameTime % config.PowerUpBonusFlashColorDuration == 0))
                {
                    if (FlashHexColors == null || FlashHexColors.Length == 0)
                    {
                        HexColor = HexColor == hexColorOriginal
                            ? config?.EnemyFlashHexColor ?? hexColorOriginal
                            : hexColorOriginal;
                    }
                    else
                    {
                        HexColor = HexColor == FlashHexColors[0]
                            ? (config?.EnemyFlashHexColor ?? nextHexColor)
                            : FlashHexColors[0];
                    }
                }
            }
            else
            {
                if (config.UnitFlashColorDuration > 0 && FlashHexColors != null && FlashHexColors.Length > 1)
                {
                    int flashColor = Math.Min(Math.Max(0, Health - 1), FlashHexColors.Length - 1);
                    if (config.UnitFlashColorDuration < 2 || gameTime % config.UnitFlashColorDuration == 0)
                        flashHexColorIndex = flashHexColorIndex == 0 ? flashColor : 0;
                    nextHexColor = FlashHexColors[flashHexColorIndex];
                }

                HexColor = nextHexColor;
            }

            if (TextureAnimationTime <= 0)
                return;
            if (gameTime % TextureAnimationTime == 0)
                textureIndex++;
        }

        /// <summary>
        /// Сменить направление движения
        /// </summary>
        public void ChangeDirection()
        {
            SetRandomDirection();

            // Todo: реализовать

            //// period duration in seconds, respawn time in frames
            //const int periodDuration = respawnTime / 8;

            //if (time() < periodDuration)
            //{
            //    // first period
            //    SetRandomDirection(true);
            //    tank.setCommand(cmdCheckTileReach);
            //}
            //else if (time() < periodDuration * 2)
            //{
            //    // second period
            //    if ((firstPlayer.isAlive && tank.number % 2 == 0) || !secondPlayer.isAlive)
            //    {
            //        tank.setCommand(cmdMoveToFirstPlayer);
            //    }
            //    else
            //    {
            //        tank.setCommand(cmdMoveToSecondPlayer);
            //    }
            //}
            //else
            //{
            //    // third period
            //    tank.setCommand(cmdMoveToEagle);
            //}
        }

        /// <summary>
        /// Инвертировать направление движения
        /// </summary>
        public void InvertDirection()
        {
            if (Direction == MoveDirection.Up)
                SetDirection(MoveDirection.Down);
            else if (Direction == MoveDirection.Down)
                SetDirection(MoveDirection.Up);
            else if (Direction == MoveDirection.Left)
                SetDirection(MoveDirection.Right);
            else if (Direction == MoveDirection.Right)
                SetDirection(MoveDirection.Left);
        }

        /// <summary>
        /// Повернуть направление движения по часовой стрелке
        /// </summary>
        public void RotateClockwise()
        {
            int nextValue = Direction.GetHashCode() + 1;
            int maxValue = Enum.GetValues(typeof(MoveDirection)).Cast<int>().Max();
            int minValue = Enum.GetValues(typeof(MoveDirection)).Cast<int>().Min();
            if (nextValue > maxValue)
                nextValue = minValue;
            SetDirection((MoveDirection)nextValue);
        }

        /// <summary>
        /// Повернуть направление движения против часовой стрелки
        /// </summary>
        public void RotateCounterClockwise()
        {
            int nextValue = Direction.GetHashCode() - 1;
            int maxValue = Enum.GetValues(typeof(MoveDirection)).Cast<int>().Max();
            int minValue = Enum.GetValues(typeof(MoveDirection)).Cast<int>().Min();
            if (nextValue < minValue)
                nextValue = maxValue;
            SetDirection((MoveDirection)nextValue);
        }

        /// <summary>
        /// Начать движение с указанными количеством кадров
        /// </summary>
        /// <param name="frames"></param>
        private void StartMoving(int frames)
        {
            MoveInertion = frames;
        }

        /// <summary>
        /// Определить следующее действие юнита
        /// </summary>
        /// <returns></returns>
        private UnitAction GetNextAction()
        {
            UnitAction action = UnitAction.Idle;
            if (IsReadyToShot())
                action |= UnitAction.Attack;
            action |= UnitAction.Move;
            return action;
        }

        /// <summary>
        /// Определить готовность к выстрелу
        /// </summary>
        /// <returns></returns>
        private bool IsReadyToShot()
        {
            if (Gun == null || !Gun.IsGunLoaded)
                return false;
            if (config.EnemyAgressivity <= 0)
                return false;

            int chance = 100 - Math.Max(0, Math.Min(100, config.EnemyAgressivity));
            //return config.Random.Next(-500, 101) >= chance;
            return config.Random.Next(0, 101) >= chance;

            // original BC
            //return config.Random.Next(1, 33) == 32;
        }

        public override UnitAction Update()
        {
            if (!IsAlive || IsSpawn)
                return UnitAction.Idle;

            Gun?.Update();
            UnitAction action;

            if (MoveInertion > 0)
            {
                MoveInertion--;
                action = UnitAction.Move;

                if (IsReadyToShot())
                {
                    action |= UnitAction.Attack;
                }

                return action;
            }

            action = GetNextAction();

            if (action.HasFlag(UnitAction.Move))
            {
                // задаём движение
                StartMoving(4);
            }

            return action;
        }

        /// <summary>
        /// Установить случайное направление движения
        /// </summary>
        protected void SetRandomDirection()
        {
            SetDirection((MoveDirection)config.Random.Next(0, 4));

            switch (Direction)
            {
                case MoveDirection.Down:
                    MoveX = 0;
                    MoveY = -1;
                    break;
                case MoveDirection.Left:
                    MoveX = -1;
                    MoveY = 0;
                    break;
                case MoveDirection.Right:
                    MoveX = 1;
                    MoveY = 0;
                    break;
                case MoveDirection.Up:
                    MoveX = 0;
                    MoveY = 1;
                    break;
            }
        }
    }
}
