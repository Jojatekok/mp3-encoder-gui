using System.Runtime.CompilerServices;

namespace LameEncoderInterface.OptionAdditions.QualityArguments
{
    public sealed class ConstantBitrate
    {
        public ushort Bitrate { get; private set; }

        public ConstantBitrate(ushort bitrateKbps)
        {
            Bitrate = bitrateKbps;
        }
    }

    public sealed class AverageBitrate
    {
        public ushort Bitrate { get; private set; }

        public AverageBitrate(ushort bitrateKbps)
        {
            Bitrate = bitrateKbps;
        }
    }

    public sealed class VariableBitrate
    {
        private byte _quality;
        public byte Quality
        {
            get { return _quality; }
            private set { if (value <= 9) _quality = value; }
        }

        public ushort MinimumBitrate { get; private set; }
        public ushort MaximumBitrate { get; private set; }

        public bool IsMinimumBitrateArgumentEnforced { get; set; }

        public VariableBitrate(byte quality, ushort minBitrateKbps, ushort maxBitrateKbps)
        {
            Quality = quality;
            MinimumBitrate = minBitrateKbps;
            MaximumBitrate = maxBitrateKbps;
            IsMinimumBitrateArgumentEnforced = true;
        }
    }
}
