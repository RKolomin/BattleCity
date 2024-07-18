using BattleCity.Common;
using BattleCity.Enums;
using BattleCity.InputControllers;
using BattleCity.Logging;
using BattleCity.Video;
using System;
using System.Collections.Generic;
using Rectangle = System.Drawing.Rectangle;

namespace BattleCity.VisualComponents
{
    public class ControllerScreen : IDisposable
    {
        public event Action<bool> Exit;

        IGameApplication gameApplication;
        IDeviceContext deviceContext;
        IControllerHub controllerHub;
        IGameGraphics graphics;
        ILogger logger;
        GameContent content;

        Rectangle titleRect;
        Rectangle fontSize;
        Rectangle optionsClipRect;
        IGameFont font;
        IGameFont titleFont;
        IGameFont hintFont;
        int selectedOptionIndex = 0;
        int listViewItemHeight;
        readonly int activeOptionTextColor = Colors.Tomato;
        readonly int titleTextColor = Colors.White;
        int screenWidth, screenHeight;
        int text_y;
        int text_x;
        int text_h;
        List<SettingMenuOption> options = new List<SettingMenuOption>();
        const string title = "CONTROL SETTINGS";

        #region Properties

        public bool IsVisible { get; set; }

        private string ContinuousFireText => gameApplication.ContinuousFire ? "YES" : "NO";

        #endregion

        public ControllerScreen(
            IGameApplication gameApplication,
            IDeviceContext deviceContext,
            IControllerHub controllerHub,
            IGameGraphics graphics,
            ILogger logger,
            GameContent content)
        {
            this.gameApplication = gameApplication;
            this.deviceContext = deviceContext;
            this.controllerHub = controllerHub;
            this.graphics = graphics;
            this.logger = logger;
            this.content = content;

            var hintFontSize = Math.Max(6, content.CommonConfig.DefaultFontSize * 0.75f);
            hintFont = graphics.CreateFont(content.GetFont(hintFontSize));
            font = graphics.CreateFont(content.GetFont(content.CommonConfig.DefaultFontSize));
            fontSize = font.MeasureString("W");

            Initialize();
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

        private void Update()
        {
            UpdateInput();
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
            deviceContext.Device.Viewport = new SlimDX.Direct3D9.Viewport(
                optionsClipRect.X, optionsClipRect.Y, optionsClipRect.Width, optionsClipRect.Height);

            int maxVisibleItems = Convert.ToInt32(optionsClipRect.Height / (double)listViewItemHeight);
            int firstVisibleIndex = selectedOptionIndex < maxVisibleItems
                ? 0
                : (selectedOptionIndex / maxVisibleItems) * maxVisibleItems;


            for (int i = firstVisibleIndex, n = 0; n < maxVisibleItems && i < options.Count; i++, n++)
            {
                var option = options[i];
                option.X = optionsClipRect.X;
                option.Y = optionsClipRect.Y + n * listViewItemHeight;
                option.Color = i == selectedOptionIndex ? activeOptionTextColor : content.GameConfig.TextColor;
                option.Draw(font);
            }

            deviceContext.Device.Viewport = currentViewport;
        }

        private void Initialize()
        {
            screenWidth = deviceContext.DeviceWidth;
            screenHeight = deviceContext.DeviceHeight;
            listViewItemHeight = Convert.ToInt32(fontSize.Height * 1.8f);

            int titleFontSize = Convert.ToInt32(screenHeight / 9d);
            titleFont = graphics.CreateFont(content.GetFont(titleFontSize));

            var top = (int)(fontSize.Height * 1.9f);
            titleRect = new Rectangle(0, top, screenWidth, Convert.ToInt32(titleFontSize * 1.2f));

            int optionsTop = titleRect.Bottom + fontSize.Height * 2;
            optionsClipRect = new Rectangle(fontSize.Height * 2, optionsTop, screenWidth, screenHeight - fontSize.Height * 2 - optionsTop);

            InitOptions();
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
                Exit?.Invoke(false);
            }
        }

        private void SetNextOptionValue(SettingMenuOption option)
        {
            switch (option.Section)
            {
                case SettingSectionEnum.ContinuousFire:
                    gameApplication.ContinuousFire = !gameApplication.ContinuousFire;
                    option.DisplayValue = ContinuousFireText;
                    return;
            }
        }

        private void SetPrevOptionValue(SettingMenuOption option)
        {
            switch (option.Section)
            {
                case SettingSectionEnum.ContinuousFire:
                    gameApplication.ContinuousFire = !gameApplication.ContinuousFire;
                    option.DisplayValue = ContinuousFireText;
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
                    Exit?.Invoke(false);
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
            AddScreenOption($"CONTINUOUS FIRE: ", content.GameConfig.TextColor, text_x, y, SettingSectionEnum.ContinuousFire, ContinuousFireText);
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

        public void Dispose()
        {
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

            deviceContext = null;
            controllerHub = null;
            content = null;
            options = null;
            graphics = null;
            logger = null;
        }
    }
}
