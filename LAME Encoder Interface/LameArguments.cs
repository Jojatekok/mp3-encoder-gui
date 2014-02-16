using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace LameEncoderInterface
{
    public sealed class LameArguments
    {
        #region Declarations

        //public Options.InputRegular InputRegular { get; private set; }
        //public Options.InputPcm InputPcm { get; private set; }
        public Options.Operational OperationalOptions { get; private set; }
        public Options.Quality QualityOptions { get; private set; }
        public Options.Filters Filters { get; private set; }
        public Options.Header HeaderOptions { get; private set; }
        public Options.Id3Tags Id3Tags { get; private set; }

        public string ExtraArguments { get; set; }

        #endregion

        #region Methods

        public LameArguments()
        {
            OperationalOptions = new Options.Operational();
            QualityOptions = new Options.Quality();
            Filters = new Options.Filters();
            HeaderOptions = new Options.Header();
            Id3Tags = new Options.Id3Tags();
        }

        public LameArguments(Options.Operational operationalOptions, Options.Quality qualityOptions, Options.Filters filters, Options.Header headerOptions, Options.Id3Tags id3Tags, string extraArguments)
        {
            OperationalOptions = operationalOptions;
            QualityOptions = qualityOptions;
            Filters = filters;
            HeaderOptions = headerOptions;
            Id3Tags = id3Tags;
            ExtraArguments = extraArguments;
        }

        public LameArguments(Options.Operational operationalOptions, Options.Quality qualityOptions, Options.Filters filters, Options.Header headerOptions, Options.Id3Tags id3Tags, params string[] extraArguments) : this(operationalOptions, qualityOptions, filters, headerOptions, id3Tags, string.Join(" ", extraArguments))
        {

        }

        public override string ToString()
        {
            var output = OperationalOptions.ToString() + QualityOptions + Filters + HeaderOptions + Id3Tags;

            // [Extra command line arguments]
            if (!string.IsNullOrWhiteSpace(ExtraArguments)) {
                output += ExtraArguments;
            } else {
                output = output.Substring(0, output.Length - 1);
            }

            return output;
        }

        #endregion
    }
}

namespace LameEncoderInterface.Options
{
    //public sealed class InputRegular
    //{

    //}

    //public sealed class InputPcm
    //{

    //}

    public sealed class Operational
    {
        public OptionAdditions.OperationalArguments.ReplayGainMode ReplayGain { get; set; }

        public override string ToString()
        {
            switch (ReplayGain) {
                case OptionAdditions.OperationalArguments.ReplayGainMode.Accurate:
                    return "--replaygain-accurate ";

                case OptionAdditions.OperationalArguments.ReplayGainMode.Fast:
                    return "--replaygain-fast ";
            }

            return "--noreplaygain ";
        }
    }

    public sealed class Quality
    {
        public OptionAdditions.QualityArguments.ConstantBitrate CbrOptions { get; set; }
        public OptionAdditions.QualityArguments.AverageBitrate AbrOptions { get; set; }
        public OptionAdditions.QualityArguments.VariableBitrate VbrOptions { get; set; }

        private byte _noiseShapingQuality;
        public byte NoiseShapingQuality
        {
            get { return _noiseShapingQuality; }
            set { if (value <= 9) _noiseShapingQuality = value; }
        }

        public override string ToString()
        {
            var output = string.Empty;

            if (CbrOptions != null) {
                output += "--cbr -b " + CbrOptions.Bitrate + " ";

            } else if (AbrOptions != null) {
                output += "--abr " + AbrOptions.Bitrate + " ";

            } else if (VbrOptions != null) {
                output += "-V " + VbrOptions.Quality +
                         " -b " + VbrOptions.MinimumBitrate +
                         " -B " + VbrOptions.MaximumBitrate + " ";

                if (VbrOptions.IsMinimumBitrateArgumentEnforced) {
                    output += "-F ";
                }
            }

            output += "-q " + NoiseShapingQuality + " ";

            return output;
        }
    }

    public sealed class Filters
    {
        public float? LowpassFrequency { get; set; }
        public float? LowpassWidth { get; set; }

        public float? HighpassFrequency { get; set; }
        public float? HighpassWidth { get; set; }

        public float? Resample { get; set; }

        public override string ToString()
        {
            var output = string.Empty;

            if (LowpassFrequency != null)
                output += "--lowpass " + LowpassFrequency.Value.ToString("0.0", Helper.InvariantCulture) + " ";
            if (LowpassWidth != null)
                output += "--lowpass-width " + LowpassWidth.Value.ToString("0.0", Helper.InvariantCulture) + " ";

            if (HighpassFrequency != null)
                output += "--highpass " + HighpassFrequency.Value.ToString("0.0", Helper.InvariantCulture) + " ";
            if (HighpassWidth != null)
                output += "--highpass-width " + HighpassWidth.Value.ToString("0.0", Helper.InvariantCulture) + " ";

            if (Resample != null)
                output += "--resample " + Resample.Value.ToString("0.0", Helper.InvariantCulture) + " ";

            return output;
        }
    }

    public sealed class Header
    {
        public bool IsCopyrighted { get; set; }
        public bool IsNonOriginal { get; set; }

        public bool IsIsoStrictlyEnforced { get; set; }

        public Header()
        {
            IsIsoStrictlyEnforced = true;
        }

        public override string ToString()
        {
            var output = string.Empty;

            if (IsIsoStrictlyEnforced)
                output += "--strictly-enforce-ISO ";

            if (IsCopyrighted) output += "-c ";
            if (IsNonOriginal) output += "-o ";

            return output;
        }
    }

    public sealed class Id3Tags
    {
        public string Title { get; set; }
        public string Artist { get; set; }
        public string Album { get; set; }
        public OptionAdditions.Id3Arguments.Genre Genre { get; set; }
        public string CoverArtPath { get; set; }
        public string Comment { get; set; }

        private ushort _year;
        public ushort Year {
            get { return _year; }
            set { if (value < 10000) _year = value; }
        }

        public byte TrackNumber { get; set; }
        public byte TrackTotal { get; set; }

        public OptionAdditions.Id3Arguments.Encoding Encoding { get; set; }

        private readonly IList<OptionAdditions.Id3Arguments.ExtraTag> _extraTags = new List<OptionAdditions.Id3Arguments.ExtraTag>();
        public IList<OptionAdditions.Id3Arguments.ExtraTag> ExtraTags {
            get { return _extraTags; }
        }

        public override string ToString()
        {
            var output = string.Empty;

            if (!string.IsNullOrWhiteSpace(Title)) output += "--tt \"" + Title + "\" ";
            if (!string.IsNullOrWhiteSpace(Artist)) output += "--ta \"" + Artist + "\" ";
            if (!string.IsNullOrWhiteSpace(Album)) output += "--tl \"" + Album + "\" ";
            if (!string.IsNullOrWhiteSpace(CoverArtPath)) output += "--ti \"" + CoverArtPath + "\" ";
            if (!string.IsNullOrWhiteSpace(Comment)) output += "--tc \"" + Comment + "\" ";
            if (Year != 0) output += "--ty " + Year + " ";

            if (Genre != null) {
                if (Genre.Id != null) {
                    output += "--tg " + Genre.Id.Value + " ";
                } else if (!string.IsNullOrWhiteSpace(Genre.Name)) {
                    output += "--tg \"" + Genre.Name + "\" ";
                }
            }

            if (TrackNumber != 0) {
                output += "--tn " + TrackNumber;
                if (TrackTotal != 0) output += "/" + TrackTotal;
                output += " ";
            }

            switch (Encoding) {
                case OptionAdditions.Id3Arguments.Encoding.Latin1:
                    output += "--id3v2-latin1 ";
                    break;

                case OptionAdditions.Id3Arguments.Encoding.Utf16:
                    output += "--id3v2-utf16 ";
                    break;
            }

            if (ExtraTags.Count != 0) {
                for (var i = ExtraTags.Count - 1; i >= 0; i--) {
                    var tmp = ExtraTags[i];
                    if (tmp != null) {
                        output += "--tv " + tmp.Id + "=" + tmp.Value + " ";
                    }
                }
            }

            return output;
        }
    }
}
