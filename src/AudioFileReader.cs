using Foster.Audio;
using PlayOnline.Core.Audio;

namespace FFXIMusicPlayer;

public class AudioFileReader
{
    public static List<string> MusicFiles { get; private set; } = [];

    public SoundInstance Instance;
    public double InstanceLength = 0;

    private Sound? _sound;

    public static void ScanForMusicFiles(string path)
    {
        if (!Directory.Exists(path))
        {
            Console.WriteLine($"{path} does not exist.");
            return;
        }

        MusicFiles.Clear();
        MusicFiles.AddRange(Directory.GetFiles(path, "*.bgw"));
    }

    public void Dispose()
    {
        _sound?.Dispose();
    }

    public void LoadBGW(string path)
    {
        Console.WriteLine($"Loading {path}...");

        var file = new AudioFile(path);
        var stream = file.OpenStream(true);

        if (stream == null)
        {
            Console.WriteLine($"Failed to open stream for: {path}");
            return;
        }

        _sound = new Sound(stream, true);

        Instance = _sound.CreateInstance();
        Instance.Protected = true;
        Instance.Looping = file.Header.Looped;
        Instance.Volume = 0.1f;

        if (file.Header.Looped)
        {
            Console.WriteLine($"Looping point at {file.Header.LoopStart} samples - {file.Header.LoopStartTime} seconds");
            Instance.LoopBegin = TimeSpan.FromSeconds(file.Header.LoopStartTime);
        }

        InstanceLength = Instance.Length.TotalSeconds;
    }

    public void Play()
    {
        if (Instance.Playing)
            Instance.Pause();
        else
            Instance.Play();
    }
}
