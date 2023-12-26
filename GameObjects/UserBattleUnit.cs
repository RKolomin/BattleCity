using BattleCity.Common;
using BattleCity.Enums;

namespace BattleCity.GameObjects
{
    /// <summary>
    /// Боевой юнит игрока
    /// </summary>
    public class UserBattleUnit : BattleUnit
    {
        /// <summary>
        /// Значение заморозки (блокировка активности) в кадрах
        /// </summary>
        public int Freeze { get; set; }

        /// <summary>
        /// Конструктор по умолчанию
        /// </summary>
        public UserBattleUnit(GameConfig config)
            : base(config)
        {
            IsUser = true;
        }

        /// <inheritdoc/>
        public override UnitAction Update()
        {
            if (!IsAlive || IsSpawn)
                return UnitAction.Idle;
            Gun?.Update();

            return UnitAction.Idle;
        }

        /// <inheritdoc/>
        public override void UpdateAnimation(int gameTime)
        {
            if (hexColorOriginal == null)
                hexColorOriginal = HexColor;

            if (Freeze > 0)
            {
                if (config.UnitFreezeAnimationFrames > 0)
                {
                    if (gameTime % config.UnitFreezeAnimationFrames == 0)
                    {
                        HexColor = HexColor == hexColorOriginal
                            ? "#00FFFFFF"
                            : hexColorOriginal;
                    }
                }
            }
            else if (hexColorOriginal != null)
            {
                HexColor = hexColorOriginal;
            }

            if (Freeze == 0)
            {
                HexColor = hexColorOriginal;

                if (TextureAnimationTime <= 0)
                    return;
                if (gameTime % TextureAnimationTime == 0)
                    textureIndex++;
            }
        }

        /// <inheritdoc/>
        public override int NextTextureId(int gameTime)
        {
            if (TextureIdList.Count == 0) return 0;
            if (TextureAnimationTime <= 0 || TextureIdList.Count == 1) return TextureIdList[0];
            textureIndex %= TextureIdList.Count;
            return TextureIdList[textureIndex % TextureIdList.Count];
        }
    }
}
