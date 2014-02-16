using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace LameEncoderInterface
{
    [Flags]
    public enum Mp3Type
    {
        Mpeg10 = 1,
        Mpeg20 = 2,
        Mpeg25 = 4
    }
}

namespace LameEncoderInterface.MP3Types
{
    public static class Any
    {
        public static IList<ushort> GetAvailableBitrates(Mp3Type mp3Types)
        {
            var output = new HashSet<ushort>();

            if (mp3Types.Contains(Mp3Type.Mpeg10)) {
                output.UnionWith(Mpeg10.Bitrates);
            }

            if (mp3Types.Contains(Mp3Type.Mpeg20)) {
                output.UnionWith(Mpeg20.Bitrates);
            }

            if (mp3Types.Contains(Mp3Type.Mpeg25)) {
                output.UnionWith(Mpeg25.Bitrates);
            }

            var tmp = new List<ushort>(output);
            return tmp.OrderByDescending(x => x).ToList();
        }

        public static IList<ushort> GetAvailableSamplingFrequencies(Mp3Type mp3Types)
        {
            var output = new List<ushort> {0};

            if (mp3Types.Contains(Mp3Type.Mpeg10)) {
                output.AddRange(Mpeg10.SamplingFrequencies);
            }

            if (mp3Types.Contains(Mp3Type.Mpeg20)) {
                output.AddRange(Mpeg20.SamplingFrequencies);
            }

            if (mp3Types.Contains(Mp3Type.Mpeg25)) {
                output.AddRange(Mpeg25.SamplingFrequencies);
            }

            return output;
        }

        private static readonly IList<ushort> _bitrates = new List<ushort> { 320, 256, 224, 192, 160, 144, 128, 112, 96, 80, 64, 56, 48, 40, 32, 24, 16, 8 };
        public static IList<ushort> Bitrates {
            get { return _bitrates; }
        }

        private static readonly IList<ushort> _samplingFrequencies = new List<ushort> { 0, 44100, 48000, 32000, 22050, 24000, 16000, 11025, 12000, 8000 };
        public static IList<ushort> SamplingFrequencies {
            get { return _samplingFrequencies; }
        }
    }

    public static class Mpeg10
    {
        private static readonly IList<ushort> _bitrates = new List<ushort> { 320, 256, 224, 192, 160, 128, 112, 96, 80, 64, 56, 48, 40, 32 };
        public static IList<ushort> Bitrates {
            get { return _bitrates; }
        }

        private static readonly IList<ushort> _samplingFrequencies = new List<ushort> { 44100, 48000, 32000 };
        public static IList<ushort> SamplingFrequencies {
            get { return _samplingFrequencies; }
        }
    }

    public static class Mpeg20
    {
        private static readonly IList<ushort> _bitrates = new List<ushort> { 160, 144, 128, 112, 96, 80, 64, 56, 48, 40, 32, 24, 16, 8 };
        public static IList<ushort> Bitrates {
            get { return _bitrates; }
        }

        private static readonly IList<ushort> _samplingFrequencies = new List<ushort> { 22050, 24000, 16000 };
        public static IList<ushort> SamplingFrequencies {
            get { return _samplingFrequencies; }
        }
    }

    public static class Mpeg25
    {
        private static readonly IList<ushort> _bitrates = new List<ushort> { 64, 56, 48, 40, 32, 24, 16, 8 };
        public static IList<ushort> Bitrates {
            get { return _bitrates; }
        }

        private static readonly IList<ushort> _samplingFrequencies = new List<ushort> { 11025, 12000, 8000 };
        public static IList<ushort> SamplingFrequencies {
            get { return _samplingFrequencies; }
        }
    }
}
