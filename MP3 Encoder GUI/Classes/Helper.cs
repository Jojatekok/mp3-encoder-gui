using LameEncoderInterface;
using LameEncoderInterface.OptionAdditions;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace MP3EncoderGUI
{
    internal static class Helper
    {
        #region Declarations

        private static readonly string _appStartDirectory = AppDomain.CurrentDomain.BaseDirectory;
        internal static string AppStartDirectory {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return _appStartDirectory; }
        }

        private static readonly CultureInfo _invariantCulture = CultureInfo.InvariantCulture;
        internal static CultureInfo InvariantCulture {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return _invariantCulture; }
        }

        #endregion

        #region Methods

        internal static bool IsNetFramework45Installed()
        {
            using (var baseKey = RegistryKey.OpenBaseKey(RegistryHive.LocalMachine, RegistryView.Registry32)) {
                using (var ndpKey = baseKey.OpenSubKey(@"SOFTWARE\Microsoft\NET Framework Setup\NDP\v4\Full\")) {
                    return ndpKey != null && (int)ndpKey.GetValue("Release") >= 378389;
                }
            }
        }

        internal static bool IsConnectedToInternet()
        {
            int tmp;
            return NativeMethods.InternetGetConnectedState(out tmp, 0);
        }

        internal static LameArguments GetEncodingParams(MainWindow window)
        {
            // [ID3 tags]
            var output = new LameArguments {
                Id3Tags = {
                    Title = window.TextBoxTitle.Text,
                    Artist = window.TextBoxArtist.Text,
                    Album = window.TextBoxAlbum.Text,
                    CoverArtPath = window.CoverArtPath,
                    Comment = window.TextBoxComment.Text
                }
            };

            if (window.NumericUpDownYear.Value != null) {
                output.Id3Tags.Year = (ushort)window.NumericUpDownYear.Value;
            }

            var genreText = window.ComboBoxGenre.Text;
            if (genreText.Length != 0) {
                byte genreId;
                output.Id3Tags.Genre = Dictionaries.MusicGenres.TryGetValue(genreText, out genreId) ?
                                       new Id3Tags.Genre(genreId) :
                                       new Id3Tags.Genre(genreText);
            }

            if (window.NumberBoxTrack1.Value != null) {
                output.Id3Tags.TrackNumber = (byte)window.NumberBoxTrack1.Value;
                if (window.NumberBoxTrack2.Value != null) {
                    output.Id3Tags.TrackTotal = (byte)window.NumberBoxTrack2.Value;
                }
            }

            // [Quality options]
            var isVbr = false;

            if (window.RadioButtonBitrateConstant.IsChecked != null && window.RadioButtonBitrateConstant.IsChecked.Value) {
                output.QualityOptions.CbrOptions = new Quality.ConstantBitrate(window.BitrateSelectorNonVbr.Value);

            } else if (window.RadioButtonBitrateAverage.IsChecked != null && window.RadioButtonBitrateAverage.IsChecked.Value) {
                output.QualityOptions.AbrOptions = new Quality.AverageBitrate(window.BitrateSelectorNonVbr.Value);

            } else if (window.RadioButtonBitrateVariable.IsChecked != null && window.RadioButtonBitrateVariable.IsChecked.Value) {
                output.QualityOptions.VbrOptions = new Quality.VariableBitrate(window.QualitySliderVbr.Value,
                                                                               window.BitrateSelectorVbr.MinValue,
                                                                               window.BitrateSelectorVbr.MaxValue);
                isVbr = true;
            }

            // [Filters]
            var samplingFrequency = isVbr ?
                                    window.SamplingFrequencySelectorVbr.Value :
                                    window.SamplingFrequencySelectorNonVbr.Value;

            if (samplingFrequency != 0) {
                output.Filters.Resample = samplingFrequency / 1000F;
            }

            // [Extra command line arguments]
            output.ExtraArguments = window.TextBoxExtraCmdArgs.Text;

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

    internal static class IconExtension
    {
        public static ImageSource ToImageSource(this Icon icon)
        {
            using (var bitmap = icon.ToBitmap()) {
                var hBitmap = bitmap.GetHbitmap();

                ImageSource wpfBitmap = Imaging.CreateBitmapSourceFromHBitmap(
                    hBitmap,
                    IntPtr.Zero,
                    Int32Rect.Empty,
                    BitmapSizeOptions.FromEmptyOptions()
                );

                if (!NativeMethods.DeleteObject(hBitmap)) {
                    throw new Win32Exception();
                }

                return wpfBitmap;
            }
        }
    }

    internal static class NativeMethods
    {
        [DllImport("wininet.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal extern static bool InternetGetConnectedState(out int description, int reservedValue);

        [DllImport("gdi32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal extern static bool DeleteObject(IntPtr hObject);
    }
}
