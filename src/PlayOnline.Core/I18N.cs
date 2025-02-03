// Copyright © 2004-2014 Tim Van Holder, Windower Team
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an "AS IS"
// BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and limitations under the License.

namespace PlayOnline.Core;

public static class I18N
{
    public static Dictionary<string, string> Messages = new Dictionary<string, string>
    {
        { "ADPCMBadChannelCount", "Channel count must be an integer between 1 and 6." },
        { "ADPCMEncodingNotSupported", "ADPCM Encoding is not implemented yet." },
        { "ADPCMInputTooSmall", "Input buffer too small - must provide {0} bytes per channel." },
        { "ADPCMOutputTooSmall", "Output buffer too small - must be able to hold {0} bytes per channel." },
        { "E:Region.Europe", "Europe &amp; Australia" },
        { "E:Region.Japan", "Japan" },
        { "E:Region.NorthAmerica", "North America" },
        { "E:Region.None", "None Available" },
        { "POLRegionNotInstalled", "The client for the requested region is not available on this machine." },
        { "SeekNotAllowed", "Seeking is not supported for this stream." },
        { "SetLengthNotAllowed", "Changing length is not supported for this stream." },
        { "WriteNotAllowed", "Writing is not supported for this stream." }
    };

    public static string GetText(string name)
    {
        if (Messages.TryGetValue(name, out var value))
            return value;

        return name;
    }
}
