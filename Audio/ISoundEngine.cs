using System;

namespace BattleCity.Audio
{
    /// <summary>
    /// Звуковой движок
    /// </summary>
    public interface ISoundEngine : IDisposable
    {
        float SfxLevel { get; set; }
        float MusicLevel { get; set; }

        bool IsPlayingSound(string sndName);
        IAudioReader PlaySound(int sndId, bool reuseExistSlot = false);
        IAudioReader PlaySound(string sndName, bool reuseExistSlot = false);
        IAudioReader PlayMusic(string musicName, bool restart);
        void Stop(IAudioReader snd);
        void StopAll();
    }
}
