using Foster.Audio;
using Foster.Framework;
using System.Numerics;
using System.Runtime.InteropServices;

namespace FFXIMusicPlayer;

public class Visualizer(MusicPlayer player)
{
    private readonly Batcher _batcher = new(player.GraphicsDevice);
    private readonly MusicPlayer _player = player;

    private const int N = 4096; // Must be a power of 2, higher = more CPU time and higher fidelity results
    private const float LineThickness = 2;
    private const float LineScale = .5f;
    private static readonly int[] FreqBin = [20, 60, 250, 500, 1000];

    // Color gradient for visualizer
    private static readonly Color[] Colors = [.. Enumerable.Range(0, 256 * 3)
        .Select(m => new Color(
            (byte)Math.Clamp(m, 0, 255),
            (byte)Math.Clamp(m - 256, 0, 255),
            (byte)Math.Clamp(m - 512, 0, 255),
            255))];

    private Complex[] _fft = new Complex[N];
    private List<float> _peakMax = new();

    public void Render()
    {
        RenderCircle(_player.Window.WidthInPixels / 2, _player.Window.HeightInPixels / 2, 50);

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

    private void RenderCircle(float centerX, float centerY, float radius)
    {
        if (_player.Music == null || !_player.Music.Instance.Active || _player.Music.Data == null)
            return;

        CalcPeakMax();

        // Take the average and add it to the radius of the circle.
        float aprox = 0;

        for (int i = 0; i < _peakMax.Count; i++)
        {
            aprox += _peakMax[i];

            // Scale
            _peakMax[i] *= LineScale;
        }

        aprox /= _peakMax.Count;
        aprox += 0.0001f; // Avoid dividing by 0 later on

        radius += aprox;

        var angle = MathF.PI / _peakMax.Count;

        for (int j = 0; j < 2; j++)
        {
            for (int i = 0; i < _peakMax.Count; i++)
            {
                // Angle in circle
                var a = i * angle + angle * .5f;
                if (j == 1)
                {
                    a *= j * -1; // Mirror
                }
                a += MathF.PI / 2; // Rotate by 90 degrees

                // Circle points
                var x = centerX + radius * MathF.Cos(a);
                var y = centerY + radius * MathF.Sin(a);

                // Color
                var c = Colors[(int)Math.Clamp((_peakMax[i] / aprox + .2f) * Colors.Length, 0, Colors.Length - 1)];

                // Translate to the circle center then translate to each point and rotate it
                _batcher.PushMatrix(Matrix3x2.CreateRotation(a) * Matrix3x2.CreateTranslation(x, y));
                _batcher.Line(new(0, 0), new(_peakMax[i], 0), LineThickness, c);
                _batcher.PopMatrix();
            }
        }
    }

    /// <summary>
    /// Heavily based on https://github.com/miha53cevic/AudioVisualizerJS/tree/master
    /// </summary>
    private void CalcPeakMax()
    {
        var cursor = (int)_player.Music!.Instance.CursorPcmFrames;

        var dataF = MemoryMarshal.Cast<byte, float>(_player.Music.Data);

        // Fill in input
        for (int i = 0; i < N; i++)
        {
            var index = (i + cursor - N / 2) * _player.Music.Instance.Channels;

            // Hamming window the input for smoother input values
            var sample = index < dataF!.Length && index >= 0 ?
                dataF[index] * (0.54f - (0.46f * MathF.Cos(2.0f * MathF.PI * (i / ((N - 1) * 1.0f)))))
                : 0;

            // Windowed sample or signal
            _fft[i] = sample;
        }

        // Calculate fft
        CalcFFT(_fft, false);

        _peakMax.Clear();

        // Calculate the magnitudes
        // Only half of the data is useful
        for (int i = 0; i < (N / 2) + 1; i++)
        {
            var freq = 1f * i * Audio.SampleRate / N;
            var magnitude = _fft[i].Magnitude;

            // Extract the peaks from defined frequency ranges
            for (int j = 0; j < FreqBin.Length - 1; j++)
            {
                if ((freq > FreqBin[j]) && (freq <= FreqBin[j + 1]))
                {
                    _peakMax.Add((float)magnitude);
                }
            }
        }
    }

    /// <summary>
    /// FFT <br/>
    /// Modified version of this implementation: https://cp-algorithms.com/algebra/fft.html
    /// </summary>
    /// <param name="a"></param>
    /// <param name="invert"></param>
    private static void CalcFFT(Complex[] a, bool invert)
    {
        int n = a.Length;

        for (int i = 1, j = 0; i < n; i++)
        {
            int bit = n >> 1;
            for (; (j & bit) > 0; bit >>= 1)
                j ^= bit;
            j ^= bit;

            if (i < j)
                (a[i], a[j]) = (a[j], a[i]);
        }

        for (int len = 2; len <= n; len <<= 1)
        {
            double ang = 2 * Math.PI / len * (invert ? -1 : 1);
            var wlen = new Complex(Math.Cos(ang), Math.Sin(ang));

            for (int i = 0; i < n; i += len)
            {
                var w = new Complex(1, 0);

                for (int j = 0; j < len / 2; j++)
                {
                    Complex u = a[i + j], v = a[i + j + len / 2] * w;
                    a[i + j] = u + v;
                    a[i + j + len / 2] = u - v;
                    w *= wlen;
                }
            }
        }

        if (invert)
        {
            for (int i = 0; i < a.Length; i++)
                a[i] /= n;
        }
    }
}
