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
    public AudioFileReader? Music;

    private readonly Gui _gui;
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

        Font = new SpriteFont(GraphicsDevice, Path.Join("assets/fonts", "monogram.ttf"), 32);
    }

    protected override void Startup()
    {
        Audio.Startup();

        // Scan for files.
        AudioFileReader.GetInstallPaths();
        AudioFileReader.ScanForMusicFiles();
    }

    protected override void Shutdown()
    {
        Music?.Dispose();
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

        if (Music == null)
            return;

        if (Input.Keyboard.Pressed(Keys.Left))
            Music.Instance.Cursor = TimeSpan.FromSeconds(Math.Clamp(Music.Instance.Cursor.TotalSeconds - 10, 0, Music.InstanceLength));

        if (Input.Keyboard.Pressed(Keys.Right))
            Music.Instance.Cursor = TimeSpan.FromSeconds(Math.Clamp(Music.Instance.Cursor.TotalSeconds + 10, 0, Music.InstanceLength));

        if (Input.Keyboard.Pressed(Keys.Up))
            Music.Instance.Pitch = Math.Clamp(Music.Instance.Pitch + .25f, 0, 4);

        if (Input.Keyboard.Pressed(Keys.Down))
            Music.Instance.Pitch = Math.Clamp(Music.Instance.Pitch - .25f, 0, 4);

        if (Input.Keyboard.Pressed(Keys.Space))
            Music.Play();
    }
}
