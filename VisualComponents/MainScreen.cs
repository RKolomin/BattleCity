using BattleCity.Common;
using BattleCity.Enums;
using BattleCity.GameObjects;
using BattleCity.InputControllers;
using BattleCity.Video;
using BattleCity.VisualComponents;
using SlimDX;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Rectangle = System.Drawing.Rectangle;

namespace BattleCity
{
    /// <summary>
    /// Стартовый / титульный экран
    /// </summary>
    public class MainScreen : IDisposable
    {
        /// <summary>
        /// Событие выбора опции
        /// </summary>
        public event Action<MainMenuOption> OptionSelected;

        IDeviceContext deviceContext;
        IControllerHub controllerHub;
        IGameFont font;
        IGameFont titleFont;
        BattleUnit selector;
        IGameGraphics graphics;
        GameContent content;
        GameAchievements gameRecord;
        readonly int versionTextColor = new Color4(1, 0.3f, 0.3f, 0.3f).ToArgb();
        readonly int titleTextColor = new Color4(1, 1, 1, 1).ToArgb();
        //readonly float titleTxScaleX = 4f;
        //readonly float titleTxScaleY = 2f;
        //readonly float titleTxOffsetX = 0;
        //readonly float titleTxOffsetY = 0;
        readonly float titleTxScaleX = 6.3f;
        readonly float titleTxScaleY = 2.27f;
        readonly float titleTxOffsetX = -0.004f;
        readonly float titleTxOffsetY = 0.0018f;
        int screenWidth, screenHeight;
        int text_y;
        int selector_y;
        int selector_x;
        int text_x;
        int text_h;
        const int animationSpeed = 5;
        int end_text_y_coord;
        int frameNumber;
        readonly Rectangle fontSize;
        Rectangle titleRect;
        List<MainMenuOption> options = new List<MainMenuOption>();
        int selectedOptionIndex = 0;

        /// <summary>
        /// Признак того, что анимация завершена
        /// </summary>
        bool IsAnimationComplete => text_y == end_text_y_coord;

        /// <summary>
        /// Конструктор
        /// </summary>
        public MainScreen(
            IDeviceContext deviceContext,
            IControllerHub controllerHub,
            GameContent content,
            IGameGraphics graphics,
            GameAchievements gameRecord)
        {
            this.deviceContext = deviceContext;
            this.controllerHub = controllerHub;
            this.content = content;
            this.graphics = graphics;
            this.gameRecord = gameRecord;
            deviceContext.DeviceResize += DeviceContext_DeviceResize;
            font = graphics.CreateFont(content.GetFont(content.CommonConfig.DefaultFontSize, content.CommonConfig.DefaultFontFileName));
            fontSize = font.MeasureString("W");
            screenWidth = deviceContext.DeviceWidth;
            screenHeight = deviceContext.DeviceHeight;
            int titleFontSize = screenHeight / 8;
            titleFont = graphics.CreateFont(content.GetFont(titleFontSize, content.CommonConfig.LogoFontFileName));

            //titleFont.PreloadText(Application.ProductName.ToUpper());

            Reset();
        }

        private void DeviceContext_DeviceResize()
        {
            screenWidth = deviceContext.DeviceWidth;
            screenHeight = deviceContext.DeviceHeight;
            Reset();
        }

        /// <summary>
        /// Сбросить в начальное состояние
        /// </summary>
        public void Reset()
        {
            selectedOptionIndex = 0;
            text_h = (int)(fontSize.Height * 1.9f);
            var selectorSize = (int)(fontSize.Height * 1.6f);

            var verticalCenter = screenHeight / 2;
            selector_y = verticalCenter + screenHeight / 2;
            selector_y += (selectorSize + text_h / 2) / 2;
            //selector_x = Convert.ToInt32(screenWidth / 4.25);
            text_x = screenWidth / 3;
            selector_x = text_x - Convert.ToInt32(text_h * 1.5);
            end_text_y_coord = verticalCenter + text_h;
            text_y = end_text_y_coord + screenHeight / 2;

            selector = new UserBattleUnit(null)
            {
                HexColor = "#FFffca57",
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

            int x = 4 * fontSize.Width;
            int width = screenWidth - 2 * x;
            titleRect = new Rectangle(
                x,
                verticalCenter + text_h + 2 * fontSize.Height,
                width,
                screenHeight);
        }

        /// <summary>
        /// Создать опции
        /// </summary>
        private void InitOptions()
        {
            options.Clear();

            var y = text_y;
            AddScreenOption("1 PLAYER", content.GameConfig.TextColor, text_x, y, GameScreenEnum.StartSinglePlayer);
            y += text_h;
            AddScreenOption("2 PLAYERS", content.GameConfig.TextColor, text_x, y, GameScreenEnum.StartMultiplayer);
            y += text_h;
            AddScreenOption("SETTINGS", content.GameConfig.TextColor, text_x, y, GameScreenEnum.Settings);
            y += text_h;
            AddScreenOption("LEVEL EDITOR", content.GameConfig.TextColor, text_x, y, GameScreenEnum.LevelEditor);
            y += text_h;
            AddScreenOption("EXIT", content.GameConfig.TextColor, text_x, y, GameScreenEnum.ExitGame);

            y += Convert.ToInt32(1.8f * text_h);
            AddScreenOption($"V.{Application.ProductVersion.Replace(".", "")}", versionTextColor, text_x, y, GameScreenEnum.None, false);
        }

        /// <summary>
        /// Добавить опцию выбора
        /// </summary>
        /// <param name="text">Отображаемый текст</param>
        /// <param name="color">Цвет текста</param>
        /// <param name="x">X-координата текста</param>
        /// <param name="y">Y-координата текста</param>
        /// <param name="nextScreen">Тип экрана при активации опции</param>
        /// <param name="selectable">Опция доступна для выбора при навигации</param>
        private void AddScreenOption(string text, int color, int x, int y, GameScreenEnum nextScreen, bool selectable = true)
        {
            var option = new MainMenuOption()
            {
                Text = text,
                Color = color,
                X = x,
                Y = y,
                NextScreen = nextScreen,
                Selectable = selectable
            };

            options.Add(option);
        }

        /// <summary>
        /// Обработать нажатия кнопок
        /// </summary>
        private void ProcessInput()
        {
            if (controllerHub.Keyboard.IsDown(KeyboardKey.NumberPadEnter) ||
                controllerHub.Keyboard.IsDown(KeyboardKey.Escape) ||
                controllerHub.IsKeyPressed(1, ButtonNames.Start, true) ||
                controllerHub.IsKeyPressed(1, ButtonNames.Attack, true))
            {
                if (!IsAnimationComplete)
                {
                    SkipAnimation();
                    return;
                }
                else
                    ActivateOption(options[selectedOptionIndex]);
            }

            if (!IsAnimationComplete)
                return;

            if (controllerHub.IsKeyPressed(1, ButtonNames.Up, true) ||
                controllerHub.IsLongPressed(1, ButtonNames.Up) ||
                controllerHub.Keyboard.IsDown(KeyboardKey.UpArrow) ||
                controllerHub.Keyboard.IsLongPress(KeyboardKey.UpArrow))
            {
                if (selectedOptionIndex == 0)
                    selectedOptionIndex = options.FindLastIndex(x => x.Selectable);
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

            else if (controllerHub.IsKeyPressed(1, ButtonNames.Cancel, true))
            {
                Application.Exit();
            }
        }

        /// <summary>
        /// Активация опции
        /// </summary>
        /// <param name="option"></param>
        private void ActivateOption(MainMenuOption option)
        {
            OptionSelected?.Invoke(option);
        }

        /// <summary>
        /// Обновить
        /// </summary>
        private void Update()
        {
            int delta_y = Math.Max(0, Math.Min(text_y - end_text_y_coord, animationSpeed));
            if (delta_y == 0)
            {
                return;
            }

            text_y -= delta_y;
            selector.Y -= delta_y;
            selector_y -= delta_y;
            titleRect.Y -= delta_y;

            foreach (var option in options)
            {
                option.Y -= delta_y;
            }
        }

        /// <summary>
        /// Пропустить анимацию
        /// </summary>
        private void SkipAnimation()
        {
            if (end_text_y_coord == text_y)
                return;
            int delta_y = end_text_y_coord - text_y;
            titleRect.Y += delta_y;
            selector.Y += delta_y;
            selector_y += delta_y;
            text_y += delta_y;
            foreach (var option in options)
            {
                option.Y += delta_y;
            }
        }



        /// <summary>
        /// Отрисовка
        /// </summary>
        /// <param name="isForeground">Признак отображения на переднем плане, при котором будет выполняться обработка нажатий кнопок</param>
        public void Render(bool isForeground)
        {
            Update();
            if (isForeground)
                ProcessInput();

            string titleText = content.GameConfig.Name; // Application.ProductName.ToUpper()

            titleFont.DrawString(titleText,
                titleRect.X, titleRect.Y, titleRect.Width, titleRect.Height,
                DrawStringFormat.Top | DrawStringFormat.Center | DrawStringFormat.WordBreak,
                titleTextColor);

            graphics.DrawBrickWallOverlay(
                0, titleRect.Y, screenWidth, screenHeight,
                fontSize.Height * titleTxScaleX, fontSize.Height * titleTxScaleY,
                ColorConverter.ToInt32(content.CommonConfig.LogoFontColor),
                titleTxOffsetX, titleTxOffsetY);

            DrawHiScore();

            selector.UpdateAnimation(frameNumber);
            if (end_text_y_coord == text_y)
            {
                graphics.BeginDrawGameObjects();
                graphics.DrawGameObject(0, 0, selector, 0, 1);
                graphics.EndDrawGameObjects();
            }

            foreach (var option in options)
                option.Draw(font);

            frameNumber++;
        }

        /// <summary>
        /// Отрисовать рекордные значения очков
        /// </summary>
        private void DrawHiScore()
        {
            int x = 4 * fontSize.Width;
            int width = screenWidth - 4 * x;
            int y = titleRect.Y - fontSize.Height * 2;
            int screenThirdWidth = width / 3;
            const int maxScoreTextLength = 8;
            const DrawStringFormat drawTextFormat = DrawStringFormat.Top | DrawStringFormat.Left | DrawStringFormat.NoClip;

            string player1HiScore = gameRecord.Player1Record.HiScoreValue.ToString("00");
            if (player1HiScore.Length < maxScoreTextLength)
                player1HiScore = new string(' ', maxScoreTextLength - player1HiScore.Length) + player1HiScore;

            string player2HiScore = gameRecord.Player2Record.HiScoreValue.ToString("00");
            if (player2HiScore.Length < maxScoreTextLength)
                player2HiScore = new string(' ', maxScoreTextLength - player2HiScore.Length) + player2HiScore;

            string hiScoreText = (gameRecord.HiScoreValue ?? 0).ToString("00");
            if (hiScoreText.Length < maxScoreTextLength)
                hiScoreText = new string(' ', maxScoreTextLength - hiScoreText.Length) + hiScoreText;

            font.DrawString(
                $"I- {player1HiScore}",
                x, y, screenThirdWidth, screenHeight,
                drawTextFormat,
                titleTextColor);

            font.DrawString($"HI- {hiScoreText}",
                2 * x + screenThirdWidth, y, screenThirdWidth, screenHeight,
                drawTextFormat, titleTextColor);

            font.DrawString($"II- {player2HiScore}",
                3 * x + 2 * screenThirdWidth, y, screenThirdWidth, screenHeight,
                drawTextFormat,
                titleTextColor);
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

            if (titleFont != null)
            {
                titleFont.Dispose();
                titleFont = null;
            }

            controllerHub = null;
            content = null;
            selector = null;
            graphics = null;
            options = null;
            gameRecord = null;
        }
    }
}