namespace SinbadStudios.SharedSystems.Runtime
{
    public class PlaySoundEffectsEvent
    {
        public string Key { get; set; }
        public float Volume { get; set; } = 1.0f;
    }

    public class PlayMusicEvent
    {
        public string Key { get; set; }
        public bool Loop { get; set; }
        public int Channel { get; set; } = 0;
        public float Volume { get; set; } = 1.0f;
    }

    public class StopMusicEvent
    {
        public int Channel { get; set; } = -1;
    }
}