using BattleCity.Common;
using BattleCity.Enums;
using BattleCity.Extensions;
using BattleCity.InputControllers;
using BattleCity.Logging;
using BattleCity.Video;
using SlimDX.Direct3D9;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Rectangle = System.Drawing.Rectangle;

namespace BattleCity.VisualComponents
{
    /// <summary>
    /// Экран отображения доп. настроек
    /// </summary>
    public class ExtrasScreen : IDisposable
    {
        public event Action<bool> Exit;
        public event Action<string> LoadMod;

        IDeviceContext deviceContext;
        IControllerHub controllerHub;
        IGameGraphics graphics;
        ILogger logger;
        GameContent content;
        readonly GameConfig originalConfig;
        const string title = "EXTRAS";
        const string ModNamePrefix = "* ";
        const string ModOptionTag = "MOD";
        const string ExitGameOption = "EXIT";
        const string Tank1990PresetOption = "TANK X 1990 PRESET";
        const string LoadDefaultGameOption = "LOAD DEFAULT GAME";
        const string LoadDefaultsOption = "RESTORE DEFAULTS";
        const string SaveConfigOption = "SAVE CONFIG";
        const string ModsDirectoryName = "Mods";
        Rectangle titleRect;
        Rectangle fontSize;
        Rectangle optionsClipRect;
        IGameFont font;
        IGameFont titleFont;
        IGameFont hintFont;
        int selectedOptionIndex = 0;
        int lineHeight;
        readonly int activeOptionTextColor = Colors.Tomato;
        readonly int titleTextColor = Colors.White;
        readonly int changedOptionTextColor = Colors.Orange;
        readonly int hintTextColor = Colors.DarkOliveGreen;
        int screenWidth, screenHeight;
        List<ExtrasMenuOption> options;
        List<string> modList = new List<string>(0);

        public bool IsVisible { get; set; }

        public ExtrasScreen(
            IDeviceContext deviceContext,
            IControllerHub controllerHub,
            IGameGraphics graphics,
            ILogger logger,
            GameContent content)
        {
            this.deviceContext = deviceContext;
            this.controllerHub = controllerHub;
            this.graphics = graphics;
            this.content = content;
            this.logger = logger;

            originalConfig = GameContentGenerator.CreateDefaultGameConfig();

            var hintFontSize = Math.Max(6, content.CommonConfig.DefaultFontSize * 0.75f);
            hintFont = graphics.CreateFont(content.GetFont(hintFontSize));
            font = graphics.CreateFont(content.GetFont(content.CommonConfig.DefaultFontSize));
            fontSize = font.MeasureString("W");

            Initialize();
        }

        private void Initialize()
        {
            screenWidth = deviceContext.DeviceWidth;
            screenHeight = deviceContext.DeviceHeight;

            lineHeight = Convert.ToInt32(fontSize.Height * 1.8f);

            int titleFontSize = Convert.ToInt32(screenHeight / 9d);

            titleFont = graphics.CreateFont(content.GetFont(titleFontSize));

            var top = (int)(fontSize.Height * 1.9f);
            titleRect = new Rectangle(0, top, screenWidth, Convert.ToInt32(titleFontSize * 1.2f));

            int optionsTop = titleRect.Bottom + fontSize.Height * 2;
            optionsClipRect = new Rectangle(fontSize.Height * 2, optionsTop, screenWidth, screenHeight - fontSize.Height * 2 - optionsTop);

            LoadModList();

            CreateOptions();
        }

        private void LoadModList()
        {
            DirectoryInfo modDir = new DirectoryInfo(ModsDirectoryName);
            if (modDir.Exists)
            {
                modList = modDir
                    .GetDirectories()
                    .Select(s => s.Name)
                    .ToList();
            }
        }

        private string GetNormalizedOptionName(string text, bool preserveAcronyms = false)
        {
            if (string.IsNullOrWhiteSpace(text))
                return string.Empty;
            StringBuilder newText = new StringBuilder(text.Length * 2);
            newText.Append(text[0]);
            for (int i = 1; i < text.Length; i++)
            {
                if (char.IsUpper(text[i]))
                    if ((text[i - 1] != ' ' && !char.IsUpper(text[i - 1])) ||
                    (preserveAcronyms && char.IsUpper(text[i - 1]) &&
                         i < text.Length - 1 && !char.IsUpper(text[i + 1])))
                        newText.Append(' ');
                newText.Append(text[i]);
            }
            return newText.ToString().ToUpper();
        }

        private void CreateOptions()
        {
            int optionNumber = 0;

            string NextOptionNumber()
            {
                optionNumber++;
                return $"{optionNumber}. ";
            }

            int playerMaxUpgradeLevel = content.GameObjects
                .GetAll(p => p.Type.HasFlag(GameObjectType.Player))
                .OrderByDescending(p => p.UpgradeLevel)
                .Select(p => p.UpgradeLevel)
                .First();

            options = new List<ExtrasMenuOption>(new ExtrasMenuOption[]
            {
                new ExtrasMenuOption(
                    NextOptionNumber() +
                    "ENABLE CHEATS (F-KEYS)",
                    content.GameConfig.CheatsEnabled.ToString(),
                    originalConfig.CheatsEnabled.ToString(),
                    new string[] { bool.TrueString, bool.FalseString }, null,
                    (p) => content.GameConfig.CheatsEnabled = bool.Parse(p)),

                new ExtrasMenuOption(
                    NextOptionNumber() +
                    GetNormalizedOptionName(nameof(content.GameConfig.StartLifes)),
                    content.GameConfig.StartLifes.ToString(),
                    originalConfig.StartLifes.ToString(),
                    Enumerable.Range(1, 99).Select(s => s.ToString()).ToArray(), null,
                    (p) => content.GameConfig.StartLifes = int.Parse(p)),

                new ExtrasMenuOption(
                    NextOptionNumber() +
                    GetNormalizedOptionName(nameof(content.GameConfig.ChallengeBonusPoints)),
                    content.GameConfig.ChallengeBonusPoints.ToString(),
                    originalConfig.ChallengeBonusPoints.ToString(),
                    new string[] {"0", "100", "500", "1000", "2000", "5000" },
                    "DISABLED",
                    (p) => content.GameConfig.ChallengeBonusPoints = int.Parse(p)),

                new ExtrasMenuOption(
                    NextOptionNumber() +
                    GetNormalizedOptionName(nameof(content.GameConfig.StageBonusPoints)),
                    content.GameConfig.StageBonusPoints.ToString(),
                    originalConfig.StageBonusPoints.ToString(),
                    new string[] {"0", "100", "500", "1000", "2000", "5000" },
                    "DISABLED",
                    (p) => content.GameConfig.StageBonusPoints = int.Parse(p)),

                new ExtrasMenuOption(
                    NextOptionNumber() +
                    GetNormalizedOptionName(nameof(content.GameConfig.RewardsExtraLifeAnEvery)),
                    content.GameConfig.RewardsExtraLifeAnEvery.ToString(),
                    originalConfig.RewardsExtraLifeAnEvery.ToString(),
                    Enumerable.Range(0, 11).Select(s => (s * 5000).ToString()).ToArray(),
                    "DISABLED",
                    (p) => content.GameConfig.RewardsExtraLifeAnEvery = int.Parse(p)),

                new ExtrasMenuOption(
                    NextOptionNumber() +
                    GetNormalizedOptionName(nameof(content.GameConfig.EnemyFriendlyFire)),
                    content.GameConfig.EnemyFriendlyFire.ToString(),
                    originalConfig.EnemyFriendlyFire.ToString(),
                    new string[] { bool.TrueString, bool.FalseString }, null,
                    (p) => content.GameConfig.EnemyFriendlyFire = bool.Parse(p)),

                new ExtrasMenuOption(
                    NextOptionNumber() +
                    GetNormalizedOptionName(nameof(content.GameConfig.PlayerFriendlyFire)),
                    content.GameConfig.PlayerFriendlyFire.ToString(),
                    originalConfig.PlayerFriendlyFire.ToString(),
                    new string[] { bool.TrueString, bool.FalseString }, null,
                    (p) => content.GameConfig.PlayerFriendlyFire = bool.Parse(p)),

                new ExtrasMenuOption(
                    NextOptionNumber() +
                    GetNormalizedOptionName(nameof(content.GameConfig.PlayerDestroyBaseAllowed)),
                    content.GameConfig.PlayerDestroyBaseAllowed.ToString(),
                    originalConfig.PlayerDestroyBaseAllowed.ToString(),
                    new string[] { bool.TrueString, bool.FalseString }, null,
                    (p) => content.GameConfig.PlayerDestroyBaseAllowed = bool.Parse(p)),

                new ExtrasMenuOption(
                    NextOptionNumber() +
                    GetNormalizedOptionName(nameof(content.GameConfig.EmenySpawnDelay)),
                    content.GameConfig.EmenySpawnDelay.ToString(),
                    originalConfig.EmenySpawnDelay.ToString(),
                    Enumerable.Range(0, 11).Select(s => (s * 30).ToString()).ToArray(),
                    "INSTANTLY",
                    (p) => content.GameConfig.EmenySpawnDelay = int.Parse(p)),

                new ExtrasMenuOption(
                    NextOptionNumber() +
                    GetNormalizedOptionName(nameof(content.GameConfig.SpawnAnimationDuration)),
                    content.GameConfig.SpawnAnimationDuration.ToString(),
                    originalConfig.SpawnAnimationDuration.ToString(),
                    Enumerable.Range(0, 6).Select(s => (s * 30).ToString()).ToArray(),
                    "INSTANTLY",
                    (p) => content.GameConfig.SpawnAnimationDuration = int.Parse(p)),

                new ExtrasMenuOption(
                    NextOptionNumber() +
                    GetNormalizedOptionName(nameof(content.GameConfig.MaxActiveEnemy)),
                    content.GameConfig.MaxActiveEnemy.ToString(),
                    originalConfig.MaxActiveEnemy.ToString(),
                    Enumerable.Range(1, 10).Select(s => s.ToString()).ToArray(), null,
                    (p) => content.GameConfig.MaxActiveEnemy = int.Parse(p)),

                new ExtrasMenuOption(
                    NextOptionNumber() +
                    GetNormalizedOptionName(nameof(content.GameConfig.MaxEnemy)),
                    content.GameConfig.MaxEnemy.ToString(),
                    originalConfig.MaxEnemy.ToString(),
                    Enumerable.Range(1, content.GameConfig.MaxEnemiesPerStage).Select(s => s.ToString()).ToArray(), null,
                    (p) => content.GameConfig.MaxEnemy = int.Parse(p)),

                new ExtrasMenuOption(
                    NextOptionNumber() +
                    GetNormalizedOptionName(nameof(content.GameConfig.EnemyFreezeDuration)),
                    content.GameConfig.EnemyFreezeDuration.ToString(),
                    originalConfig.EnemyFreezeDuration.ToString(),
                    Enumerable.Range(0, 11).Select(s => (s * 60).ToString()).ToArray(),
                    "DISABLED",
                    (p) => content.GameConfig.EnemyFreezeDuration = int.Parse(p)),

                new ExtrasMenuOption(
                    NextOptionNumber() +
                    GetNormalizedOptionName(nameof(content.GameConfig.PlayerFreezeDuration)),
                    content.GameConfig.PlayerFreezeDuration.ToString(),
                    originalConfig.PlayerFreezeDuration.ToString(),
                    Enumerable.Range(0, 11).Select(s => (s * 60).ToString()).ToArray(),
                    "DISABLED",
                    (p) => content.GameConfig.PlayerFreezeDuration = int.Parse(p)),

                new ExtrasMenuOption(
                    NextOptionNumber() +
                    GetNormalizedOptionName(nameof(content.GameConfig.MaxActivePowerUpsOnField)),
                    content.GameConfig.MaxActivePowerUpsOnField.ToString(),
                    originalConfig.MaxActivePowerUpsOnField.ToString(),
                    Enumerable.Range(-1, 12).Select(s => s.ToString()).ToArray(),
                    "INFINITE",
                    (p) => content.GameConfig.MaxActivePowerUpsOnField = int.Parse(p)),

                new ExtrasMenuOption(
                    NextOptionNumber() +
                    GetNormalizedOptionName(nameof(content.GameConfig.PlayerPowerUpAllowed)),
                    content.GameConfig.PlayerPowerUpAllowed.ToString(),
                    originalConfig.PlayerPowerUpAllowed.ToString(),
                    new string[] { bool.TrueString, bool.FalseString }, null,
                    (p) => content.GameConfig.PlayerPowerUpAllowed = bool.Parse(p)),

                new ExtrasMenuOption(
                    NextOptionNumber() +
                    GetNormalizedOptionName(nameof(content.GameConfig.EnemyPowerUpAllowed)),
                    content.GameConfig.EnemyPowerUpAllowed.ToString(),
                    originalConfig.EnemyPowerUpAllowed.ToString(),
                    new string[] { bool.TrueString, bool.FalseString }, null,
                    (p) => content.GameConfig.EnemyPowerUpAllowed = bool.Parse(p)),

                new ExtrasMenuOption(
                    NextOptionNumber() +
                    GetNormalizedOptionName(nameof(content.GameConfig.EnemyPowerUpHasEffect)),
                    content.GameConfig.EnemyPowerUpHasEffect.ToString(),
                    originalConfig.EnemyPowerUpHasEffect.ToString(),
                    new string[] { bool.TrueString, bool.FalseString }, null,
                    (p) => content.GameConfig.EnemyPowerUpHasEffect = bool.Parse(p)),

                new ExtrasMenuOption(
                    NextOptionNumber() +
                    GetNormalizedOptionName(nameof(content.GameConfig.PowerUpLifetimeDuration)),
                    content.GameConfig.PowerUpLifetimeDuration.ToString(),
                    originalConfig.PowerUpLifetimeDuration.ToString(),
                    Enumerable.Range(0, 11).Select(s => (s * 60).ToString()).ToArray(),
                    "INFINITE",
                    (p) => content.GameConfig.PowerUpLifetimeDuration = int.Parse(p)),

                new ExtrasMenuOption(
                    NextOptionNumber() +
                    GetNormalizedOptionName(nameof(content.GameConfig.BonusedEnemySpawnChance)),
                    content.GameConfig.BonusedEnemySpawnChance.ToString(),
                    originalConfig.BonusedEnemySpawnChance.ToString(),
                    Enumerable.Range(0, 101).Select(s => s.ToString()).ToArray(),
                    "NEVER",
                    (p) => content.GameConfig.BonusedEnemySpawnChance = int.Parse(p)),

                new ExtrasMenuOption(
                    NextOptionNumber() +
                    GetNormalizedOptionName(nameof(content.GameConfig.MaxBonusedUnitsOnField)),
                    content.GameConfig.MaxBonusedUnitsOnField.ToString(),
                    originalConfig.MaxBonusedUnitsOnField.ToString(),
                    Enumerable.Range(-1, 102).Select(s => s.ToString()).ToArray(),
                    "INFINITE",
                    (p) => content.GameConfig.MaxBonusedUnitsOnField = int.Parse(p)),

                new ExtrasMenuOption(
                    NextOptionNumber() +
                    GetNormalizedOptionName(nameof(content.GameConfig.HidePowerUpsIfBonusedEnemySpawned)),
                    content.GameConfig.HidePowerUpsIfBonusedEnemySpawned.ToString(),
                    originalConfig.HidePowerUpsIfBonusedEnemySpawned.ToString(),
                    new string[] { bool.TrueString, bool.FalseString }, null,
                    (p) => content.GameConfig.HidePowerUpsIfBonusedEnemySpawned = bool.Parse(p)),

                new ExtrasMenuOption(
                    NextOptionNumber() +
                    GetNormalizedOptionName(nameof(content.GameConfig.MaxExtraBonusPerUnit)),
                    content.GameConfig.MaxExtraBonusPerUnit.ToString(),
                    originalConfig.MaxExtraBonusPerUnit.ToString(),
                    Enumerable.Range(0, 4).Select(s => s.ToString()).ToArray(),
                    "NEVER",
                    (p) => content.GameConfig.MaxExtraBonusPerUnit = int.Parse(p)),

                new ExtrasMenuOption(
                    NextOptionNumber() +
                    GetNormalizedOptionName(nameof(content.GameConfig.TowerTempDefenseDuration)),
                    content.GameConfig.TowerTempDefenseDuration.ToString(),
                    originalConfig.TowerTempDefenseDuration.ToString(),
                    Enumerable.Range(-1, 22).Select(s => s.ToString()).ToArray(),
                    "INFINITE",
                    (p) => content.GameConfig.TowerTempDefenseDuration = int.Parse(p)),

                new ExtrasMenuOption(
                    NextOptionNumber() +
                    GetNormalizedOptionName(nameof(content.GameConfig.EnemyAgressivity)),
                    content.GameConfig.EnemyAgressivity.ToString(),
                    originalConfig.EnemyAgressivity.ToString(),
                    Enumerable.Range(0, 21).Select(s => s.ToString()).ToArray(),
                    "PASSIVE",
                    (p) => content.GameConfig.EnemyAgressivity = int.Parse(p)),

                new ExtrasMenuOption(
                    NextOptionNumber() +
                    GetNormalizedOptionName(nameof(content.GameConfig.ForceRandomEnemies)),
                    content.GameConfig.ForceRandomEnemies.ToString(),
                    originalConfig.ForceRandomEnemies.ToString(),
                    new string[] { bool.TrueString, bool.FalseString }, null,
                    (p) => content.GameConfig.ForceRandomEnemies = bool.Parse(p)),

                new ExtrasMenuOption(
                    NextOptionNumber() +
                    GetNormalizedOptionName(nameof(content.GameConfig.PlayerDefaultUpgradeLevel)),
                    content.GameConfig.PlayerDefaultUpgradeLevel.ToString(),
                    originalConfig.PlayerDefaultUpgradeLevel.ToString(),
                    Enumerable.Range(0, playerMaxUpgradeLevel).Select(s => s.ToString()).ToArray(),
                    null,
                    (p) => content.GameConfig.PlayerDefaultUpgradeLevel = int.Parse(p)),

                new ExtrasMenuOption(
                    NextOptionNumber() +
                    GetNormalizedOptionName(nameof(content.GameConfig.UnitMaxUpgradeLevel)),
                    content.GameConfig.UnitMaxUpgradeLevel.ToString(),
                    originalConfig.UnitMaxUpgradeLevel.ToString(),
                    Enumerable.Range(-1, 8).Select(s => s.ToString()).ToArray(),
                    "NO LIMITS",
                    (p) => content.GameConfig.UnitMaxUpgradeLevel = int.Parse(p)),

                new ExtrasMenuOption(
                    NextOptionNumber() +
                    GetNormalizedOptionName(nameof(content.GameConfig.ResetUnitUpgradesOnStageStart)),
                    content.GameConfig.ResetUnitUpgradesOnStageStart.ToString(),
                    originalConfig.ResetUnitUpgradesOnStageStart.ToString(),
                    new string[] { bool.TrueString, bool.FalseString }, null,
                    (p) => content.GameConfig.ResetUnitUpgradesOnStageStart = bool.Parse(p)),

                new ExtrasMenuOption(
                    NextOptionNumber() +
                    GetNormalizedOptionName(nameof(content.GameConfig.PlayerSpawnShieldDuration)),
                    content.GameConfig.PlayerSpawnShieldDuration.ToString(),
                    originalConfig.PlayerSpawnShieldDuration.ToString(),
                    Enumerable.Range(0, 16).Select(s => (s * 60).ToString()).ToArray(),
                    "DISABLED",
                    (p) => content.GameConfig.PlayerSpawnShieldDuration = int.Parse(p)),

                new ExtrasMenuOption(
                    NextOptionNumber() +
                    GetNormalizedOptionName(nameof(content.GameConfig.EnemySpawnShieldDuration)),
                    content.GameConfig.EnemySpawnShieldDuration.ToString(),
                    originalConfig.EnemySpawnShieldDuration.ToString(),
                    Enumerable.Range(0, 16).Select(s => (s * 60).ToString()).ToArray(),
                    "DISABLED",
                    (p) => content.GameConfig.EnemySpawnShieldDuration = int.Parse(p)),

                new ExtrasMenuOption(
                    NextOptionNumber() +
                    GetNormalizedOptionName(nameof(content.GameConfig.ExtraShieldDuration)),
                    content.GameConfig.ExtraShieldDuration.ToString(),
                    originalConfig.ExtraShieldDuration.ToString(),
                    Enumerable.Range(0, 16).Select(s => (s * 60).ToString()).ToArray(),
                    "DISABLED",
                    (p) => content.GameConfig.ExtraShieldDuration = int.Parse(p)),

                new ExtrasMenuOption(
                    NextOptionNumber() +
                    GetNormalizedOptionName(nameof(content.GameConfig.PointsTextShowDuration)),
                    content.GameConfig.PointsTextShowDuration.ToString(),
                    originalConfig.PointsTextShowDuration.ToString(),
                    Enumerable.Range(0, 9).Select(s => (s * 30).ToString()).ToArray(),
                    "DISABLED",
                    (p) => content.GameConfig.PointsTextShowDuration = int.Parse(p)),

                new ExtrasMenuOption(
                    NextOptionNumber() +
                    GetNormalizedOptionName(nameof(content.GameConfig.TreasureBonusPoints)),
                    content.GameConfig.TreasureBonusPoints.ToString(),
                    originalConfig.TreasureBonusPoints.ToString(),
                    Enumerable.Range(0, 21).Select(s => (s * 100).ToString()).ToArray(),
                    "DISABLED",
                    (p) => content.GameConfig.TreasureBonusPoints = int.Parse(p)),

                new ExtrasMenuOption(
                    NextOptionNumber() +
                    GetNormalizedOptionName(nameof(content.GameConfig.PlaceholderFlickerFrames)),
                    content.GameConfig.PlaceholderFlickerFrames.ToString(),
                    originalConfig.PlaceholderFlickerFrames.ToString(),
                    Enumerable.Range(0, 5).Select(s => (s * 15).ToString()).ToArray(),
                    "DISABLED",
                    (p) => content.GameConfig.PlaceholderFlickerFrames = int.Parse(p)),

                new ExtrasMenuOption(
                    NextOptionNumber() +
                    GetNormalizedOptionName(nameof(content.GameConfig.ShowChessboard)),
                    content.GameConfig.ShowChessboard.ToString(),
                    originalConfig.ShowChessboard.ToString(),
                    new string[] { bool.TrueString, bool.FalseString }, null,
                    (p) => content.GameConfig.ShowChessboard = bool.Parse(p)),

                new ExtrasMenuOption(
                    NextOptionNumber() +
                    GetNormalizedOptionName(nameof(content.GameConfig.ShowGameOverScreen)),
                    content.GameConfig.ShowGameOverScreen.ToString(),
                    originalConfig.ShowGameOverScreen.ToString(),
                    new string[] { bool.TrueString, bool.FalseString }, null,
                    (p) => content.GameConfig.ShowGameOverScreen = bool.Parse(p)),

                new ExtrasMenuOption(
                    NextOptionNumber() +
                    GetNormalizedOptionName(nameof(content.GameConfig.ShowHiScoreScreen)),
                    content.GameConfig.ShowHiScoreScreen.ToString(),
                    originalConfig.ShowHiScoreScreen.ToString(),
                    new string[] { bool.TrueString, bool.FalseString }, null,
                    (p) => content.GameConfig.ShowHiScoreScreen = bool.Parse(p)),

                //new ExtrasMenuOption() { Text = "LOAD TANK 1990" },
                //new ExtrasMenuOption() { Text = "RESTORE DEFAULTS" },
                //new ExtrasMenuOption() { Text = "EXIT" }
            });

            options.AddRange(
                modList.Select(s => new ExtrasMenuOption() { Text = $"{ModNamePrefix}{s}", Tag = ModOptionTag })
            );

            options.Add(new ExtrasMenuOption() { Text = Tank1990PresetOption });

            if (!content.IsDefaultContentDirectory)
            {
                options.Add(new ExtrasMenuOption() { Text = LoadDefaultGameOption, Tag = ModOptionTag });
                options.Add(new ExtrasMenuOption() { Text = SaveConfigOption });
            }

            options.AddRange(new[]
            {
                new ExtrasMenuOption() { Text = LoadDefaultsOption },
                new ExtrasMenuOption() { Text = ExitGameOption }
            });
        }

        private void Update()
        {
            UpdateInput();
        }

        private void UpdateInput()
        {
            if (controllerHub.Keyboard.IsDown(KeyboardKey.NumberPadEnter) ||
                controllerHub.IsKeyPressed(1, ButtonNames.Start, true) ||
                controllerHub.IsKeyPressed(1, ButtonNames.Attack, true))
            {
                ActivateOption(options[selectedOptionIndex]);
            }
            if (controllerHub.IsKeyPressed(1, ButtonNames.Up, true) ||
                controllerHub.IsLongPressed(1, ButtonNames.Up) ||
                controllerHub.Keyboard.IsDown(KeyboardKey.UpArrow) ||
                controllerHub.Keyboard.IsLongPress(KeyboardKey.UpArrow))
            {
                if (selectedOptionIndex == 0)
                    selectedOptionIndex = options.Count - 1;
                else
                    selectedOptionIndex--;
            }
            else if (controllerHub.IsKeyPressed(1, ButtonNames.Down, true) ||
                    controllerHub.IsLongPressed(1, ButtonNames.Down) ||
                    controllerHub.Keyboard.IsDown(KeyboardKey.DownArrow) ||
                    controllerHub.Keyboard.IsLongPress(KeyboardKey.DownArrow))
            {
                if (selectedOptionIndex < options.Count - 1)
                    selectedOptionIndex++;
                else
                    selectedOptionIndex = 0;
            }
            else if (controllerHub.Keyboard.IsDown(KeyboardKey.PageDown))
            {
                int maxVisibleItems = Convert.ToInt32(optionsClipRect.Height / (double)lineHeight);
                selectedOptionIndex += maxVisibleItems;
                if (selectedOptionIndex >= options.Count)
                    selectedOptionIndex = 0;
                else
                    selectedOptionIndex -= selectedOptionIndex % maxVisibleItems;
            }
            else if (controllerHub.Keyboard.IsDown(KeyboardKey.PageUp))
            {
                int maxVisibleItems = Convert.ToInt32(optionsClipRect.Height / (double)lineHeight);
                selectedOptionIndex -= maxVisibleItems;
                if (selectedOptionIndex < 0)
                    selectedOptionIndex = options.Count - 1;
                else
                    selectedOptionIndex -= selectedOptionIndex % maxVisibleItems;
            }
            else if (controllerHub.IsKeyPressed(1, ButtonNames.Right, true) ||
                    controllerHub.IsLongPressed(1, ButtonNames.Right))
            {
                SetNextOptionValue(options[selectedOptionIndex]);
            }
            else if (controllerHub.IsKeyPressed(1, ButtonNames.Left, true) ||
                    controllerHub.IsLongPressed(1, ButtonNames.Left))
            {
                SetPrevOptionValue(options[selectedOptionIndex]);
            }
            else if (controllerHub.IsKeyPressed(1, ButtonNames.DeleteObject, true) ||
                    controllerHub.Keyboard.IsDown(KeyboardKey.Backspace))
            {
                options[selectedOptionIndex].Reset();
            }

            else if (controllerHub.IsKeyPressed(1, ButtonNames.Cancel, true))
            {
                Exit?.Invoke(false);
            }
        }

        private void SetNextOptionValue(ExtrasMenuOption option)
        {
            option?.NextValue();
        }

        private void SetPrevOptionValue(ExtrasMenuOption option)
        {
            option?.PreviousValue();
        }

        private void ActivateOption(MenuOption option)
        {
            if (option == null)
                return;

            if (option.Tag == ModOptionTag)
            {
                LoadMod(Path.Combine(ModsDirectoryName, option.Text.Substring(ModNamePrefix.Length)));
            }
            else
            {
                if (option.Text == ExitGameOption)
                {
                    Exit?.Invoke(false);
                }
                else if (option.Text == SaveConfigOption)
                {
                    content.GameConfig.Save();
                }
                else if (option.Text == LoadDefaultGameOption)
                {
                    LoadMod?.Invoke(null);
                }
                else if (option.Text == Tank1990PresetOption)
                {
                    var config = GameContentGenerator.CreateTank1990GameConfig(content.GameConfig.DirectoryPath);
                    content.GameConfig.CopyFrom(config);
                    CreateOptions();
                    selectedOptionIndex = 0;
                    //Exit?.Invoke(true);
                }
                else if (option.Text == LoadDefaultsOption)
                {
                    if (content.IsDefaultContentDirectory)
                    {
                        var defaultConfig = GameContentGenerator.CreateDefaultGameConfig(content.GameConfig.DirectoryPath);
                        content.GameConfig.CopyFrom(defaultConfig);
                    }
                    else
                    {
                        var config = GameConfig.Load(content.GameConfig.DirectoryPath, content.GameConfig.FileName, logger);
                        content.GameConfig.CopyFrom(config);
                    }

                    CreateOptions();
                }
            }
        }

        private void DrawTitle()
        {
            titleFont.DrawString(title, titleRect, DrawStringFormat.Top | DrawStringFormat.Center | DrawStringFormat.WordBreak, titleTextColor);
            graphics.DrawBrickWallOverlay(
                0, titleRect.Y, screenWidth, screenHeight,
                fontSize.Height * 4, fontSize.Height * 2,
                ColorConverter.ToInt32(content.CommonConfig.LogoFontColor));
        }

        private void DrawOptions()
        {
            var currentViewport = deviceContext.Device.Viewport;
            deviceContext.Device.Viewport = new Viewport(
                optionsClipRect.X, optionsClipRect.Y, optionsClipRect.Width, optionsClipRect.Height);

            int maxVisibleItems = Convert.ToInt32(optionsClipRect.Height / (double)lineHeight);
            int firstVisibleIndex = selectedOptionIndex < maxVisibleItems
                ? 0
                : (selectedOptionIndex / maxVisibleItems) * maxVisibleItems;


            for (int i = firstVisibleIndex, n = 0; n < maxVisibleItems && i < options.Count; i++, n++)
            {
                var option = options[i];
                option.X = optionsClipRect.X;
                option.Y = optionsClipRect.Y + n * lineHeight;

                if (option.IsChanged)
                {
                    option.Color = i == selectedOptionIndex ? activeOptionTextColor : changedOptionTextColor;
                }
                else
                {
                    option.Color = i == selectedOptionIndex ? activeOptionTextColor : content.GameConfig.TextColor;
                }


                option.Draw(font);
            }

            deviceContext.Device.Viewport = currentViewport;

            hintFont.DrawString("LEFT/RIGHT: EDIT  BACKSPACE: RESET  UP/DN/PGUP/PGDN: NAVIGATION",
                optionsClipRect.X, Convert.ToInt32(screenHeight - lineHeight), hintTextColor);
        }

        /// <summary>
        /// Сбросить в начальное состояние
        /// </summary>
        public void Reset()
        {
            selectedOptionIndex = 0;
        }

        /// <summary>
        /// Отрисовка
        /// </summary>
        public void Render()
        {
            Update();
            DrawTitle();
            DrawOptions();
        }

        /// <summary>
        /// Удаление всех используемых объектов, освобождение памяти
        /// </summary>
        public void Dispose()
        {
            if (deviceContext != null)
            {
                deviceContext = null;
            }

            if (font != null)
            {
                font.Dispose();
                font = null;
            }

            if (titleFont != null)
            {
                titleFont.Dispose();
                titleFont = null;
            }

            if (hintFont != null)
            {
                hintFont.Dispose();
                hintFont = null;
            }

            controllerHub = null;
            content = null;
            options = null;
            graphics = null;
            logger = null;
        }
    }
}
