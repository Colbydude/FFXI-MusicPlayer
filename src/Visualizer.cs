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
            // Playback bar.
            _batcher.Rect(new Rect(playBarPos, playBarSize), Color.Gray * .75f);
            _batcher.Rect(new Rect(playBarPos, (float)(playBarSize.X * _player.Music.Instance.Cursor.TotalSeconds / _player.Music.InstanceLength), playBarSize.Y), Color.Red * .75f);

            // Loop point indicator.
            if (_player.Music.Instance.LoopBegin.TotalSeconds > 0)
            {
                var loopPositionX = (float)(_player.Music.Instance.LoopBegin.TotalSeconds / _player.Music.InstanceLength) * playBarSize.X;
                _batcher.Rect(new Rect(new Vector2(playBarPos.X + loopPositionX, playBarPos.Y), new Vector2(2, playBarSize.Y)), Color.Yellow * .75f);
            }

            // Playback text.
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
