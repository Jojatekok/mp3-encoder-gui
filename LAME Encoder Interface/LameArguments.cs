using System.Collections.Generic;
using LameEncoderInterface.OptionAdditions;

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
            var output = string.Empty;

            // [Operational options]
            switch (OperationalOptions.ReplayGain) {
                case Operational.ReplayGainMode.Accurate:
                    output += "--replaygain-accurate ";
                    break;

                case Operational.ReplayGainMode.Fast:
                    output += "--replaygain-fast ";
                    break;

                case Operational.ReplayGainMode.Disabled:
                    output += "--noreplaygain ";
                    break;
            }

            // [Quality options]
            if (QualityOptions.CbrOptions != null) {
                output += "--cbr -b " + QualityOptions.CbrOptions.Bitrate + " ";

            } else if (QualityOptions.AbrOptions != null) {
                output += "--abr " + QualityOptions.AbrOptions.Bitrate + " ";

            } else if (QualityOptions.VbrOptions != null) {
                output += "-V " + QualityOptions.VbrOptions.Quality +
                         " -b " + QualityOptions.VbrOptions.MinimumBitrate +
                         " -B " + QualityOptions.VbrOptions.MaximumBitrate + " ";

                if (QualityOptions.VbrOptions.IsMinimumBitrateArgumentEnforced) {
                    output += "-F ";
                }
            }

            output += "-q " + QualityOptions.NoiseShapingQuality + " ";

            // [Filters]
            if (Filters.LowpassFrequency != null) {
                output += "--lowpass " + Filters.LowpassFrequency.Value.ToString("0.0", Helper.InvariantCulture) + " ";
            }
            if (Filters.LowpassWidth != null) {
                output += "--lowpass-width " + Filters.LowpassWidth.Value.ToString("0.0", Helper.InvariantCulture) + " ";
            }

            if (Filters.HighpassFrequency != null) {
                output += "--highpass " + Filters.HighpassFrequency.Value.ToString("0.0", Helper.InvariantCulture) + " ";
            }
            if (Filters.HighpassWidth != null) {
                output += "--highpass-width " + Filters.HighpassWidth.Value.ToString("0.0", Helper.InvariantCulture) + " ";
            }

            if (Filters.Resample != null) {
                output += "--resample " + Filters.Resample.Value.ToString("0.0", Helper.InvariantCulture) + " ";
            }

            // [Header options]
            if (HeaderOptions.IsIsoStrictlyEnforced) {
                output += "--strictly-enforce-ISO ";
            }

            if (HeaderOptions.IsCopyrighted) output += "-c ";
            if (HeaderOptions.IsNonOriginal) output += "-o ";

            // [ID3 tags]
            if (!string.IsNullOrWhiteSpace(Id3Tags.Title)) output += "--tt \"" + Id3Tags.Title + "\" ";
            if (!string.IsNullOrWhiteSpace(Id3Tags.Artist)) output += "--ta \"" + Id3Tags.Artist + "\" ";
            if (!string.IsNullOrWhiteSpace(Id3Tags.Album)) output += "--tl \"" + Id3Tags.Album + "\" ";
            if (!string.IsNullOrWhiteSpace(Id3Tags.CoverArtPath)) output += "--ti \"" + Id3Tags.CoverArtPath + "\" ";
            if (!string.IsNullOrWhiteSpace(Id3Tags.Comment)) output += "--tc \"" + Id3Tags.Comment + "\" ";
            if (Id3Tags.Year != 0) output += "--ty " + Id3Tags.Year + " ";

            if (Id3Tags.Genre != null) {
                if (Id3Tags.Genre.Id != null) {
                    output += "--tg " + Id3Tags.Genre.Id.Value + " ";
                } else if (!string.IsNullOrWhiteSpace(Id3Tags.Genre.Name)) {
                    output += "--tg \"" + Id3Tags.Genre.Name + "\" ";
                }
            }

            if (Id3Tags.TrackNumber != 0) {
                output += "--tn " + Id3Tags.TrackNumber;
                if (Id3Tags.TrackTotal != 0) output += "/" + Id3Tags.TrackTotal;
                output += " ";
            }

            switch (Id3Tags.Encoding) {
                case OptionAdditions.Id3Tags.Encoding.Latin1:
                    output += "--id3v2-latin1 ";
                    break;

                case OptionAdditions.Id3Tags.Encoding.Utf16:
                    output += "--id3v2-utf16 ";
                    break;
            }

            if (Id3Tags.ExtraTags.Count != 0) {
                for (var i = Id3Tags.ExtraTags.Count - 1; i >= 0; i--) {
                    var tmp = Id3Tags.ExtraTags[i];
                    if (tmp != null) {
                        output += "--tv " + tmp.Id + "=" + tmp.Value + " ";
                    }
                }
            }

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
        public OptionAdditions.Operational.ReplayGainMode ReplayGain { get; set; }
    }

    public sealed class Quality
    {
        public OptionAdditions.Quality.ConstantBitrate CbrOptions { get; set; }
        public OptionAdditions.Quality.AverageBitrate AbrOptions { get; set; }
        public OptionAdditions.Quality.VariableBitrate VbrOptions { get; set; }

        private byte _noiseShapingQuality;
        public byte NoiseShapingQuality
        {
            get { return _noiseShapingQuality; }
            set { if (value <= 9) _noiseShapingQuality = value; }
        }
    }

    public sealed class Filters
    {
        public float? LowpassFrequency { get; set; }
        public float? LowpassWidth { get; set; }

        public float? HighpassFrequency { get; set; }
        public float? HighpassWidth { get; set; }

        public float? Resample { get; set; }
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
    }

    public sealed class Id3Tags
    {
        public string Title { get; set; }
        public string Artist { get; set; }
        public string Album { get; set; }
        public OptionAdditions.Id3Tags.Genre Genre { get; set; }
        public string CoverArtPath { get; set; }
        public string Comment { get; set; }

        private ushort _year;
        public ushort Year {
            get { return _year; }
            set { if (value < 10000) _year = value; }
        }

        public byte TrackNumber { get; set; }
        public byte TrackTotal { get; set; }

        public OptionAdditions.Id3Tags.Encoding Encoding { get; set; }

        private readonly IList<OptionAdditions.Id3Tags.ExtraTag> _extraTags = new List<OptionAdditions.Id3Tags.ExtraTag>();
        public IList<OptionAdditions.Id3Tags.ExtraTag> ExtraTags {
            get { return _extraTags; }
        }
    }
}
