using Foster.Audio;
using Foster.Framework;
using FosterImGUI;

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
    public AudioFileReader? Music => _music;

    private readonly Renderer _imRenderer;
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
        _imRenderer = new(this);
        _visualizer = new(this);

        Font = new SpriteFont(GraphicsDevice, Path.Join("Assets", "monogram.ttf"), 32);
    }

    protected override void Startup()
    {
        Audio.Startup();
        AudioFileReader.ScanForMusicFiles();

        _music = new();
        // @TEMP
        _music.LoadBGW(AudioFileReader.MusicFiles[61]);
    }

    protected override void Shutdown()
    {
        _music?.Dispose();
        _imRenderer.Dispose();
        Audio.Shutdown();
    }

    protected override void Update()
    {
        HandleInput();
        Audio.Update();
        Gui.UpdateGui(this, _imRenderer);
    }

    protected override void Render()
    {
        Window.Clear(Color.Black);
        _visualizer.Render();
        _imRenderer.Render();
    }

    private void HandleInput()
    {
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
