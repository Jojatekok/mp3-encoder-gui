using System.Collections.Generic;

namespace MP3EncoderGUI
{
    public static class Dictionaries
    {
        private static readonly Dictionary<string, byte> _musicGenres = LameEncoderInterface.OptionAdditions.Id3Arguments.Genre.DefaultGenres;
        public static Dictionary<string, byte> MusicGenres {
            get { return _musicGenres; }
        }
    }
}
