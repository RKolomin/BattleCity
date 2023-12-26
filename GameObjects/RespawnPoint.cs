namespace BattleCity.GameObjects
{
    /// <summary>
    /// Позиция появления
    /// </summary>
    public class RespawnPoint : GameFieldObject
    {
        private int elapsedFrames;
        public int ShowDelay { get; set; }
        public GameFieldObject SpawnObject { get; set; }

        public void Update()
        {
            if (ShowDelay > 0)
            {
                ShowDelay--;
                if (ShowDelay == 0)
                {
                    IsVisible = true;
                }
                return;
            }

            if (elapsedFrames > 0)
                elapsedFrames--;
            else if (SpawnObject != null)
            {
                SpawnObject.X = X;
                SpawnObject.Y = Y;
            }
        }

        public void Reset(int elapsedFrames)
        {
            ShowDelay = 0;
            this.elapsedFrames = elapsedFrames;
            SpawnObject = null;
            IsVisible = false;
        }

        public bool IsReady => elapsedFrames <= 0;
    }
}