using BattleCity.Audio;
using BattleCity.Common;
using BattleCity.Enums;
using BattleCity.GameObjects;
using BattleCity.InputControllers;
using BattleCity.Logging;
using BattleCity.Video;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Rectangle = System.Drawing.Rectangle;

namespace BattleCity.VisualComponents
{
    /// <summary>
    /// Экран отображения настроек
    /// </summary>
    public class SettingsScreen : IDisposable
    {
        #region events

        public event Action Exit;

        #endregion

        #region members

        IGameApplication gameApplication;
        IDeviceContext deviceContext;
        IControllerHub controllerHub;
        ISoundEngine soundEngine;
        IGameFont font;
        IGameFont titleFont;
        BattleUnit selector;
        IGameGraphics graphics;
        GameContent content;
        ExtrasScreen extrasScreen;
        ControllerScreen controllerScreen;
        List<SettingMenuOption> options = new List<SettingMenuOption>();
        int text_y;
        int selector_y;
        int selector_x;
        int text_x;
        int text_h;
        readonly Rectangle fontSize;
        readonly string sndTestName, musicTestName;
        readonly int titleTextColor = Colors.White;
        readonly float titleTxScaleX = 6.93f;
        readonly float titleTxScaleY = 2.537f;
        readonly float titleTxOffsetX = 0.001f;
        readonly float titleTxOffsetY = 0.0021f;
        int screenWidth, screenHeight;
        const string title = "SETTINGS";
        Rectangle titleRect;
        int selectedOptionIndex = 0;
        int frameNumber;

        #endregion

        #region Properties

        private string SfxLevelText => Convert.ToInt32(soundEngine.SfxLevel * 100).ToString();

        private string MusicLevelText => Convert.ToInt32(soundEngine.MusicLevel * 100).ToString();

        private string FullScreenModeText => gameApplication.IsFullScreen ? "YES" : "NO";

        private string AspectRatioModeText => gameApplication.SaveAspectRatio ? "YES" : "NO";

        private string ContinuousFireText => gameApplication.ContinuousFire ? "YES" : "NO";

        private string ScanlinesFxLevelText => Math.Max(0, Math.Min(100, gameApplication.ScanlinesFxLevel)).ToString();

        #endregion

        #region Constructor

        public SettingsScreen(
            IGameApplication gameApplication,
            IDeviceContext deviceContext,
            IControllerHub controllerHub,
            ISoundEngine soundEngine,
            ILogger logger,
            GameContent content,
            IGameGraphics graphics)
        {
            this.gameApplication = gameApplication;
            this.deviceContext = deviceContext;
            this.controllerHub = controllerHub;
            this.soundEngine = soundEngine;
            this.content = content;
            this.graphics = graphics;
            deviceContext.DeviceResize += DeviceContext_DeviceResize;
            font = graphics.CreateFont(content.GetFont(content.CommonConfig.DefaultFontSize));
            fontSize = font.MeasureString("W");
            sndTestName = Path.GetFileNameWithoutExtension(content.CommonConfig.CheckSoundLevelFileName);
            musicTestName = Path.GetFileNameWithoutExtension(content.CommonConfig.CheckMusicLevelFileName);
            extrasScreen = new ExtrasScreen(deviceContext, controllerHub, graphics, logger, content);
            extrasScreen.Exit += ExtrasScreen_Exit;
            extrasScreen.LoadMod += ExtrasScreen_LoadMod;
            controllerScreen = new ControllerScreen(gameApplication, deviceContext, controllerHub, graphics, logger, content);
            controllerScreen.Exit += ControllerScreen_Exit;
            Initialize();
        }

        #endregion

        #region methods

        private void ExtrasScreen_LoadMod(string contentDirectoryName)
        {
            gameApplication.SetContentDirectory(contentDirectoryName);
            Initialize();
        }

        private void ExtrasScreen_Exit(bool returnToMainScreen)
        {
            extrasScreen.IsVisible = false;
            if (returnToMainScreen)
                Exit?.Invoke();
        }

        private void ControllerScreen_Exit(bool returnToMainScreen)
        {
            controllerScreen.IsVisible = false;
            if (returnToMainScreen)
                Exit?.Invoke();
        }

        private void DestroyTitleFont()
        {
            if (titleFont != null)
            {
                titleFont.Dispose();
                titleFont = null;
            }
        }

        private void CreateTitleFont()
        {
            int titleFontSize = Convert.ToInt32(screenHeight / 9d);
            titleFont = graphics.CreateFont(content.GetFont(titleFontSize));
        }

        private void DeviceContext_DeviceResize()
        {
            Initialize();
        }

        /// <summary>
        /// Сбросить в начальное состояние
        /// </summary>
        public void Reset()
        {
            selectedOptionIndex = 0;
            controllerScreen.IsVisible = false;
            extrasScreen.IsVisible = false;
            selector.Y = selector_y + (selectedOptionIndex * text_h);
        }

        /// <summary>
        /// Отрисовка
        /// </summary>
        public void Render()
        {
            if (extrasScreen.IsVisible)
            {
                extrasScreen.Render();
            }
            else if (controllerScreen.IsVisible)
            {
                controllerScreen.Render();
            }
            else
            {
                Update();

                titleFont.DrawString(title, titleRect, DrawStringFormat.Top | DrawStringFormat.Center | DrawStringFormat.WordBreak, titleTextColor);
                graphics.DrawBrickWallOverlay(
                    0, titleRect.Y, screenWidth, screenHeight,
                    fontSize.Height * titleTxScaleX, fontSize.Height * titleTxScaleY,
                    ColorConverter.ToInt32(content.CommonConfig.LogoFontColor),
                    titleTxOffsetX, titleTxOffsetY);

                selector.UpdateAnimation(frameNumber);
                graphics.BeginDrawGameObjects();
                graphics.DrawGameObject(0, 0, selector, 0, 1);
                graphics.EndDrawGameObjects();

                foreach (var option in options)
                    option.Draw(font);
            }
        }

        private void Initialize()
        {
            DestroyTitleFont();

            screenWidth = deviceContext.DeviceWidth;
            screenHeight = deviceContext.DeviceHeight;

            CreateTitleFont();

            text_h = (int)(fontSize.Height * 1.9f);
            var selectorSize = (int)(fontSize.Height * 1.6f);

            var top = screenHeight - 9 * text_h;
            selector_y = top + (selectorSize - text_h) / 2;
            text_x = screenWidth / 3;
            selector_x = text_x - Convert.ToInt32(text_h * 1.5);
            var end_text_y_coord = top + (text_h - fontSize.Height) / 2;
            text_y = end_text_y_coord;

            selector = new UserBattleUnit(null)
            {
                HexColor = "#FFFFCA57",
                TextureIdList = content.GameObjects
                    .GetAll(p => p.Type.HasFlag(GameObjectType.Player))
                    .OrderBy(p => p.UpgradeLevel)
                    .Select(p => p.TextureIdList)
                    .FirstOrDefault(),
                Width = text_h,
                Height = text_h,
                Y = selector_y,
                X = selector_x,
                IsVisible = true,
                Direction = MoveDirection.Right,
                TextureAnimationTime = 3
            };

            InitOptions();

            selector.Y = selector_y + (selectedOptionIndex * text_h);

            titleRect = new Rectangle(0, text_h, screenWidth, screenHeight);
        }

        private void Update()
        {
            UpdateInput();
            frameNumber++;
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
                    selectedOptionIndex = options.FindLastIndex(x => x.Selectable);// options.Count - 1;
                else
                    selectedOptionIndex--;
                selector.Y = selector_y + (selectedOptionIndex * text_h);
            }
            else if (controllerHub.IsKeyPressed(1, ButtonNames.Down, true) ||
                    controllerHub.IsLongPressed(1, ButtonNames.Down) ||
                    controllerHub.Keyboard.IsDown(KeyboardKey.DownArrow) ||
                    controllerHub.Keyboard.IsLongPress(KeyboardKey.DownArrow))
            {
                if (selectedOptionIndex < options.FindLastIndex(x => x.Selectable))
                    selectedOptionIndex++;
                else
                    selectedOptionIndex = options.FindIndex(x => x.Selectable);
                selector.Y = selector_y + (selectedOptionIndex * text_h);
            }
            else if (controllerHub.IsKeyPressed(1, ButtonNames.Right, true) ||
                    controllerHub.IsLongPressed(1, ButtonNames.Right) ||
                controllerHub.Keyboard.IsDown(KeyboardKey.RightArrow) ||
                controllerHub.Keyboard.IsLongPress(KeyboardKey.RightArrow))
            {
                SetNextOptionValue(options[selectedOptionIndex]);
            }
            else if (controllerHub.IsKeyPressed(1, ButtonNames.Left, true) ||
                    controllerHub.IsLongPressed(1, ButtonNames.Left) ||
                controllerHub.Keyboard.IsDown(KeyboardKey.LeftArrow) ||
                controllerHub.Keyboard.IsLongPress(KeyboardKey.LeftArrow))
            {
                SetPrevOptionValue(options[selectedOptionIndex]);
            }

            else if (controllerHub.IsKeyPressed(1, ButtonNames.Cancel, true) ||
                    controllerHub.Keyboard.IsDown(KeyboardKey.F12))
            {
                Exit?.Invoke();
            }
        }

        private void SetNextOptionValue(SettingMenuOption option)
        {
            switch (option.Section)
            {
                case SettingSectionEnum.SoundLevel:
                    soundEngine.SfxLevel = Math.Min(1, soundEngine.SfxLevel + 0.05f);
                    option.DisplayValue = SfxLevelText;
                    soundEngine.PlaySound(sndTestName);
                    return;
                case SettingSectionEnum.MusicLevel:
                    soundEngine.MusicLevel = Math.Min(1, soundEngine.MusicLevel + 0.05f);
                    option.DisplayValue = MusicLevelText;
                    soundEngine.PlayMusic(musicTestName, true);
                    return;
                //case SettingSectionEnum.ContinuousFire:
                //    gameApplication.ContinuousFire = !gameApplication.ContinuousFire;
                //    option.DisplayValue = ContinuousFireText;
                //    return;
                case SettingSectionEnum.FullScreenMode:
                    gameApplication.SwitchFullScreenMode();
                    option.DisplayValue = FullScreenModeText;
                    return;
                case SettingSectionEnum.AspectRatioMode:
                    gameApplication.SwitchAspectRatio();
                    option.DisplayValue = AspectRatioModeText;
                    return;
                case SettingSectionEnum.ScanlinesFxLevel:
                    gameApplication.ScanlinesFxLevel = Math.Min(100, gameApplication.ScanlinesFxLevel + 5);
                    option.DisplayValue = ScanlinesFxLevelText;
                    return;
            }
        }

        private void SetPrevOptionValue(SettingMenuOption option)
        {
            switch (option.Section)
            {
                case SettingSectionEnum.SoundLevel:
                    soundEngine.SfxLevel = Math.Max(0, soundEngine.SfxLevel - 0.05f);
                    option.DisplayValue = SfxLevelText;
                    soundEngine.PlaySound(sndTestName);
                    return;
                case SettingSectionEnum.MusicLevel:
                    soundEngine.MusicLevel = Math.Max(0, soundEngine.MusicLevel - 0.05f);
                    option.DisplayValue = MusicLevelText;
                    soundEngine.PlayMusic(musicTestName, true);
                    return;
                //case SettingSectionEnum.ContinuousFire:
                //    gameApplication.ContinuousFire = !gameApplication.ContinuousFire;
                //    option.DisplayValue = ContinuousFireText;
                //    return;
                case SettingSectionEnum.FullScreenMode:
                    gameApplication.SwitchFullScreenMode();
                    option.DisplayValue = FullScreenModeText;
                    return;
                case SettingSectionEnum.AspectRatioMode:
                    gameApplication.SwitchAspectRatio();
                    option.DisplayValue = AspectRatioModeText;
                    return;
                case SettingSectionEnum.ScanlinesFxLevel:
                    gameApplication.ScanlinesFxLevel = Math.Max(0, gameApplication.ScanlinesFxLevel - 5);
                    option.DisplayValue = ScanlinesFxLevelText;
                    return;
            }
        }

        /// <summary>
        /// Активация опции
        /// </summary>
        /// <param name="option"></param>
        private void ActivateOption(SettingMenuOption option)
        {
            switch (option.Section)
            {
                case SettingSectionEnum.Exit:
                    Exit?.Invoke();
                    return;
                case SettingSectionEnum.Extras:
                    extrasScreen.Reset();
                    extrasScreen.IsVisible = true;
                    return;
                case SettingSectionEnum.Controllers:
                    controllerScreen.Reset();
                    controllerScreen.IsVisible = true;
                    return;
            }
        }

        /// <summary>
        /// Создать опции
        /// </summary>
        private void InitOptions()
        {
            options.Clear();

            var y = text_y;
            AddScreenOption($"SOUND LEVEL: ", content.GameConfig.TextColor, text_x, y, SettingSectionEnum.SoundLevel, SfxLevelText);
            y += text_h;
            AddScreenOption($"MUSIC LEVEL: ", content.GameConfig.TextColor, text_x, y, SettingSectionEnum.MusicLevel, MusicLevelText);
            y += text_h;
            AddScreenOption($"FULL SCREEN: ", content.GameConfig.TextColor, text_x, y, SettingSectionEnum.FullScreenMode, FullScreenModeText);
            y += text_h;
            AddScreenOption($"SAVE ASPECT RATIO: ", content.GameConfig.TextColor, text_x, y, SettingSectionEnum.AspectRatioMode, AspectRatioModeText);
            y += text_h;
            AddScreenOption($"SCANLINES FX LEVEL: ", content.GameConfig.TextColor, text_x, y, SettingSectionEnum.ScanlinesFxLevel, ScanlinesFxLevelText);
            y += text_h;
            //AddScreenOption($"CONTINUOUS FIRE: ", content.GameConfig.TextColor, text_x, y, SettingSectionEnum.ContinuousFire, ContinuousFireText);
            //y += text_h;
            AddScreenOption($"CONTROL", content.GameConfig.TextColor, text_x, y, SettingSectionEnum.Controllers);
            y += text_h;
            AddScreenOption($"EXTRAS", content.GameConfig.TextColor, text_x, y, SettingSectionEnum.Extras);
            y += text_h;
            AddScreenOption("EXIT", content.GameConfig.TextColor, text_x, y, SettingSectionEnum.Exit);
        }

        /// <summary>
        /// Добавить опцию выбора
        /// </summary>
        /// <param name="text"></param>
        /// <param name="c"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="nextScreen"></param>
        /// <param name="selectable"></param>
        private void AddScreenOption(string text, int c, int x, int y, SettingSectionEnum optionType, string displayValue = "")
        {
            var option = new SettingMenuOption()
            {
                Text = text,
                Color = c,
                X = x,
                Y = y,
                Section = optionType,
                DisplayValue = displayValue
            };

            options.Add(option);
        }

        /// <summary>
        /// Удаление всех используемых объектов, освобождение памяти
        /// </summary>
        public void Dispose()
        {
            if (deviceContext != null)
            {
                deviceContext.DeviceResize -= DeviceContext_DeviceResize;
                deviceContext = null;
            }

            if (font != null)
            {
                font.Dispose();
                font = null;
            }

            if (extrasScreen != null)
            {
                extrasScreen.Exit -= ExtrasScreen_Exit;
                extrasScreen.LoadMod -= ExtrasScreen_LoadMod;
                extrasScreen.Dispose();
                extrasScreen = null;
            }

            if(controllerScreen != null)
            {
                controllerScreen.Exit -= ControllerScreen_Exit;
                controllerScreen.Dispose();
                controllerScreen = null;
            }

            DestroyTitleFont();

            gameApplication = null;
            soundEngine = null;
            controllerHub = null;
            content = null;
            selector = null;
            graphics = null;
            options = null;
        }

        #endregion

    }
}
