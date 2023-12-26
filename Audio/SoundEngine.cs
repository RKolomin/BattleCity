using BattleCity.Enums;
using BattleCity.Logging;
using BattleCity.Repositories;

namespace BattleCity.Audio
{
    public class SoundEngine : ISoundEngine
    {
        private SoundRepository soundRepository;
        private IAudioPlayback audioPlayback;
        private ISoundMixer soundMixer;
        //private List<IAudioReader> activeAudioSlots;

        public float SfxLevel
        {
            get => soundMixer.GetSoundLevel(SoundType.Sound);
            set => soundMixer.SetSoundLevel(SoundType.Sound, value);
        }
        public float MusicLevel
        {
            get => soundMixer.GetSoundLevel(SoundType.Music);
            set => soundMixer.SetSoundLevel(SoundType.Music, value);
        }

        public SoundEngine(ILogger logger, SoundRepository soundRepository, int maxAudioSlots, int latency)
        {
            //activeAudioSlots = new List<IAudioReader>(maxAudioSlots);
            this.soundRepository = soundRepository;
            soundMixer = new SoundMixer(logger, maxAudioSlots);
            audioPlayback = new XAPlayback(soundMixer, latency);
            audioPlayback.Start();
        }

        public bool IsPlayingSound(string sndName)
        {
            return soundMixer.Contains(sndName, SoundType.Sound);
        }

        public IAudioReader PlaySound(int sndId, bool reuseExistSlot = false)
        {
            var reader = soundRepository.GetOrCreateAudioReader(sndId);
            PlaySound(reader, reuseExistSlot);
            return reader;
        }

        public IAudioReader PlaySound(string sndName, bool reuseExistSlot = false)
        {
            var reader = soundRepository.GetOrCreateAudioReader(sndName, SoundType.Sound);
            PlaySound(reader, reuseExistSlot);
            return reader;
        }

        public IAudioReader PlayMusic(string musicName, bool restart)
        {
            var reader = soundRepository.GetOrCreateAudioReader(musicName, SoundType.Music);
            PlaySound(reader, true, restart);
            return reader;
        }

        private void PlaySound(IAudioReader snd, bool reuseExistSlot = false, bool restartExistSlot = false)
        {
            if (snd == null)
                return;

            soundMixer.Add(snd, reuseExistSlot, restartExistSlot);
        }

        public void Stop(IAudioReader snd)
        {
            if (snd != null)
                soundMixer.Remove(snd);
        }

        public void StopAll()
        {
            soundMixer.Clear(null);
        }

        public void Dispose()
        {
            soundRepository = null;

            if (audioPlayback != null)
            {
                audioPlayback.Dispose();
                audioPlayback = null;
            }

            if (soundMixer != null)
            {
                soundMixer.Clear(null);
                soundMixer = null;
            }
        }
    }
}
