// Copyright © 2004-2014 Tim Van Holder, Windower Team
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS"
// BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and limitations under the License.

namespace PlayOnline.Core.Audio;

internal class ADPCMCodec
{
    private static readonly int[] Filter0 = [0x0000, 0x00F0, 0x01CC, 0x0188, 0x01E8];
    private static readonly int[] Filter1 = [0x0000, 0x0000, -0x00D0, -0x00DC, -0x00F0];

    private int Channels;
    private int BlockSize;
    private int[] DecoderState = [];

    public ADPCMCodec(int channels, int blockSize) { Reset(channels, blockSize); }

    public void Reset(int channels, int blockSize)
    {
        if (channels <= 0 || channels > 6)
            throw new ArgumentException(I18N.GetText("ADPCMBadChannelCount"), nameof(channels));

        Channels = channels;
        BlockSize = blockSize;
        DecoderState = new int[2 * channels];
    }

    public void DecodeSampleBlock(byte[] bytesIn, byte[] bytesOut)
    {
        if (bytesIn.Length < ((1 + BlockSize / 2) * Channels))
            throw new ArgumentException(string.Format(I18N.GetText("ADPCMInputTooSmall"), 1 + BlockSize / 2), nameof(bytesIn));

        if (bytesOut.Length < (BlockSize * Channels * 2))
            throw new ArgumentException(string.Format(I18N.GetText("ADPCMOutputTooSmall"), 2 * BlockSize), nameof(bytesOut));

        for (int channel = 0; channel < Channels; ++channel)
        {
            int baseIndex = channel * (1 + BlockSize / 2);
            int scale = 0x0C - (bytesIn[baseIndex + 0] & 0x0F);
            int index = bytesIn[baseIndex + 0] >> 4;

            if (index < 5)
            {
                for (byte sample = 0; sample < (BlockSize / 2); ++sample)
                {
                    byte sampleByte = bytesIn[baseIndex + sample + 1];

                    for (byte nibble = 0; nibble < 2; ++nibble)
                    {
                        int value = (sampleByte >> (4 * nibble)) & 0x0F;

                        if (value >= 8)
                            value -= 16;

                        int tempValue = value << scale;
                        tempValue += (DecoderState[channel * 2 + 0] * Filter0[index] +
                                       DecoderState[channel * 2 + 1] * Filter1[index]) / 256;
                        DecoderState[channel * 2 + 1] = DecoderState[channel * 2 + 0];
                        DecoderState[channel * 2 + 0] = Round(tempValue);

                        bytesOut[((2 * sample + nibble) * Channels + channel) * 2 + 0] =
                            (byte)((DecoderState[channel * 2 + 0] >> 0) & 0xff);
                        bytesOut[((2 * sample + nibble) * Channels + channel) * 2 + 1] =
                            (byte)((DecoderState[channel * 2 + 0] >> 8) & 0xff);
                    }
                }
            }
        }
    }

    public void EncodeSampleBlock(byte[] In, byte[] Out)
    {
        throw new NotImplementedException(I18N.GetText("ADPCMEncodingNotSupported"));
    }

    private int Round(int Value)
    {
        if (Value > 0x7FFF)
            return 0x7FFF;

        if (Value < -0x8000)
            return -0x8000;

        return Value;
    }
}
