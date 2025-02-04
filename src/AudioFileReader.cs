using Foster.Audio;
using PlayOnline.Core.Audio;

namespace FFXIMusicPlayer;

public class AudioFileReader
{
    public static List<string> MusicFiles { get; private set; } = [];

    public byte[]? Data;
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
        Instance.Looping = true;
        Instance.Volume = 0.1f;
        InstanceLength = Instance.Length.TotalSeconds;

        // @TODO: Load decoded PCM data into Data so the visualizer can work.
        // stream.Seek(0, SeekOrigin.Begin);

        // using var memoryStream = new MemoryStream();
        // byte[] buffer = new byte[4096]; // Use a reasonable buffer size
        // int bytesRead;

        // while ((bytesRead = stream.Read(buffer, 0, buffer.Length)) > 0)
        // {
        //     memoryStream.Write(buffer, 0, bytesRead);
        // }

        // Data = memoryStream.ToArray();
        // Console.WriteLine($"Decoded PCM size: {Data.Length} bytes");
    }

    public void Play()
    {
        if (Instance.Playing)
            Instance.Pause();
        else
            Instance.Play();
    }
}
