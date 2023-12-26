using BattleCity.Audio;
using BattleCity.Common;
using BattleCity.Enums;
using BattleCity.InputControllers;
using BattleCity.Video;
using System;
using Rectangle = System.Drawing.Rectangle;

namespace BattleCity.VisualComponents
{
    public class GameOverScreen : IDisposable
    {
        public event Action Exit;

        private IDeviceContext deviceContext;
        private ISoundEngine soundEngine;
        private IAudioReader snd;
        private IGameGraphics graphics;
        private IControllerHub controllerHub;
        private GameContent content;
        private IGameFont font;
        private bool isComplete;
        private bool isSndPlayed;
        private int frameNumber = 0;
        private readonly int fontSize = 24;
        private readonly int maxFrames = 5 * 60;

        public GameOverScreen(
            IDeviceContext deviceContext, 
            ISoundEngine soundEngine, 
            IGameGraphics graphics,
            IControllerHub controllerHub,
            GameContent content)
        {
            this.content = content;
            this.graphics = graphics;
            this.deviceContext = deviceContext;
            this.soundEngine = soundEngine;
            this.controllerHub = controllerHub;
            int titleFontSize = deviceContext.DeviceHeight / 6;
            font = graphics.CreateFont(content.GetFont(titleFontSize));
        }

        public void Show()
        {
            frameNumber = 0;
            isComplete = false;
            isSndPlayed = false;
        }

        public void Render()
        {
            if (!isComplete &&
                (controllerHub.IsKeyPressed(1, ButtonNames.Start, true) ||
                controllerHub.IsKeyPressed(1, ButtonNames.Attack, true) ||
                controllerHub.IsKeyPressed(2, ButtonNames.Start, true) ||
                controllerHub.IsKeyPressed(2, ButtonNames.Attack, true)))
            {
                frameNumber = maxFrames;
                soundEngine?.Stop(snd);
            }

            if (frameNumber < maxFrames)
                frameNumber++;

            if (frameNumber > 20)
            {
                if (!isSndPlayed)
                {
                    isSndPlayed = true;
                    snd = soundEngine.PlayMusic("game_over", true);
                }

                var titleRect = new Rectangle(0, 0, deviceContext.DeviceWidth, deviceContext.DeviceHeight);
                var titleTextColor = Colors.White;

                font.DrawString(
                    "GAME OVER",
                    titleRect, DrawStringFormat.Top | DrawStringFormat.VerticalCenter | DrawStringFormat.Center | DrawStringFormat.WordBreak,
                    titleTextColor);

                graphics.DrawBrickWallOverlay(
                    0, titleRect.Y, 
                    deviceContext.DeviceWidth, deviceContext.DeviceHeight, 
                    fontSize * 4, fontSize * 2,
                    ColorConverter.ToInt32(content.CommonConfig.LogoFontColor));
            }

            if (frameNumber == maxFrames && !isComplete)
            {
                isComplete = true;
                soundEngine.Stop(snd);
                snd = null;
                Exit?.Invoke();
            }
        }

        public void Dispose()
        {
            if (font != null)
            {
                font.Dispose();
                font = null;
            }

            deviceContext = null;
            snd = null;
            soundEngine = null;
            graphics = null;
            controllerHub = null;
            content = null;
        }
    }
}
