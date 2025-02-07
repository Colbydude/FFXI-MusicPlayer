using Foster.Audio;
using PlayOnline.Core.Audio;
using System.Text.Json;

namespace FFXIMusicPlayer;

public class AudioFileReader : IDisposable
{
    public static InstallPaths InstallPaths { get; private set; } = new();
    public static Dictionary<string, MusicPathData>? PathData;

    public SoundInstance Instance;
    public double InstanceLength = 0;

    private bool _disposed;
    private Sound? _sound;

    public static void GetInstallPaths()
    {
        InstallPaths = Registry.GetInstallDirectories()
            ?? throw new InvalidOperationException("Install paths could not be determined.");
    }

    public static void ScanForMusicFiles()
    {
        var json = File.ReadAllText(Path.Join("assets/json", "music.json"));
        PathData = JsonSerializer.Deserialize<Dictionary<string, MusicPathData>>(json, MusicPathData.SerializerOptions) ?? throw new Exception("Could not read music.json");

        foreach (var entry in PathData)
        {
            string basePath = MapTokenToPath(entry.Key);

            foreach (var subdir in entry.Value.Subdirectories)
            {
                string subdirPath = Path.Join(basePath, subdir.Path);

                if (!Directory.Exists(subdirPath))
                    continue;

                var existingFiles = new Dictionary<string, MusicPathData.MusicFileInfo>(
                    subdir.Files.Count,
                    StringComparer.OrdinalIgnoreCase
                );

                foreach (var file in subdir.Files)
                    existingFiles[file.FileName] = file;

                foreach (var fullPath in Directory.EnumerateFiles(subdirPath, "*.bgw"))
                {
                    string fileName = Path.GetFileName(fullPath);

                    if (existingFiles.TryGetValue(fileName, out var jsonFile))
                        jsonFile.FullPath = fullPath;
                    else
                        subdir.Files.Add(new() { DisplayName = fileName, FileName = fileName, FullPath = fullPath });
                }

                // Sorting only if new files were added
                if (subdir.Files.Count != existingFiles.Count)
                    subdir.Files.Sort((a, b) => string.Compare(a.FileName, b.FileName, StringComparison.Ordinal));
            }
        }
    }

    private static string MapTokenToPath(string token)
    => token switch
    {
        InstallPathTokens.FinalFantasyXI => InstallPaths.FinalFantasyXI,
        InstallPathTokens.PlayOnlineViewer => InstallPaths.PlayOnlineViewer,
        InstallPathTokens.TetraMaster => InstallPaths.TetraMaster,
        _ => string.Empty,
    };

    ~AudioFileReader()
    {
        Dispose();
    }

    public void Dispose()
    {
        if (_disposed) return;

        _sound?.Dispose();
        _sound = null;
        _disposed = true;

        GC.SuppressFinalize(this);
    }

    public void LoadBGW(string path)
    {
        Console.WriteLine($"Loading {path}...");

        try
        {
            var file = new AudioFile(path);
            using var stream = file.OpenStream(true) ?? throw new IOException($"Failed to open audio stream: {path}");

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
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading BGW file '{path}': {ex.Message}");
        }
    }

    public void Play()
    {
        if (Instance.Playing)
            Instance.Pause();
        else
            Instance.Play();
    }
}
