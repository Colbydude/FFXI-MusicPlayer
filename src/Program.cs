using Foster.Audio;
using Foster.Framework;

namespace FFXIMusicPlayer;

class Program
{
    public static void Main()
    {
        using var player = new MusicPlayer();
        player.Run();
    }
}

public class MusicPlayer : App
{
    public readonly SpriteFont Font;
    public string? InstallPath { get; private set; }
    public AudioFileReader? Music => _music;

    private readonly Gui _gui;
    private AudioFileReader? _music;
    private readonly Visualizer _visualizer;

    public MusicPlayer() : base(new AppConfig()
    {
        ApplicationName = "FFXI Music Player",
        WindowTitle = "FFXI Music Player",
        Width = 1280,
        Height = 720
    })
    {
        _gui = new(this);
        _visualizer = new(this);

        Font = new SpriteFont(GraphicsDevice, Path.Join("Assets", "monogram.ttf"), 32);
    }

    protected override void Startup()
    {
        Audio.Startup();

        // Scan for files.
        InstallPath = Registry.GetInstallDirectory() ?? throw new Exception("Install path could not be determined.");
        AudioFileReader.ScanForMusicFiles(Path.Join(InstallPath, "sound\\win\\music\\data"));

        // Load first file.
        _music = new();
        _music.LoadBGW(AudioFileReader.MusicFiles[0]);
    }

    protected override void Shutdown()
    {
        _music?.Dispose();
        _gui.Dispose();
        Audio.Shutdown();
    }

    protected override void Update()
    {
        HandleInput();
        Audio.Update();
        _gui.Update();
    }

    protected override void Render()
    {
        Window.Clear(Color.Black);
        _visualizer.Render();
        _gui.Render();
    }

    private void HandleInput()
    {
        if (Input.Keyboard.Pressed(Keys.Escape))
            Exit();

        if (_music == null)
            return;

        if (Input.Keyboard.Pressed(Keys.Left))
            _music.Instance.Cursor = TimeSpan.FromSeconds(Math.Clamp(_music.Instance.Cursor.TotalSeconds - 10, 0, _music.InstanceLength));

        if (Input.Keyboard.Pressed(Keys.Right))
            _music.Instance.Cursor = TimeSpan.FromSeconds(Math.Clamp(_music.Instance.Cursor.TotalSeconds + 10, 0, _music.InstanceLength));

        if (Input.Keyboard.Pressed(Keys.Up))
            _music.Instance.Pitch = Math.Clamp(_music.Instance.Pitch + .25f, 0, 4);

        if (Input.Keyboard.Pressed(Keys.Down))
            _music.Instance.Pitch = Math.Clamp(_music.Instance.Pitch - .25f, 0, 4);

        if (Input.Keyboard.Pressed(Keys.Space))
            _music.Play();
    }
}
