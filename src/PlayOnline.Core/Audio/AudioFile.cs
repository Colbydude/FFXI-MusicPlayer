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

public enum AudioFileType
{
    Unknown,
    BGMStream,
    SoundEffect,
}

public enum SampleFormat : uint
{
    ADPCM = 0,
    PCM = 1,
    ATRAC3 = 3,
}

public class AudioFileHeader
{
    // Direct Members
    public int Size;
    public SampleFormat SampleFormat;
    public int ID;
    public int SampleBlocks;
    public int LoopStart;
    public int SampleRateLow;
    public int SampleRateHigh;
    public int Unknown1;
    public byte Unknown2;
    public byte Unknown3;
    public byte Channels;
    public byte BlockSize;
    public int Unknown4;

    // Indirect Members
    public double Length => SamplesToSeconds(SampleBlocks);
    public bool Looped => LoopStart >= 0;
    public double LoopStartTime => SamplesToSeconds(LoopStart);
    public int SampleRate => SampleRateHigh + SampleRateLow;

    /// <summary>
    /// Convert the length of the track in samples to seconds.
    /// </summary>
    public double SamplesToSeconds(long Samples)
    {
        double ByteCount = Samples;

        if (SampleFormat == SampleFormat.ADPCM)
            ByteCount *= BlockSize;

        return ByteCount / SampleRate;
    }

    /// <summary>
    /// Convert the length of the track in seconds back to samples.
    /// </summary>
    public long SecondsToSamples(double Seconds)
    {
        double ByteCount = Seconds * SampleRate;

        if (SampleFormat == SampleFormat.ADPCM)
            ByteCount /= BlockSize;

        return (long)Math.Floor(ByteCount);
    }
}

public class AudioFile
{
    public static byte BitsPerSample => 16;

    public AudioFileHeader Header => _header;
    public string Path => _path;
    public bool Playable => _header != null && AudioFileStream.IsFormatSupported(_header.SampleFormat);
    public AudioFileType Type => _type;

    private readonly string _path;
    private AudioFileType _type;
    private readonly AudioFileHeader _header;

    public AudioFile(string Path)
    {
        _path = Path;
        _header = new AudioFileHeader();

        try
        {
            BinaryReader BR = new(new FileStream(_path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, 0x30), Encoding.ASCII);
            DetermineType(BR);

            if (_type != AudioFileType.Unknown)
            {
                switch (_type)
                {
                    case AudioFileType.BGMStream:
                        _header.SampleFormat = (SampleFormat)BR.ReadInt32();
                        _header.Size = BR.ReadInt32();
                        break;
                    case AudioFileType.SoundEffect:
                        _header.Size = BR.ReadInt32();
                        _header.SampleFormat = (SampleFormat)BR.ReadInt32();
                        break;
                }

                _header.ID = BR.ReadInt32();
                _header.SampleBlocks = BR.ReadInt32();
                _header.LoopStart = BR.ReadInt32();
                _header.SampleRateHigh = BR.ReadInt32();
                _header.SampleRateLow = BR.ReadInt32();
                _header.Unknown1 = BR.ReadInt32();
                _header.Unknown2 = BR.ReadByte();
                _header.Unknown3 = BR.ReadByte();
                _header.Channels = BR.ReadByte();
                _header.BlockSize = BR.ReadByte();

                switch (_type)
                {
                    case AudioFileType.BGMStream:
                        _header.Unknown4 = 0;
                        break;
                    case AudioFileType.SoundEffect:
                        _header.Unknown4 = BR.ReadInt32();
                        break;
                }
            }

            BR.Close();
        }
        catch
        {
            _type = AudioFileType.Unknown;
        }
    }

    /// <summary>
    ///
    /// </summary>
    public AudioFileStream? OpenStream()
    {
        return OpenStream(false);
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="AddWAVHeader"></param>
    /// <returns></returns>
    public AudioFileStream? OpenStream(bool AddWAVHeader)
    {
        if (_type == AudioFileType.Unknown || _header == null)
            return null;

        if (!AudioFileStream.IsFormatSupported(_header.SampleFormat))
            return null;

        return new AudioFileStream(_path, _header, AddWAVHeader);
    }

    /// <summary>
    /// Determine if the file is a BGMStream or SoundEffect based on the header.
    /// </summary>
    private void DetermineType(BinaryReader BR)
    {
        string marker = new(BR.ReadChars(8));

        if (marker == "SeWave\0\0")
            _type = AudioFileType.SoundEffect;
        else
        {
            marker += new string(BR.ReadChars(4));

            if (marker == "BGMStream\0\0\0")
                _type = AudioFileType.BGMStream;
        }
    }
}
