using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Media;

namespace MP3EncoderGUI
{
    internal static class Helper
    {
        #region Declarations

        private static readonly string _appStartDirectory = AppDomain.CurrentDomain.BaseDirectory;
        internal static string AppStartDirectory {
            get { return _appStartDirectory; }
        }

        private static readonly CultureInfo _invariantCulture = CultureInfo.InvariantCulture;
        internal static CultureInfo InvariantCulture {
            get { return _invariantCulture; }
        }

        #endregion

        #region Methods

        internal static bool IsNetFramework45Installed()
        {
            using (var ndpKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32).OpenSubKey(@"SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full\")) {
                return ndpKey != null && (int)ndpKey.GetValue("Release") >= 378389;
            }
        }

        internal static string GetEncodingParams(MainWindow window)
        {
            var output = string.Empty;

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
                if (Dictionaries.MusicGenres.TryGetValue(window.ComboBoxGenre.Text, out genreId)) {
                    output += " --tg " + genreId;
                } else {
                    output += " --tg \"" + window.ComboBoxGenre.Text + "\"";
                }
            }

            // [ID3] Year
            if (window.NumericUpDownYear.Value != null) {
                output += " --ty " + window.NumericUpDownYear.Value;
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

            // Quality options
            if (window.RadioButtonBitrateConstant.IsChecked != null && window.RadioButtonBitrateConstant.IsChecked.Value) {
                output += " --cbr -b " + window.BitrateSelectorNonVbr.Value;

            } else if (window.RadioButtonBitrateAverage.IsChecked != null && window.RadioButtonBitrateAverage.IsChecked.Value) {
                output += " --abr " + window.BitrateSelectorNonVbr.Value;

            } else if (window.RadioButtonBitrateVariable.IsChecked != null && window.RadioButtonBitrateVariable.IsChecked.Value) {
                output += " -F -b " + window.BitrateSelectorVbr.MinValue +
                          " -B " + window.BitrateSelectorVbr.MaxValue +
                          " -V " + window.QualitySliderVbr.Value;
            }

            var tmp = window.SamplingFrequencySelectorNonVbr.Value;
            if (tmp != 0) {
                output += " --resample " + ((float)tmp / 1000).ToString("0.0");
            }

            // Extra command line arguments
            if (window.TextBoxExtraCmdArgs.Text.Length != 0) {
                output += " " + window.TextBoxExtraCmdArgs.Text;
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

        internal static IEnumerable<T> FindVisualChildren<T>(DependencyObject depObj) where T : DependencyObject
        {
            for (var i = VisualTreeHelper.GetChildrenCount(depObj) - 1; i >= 0; i--) {
                var child = VisualTreeHelper.GetChild(depObj, i);
                
                if (child is T) {
                    yield return child as T;
                }

                foreach (var childOfChild in FindVisualChildren<T>(child)) {
                    yield return childOfChild;
                }
            }
        }

        #endregion
    }

    internal static class NativeMethods
    {
        [DllImport("wininet.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal extern static bool InternetGetConnectedState(out int description, int reservedValue);
    }

    public class UshortValueChangedEventArgs : EventArgs
    {
        public ushort NewValue { get; private set; }

        public UshortValueChangedEventArgs(ushort newValue)
        {
            NewValue = newValue;
        }
    }
}
