using System;

namespace BattleCity.Audio
{
    public interface IAudioPlayback : IDisposable
    {
        float MasterLevel { get; set; }
        float FrequencyRatio { get; set; }
        void Start();
    }
}
