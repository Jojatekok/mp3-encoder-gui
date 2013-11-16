using System;
using System.Globalization;
using System.IO;

namespace MP3EncoderGUI
{
    public static class Helper
    {
        public static readonly CultureInfo InvariantCulture = CultureInfo.InvariantCulture;

        private const string DefaultEncodingParams = "--replaygain-accurate --strictly-enforce-ISO --id3v2-latin1 -q 0 -b 320";

        public static string GetEncodingParams(MainWindow window)
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

        public static byte[] ReadAllBytes(FileStream stream)
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
