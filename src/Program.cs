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
    private readonly Renderer _imRenderer;
    private MusicFileReader? _music;

    public MusicPlayer() : base(new AppConfig()
    {
        ApplicationName = "FFXI Music Player",
        WindowTitle = "FFXI Music Player",
        Width = 1280,
        Height = 720
    })
    {
        _imRenderer = new(this);
    }

    protected override void Startup()
    {
        Audio.Startup();
        MusicFileReader.ScanForMusicFiles();

        _music = new();
        _music.LoadWav(MusicFileReader.MusicFiles[0]);
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
        _imRenderer.Render();
    }

    private void HandleInput()
    {
        if (Input.Keyboard.Pressed(Keys.Space))
            _music?.Play();
    }
}
