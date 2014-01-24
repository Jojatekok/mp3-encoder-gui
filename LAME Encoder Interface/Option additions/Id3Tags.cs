﻿using System.Collections.Generic;

namespace LameEncoderInterface.OptionAdditions
{
    public static class Id3Tags
    {
        #region Genres

        public sealed class Genre
        {
            public string Name { get; private set; }
            public byte? Id { get; private set; }

            public Genre(string name)
            {
                Name = name;
            }

            public Genre(byte id)
            {
                Id = id;
            }
        }

        private static readonly Dictionary<string, byte> _genres = new Dictionary<string, byte> {
            { "A Cappella", 123 },
            { "Acid", 34 },
            { "Acid Jazz", 74 },
            { "Acid Punk", 73 },
            { "Acoustic", 99 },
            { "Alternative", 20 },
            { "Alternative Rock", 40 },
            { "Ambient", 26 },
            { "Anime", 145 },
            { "Avantgarde", 90 },
            { "Ballad", 116 },
            { "Bass", 41 },
            { "Beat", 135 },
            { "Bebob", 85 },
            { "Big Band", 96 },
            { "Black Metal", 138 },
            { "Bluegrass", 89 },
            { "Blues", 0 },
            { "Booty Bass", 107 },
            { "BritPop", 132 },
            { "Cabaret", 65 },
            { "Celtic", 88 },
            { "Chamber Music", 104 },
            { "Chanson", 102 },
            { "Chorus", 97 },
            { "Christian Gangsta", 136 },
            { "Christian Rap", 61 },
            { "Christian Rock", 141 },
            { "Classical", 32 },
            { "Classic Rock", 1 },
            { "Club", 112 },
            { "Club-House", 128 },
            { "Comedy", 57 },
            { "Contemporary Christian", 140 },
            { "Country", 2 },
            { "Crossover", 139 },
            { "Cult", 58 },
            { "Dance", 3 },
            { "Dance Hall", 125 },
            { "Darkwave", 50 },
            { "Death Metal", 22 },
            { "Disco", 4 },
            { "Dream", 55 },
            { "Drum & Bass", 127 },
            { "Drum Solo", 122 },
            { "Duet", 120 },
            { "Easy Listening", 98 },
            { "Electronic", 52 },
            { "Ethnic", 48 },
            { "Eurodance", 54 },
            { "Euro-House", 124 },
            { "Euro-Techno", 25 },
            { "Fast Fusion", 84 },
            { "Folk", 80 },
            { "Folklore", 115 },
            { "Folk-Rock", 81 },
            { "Freestyle", 119 },
            { "Funk", 5 },
            { "Fusion", 30 },
            { "Game", 36 },
            { "Gangsta", 59 },
            { "Goa", 126 },
            { "Gospel", 38 },
            { "Gothic", 49 },
            { "Gothic Rock", 91 },
            { "Grunge", 6 },
            { "Hardcore", 129 },
            { "Hard Rock", 79 },
            { "Heavy Metal", 137 },
            { "Hip-Hop", 7 },
            { "House", 35 },
            { "Humour", 100 },
            { "Indie", 131 },
            { "Industrial", 19 },
            { "Instrumental", 33 },
            { "Instrumental Pop", 46 },
            { "Instrumental Rock", 47 },
            { "Jazz", 8 },
            { "Jazz+Funk", 29 },
            { "JPop", 146 },
            { "Jungle", 63 },
            { "Latin", 86 },
            { "Lo-Fi", 71 },
            { "Meditative", 45 },
            { "Merengue", 142 },
            { "Metal", 9 },
            { "Musical", 77 },
            { "National Folk", 82 },
            { "Native US", 64 },
            { "Negerpunk", 133 },
            { "New Age", 10 },
            { "New Wave", 66 },
            { "Noise", 39 },
            { "Oldies", 11 },
            { "Opera", 103 },
            { "Other", 12 },
            { "Polka", 75 },
            { "Polsk Punk", 134 },
            { "Pop", 13 },
            { "Pop-Folk", 53 },
            { "Pop/Funk", 62 },
            { "Porn Groove", 109 },
            { "Power Ballad", 117 },
            { "Pranks", 23 },
            { "Primus", 108 },
            { "Progressive Rock", 92 },
            { "Psychedelic", 67 },
            { "Psychedelic Rock", 93 },
            { "Punk", 43 },
            { "Punk Rock", 121 },
            { "Rap", 15 },
            { "Rave", 68 },
            { "R&B", 14 },
            { "Reggae", 16 },
            { "Retro", 76 },
            { "Revival", 87 },
            { "Rhythmic Soul", 118 },
            { "Rock", 17 },
            { "Rock & Roll", 78 },
            { "Salsa", 143 },
            { "Samba", 114 },
            { "Satire", 110 },
            { "Showtunes", 69 },
            { "Ska", 21 },
            { "Slow Jam", 111 },
            { "Slow Rock", 95 },
            { "Sonata", 105 },
            { "Soul", 42 },
            { "Sound Clip", 37 },
            { "Soundtrack", 24 },
            { "Southern Rock", 56 },
            { "Space", 44 },
            { "Speech", 101 },
            { "Swing", 83 },
            { "Symphonic Rock", 94 },
            { "Symphony", 106 },
            { "SynthPop", 147 },
            { "Tango", 113 },
            { "Techno", 18 },
            { "Techno-Industrial", 51 },
            { "Terror", 130 },
            { "Thrash Metal", 144 },
            { "Top 40", 60 },
            { "Trailer", 70 },
            { "Trance", 31 },
            { "Tribal", 72 },
            { "Trip-Hop", 27 },
            { "Vocal", 28 }
        };
        public static Dictionary<string, byte> Genres {
            get { return _genres; }
        }

        #endregion

        #region Other additions

        public enum Encoding
        {
            Latin1 = 0,
            Utf16 = 1
        }

        public sealed class ExtraTag
        {
            public string Id { get; private set; }
            public object Value { get; private set; }

            public ExtraTag(string id, object value)
            {
                Id = id;
                Value = value;
            }
        }

        #endregion
    }
}
