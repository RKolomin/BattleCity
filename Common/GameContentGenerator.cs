using BattleCity.Enums;
using BattleCity.GameObjects;
using BattleCity.Logging;
using BattleCity.Repositories;
using System.Collections.Generic;
using System.Drawing;

namespace BattleCity.Common
{
    /// <summary>
    /// Статический класс генерации игрового контента
    /// </summary>
    static class GameContentGenerator
    {
        /// <summary>
        /// Создать контент по умолчанию
        /// </summary>
        /// <param name="contentDirectoryName">Директория контента</param>
        /// <param name="logger">Сервис логирования</param>
        public static void CreateDefaultContent(string contentDirectoryName = "Data", ILogger logger = null)
        {
            GameContent resx = new GameContent(contentDirectoryName, logger);

            resx.GameConfig = CreateDefaultGameConfig(resx.ContentDirectory);
            resx.GameConfig.Save();

            CreateDefaultTextureResources(resx.Textures);
            resx.Textures.Save();

            CreateDefaultSoundResources(resx.Sounds, resx.CommonConfig);
            resx.Sounds.Save();

            CreateDefaultGameObjects(resx, resx.GameObjects);
            resx.GameObjects.Save();
        }

        public static GameConfig CreateTank1990GameConfig(string contentDirectoryName = "Data")
        {
            return new GameConfig(contentDirectoryName)
            {
                Name = "TANK X 1990",
                RandomSeed = 101010,
                EnemyFlashHexColor = "#FFFF545D",
                TowerTempDefenseDuration = 20,
                UnitMaxUpgradeLevel = 6,
                ShowChessboard = false,
                EnemyAgressivity = 6,
                EmenySpawnDelay = 0,
                SpawnAnimationDuration = 60,
                StageCompleteDelayTime = 3,
                ChallengeBonusPoints = 1000,
                RewardsExtraLifeAnEvery = 20000,
                MaxExtraBonusPerUnit = 3,
                MaxActivePowerUpsOnField = 1,   // x ActivePlayers
                MaxBonusedUnitsOnField = -1,
                HidePowerUpsIfBonusedEnemySpawned = false,
                PowerUpLifetimeDuration = 0,    // 5 * 60,
                UnitFreezeAnimationFrames = 10,
                PowerUpBonusFlashColorDuration = 10,
                EnemyFreezeDuration = 10 * 60,
                PlayerFreezeDuration = 5 * 60,
                PlayerMoveSpeedMultiply = 1,
                EnemyMoveSpeedMultiply = 1,
                BonusedEnemySpawnChance = 15,
                EnemyPowerUpHasEffect = true,
                PlayerPowerUpAllowed = true,
                EnemyPowerUpAllowed = true,
                MoveInertionDuration = 4,
                PlayerSpawnShieldDuration = 3 * 60,
                EnemySpawnShieldDuration = 0,
                ExtraShieldDuration = 10 * 60,
                PointsTextShowDuration = 60,
                TreasureBonusPoints = 500,
                SubPixelSize = 10,
                MaxActiveEnemy = 4,             // x ActivePlayers
                MaxEnemiesPerStage = 40,
                EnemySpawnPositionCount = 3,
                EnemyFirstSpawnPositionIndex = 1,
                MaxEnemy = 20,
                StartLifes = 3,
                PlayerDefaultUpgradeLevel = 1,
                FieldWidth = 52,
                FieldHeight = 52,
                TextHexColor = "#FFDCDCDC",
                TransitionScreenBackgroundHexColor = "#FF5F5F5F",
                //BackgroundHexColor = "#FF1F1F1F",
                BackgroundHexColor = "#FF454545",
                BattleGroundHexColor = "#FF000000",
                ChessCellHexColor1 = "FF000000",
                ChessCellHexColor2 = "FF0A0A0A",
                AllowPlayerJoin = true,
                ResetUnitUpgradesOnStageStart = false,
                PlayerFriendlyFire = true,
                EnemyFriendlyFire = false,
                PlayerDestroyBaseAllowed = true,
                ShowGameOverScreen = true,
                ShowHiScoreScreen = true,
            };
        }

        /// <summary>
        /// Создать игровую конфигурацию по умолчанию
        /// </summary>
        /// <param name="contentDirectoryName">Директория контента. По умолчанию Data</param>
        /// <returns></returns>
        public static GameConfig CreateDefaultGameConfig(string contentDirectoryName = "Data")
        {
            return new GameConfig(contentDirectoryName)
            {
                Name = "BATTLE CITY",
                RandomSeed = 112233,
                EnemyFlashHexColor = "#FFFF545D",
                TowerTempDefenseDuration = 20,
                UnitMaxUpgradeLevel = 3,
                ShowChessboard = false,
                EnemyAgressivity = 3,
                EmenySpawnDelay = 100,
                SpawnAnimationDuration = 90,
                StageCompleteDelayTime = 3,
                ChallengeBonusPoints = 1000,
                RewardsExtraLifeAnEvery = 20000,
                MaxExtraBonusPerUnit = 1,
                MaxActivePowerUpsOnField = 1,   // x ActivePlayers
                MaxBonusedUnitsOnField = 1,
                HidePowerUpsIfBonusedEnemySpawned = true,
                PowerUpLifetimeDuration = 0,    // 5 * 60,
                UnitFreezeAnimationFrames = 10,
                PowerUpBonusFlashColorDuration = 10,
                EnemyFreezeDuration = 10 * 60,
                PlayerFreezeDuration = 5 * 60,
                PlayerMoveSpeedMultiply = 1,
                EnemyMoveSpeedMultiply = 1,
                BonusedEnemySpawnChance = 15,
                EnemyPowerUpHasEffect = true,
                PlayerPowerUpAllowed = true,
                EnemyPowerUpAllowed = false,
                MoveInertionDuration = 4,
                PlayerSpawnShieldDuration = 3 * 60,
                EnemySpawnShieldDuration = 0,
                ExtraShieldDuration = 10 * 60,
                PointsTextShowDuration = 60,
                TreasureBonusPoints = 500,
                SubPixelSize = 10,
                MaxActiveEnemy = 4,             // x ActivePlayers
                MaxEnemiesPerStage = 40,
                EnemySpawnPositionCount = 3,
                EnemyFirstSpawnPositionIndex = 1,
                MaxEnemy = 20,
                StartLifes = 3,
                PlayerDefaultUpgradeLevel = 0,
                FieldWidth = 52,
                FieldHeight = 52,
                TextHexColor = "#FFDCDCDC",
                TransitionScreenBackgroundHexColor = "#FF5F5F5F",
                //BackgroundHexColor = "#FF1F1F1F",
                BackgroundHexColor = "#FF454545",
                BattleGroundHexColor = "#FF000000",
                ChessCellHexColor1 = "FF000000",
                ChessCellHexColor2 = "FF0A0A0A",
                AllowPlayerJoin = true,
                ResetUnitUpgradesOnStageStart = false,
                PlayerFriendlyFire = true,
                EnemyFriendlyFire = false,
                PlayerDestroyBaseAllowed = true,
                ShowGameOverScreen = true,
                ShowHiScoreScreen = true,
            };
        }

        /// <summary>
        /// Создать текстурные ресурсы
        /// </summary>
        /// <param name="Textures"></param>
        public static void CreateDefaultTextureResources(TextureRepository Textures)
        {
            int black = Color.Black.ToArgb();
            int transparent = Color.Transparent.ToArgb();
            Textures.Add("player_status.png", black);
            Textures.Add("brick.png");
            Textures.Add("enemy_status.png", transparent);
            Textures.Add("grass.png");
            Textures.Add("ice.png");
            Textures.Add("iron.png");
            Textures.Add("ship.png", black);
            Textures.Add("font_overlay.png");
            Textures.Add("destroyed_base.png", black);
            Textures.Add("base.png", black);
            Textures.Add("stage_flag.png", transparent);
            Textures.Add("bullet.png", black);
            Textures.Add("water1.png");
            Textures.Add("water2.png");
            Textures.Add("water3.png");
            Textures.Add("light_tank1.png", black);
            Textures.Add("light_tank2.png", black);
            Textures.Add("middle_tank1.png", black);
            Textures.Add("middle_tank2.png", black);
            Textures.Add("btr1.png", black);
            Textures.Add("btr2.png", black);
            Textures.Add("heavy_tank1.png", black);
            Textures.Add("heavy_tank2.png", black);
            Textures.Add("player_tank1.png", black);
            Textures.Add("player_tank2.png", black);
            Textures.Add("blast1.png", black);
            Textures.Add("blast2.png", black);
            Textures.Add("blast3.png", black);
            Textures.Add("boom1.png", black);
            Textures.Add("boom2.png", black);
            Textures.Add("shield1.png", black);
            Textures.Add("shield2.png", black);
            Textures.Add("lifeup.png", black);
            Textures.Add("defense.png", black);
            Textures.Add("kill_enemy.png", black);
            Textures.Add("invulnerability.png", black);
            Textures.Add("upgrade_weapon.png", black);
            Textures.Add("super_weapon.png", black);
            Textures.Add("freeze_enemy.png", black);
            Textures.Add("ship_shape1.png", black);
            Textures.Add("player_tank1_lv1.png", black);
            Textures.Add("player_tank2_lv1.png", black);
            Textures.Add("player_tank1_lv2.png", black);
            Textures.Add("player_tank2_lv2.png", black);
            Textures.Add("player_tank1_lv3.png", black);
            Textures.Add("player_tank2_lv3.png", black);
            Textures.Add("spawn_1.png", black);
            Textures.Add("spawn_2.png", black);
            Textures.Add("spawn_3.png", black);
            Textures.Add("spawn_4.png", black);
        }


        /// <summary>
        /// Создать звуковые ресурсы по умолчанию
        /// </summary>
        /// <param name="sounds"></param>
        public static void CreateDefaultSoundResources(SoundRepository sounds, CommonConfig commonConfig)
        {
            sounds.Add("base_explode.wav", false);
            sounds.Add("bonus_appear.wav", false);
            sounds.Add("bonus_destroy.wav", false);
            sounds.Add("bonus_points.wav", false);
            sounds.Add("count.wav", false);
            sounds.Add("enemy_explode.wav", false);
            sounds.Add("game_over.wav", false, SoundType.Music);
            sounds.Add("high_score.wav", false, SoundType.Music);
            sounds.Add("hit_armor.wav", false);
            sounds.Add("hit_brick.wav", false);
            sounds.Add("hit_wall.wav", false);
            sounds.Add("level_start.wav", false, SoundType.Music);
            sounds.Add("pause.wav", false);
            sounds.Add("player_explode.wav", false);
            sounds.Add("player_shot.wav", false);
            sounds.Add("move.wav", true);
            sounds.Add("enemy_move.wav", true);
            sounds.Add("ice.wav", false);
            sounds.Add("extra_life.wav", false);

            sounds.Add(commonConfig.CheckSoundLevelFileName, false, SoundType.Sound);
            sounds.Add(commonConfig.CheckMusicLevelFileName, false, SoundType.Music);
        }

        /// <summary>
        /// Получить идентификатор звукового ресурса по наименованию
        /// </summary>
        /// <param name="resx"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        static int GetSoundId(GameContent resx, string name)
        {
            var resource = resx.Sounds.GetByName(name);
            if (resource == null)
                return 0;
            return resource.Id;
        }

        /// <summary>
        /// Получить список идентификаторов текстурных ресурсов их по наименованиям
        /// </summary>
        /// <param name="resx"></param>
        /// <param name="names"></param>
        /// <returns></returns>
        static List<int> GetTextureList(GameContent resx, params string[] names)
        {
            List<int> list = new List<int>(names.Length);

            foreach (string name in names)
            {
                if (string.IsNullOrEmpty(name))
                {
                    list.Add(-1);
                    continue;
                }
                var resource = resx.Textures.GetByName(name);
                if (resource != null)
                    list.Add(resource.Id);
                else list.Add(0);
            }

            return list;
        }

        /// <summary>
        /// Создать игровые объекты по умолчанию
        /// </summary>
        /// <param name="resx"></param>
        /// <param name="Items"></param>
        public static void CreateDefaultGameObjects(GameContent resx, GameObjectRepository Items)
        {
            Items.Add(new GameFieldObject()
            {
                Type = GameObjectType.Barrier | GameObjectType.Destroyable,
                Armor = 1,
                TextureIdList = GetTextureList(resx, "brick"),
                Name = "BRICK",
                //HexColor = "#ffffae00",
                //HexColor = "#ffc75b42",
                Health = 0,
                Width = 1,
                Height = 1,
                UVTileX = 0.5f,
                UVTileY = 0.5f,
                DestroySndId = GetSoundId(resx, "hit_brick"),
            });

            Items.Add(new GameFieldObject()
            {
                Type = GameObjectType.Barrier | GameObjectType.Destroyable,
                Armor = 3,
                TextureIdList = GetTextureList(resx, "iron"),
                Name = "IRON",
                Health = 0,
                Width = 2,
                Height = 2,
                DestroySndId = GetSoundId(resx, "hit_armor"),
            });

            Items.Add(new GameFieldObject()
            {
                Type = GameObjectType.Barrier | GameObjectType.Water | GameObjectType.Animation,
                TextureIdList = GetTextureList(resx, "water1", "water2", "water3"),
                TextureAnimationTime = 45,
                Name = "WATER",
                Width = 2,
                Height = 2
            });

            Items.Add(new GameFieldObject()
            {
                Type = GameObjectType.Animation | GameObjectType.SpawPosition,
                TextureAnimationTime = 5,
                TextureIdList = GetTextureList(resx, "spawn_1", "spawn_2", "spawn_3", "spawn_4"),
                Name = "RESPAWN_POINT",
                Width = 4,
                Height = 4
            });

            Items.Add(new GameFieldObject()
            {
                Type = GameObjectType.Ice,
                TextureIdList = GetTextureList(resx, "ice"),
                Name = "ICE",
                Width = 2,
                Height = 2,
                CollideSndId = GetSoundId(resx, "ice")
            });

            Items.Add(new GameFieldObject()
            {
                Type = GameObjectType.Forest | GameObjectType.Destroyable,
                Armor = 4,
                TextureIdList = GetTextureList(resx, "grass"),
                Name = "FOREST",
                Health = 0,
                Width = 2,
                Height = 2,
                DrawOrder = 2,
                DestroySndId = GetSoundId(resx, "hit_brick"),
            });

            Items.Add(new GameFieldObject()
            {
                Type = GameObjectType.Barrier | GameObjectType.Destroyable | GameObjectType.Tower,
                Armor = 0,
                TextureIdList = GetTextureList(resx, "base"),
                Name = "TOWER",
                Health = 1,
                Width = 4,
                Height = 4,
                DestroySndId = GetSoundId(resx, "base_explode")
            });

            Items.Add(new GameFieldObject()
            {
                Type = GameObjectType.Barrier | GameObjectType.DestoyedTower,
                TextureIdList = GetTextureList(resx, "destroyed_base"),
                Name = "DESTOYED_TOWER",
                Width = 4,
                Height = 4
            });

            Items.Add(new GameFieldObject()
            {
                Type = GameObjectType.Animation,
                Armor = 0,
                TextureAnimationTime = 4,
                TextureIdList = GetTextureList(resx, "blast1", "blast2", "blast3"),
                Name = "BULLET_EXPLOSION",
                Health = 0,
                Width = 4,
                Height = 4,
                DrawOrder = 2
            });

            Items.Add(new GameFieldObject()
            {
                Type = GameObjectType.Projectile | GameObjectType.Destroyable,
                Armor = 0,
                TextureIdList = GetTextureList(resx, "bullet"),
                Name = "BULLET",
                Health = 0,
                Width = 1,
                Height = 1,
                DrawOrder = 1,
                MoveSpeed = 1
            });

            Items.Add(new GameFieldObject()
            {
                TextureAnimationTime = 5,
                Type = GameObjectType.Animation,
                Armor = 0,
                TextureIdList = GetTextureList(resx, "blast1", "blast2", "blast3", "boom1", "boom2"),
                Name = "UNIT_EXPLOSION",
                Health = 0,
                Width = 8,
                Height = 8,
                DrawOrder = 2
            });

            Items.Add(new GameFieldObject()
            {
                TextureAnimationTime = 2,
                Type = GameObjectType.Animation,
                TextureIdList = GetTextureList(resx, "shield1", "shield2"),
                Name = "SHIELD",
                Width = 2,
                Height = 2,
                DrawOrder = 3
            });


            #region PLAYER UNIT

            // UPGRADE LEVEL: 0
            Items.Add(new GameFieldObject()
            {
                Type = GameObjectType.Player | GameObjectType.Destroyable | GameObjectType.Unit | GameObjectType.Barrier,
                TextureIdList = GetTextureList(resx, "player_tank1", "player_tank2"),
                Name = "PLAYER_TANK",
                HexColor = "#FFE7CD46",
                UpgradeLevel = 0,
                Direction = MoveDirection.Up,
                DestroySndId = GetSoundId(resx, "player_explode"),
                DrawOrder = 1,
                Armor = 0,
                Health = 1,
                Width = 4,
                Height = 4,
                MoveSpeed = 2m,
                TextureAnimationTime = 2,
                Gun = new Gun()
                {
                    ShotSndId = GetSoundId(resx, "player_shot"),
                    BulletSpeed = 4,
                    BulletPower = 1,
                    GunReloadDelay = 10
                }
            });

            // UPGRADE LEVEL: 1
            Items.Add(new GameFieldObject()
            {
                Type = GameObjectType.Player | GameObjectType.Destroyable | GameObjectType.Unit | GameObjectType.Barrier,
                TextureIdList = GetTextureList(resx, "player_tank1_lv1", "player_tank2_lv1"),
                Name = "PLAYER_TANK",
                HexColor = "#FFE7CD46",
                UpgradeLevel = 1,
                Direction = MoveDirection.Up,
                DestroySndId = GetSoundId(resx, "player_explode"),
                DrawOrder = 1,
                Armor = 0,
                Health = 1,
                Width = 4,
                Height = 4,
                MoveSpeed = 2m,
                TextureAnimationTime = 2,
                Gun = new Gun()
                {
                    ShotSndId = GetSoundId(resx, "player_shot"),
                    BulletSpeed = 6,
                    BulletPower = 1,
                    GunReloadDelay = 10,
                }
            });

            // UPGRADE LEVEL: 2
            Items.Add(new GameFieldObject()
            {
                Type = GameObjectType.Player | GameObjectType.Destroyable | GameObjectType.Unit | GameObjectType.Barrier,
                TextureIdList = GetTextureList(resx, "player_tank1_lv2", "player_tank2_lv2"),
                Name = "PLAYER_TANK",
                HexColor = "#FFE7CD46",
                UpgradeLevel = 2,
                Direction = MoveDirection.Up,
                DestroySndId = GetSoundId(resx, "player_explode"),
                DrawOrder = 1,
                Armor = 0,
                Health = 1,
                Width = 4,
                Height = 4,
                MoveSpeed = 2m,
                TextureAnimationTime = 2,
                Gun = new Gun()
                {
                    ShotSndId = GetSoundId(resx, "player_shot"),
                    BulletSpeed = 6,
                    BulletPower = 1,
                    Capacity = 2,
                    InitialCapacity = 2,
                    GunReloadDelay = 10,
                }
            });

            // UPGRADE LEVEL: 3
            Items.Add(new GameFieldObject()
            {
                Type = GameObjectType.Player | GameObjectType.Destroyable | GameObjectType.Unit | GameObjectType.Barrier,
                TextureIdList = GetTextureList(resx, "player_tank1_lv3", "player_tank2_lv3"),
                Name = "PLAYER_TANK",
                HexColor = "#FFE7CD46",
                UpgradeLevel = 3,
                Direction = MoveDirection.Up,
                DestroySndId = GetSoundId(resx, "player_explode"),
                DrawOrder = 1,
                Armor = 0,
                Health = 1,
                Width = 4,
                Height = 4,
                MoveSpeed = 2m,
                TextureAnimationTime = 2,
                Gun = new Gun()
                {
                    ShotSndId = GetSoundId(resx, "player_shot"),
                    BulletSpeed = 6,
                    Capacity = 2,
                    InitialCapacity = 2,
                    GunReloadDelay = 10,
                    BulletPower = 3,
                }
            });

            // UPGRADE LEVEL: 6
            Items.Add(new GameFieldObject()
            {
                Type = GameObjectType.Player | GameObjectType.Destroyable | GameObjectType.Unit | GameObjectType.Barrier,
                TextureIdList = GetTextureList(resx, "player_tank1_lv3", "player_tank2_lv3"),
                Name = "PLAYER_TANK",
                HexColor = "#FFE7CD46",
                UpgradeLevel = 6,
                Direction = MoveDirection.Up,
                DestroySndId = GetSoundId(resx, "player_explode"),
                DrawOrder = 1,
                Armor = 0,
                Health = 1,
                Width = 4,
                Height = 4,
                MoveSpeed = 2m,
                TextureAnimationTime = 2,
                Gun = new Gun()
                {
                    ShotSndId = GetSoundId(resx, "player_shot"),
                    BulletSpeed = 6,
                    Capacity = 2,
                    InitialCapacity = 2,
                    GunReloadDelay = 10,
                    BulletPower = 4,
                }
            });

            #endregion


            #region ENEMY UNITS

            Items.Add(new GameFieldObject()
            {
                Type = GameObjectType.Destroyable | GameObjectType.Enemy | GameObjectType.Unit | GameObjectType.Barrier,
                Armor = 0,
                TextureIdList = GetTextureList(resx, "light_tank1", "light_tank2"),
                Name = "BASIC_TANK",
                FlashHexColors = new string[] { "#FFFFFFFF", "#FFFFA357", "#FF47C44D" },
                DestroySndId = GetSoundId(resx, "enemy_explode"),
                DrawOrder = 1,
                Health = 1,
                Width = 4,
                Height = 4,
                MoveSpeed = 1.5m,
                TextureAnimationTime = 3,
                BonusPoints = 100,
                Gun = new Gun()
                {
                    BulletSpeed = 4,
                    BulletPower = 1,
                    GunReloadDelay = 10
                }
            });

            Items.Add(new GameFieldObject()
            {
                Type = GameObjectType.Destroyable | GameObjectType.Enemy | GameObjectType.Unit | GameObjectType.Barrier,
                Armor = 0,
                TextureIdList = GetTextureList(resx, "middle_tank1", "middle_tank2"),
                Name = "POWER_TANK",
                FlashHexColors = new string[] { "#FFFFFFFF", "#FFFFA357", "#FF47C44D" },
                DestroySndId = GetSoundId(resx, "enemy_explode"),
                DrawOrder = 1,
                Health = 1,
                Width = 4,
                Height = 4,
                MoveSpeed = 2m,
                TextureAnimationTime = 2,
                BonusPoints = 300,
                Gun = new Gun()
                {
                    BulletSpeed = 6,
                    BulletPower = 1,
                    GunReloadDelay = 10
                }
            });

            Items.Add(new GameFieldObject()
            {
                Type = GameObjectType.Destroyable | GameObjectType.Enemy | GameObjectType.Unit | GameObjectType.Barrier,
                Armor = 0,
                TextureIdList = GetTextureList(resx, "btr1", "btr2"),
                Name = "FAST_TANK",
                FlashHexColors = new string[] { "#FFFFFFFF", "#FFFFA357", "#FF47C44D" },
                DestroySndId = GetSoundId(resx, "enemy_explode"),
                DrawOrder = 1,
                Health = 1,
                Width = 4,
                Height = 4,
                MoveSpeed = 2.5m,
                TextureAnimationTime = 2,
                BonusPoints = 200,
                Gun = new Gun()
                {
                    BulletSpeed = 4,
                    BulletPower = 1,
                    GunReloadDelay = 10
                }
            });

            Items.Add(new GameFieldObject()
            {
                Type = GameObjectType.Destroyable | GameObjectType.Enemy | GameObjectType.Unit | GameObjectType.Barrier,
                Armor = 0,
                TextureIdList = GetTextureList(resx, "heavy_tank1", "heavy_tank2"),
                Name = "HEAVY_TANK",
                FlashHexColors = new string[] { "#FFFFFFFF", "#FFFFA357", "FFFFBE30", "#FF47C44D" },
                DestroySndId = GetSoundId(resx, "enemy_explode"),
                DrawOrder = 1,
                Health = 1,
                Width = 4,
                Height = 4,
                MoveSpeed = 1m,
                TextureAnimationTime = 3,
                BonusPoints = 400,
                Gun = new Gun()
                {
                    BulletSpeed = 4,
                    BulletPower = 1,
                    GunReloadDelay = 10
                }
            });

            //Items.Add(new GameFieldObject()
            //{
            //    Type = GameObjectType.Destroyable | GameObjectType.Enemy | GameObjectType.Unit,
            //    Armor = 0,
            //    TextureIdList = GetTextureList(resx, "player_tank1_lv3", "player_tank2_lv3"),
            //    Name = "SUPER_HEAVY_TANK",
            //    FlashHexColors = new string[] { "#FF61adba", "#FFFFA357", "FFFFBE30", "#FF47C44D" },
            //    DestroySndId = GetSoundId(resx, "enemy_explode"),
            //    DrawOrder = 1,
            //    Health = 1,
            //    Width = 4,
            //    Height = 4,
            //    MoveSpeed = 1.65m,
            //    TextureAnimationTime = 3,
            //    BonusPoints = 400,
            //    Gun = new Gun()
            //    {
            //        BulletSpeed = 4,
            //        BulletPower = 1,
            //        GunReloadDelay = 10
            //    }
            //});

            #endregion


            #region POWER UPS

            Items.Add(new GameFieldObject()
            {
                TextureAnimationTime = 15,
                Type = GameObjectType.Animation | GameObjectType.PowerUp,
                TextureIdList = GetTextureList(resx, "lifeup", ""),
                Name = "LIFE_UP_POWERUP",
                Width = 4,
                Height = 4,
                DrawOrder = 3,
                DestroySndId = GetSoundId(resx, "extra_life"),
                AppearSndId = GetSoundId(resx, "bonus_appear"),
                BonusPoints = 500,
            });

            Items.Add(new GameFieldObject()
            {
                TextureAnimationTime = 15,
                Type = GameObjectType.Animation | GameObjectType.PowerUp,
                TextureIdList = GetTextureList(resx, "kill_enemy", ""),
                Name = "KILL_ENEMY_POWERUP",
                Width = 4,
                Height = 4,
                DrawOrder = 3,
                DestroySndId = GetSoundId(resx, "bonus_destroy"),
                AppearSndId = GetSoundId(resx, "bonus_appear"),
                BonusPoints = 500,
            });

            Items.Add(new GameFieldObject()
            {
                TextureAnimationTime = 15,
                Type = GameObjectType.Animation | GameObjectType.PowerUp,
                TextureIdList = GetTextureList(resx, "defense", ""),
                Name = "DEFENSE_POWERUP",
                Width = 4,
                Height = 4,
                DrawOrder = 3,
                DestroySndId = GetSoundId(resx, "bonus_destroy"),
                AppearSndId = GetSoundId(resx, "bonus_appear"),
                BonusPoints = 500,
            });

            Items.Add(new GameFieldObject()
            {
                TextureAnimationTime = 15,
                Type = GameObjectType.Animation | GameObjectType.PowerUp,
                TextureIdList = GetTextureList(resx, "invulnerability", ""),
                Name = "SHIELD_POWERUP",
                Width = 4,
                Height = 4,
                DrawOrder = 3,
                DestroySndId = GetSoundId(resx, "bonus_destroy"),
                AppearSndId = GetSoundId(resx, "bonus_appear"),
                BonusPoints = 500,
            });

            Items.Add(new GameFieldObject()
            {
                TextureAnimationTime = 15,
                Type = GameObjectType.Animation | GameObjectType.PowerUp,
                TextureIdList = GetTextureList(resx, "upgrade_weapon", ""),
                Name = "WEAPON_UPGRADE_POWERUP",
                Width = 4,
                Height = 4,
                DrawOrder = 3,
                DestroySndId = GetSoundId(resx, "bonus_destroy"),
                AppearSndId = GetSoundId(resx, "bonus_appear"),
                BonusPoints = 500,
            });

            Items.Add(new GameFieldObject()
            {
                TextureAnimationTime = 15,
                Type = GameObjectType.Animation | GameObjectType.PowerUp,
                TextureIdList = GetTextureList(resx, "super_weapon", ""),
                Name = "SUPER_WEAPON_POWERUP",
                Width = 4,
                Height = 4,
                DrawOrder = 3,
                DestroySndId = GetSoundId(resx, "bonus_destroy"),
                AppearSndId = GetSoundId(resx, "bonus_appear"),
                BonusPoints = 500,
            });

            Items.Add(new GameFieldObject()
            {
                TextureAnimationTime = 15,
                Type = GameObjectType.Animation | GameObjectType.PowerUp,
                TextureIdList = GetTextureList(resx, "freeze_enemy", ""),
                Name = "FREEZE_ENEMY_POWERUP",
                Width = 4,
                Height = 4,
                DrawOrder = 3,
                DestroySndId = GetSoundId(resx, "bonus_destroy"),
                AppearSndId = GetSoundId(resx, "bonus_appear"),
                BonusPoints = 500,
            });

            Items.Add(new GameFieldObject()
            {
                TextureAnimationTime = 15,
                Type = GameObjectType.Animation | GameObjectType.PowerUp,
                TextureIdList = GetTextureList(resx, "ship", ""),
                Name = "SHIP_POWERUP",
                Width = 4,
                Height = 4,
                DrawOrder = 3,
                DestroySndId = GetSoundId(resx, "bonus_destroy"),
                AppearSndId = GetSoundId(resx, "bonus_appear"),
                BonusPoints = 500,
            });

            #endregion


            #region STATUS OBJECTS (SIDE PANEL)

            Items.Add(new GameFieldObject()
            {
                Type = GameObjectType.Animation | GameObjectType.Ship,
                TextureIdList = GetTextureList(resx, "ship_shape1"),
                Name = "SHIP_SHAPE",
                Width = 2,
                Height = 2,
                DrawOrder = 3
            });

            Items.Add(new GameFieldObject()
            {
                Type = GameObjectType.None,
                TextureIdList = GetTextureList(resx, "stage_flag"),
                Name = "STAGE_NUMBER",
                Health = 0,
                Width = 2,
                Height = 2,
                DrawOrder = 3
            });

            Items.Add(new GameFieldObject()
            {
                Type = GameObjectType.None,
                TextureIdList = GetTextureList(resx, "enemy_status"),
                Name = "ENEMY_STATUS",
                Health = 0,
                Width = 2,
                Height = 2,
                DrawOrder = 4
            });

            Items.Add(new GameFieldObject()
            {
                Type = GameObjectType.None,
                TextureIdList = GetTextureList(resx, "stage_flag"),
                Name = "STAGE_STATUS",
                Health = 0,
                Width = 4,
                Height = 4,
                DrawOrder = 4
            });

            Items.Add(new GameFieldObject()
            {
                Type = GameObjectType.None,
                TextureIdList = GetTextureList(resx, "player_status"),
                Name = "PLAYER_STATUS",
                Width = 2,
                Height = 2,
                DrawOrder = 4
            });

            #endregion

        }
    }
}