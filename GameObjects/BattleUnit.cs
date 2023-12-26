using BattleCity.Common;
using BattleCity.Enums;
using System;

namespace BattleCity.GameObjects
{
    /// <summary>
    /// Боевой юнит
    /// </summary>
    public abstract class BattleUnit : GameFieldObject
    {
        #region members

        // текущий индекс мерцающего цвета
        protected int flashHexColorIndex = 0;

        // текущий индекс текстуры
        protected int textureIndex = 0;

        // оставшееся значение скорости движения
        protected decimal remainMoveSpeedPortion;

        // игровые конфиругации
        protected GameConfig config;

        // начальный цвет
        protected string hexColorOriginal;

        #endregion


        #region Properties

        /// <summary>
        /// Признак того, что юнит живой
        /// </summary>
        public bool IsAlive { get; set; } = true;

        /// <summary>
        /// Вектор движения по оси X
        /// </summary>
        public int MoveX { get; set; } = 0;

        /// <summary>
        /// Вектор движения по оси Y
        /// </summary>
        public int MoveY { get; set; } = -1;

        /// <summary>
        /// Количество кадров, при котором действует неуязвимость
        /// </summary>
        public int Invincibility { get; set; }

        /// <summary>
        /// Щит (объект анимации)
        /// </summary>
        public AnimationObject Shield { get; set; }

        /// <summary>
        /// Инерция движения. По факту количество кадров, при котором выполняется автоматическое движение
        /// </summary>
        public int MoveInertion { get; set; }

        /// <summary>
        /// Форсировать движение по инерции
        /// </summary>
        public bool ForceMoveInertion { get; set; }

        /// <summary>
        /// Признак того, что данным юнитом управляет пользователь (игрок)
        /// </summary>
        public bool IsUser { get; protected set; }

        #endregion


        public BattleUnit(GameConfig config)
        {
            this.config = config;
        }

        ~BattleUnit()
        {
            config = null;
        }


        #region public methods

        /// <summary>
        /// Выполнить движение по заданному вектору движения <see cref="MoveDirection"/>
        /// </summary>
        /// <param name="maxPositionX">Ограничение по оси X (в условных единицах)</param>
        /// <param name="maxPositionY">Ограничение по оси Y (в условных единицах</param>
        /// <param name="subPixelSize">Размер субпикселя</param>
        /// <param name="moveSpeedMultiply">Множитель скорости</param>
        public FieldBoundsCollision Move(int maxPositionX, int maxPositionY, int subPixelSize, int moveSpeedMultiply)
        {
            // определяем направление движения
            switch (Direction)
            {
                case MoveDirection.Up:
                    MoveY = -1;
                    MoveX = 0;
                    break;
                case MoveDirection.Down:
                    MoveY = 1;
                    MoveX = 0;
                    break;
                case MoveDirection.Left:
                    MoveX = -1;
                    MoveY = 0;
                    break;
                case MoveDirection.Right:
                    MoveX = 1;
                    MoveY = 0;
                    break;
                default:
                    MoveX = 0;
                    MoveY = 0;
                    break;
            }

            decimal stepWithPercition = MoveSpeed + remainMoveSpeedPortion;
            int step = (int)stepWithPercition;
            remainMoveSpeedPortion = stepWithPercition - step;

            SubPixelX += Convert.ToInt32(MoveX * step * moveSpeedMultiply);
            if (SubPixelX < 0)
            {
                X--;
                SubPixelX = subPixelSize + SubPixelX;
            }
            else
            {
                X += SubPixelX / subPixelSize;
                SubPixelX %= subPixelSize;
            }

            SubPixelY += Convert.ToInt32(MoveY * step * moveSpeedMultiply);
            if (SubPixelY < 0)
            {
                Y--;
                SubPixelY = subPixelSize + SubPixelY;
            }
            else
            {
                Y += SubPixelY / subPixelSize;
                SubPixelY %= subPixelSize;
            }

            if (X < 0 || (X == 0 && SubPixelX < 0))
            {
                X = 0;
                SubPixelX = 0;
                return FieldBoundsCollision.Collided;
            }
            else if ((X > maxPositionX - Width) || (X == maxPositionX - Width && SubPixelX > 0))
            {
                X = maxPositionX - Width;
                SubPixelX = 0;
                return FieldBoundsCollision.Collided;
            }

            if (Y < 0 || (Y == 0 && SubPixelY < 0))
            {
                Y = 0;
                SubPixelY = 0;
                return FieldBoundsCollision.Collided;
            }
            else if ((Y > maxPositionY - Height) || (Y == maxPositionY - Height && SubPixelY > 0))
            {
                Y = maxPositionY - Height;
                SubPixelY = 0;
                return FieldBoundsCollision.Collided;
            }

            return FieldBoundsCollision.None;
        }

        /// <summary>
        /// Задать направление движения
        /// </summary>
        /// <param name="moveDirection">Направление</param>
        public void SetDirection(MoveDirection moveDirection)
        {
            if (Direction == moveDirection) return;
            var oldDirection = Direction;
            Direction = moveDirection;
            if ((int)oldDirection % 2 != (int)moveDirection % 2)
            {
                NormalizePosition(config.SubPixelSize);
                //NormalizePosition();
            }
        }

        /// <summary>
        /// Обновить анимацию
        /// </summary>
        /// <param name="gameTime"></param>
        public abstract void UpdateAnimation(int gameTime);

        /// <summary>
        /// Сбросить защиту (аннулировать время действия щита)
        /// </summary>
        public void ResetShield()
        {
            Shield = null;
            Invincibility = 0;
        }

        /// <summary>
        /// Прекратить движение
        /// </summary>
        public void StopMoving()
        {
            MoveInertion = 0;
            remainMoveSpeedPortion = 0;
            ForceMoveInertion = false;
        }

        /// <summary>
        /// Обновление, при котором определяется действие юнита
        /// </summary>
        /// <returns></returns>
        public abstract UnitAction Update();

        #endregion


        #region private methods

        /// <summary>
        /// Нормализовать координаты позиции
        /// </summary>
        /// <param name="subPixelSize"></param>
        private void NormalizePosition(int subPixelSize)
        {
            int x = X * subPixelSize + SubPixelX;
            int y = Y * subPixelSize + SubPixelY;

            int sbx = x % (subPixelSize);
            if (sbx >= subPixelSize / 2)
            {
                X = SubPixelX > 0 ? X + 1 : X - 1;
                SubPixelX = 0;
            }
            else
            {
                SubPixelX -= sbx % subPixelSize;
            }

            int sby = y % (subPixelSize);
            if (sby >= subPixelSize / 2)
            {
                Y = SubPixelY > 0 ? Y + 1 : Y - 1;
                SubPixelY = 0;
            }
            else
            {
                SubPixelY -= sby % subPixelSize;
            }
        }

        // Грубое округление по модулю 2, иногда приводит к застряванию в кирпичах
        ///// <summary>
        ///// Нормализовать координаты позиции
        ///// </summary>
        //private void NormalizePosition()
        //{
        //    if (X % 2 != 0)
        //    {
        //        X = Math.Max(0, X - 1);
        //    }

        //    SubPixelX = 0;

        //    if (Y % 2 != 0)
        //    {
        //        Y = Math.Max(0, Y - 1);
        //    }

        //    SubPixelY = 0;
        //}

        #endregion
    }

}