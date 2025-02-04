using Foster.Framework;
using System.Numerics;

namespace FFXIMusicPlayer;

public class Visualizer(MusicPlayer player)
{
    private readonly Batcher _batcher = new(player.GraphicsDevice);
    private readonly MusicPlayer _player = player;

    public void Render()
    {
        var playBarPos = new Vector2(0, _player.Window.HeightInPixels - 30);
        var playBarSize = new Vector2(_player.Window.WidthInPixels, 30);

        if (_player.Music != null)
        {
            _batcher.Rect(new Rect(playBarPos, playBarSize), Color.Gray * .75f);
            _batcher.Rect(new Rect(playBarPos, (float)(playBarSize.X * _player.Music.Instance.Cursor.TotalSeconds / _player.Music.InstanceLength), playBarSize.Y), Color.Red * .75f);
            _batcher.Text(
                _player.Font,
                $"{_player.Music.Instance.Cursor:mm':'ss}/{TimeSpan.FromSeconds(_player.Music.InstanceLength):mm':'ss}",
                new Vector2(playBarSize.X / 2, playBarPos.Y - (playBarSize.Y / 2)),
                new Vector2(0.5f, 0.5f),
                Color.White
            );
            _batcher.Text(
                _player.Font,
                $"x{_player.Music.Instance.Pitch}",
                new Vector2(playBarSize.X / 2, playBarPos.Y - (playBarSize.Y + 16)),
                new Vector2(0.5f, 0f),
                Color.White
            );
        }

        _batcher.Render(_player.Window);
        _batcher.Clear();
    }
}
