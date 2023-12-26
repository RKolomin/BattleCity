using BattleCity.Common;
using BattleCity.Enums;
using BattleCity.Video;
using System;

namespace BattleCity.VisualComponents
{
    /// <summary>
    /// Анимация экранных переходов
    /// </summary>
    public class ScreenTransition : IDisposable
    {
        #region events

        public event Action Closed;
        public event Action Opened;

        #endregion

        #region Properties

        public bool AutoOpen { get; set; } = true;
        public GameScreenEnum CurrentScreen { get; set; }
        public GameScreenEnum NextScreen { get; set; }
        public GameScreenShowState State { get; protected set; } = GameScreenShowState.Closing;

        #endregion

        #region members

        protected IGameGraphics graphics;
        protected GameConfig gameConfig;
        protected IDeviceContext deviceContext;
        private readonly int speed = 15;
        protected int width;
        protected int height;
        protected int progress = 0;

        #endregion

        #region Constructors

        public ScreenTransition(IGameGraphics graphics, IDeviceContext deviceContext, GameConfig gameConfig)
        {
            this.graphics = graphics;
            this.gameConfig = gameConfig;
            this.deviceContext = deviceContext;
            width = deviceContext.DeviceWidth;
            height = deviceContext.DeviceHeight;
        }

        #endregion

        #region public methods

        public virtual void Dispose()
        {
            deviceContext = null;
            gameConfig = null;
        }

        public virtual void Reset()
        {
            progress = 0;
            State = GameScreenShowState.Closing;
        }

        public void Open(bool faded)
        {
            if (faded)
            {
                progress = height / 2;
                if (height % 2 != 0)
                    progress++;
                State = GameScreenShowState.Opening;
            }
            else
            {
                progress = 0;
                State = GameScreenShowState.Normal;
            }
        }

        public void Close(bool faded)
        {
            if (faded)
            {
                progress = 0;
                State = GameScreenShowState.Closing;
            }
            else
            {
                State = GameScreenShowState.Normal;
                progress = height / 2;
                if (height % 2 != 0)
                    progress++;
            }
        }

        public virtual void Render()
        {
            Update();
            graphics.FillRect(0, 0, width, progress, gameConfig.TransitionScreenBackgroundColor);
            graphics.FillRect(0, Math.Max(height / 2, height - progress), width, progress, gameConfig.TransitionScreenBackgroundColor);
        }

        #endregion

        #region private methods

        protected virtual void OnClosed()
        {
            Closed?.Invoke();
        }

        private void Update()
        {
            switch (State)
            {
                case GameScreenShowState.Closing:
                    {
                        if (progress >= height / 2)
                        {
                            State = GameScreenShowState.Opening;
                        }
                        progress += speed;
                        if (progress >= height / 2)
                        {
                            progress = Math.Min(height / 2, progress);
                            if (height % 2 != 0)
                                progress++;
                            OnClosed();
                        }
                    }
                    return;
                case GameScreenShowState.Opening:
                    {
                        if (progress == 0)
                        {
                            State = GameScreenShowState.Normal;
                        }
                        progress -= speed;
                        if (progress <= 0)
                        {
                            progress = Math.Min(0, progress);
                            Opened?.Invoke();
                        }
                    }
                    return;
            }
        }

        #endregion

    }
}