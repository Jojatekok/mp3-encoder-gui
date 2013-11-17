using System;
using System.Globalization;
using System.IO;

namespace MP3EncoderGUI
{
    internal static class Helper
    {
        private static readonly CultureInfo _invariantCulture = CultureInfo.InvariantCulture;
        internal static CultureInfo InvariantCulture {
            get { return _invariantCulture; }
        }

        private const string _defaultEncodingParams = "--replaygain-accurate --strictly-enforce-ISO --id3v2-latin1 -q 0 -b 320";
        internal static string DefaultEncodingParams {
            get { return _defaultEncodingParams; }
        }

        internal static string GetEncodingParams(MainWindow window)
        {
            var output = DefaultEncodingParams;

            // [ID3] Title
            if (window.TextBoxTitle.Text.Length != 0) {
                output += " --tt \"" + window.TextBoxTitle.Text + "\"";
            }

            // [ID3] Artist
            if (window.TextBoxArtist.Text.Length != 0) {
                output += " --ta \"" + window.TextBoxArtist.Text + "\"";
            }

            // [ID3] Album
            if (window.TextBoxAlbum.Text.Length != 0) {
                output += " --tl \"" + window.TextBoxAlbum.Text + "\"";
            }

            // [ID3] Genre
            if (window.ComboBoxGenre.Text.Length != 0) {
                byte genreId;
                if (MusicGenres.GenreDictionary.TryGetValue(window.ComboBoxGenre.Text, out genreId)) {
                    output += " --tg " + genreId;
                } else {
                    output += " --tg \"" + window.ComboBoxGenre.Text + "\"";
                }
            }

            // [ID3] Year
            if (window.TextBoxYear.Value != null) {
                output += " --ty " + window.TextBoxYear.Value;
            }

            // [ID3] Track
            if (window.NumberBoxTrack1.Value != null) {
                output += " --tn " + window.NumberBoxTrack1.Value;
                if (window.NumberBoxTrack2.Value != null) {
                    output += "/" + window.NumberBoxTrack2.Value;
                }
            }

            // [ID3] Comment
            if (window.TextBoxComment.Text.Length != 0) {
                output += " --tc \"" + window.TextBoxComment.Text + "\"";
            }

            // [ID3] Cover art
            if (window.CoverArtPath != null) {
                output += " --ti \"" + window.CoverArtPath + "\"";
            }

            return output;
        }

        internal static byte[] ReadAllBytes(FileStream stream)
        {
            var length = stream.Length;
            if (length > 2147483647L) {
                throw new IOException("The file cannot be bigger than 2GB.");
            }

            var i = (int)length;
            var offset = 0;
            var output = new byte[i];
            while (i > 0) {
                var parsed = stream.Read(output, offset, i);
                if (parsed == 0) {
                    return output;
                }

                offset += parsed;
                i -= parsed;
            }

            return output;
        }
    }
}
