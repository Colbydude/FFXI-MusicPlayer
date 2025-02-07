using System.Text.Json;

namespace FFXIMusicPlayer;

public class MusicPathData
{
    public static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public class Subdirectory()
    {
        public required string Name { get; set; }
        public required string Path { get; set; }
        public required List<MusicFileInfo> Files { get; set; }
    }

    public class MusicFileInfo()
    {
        public string? DisplayName { get; set; }
        public required string FileName { get; set; }

        public string? FullPath { get; set; }
    }

    public required List<Subdirectory> Subdirectories { get; set; }
}
