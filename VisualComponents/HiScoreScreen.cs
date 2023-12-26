using BattleCity.Audio;
using BattleCity.Common;
using BattleCity.Enums;
using BattleCity.InputControllers;
using BattleCity.Video;
using System;
using Rectangle = System.Drawing.Rectangle;

namespace BattleCity.VisualComponents
{
    public class HiScoreScreen : IDisposable
    {
        public event Action Exit;

        private IDeviceContext deviceContext;
        private IControllerHub controllerHub;
        private ISoundEngine soundEngine;
        private IAudioReader snd;
        private GameContent content;
        private GameAchievements gameRecord;
        private IGameGraphics graphics;
        private IGameFont font;
        // оставшееся количество кадров анимации
        private int remainFrames;
        // признак окончания анимации
        private bool isComplete;
        private readonly int fontSize = 24;

        public HiScoreScreen(
            IDeviceContext deviceContext,
            IControllerHub controllerHub,
            ISoundEngine soundEngine,
            IGameGraphics graphics,
            GameContent content,
            GameAchievements gameRecord)
        {
            this.content = content;
            this.controllerHub = controllerHub;
            this.deviceContext = deviceContext;
            this.soundEngine = soundEngine;
            this.graphics = graphics;
            int titleFontSize = deviceContext.DeviceHeight / 8;
            font = graphics.CreateFont(content.GetFont(titleFontSize));
            this.gameRecord = gameRecord;
        }

        /// <summary>
        /// Сбросить в начальное состояние
        /// </summary>
        public void Reset()
        {
            snd?.Reset();

            snd = soundEngine.PlayMusic("high_score", true);

            if (snd != null)
                remainFrames = Math.Max(2, (snd.Duration / 1000) + 1) * 60;
            else 
                remainFrames = 3 * 60;
            isComplete = false;
        }

        public void Render()
        {
            if (remainFrames > 0)
                remainFrames--;

            if (!isComplete &&
                (controllerHub.IsKeyPressed(1, ButtonNames.Start, true) ||
                controllerHub.IsKeyPressed(1, ButtonNames.Attack, true) ||
                controllerHub.IsKeyPressed(2, ButtonNames.Start, true) ||
                controllerHub.IsKeyPressed(2, ButtonNames.Attack, true)))
            {
                remainFrames = 0;
                soundEngine?.Stop(snd);
            }

            var titleRect = new Rectangle(0, 0, deviceContext.DeviceWidth, deviceContext.DeviceHeight);
            var titleTextColor = (remainFrames / 5) % 2 == 0 ? Colors.White : Colors.Purple;

            font.DrawString(
                $"HISCRORES {gameRecord.HiScoreValue}",
                titleRect, DrawStringFormat.Top | DrawStringFormat.VerticalCenter | DrawStringFormat.Center | DrawStringFormat.WordBreak,
                titleTextColor);

            graphics.DrawBrickWallOverlay(
                0, titleRect.Y, 
                deviceContext.DeviceWidth, deviceContext.DeviceHeight, 
                fontSize * 4, fontSize * 2, 
                ColorConverter.ToInt32(content.CommonConfig.LogoFontColor));

            if (remainFrames == 0 && !isComplete)
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

            snd = null;
            content = null;
            graphics = null;
            gameRecord = null;
            soundEngine = null;
            controllerHub = null;
            deviceContext = null;
        }
    }
}
