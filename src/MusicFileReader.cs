using Foster.Audio;

namespace FFXIMusicPlayer;

public class MusicFileReader
{
    public static List<string> MusicFiles { get; private set; } = [];

    // @TEMP
    private const string PATH = "C:\\Users\\Colby\\Desktop\\Ximc004\\POLViewer";

    private int _channels;
    private byte[]? _data;
    private SoundInstance _instance;
    private double _instanceLength;
    private Sound? _sound;

    public static void ScanForMusicFiles()
    {
        if (!Directory.Exists(PATH))
        {
            Console.WriteLine($"{PATH} does not exist.");
            return;
        }

        MusicFiles.Clear();
        MusicFiles.AddRange(Directory.GetFiles(PATH, "*.wav"));
    }

    public void Dispose()
    {
        _sound?.Dispose();
    }

    public void LoadWav(string path)
    {
        var encodedData = File.ReadAllBytes(path);
        var format = AudioFormat.F32;
        _channels = 0; // determine automatically
        var sampleRate = Audio.SampleRate;

        if (Sound.TryDecode(encodedData, ref format, ref _channels, ref sampleRate, out var frameCount, out _data))
        {
            _sound = new Sound(_data!, format, _channels, sampleRate, frameCount);
            _instance = _sound.CreateInstance();
            _instance.Protected = true;
            _instance.Looping = true;
            _instance.Volume = 0.1f;
            _instanceLength = _instance.Length.TotalSeconds;
        }
        else
            throw new Exception("Could not parse encoded data.");
    }

    public void Play()
    {
        if (_instance.Playing)
            _instance.Pause();
        else
            _instance.Play();
    }
}
