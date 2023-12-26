namespace BattleCity.Audio
{
    public interface IAudioSource : IPcmOutputStream
    {
        long Length { get; }
        long Position { get; set; }
        bool IsEof { get; }
    }
}
