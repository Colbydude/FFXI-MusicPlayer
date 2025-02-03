// Copyright © 2004-2014 Tim Van Holder, Windower Team
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS"
// BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and limitations under the License.

using System.Text;

namespace PlayOnline.Core.Audio;

public class AudioFileStream : Stream
{
    public static bool IsFormatSupported(SampleFormat SF)
    {
        return SF switch
        {
            SampleFormat.ADPCM or SampleFormat.PCM => true,
            _ => false,
        };
    }

    public override long Length
    {
        get
        {
            long bytes = _header.SampleBlocks * _header.BlockSize * _header.Channels * 2;

            if (_header.SampleFormat == SampleFormat.PCM)
                bytes = _header.Size - 0x30;

            if (_addWAVHeader)
                bytes += 0x2C;

            return bytes;
        }
    }

    public override long Position
    {
        get
        {
            if (_addWAVHeader && _position <= 0x2C)
                return _position;

            long rawPos = _position;

            if (_addWAVHeader)
                rawPos -= 0x2C;

            long cookedPos = rawPos;

            if (_header.SampleFormat == SampleFormat.ADPCM)
            {
                double BlockPos = (double)rawPos / (1 + _header.BlockSize / 2);
                cookedPos = (long)Math.Floor(BlockPos * _header.BlockSize * 2);
            }

            if (_addWAVHeader)
                cookedPos += 0x2C;

            return cookedPos;
        }
        set { throw new NotSupportedException(I18N.GetText("SeekNotAllowed")); }
    }

    public override bool CanRead => true;
    public override bool CanSeek => false; // Disallow this for now
    public override bool CanWrite => false;

    private readonly bool _addWAVHeader;
    private readonly ADPCMCodec? _codec;
    private readonly FileStream _file;
    private readonly AudioFileHeader _header;

    private byte[]? _buffer = [];
    private readonly int _bufferBlocks = 32;
    private int _bufferPos;
    private int _bufferSize;
    private long _position;
    private readonly byte[]? _WAVHeader;

    internal AudioFileStream(string path, AudioFileHeader header)
        : this(path, header, false) { }

    internal AudioFileStream(string Path, AudioFileHeader Header, bool addWAVHeader)
    {
        _header = Header;
        _addWAVHeader = addWAVHeader;
        _WAVHeader = null;
        _codec = null;
        _position = 0;

        if (_header.SampleFormat == SampleFormat.ADPCM)
            _codec = new ADPCMCodec(_header.Channels, _header.BlockSize);

        _file = new FileStream(Path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        _file.Seek(0x30, SeekOrigin.Begin);

        if (addWAVHeader)
        {
            // Prepare WAV file header
            _WAVHeader = new byte[0x2C];
            BinaryWriter BW = new(new MemoryStream(_WAVHeader, true), Encoding.ASCII);

            // File Header
            BW.Write("RIFF".ToCharArray());
            BW.Write((int)Length);

            // Wave Format Header
            BW.Write("WAVEfmt ".ToCharArray());
            BW.Write(0x10);

            // Wave Format Data
            BW.Write((short)1); // PCM
            BW.Write((short)_header.Channels);
            BW.Write(_header.SampleRate);
            BW.Write(2 * _header.Channels * _header.SampleRate); // bytes per second
            BW.Write((short)(2 * _header.Channels)); // bytes per sample
            BW.Write((short)16); // bits

            // Wave Data Header
            BW.Write("data".ToCharArray());
            BW.Write((int)(Length - 0x2C));
            BW.Close();
        }

        _buffer = null;
        _bufferPos = 0;
        _bufferSize = 0;
    }

    /// <summary>
    ///
    /// </summary>
    public override void Close()
    {
        _file.Close();
        base.Close();
    }

    /// <summary>
    ///
    /// </summary>
    public override void Flush() => _file.Flush();

    /// <summary>
    ///
    /// </summary>
    /// <param name="buffer"></param>
    /// <param name="offset"></param>
    /// <param name="count"></param>
    public override int Read(byte[] buffer, int offset, int count)
    {
        int bytesRead = 0;

        if (_addWAVHeader && _position < 0x2C)
        {
            int headerBytesToRead = (int)Math.Min(0x2C, count - _position);
            Array.Copy(_WAVHeader!, _position, buffer, offset, headerBytesToRead);
            _position += headerBytesToRead;
            bytesRead += headerBytesToRead;
            count -= headerBytesToRead;
        }

        if (_header.SampleFormat == SampleFormat.PCM)
        {
            int rawBytesRead = _file.Read(buffer, offset + bytesRead, count);
            bytesRead += rawBytesRead;
            _position += rawBytesRead;
        }
        else
        {
        ReadSomeMore:
            int bytesAvailable = _bufferSize - _bufferPos;

            if (bytesAvailable >= count)
            {
                Array.Copy(_buffer!, _bufferPos, buffer, offset + bytesRead, count);
                _bufferPos += count;
                bytesRead += count;
            }
            else
            {
                if (bytesAvailable > 0)
                {
                    Array.Copy(_buffer!, _bufferPos, buffer, offset + bytesRead, bytesAvailable);
                    count -= bytesAvailable;
                    bytesRead += bytesAvailable;
                }
                if (_buffer == null || _bufferSize == _buffer.Length)
                {
                    // There's more data to be read from the file.
                    FillBuffer();
                    goto ReadSomeMore;
                }
            }
        }

        return bytesRead;
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="offset"></param>
    /// <param name="origin"></param>
    public override long Seek(long offset, SeekOrigin origin)
    {
        if (origin == SeekOrigin.Current)
            offset += Position;
        else if (origin == SeekOrigin.Current)
            offset += Length;

        Position = offset;

        return Position;
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="value"></param>
    public override void SetLength(long value) { throw new NotSupportedException(I18N.GetText("SetLengthNotAllowed")); }

    /// <summary>
    ///
    /// </summary>
    /// <param name="buffer"></param>
    /// <param name="offset"></param>
    /// <param name="count"></param>
    public override void Write(byte[] buffer, int offset, int count)
    {
        throw new NotSupportedException(I18N.GetText("WriteNotAllowed"));
    }

    /// <summary>
    ///
    /// </summary>
    private void FillBuffer()
    {
        _buffer ??= new byte[_header.BlockSize * _header.Channels * 2 * _bufferBlocks];
        _bufferPos = 0;
        _bufferSize = 0;
        byte[] ADPCMBlock = new byte[(1 + _header.BlockSize / 2) * _header.Channels];
        byte[] PCMBlock = new byte[_header.BlockSize * _header.Channels * 2];

        for (int i = 0; i < _bufferBlocks; ++i)
        {
            int bytesRead = _file.Read(ADPCMBlock, 0, ADPCMBlock.Length);
            _position += bytesRead;

            if (bytesRead == ADPCMBlock.Length)
            {
                _codec!.DecodeSampleBlock(ADPCMBlock, PCMBlock);
                Array.Copy(PCMBlock, 0, _buffer, _bufferSize, PCMBlock.Length);
                _bufferSize += PCMBlock.Length;
            }
        }
    }
}
