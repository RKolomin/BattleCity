using System;

namespace BattleCity.VisualComponents
{
    /// <summary>
    /// Заглушка. Используется для отсрочки появления экрана результатов прохождения Stage
    /// </summary>
    public class StageCompleteOverlay : IDisposable
    {
        public bool IsVisible { get; private set; }
        public int ElapsedFrames { get; private set; }

        public void Show(int durationInFrames)
        {
            ElapsedFrames = durationInFrames;
            IsVisible = true;
        }

        public void Hide()
        {
            IsVisible = false;
        }

        public void Update()
        {
            if (ElapsedFrames > 0)
                ElapsedFrames--;
        }

        public void Dispose()
        {
            
        }
    }
}
