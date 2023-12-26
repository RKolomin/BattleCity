using BattleCity.Common;
using BattleCity.Enums;
using BattleCity.InputControllers;
using BattleCity.Video;
using System;

namespace BattleCity.VisualComponents
{
    /// <summary>
    /// Экран выбора / отображения номера уровня (stage)
    /// </summary>
    public class StageSelectorScreenTransition : ScreenTransition
    {
        #region events

        public event Action Exit;
        public event Action<int> StageSelected;

        #endregion


        #region members

        IGameFont font;
        IControllerHub controllerHub;
        int selectedStage;
        readonly int totalStages;
        const int autoStageSelectDelayTime = 60;
        int time = 0;
        bool stageSelected = false;

        #endregion


        #region Constructors

        public StageSelectorScreenTransition(
            IDeviceContext deviceContext,
            IControllerHub controllerHub,
            IGameGraphics graphics,
            GameContent content,
            int selectedStage = 1)
            : base(graphics, deviceContext, content.GameConfig)
        {
            this.controllerHub = controllerHub;
            deviceContext.DeviceResize += DeviceContext_DeviceResize;
            this.selectedStage = selectedStage;
            totalStages = content.GetMaxStageNumber();
            font = graphics.CreateFont(content.GetFont(content.CommonConfig.DefaultFontSize));
        }

        #endregion
        

        #region public methods

        public override void Dispose()
        {
            if (deviceContext != null)
            {
                deviceContext.DeviceResize -= DeviceContext_DeviceResize;
            }

            if (font != null)
            {
                font.Dispose();
                font = null;
            }

            controllerHub = null;

            base.Dispose();
        }

        public void StartNextStage(int stage)
        {
            progress = 0;
            selectedStage = stage;
            State = GameScreenShowState.Closing;
            time = autoStageSelectDelayTime;
            stageSelected = true;
        }

        public override void Render()
        {
            base.Render();

            if (State == GameScreenShowState.Normal)
            {
                if (!stageSelected)
                    ProcessInput();
                string text = "STAGE " + selectedStage;
                var size = font.MeasureString(text);
                var x = (width - size.Width) / 2;
                var y = (height - size.Height) / 2;
                font.DrawString(text, x, y, gameConfig.BattleGroundColor);

                if (stageSelected)
                {
                    time--;
                    if (time <= 0)
                    {
                        State = GameScreenShowState.Opening;
                        StageSelected?.Invoke(selectedStage);
                    }
                }
            }
        }

        public override void Reset()
        {
            base.Reset();
            time = 0;
            selectedStage = 1;
            stageSelected = false;
        }

        #endregion


        #region private / protected methods

        private void DeviceContext_DeviceResize()
        {
            width = deviceContext.DeviceWidth;
            height = deviceContext.DeviceHeight;
        }

        protected override void OnClosed()
        {
            base.OnClosed();
            Close(false);
        }

        private void ProcessInput()
        {
            if (controllerHub.IsKeyPressed(1, ButtonNames.Up, true) ||
                controllerHub.IsLongPressed(1, ButtonNames.Up) ||
                controllerHub.Keyboard.IsDown(KeyboardKey.UpArrow) ||
                controllerHub.Keyboard.IsLongPress(KeyboardKey.UpArrow))
            {
                if (selectedStage >= totalStages)
                    selectedStage = 1;
                else selectedStage++;
            }
            else if (controllerHub.IsKeyPressed(1, ButtonNames.Down, true)
                || controllerHub.IsLongPressed(1, ButtonNames.Down) ||
                controllerHub.Keyboard.IsDown(KeyboardKey.DownArrow) ||
                controllerHub.Keyboard.IsLongPress(KeyboardKey.DownArrow))
            {
                if (selectedStage > 1)
                    selectedStage--;
                else selectedStage = totalStages;
            }
            else if (controllerHub.Keyboard.IsDown(KeyboardKey.Enter)
                || controllerHub.Keyboard.IsDown(KeyboardKey.NumberPadEnter)
                || controllerHub.IsKeyPressed(1, ButtonNames.Start, true)
                || controllerHub.IsKeyPressed(1, ButtonNames.Attack, true))
            {
                stageSelected = true;
            }
            else if (controllerHub.IsKeyPressed(1, ButtonNames.Cancel, true) ||
                controllerHub.Keyboard.IsDown(KeyboardKey.Escape))
            {
                Exit?.Invoke();
            }
        }


        #endregion

    }
}
