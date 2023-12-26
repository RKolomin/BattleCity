using BattleCity.Enums;
using SlimDX;
using System;
using Rectangle = System.Drawing.Rectangle;

namespace BattleCity.GameObjects
{
    /// <summary>
    /// Снаряд, выпущенный пушкой (Gun)
    /// </summary>
    public class Bullet
    {
        private readonly Gun gun;
        private readonly int subPixelSize;

        public int X { get; private set; }
        public int Y { get; private set; }
        public int SubPixelX { get; set; }
        public int SubPixelY { get; set; }
        public int MoveX { get; set; }
        public int MoveY { get; set; }
        public int Width { get; set; } = 2;
        public int Height { get; set; } = 1;
        public decimal Speed { get; private set; }
        public int Power { get; set; }
        public MoveDirection Direction { get; }

        /// <summary>
        /// Юнит, который выпустил снаряд
        /// </summary>
        public BattleUnit Owner { get; }

        /// <summary>
        /// Объект поля (отображение снаряда)
        /// </summary>
        public GameFieldObject BulletObject { get; private set; }

        /// <summary>
        /// Ограничительный прямоугольник
        /// </summary>
        public Rectangle BoundingBox { get; private set; }

        /// <summary>
        /// Конструктор
        /// </summary>
        /// <param name="gun">Пушка, из который выпущен снаряд</param>
        /// <param name="bulletOwner">Юнит-владелец пушки</param>
        /// <param name="subPixelSize"></param>
        public Bullet(Gun gun, BattleUnit bulletOwner, int subPixelSize)
        {
            this.gun = gun;
            this.subPixelSize = subPixelSize;
            Owner = bulletOwner;
            Power = gun.BulletPower;
            Speed = gun.BulletSpeed == 0
                ? Math.Max(1, Owner.MoveSpeed * 2)
                : Math.Max(gun.BulletSpeed, Owner.MoveSpeed);
            X = Owner.X;
            Y = Owner.Y;
            Direction = Owner.Direction;

            int subPixelX;
            int subPixelY;

            switch (Owner.Direction)
            {
                case MoveDirection.Down:
                    Width = gun.BulletWidth;
                    Height = gun.BulletHeight;
                    Y += Owner.Height;
                    subPixelX = ((Owner.Width - Width) * subPixelSize) / 2;
                    X += subPixelX / subPixelSize;
                    SubPixelX = Owner.SubPixelX + subPixelX % subPixelSize;
                    subPixelY = Owner.SubPixelY;
                    Y += subPixelY / subPixelSize;
                    SubPixelY = subPixelY % subPixelSize;
                    MoveX = 0;
                    MoveY = 1;
                    BoundingBox = new Rectangle(
                        X * subPixelSize + SubPixelX,
                        Y * subPixelSize + SubPixelY,
                        Width * subPixelSize,
                        Height * subPixelSize);
                    break;
                case MoveDirection.Up:
                    Width = gun.BulletWidth;
                    Height = gun.BulletHeight;
                    subPixelX = ((Owner.Width - Width) * subPixelSize) / 2;
                    X += subPixelX / subPixelSize;
                    Y -= Height;
                    SubPixelX = Owner.SubPixelX + subPixelX % subPixelSize;
                    subPixelY = Owner.SubPixelY;
                    Y -= subPixelY / subPixelSize;
                    SubPixelY = subPixelY % subPixelSize;
                    MoveX = 0;
                    MoveY = -1;
                    BoundingBox = new Rectangle(
                        X * subPixelSize + SubPixelX,
                        Y * subPixelSize + SubPixelY,
                        Width * subPixelSize,
                        Height * subPixelSize);
                    break;
                case MoveDirection.Right:
                    Height = gun.BulletWidth;
                    Width = gun.BulletHeight;
                    subPixelY = ((Owner.Height - Height) * subPixelSize) / 2;
                    Y += subPixelY / subPixelSize;
                    X += Owner.Width;
                    subPixelX = Owner.SubPixelX;
                    X += subPixelX / subPixelSize;
                    SubPixelX = subPixelX % subPixelSize;
                    SubPixelY = Owner.SubPixelY + subPixelY % subPixelSize;
                    MoveX = 1;
                    MoveY = 0;
                    BoundingBox = new Rectangle(
                        X * subPixelSize + SubPixelX,
                        Y * subPixelSize + SubPixelY,
                        Width * subPixelSize,
                        Height * subPixelSize);
                    break;
                case MoveDirection.Left:
                    Height = gun.BulletWidth;
                    Width = gun.BulletHeight;
                    subPixelY = ((Owner.Height - Height) * subPixelSize) / 2;
                    Y += subPixelY / subPixelSize;
                    X -= Width;
                    subPixelX = Owner.SubPixelX;
                    X -= subPixelX / subPixelSize;
                    SubPixelX = subPixelX % subPixelSize;
                    SubPixelY = Owner.SubPixelY + subPixelY % subPixelSize;
                    MoveX = -1;
                    MoveY = 0;
                    BoundingBox = new Rectangle(
                        X * subPixelSize + SubPixelX,
                        Y * subPixelSize + SubPixelY,
                        Width * subPixelSize,
                        Height * subPixelSize);
                    break;
            }
        }

        public BoundingBox GetAABB()
        {
            return new BoundingBox(
                new Vector3(BoundingBox.X, BoundingBox.Y, 0), 
                new Vector3(BoundingBox.X + BoundingBox.Width, BoundingBox.Y + BoundingBox.Height, 0)
                );
        }

        /// <summary>
        /// Уничтожить снаряд и перезарядить пушку
        /// </summary>
        public void Destroy()
        {
            if (Owner != null && Owner.IsAlive && Owner.Gun == gun)
                Owner.Gun.ReloadGun(false);
        }

        public void SetBulletObject(GameFieldObject bulletObject)
        {
            BulletObject = bulletObject;

            // обновляем визуальный объект снаряда
            if (BulletObject != null)
            {
                UpdateBulletObject();
            }
        }

        public void Move(decimal? speed = null)
        {
            speed = speed ?? Speed;
            SubPixelX += Convert.ToInt32(MoveX * speed);
            X += SubPixelX / subPixelSize;
            SubPixelX %= subPixelSize;

            SubPixelY += Convert.ToInt32(MoveY * speed);
            Y += SubPixelY / subPixelSize;
            SubPixelY %= subPixelSize;

            BoundingBox = new Rectangle(
                X * subPixelSize + SubPixelX,
                Y * subPixelSize + SubPixelY,
                BoundingBox.Width,
                BoundingBox.Height);
            
            // обновляем визуальный объект снаряда
            if (BulletObject != null)
            {
                UpdateBulletObject();
            }
        }

        private void UpdateBulletObject()
        {
            BulletObject.X = X;
            BulletObject.Y = Y;
            BulletObject.SubPixelX = SubPixelX;
            BulletObject.SubPixelY = SubPixelY;
            BulletObject.Direction = Direction;

            int subPixelX = ((Width - BulletObject.Width) * subPixelSize) / 2;
            int subPixelY = ((Height - BulletObject.Height) * subPixelSize) / 2;

            BulletObject.X += subPixelX / subPixelSize;
            BulletObject.SubPixelX += subPixelX % subPixelSize;

            BulletObject.Y += subPixelY / subPixelSize;
            BulletObject.SubPixelY += subPixelY % subPixelSize;
        }
    }
}