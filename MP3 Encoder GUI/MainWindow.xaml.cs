using Microsoft.Win32;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace MP3EncoderGUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        const string LameLocation = @"lame\lame.exe";
        const string DefaultEncodingParams = "--replaygain-accurate --strictly-enforce-ISO --id3v2-latin1 -q 0 -b 320";

        private static readonly CultureInfo InvariantCulture = CultureInfo.InvariantCulture;

        private Process _lameProc;
        private SynchronizationContext _syncContext;

        private string _coverArtPath;

        public MainWindow()
        {
            InitializeComponent();
            _syncContext = SynchronizationContext.Current;

            ComboBoxGenre.ItemsSource = MusicGenres.GenreDictionary.Keys;
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            if (_lameProc != null) {
                if (!_lameProc.HasExited) {
                    if (MessageBox.Show("Are you sure you want to exit?", "The encoding is still in progress", MessageBoxButton.YesNo, MessageBoxImage.Exclamation) == MessageBoxResult.Yes) {
                        _lameProc.Kill();
                    } else {
                        e.Cancel = true;
                        return;
                    }
                }

                _lameProc.Dispose();
            }
        }

        private void TextBoxInputFile_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (TextBoxInputFile.Text.Length != 0) {
                RecommendOutputFileName();
            } else {
                TextBoxOutputFile.Text = string.Empty;
            }
        }

        private void TextBoxOutputFile_LostFocus(object sender, RoutedEventArgs e)
        {
            if (TextBoxOutputFile.Text.Length == 0 && TextBoxInputFile.Text.Length != 0) {
                RecommendOutputFileName();
            }
        }

        private void RecommendOutputFileName()
        {
            var tmp = TextBoxInputFile.Text.LastIndexOf('.');

            if (tmp == -1) {
                TextBoxOutputFile.Text = TextBoxInputFile.Text + ".mp3";
            } else {
                TextBoxOutputFile.Text = TextBoxInputFile.Text.Substring(0, tmp) + ".mp3";
            }
        }

        private void ButtonStart_Click(object sender, RoutedEventArgs e)
        {
            GridProgress.Visibility = Visibility.Visible;
            ButtonStart.Visibility = Visibility.Hidden;

            var inputFile = TextBoxInputFile.Text;

            if (inputFile.Length != 0 && File.Exists(inputFile)) {
                var encParams = DefaultEncodingParams;

                // [ID3] Title
                if (TextBoxTitle.Text.Length != 0) {
                    encParams += " --tt \"" + TextBoxTitle.Text + "\"";
                }

                // [ID3] Artist
                if (TextBoxArtist.Text.Length != 0) {
                    encParams += " --ta \"" + TextBoxArtist.Text + "\"";
                }

                // [ID3] Album
                if (TextBoxAlbum.Text.Length != 0) {
                    encParams += " --tl \"" + TextBoxAlbum.Text + "\"";
                }

                // [ID3] Genre
                if (ComboBoxGenre.Text.Length != 0) {
                    byte genreId;
                    if (MusicGenres.GenreDictionary.TryGetValue(ComboBoxGenre.Text, out genreId)) {
                        encParams += " --tg " + genreId;
                    } else {
                        encParams += " --tg \"" + ComboBoxGenre.Text + "\"";
                    }
                }

                // [ID3] Year
                if (TextBoxYear.Value != 0) {
                    encParams += " --ty " + TextBoxYear.Value;
                }

                // [ID3] Track
                if (NumberBoxTrack1.Value != null) {
                    encParams += " --tn " + NumberBoxTrack1.Value;
                    if (NumberBoxTrack2.Value != null) {
                        encParams += "/" + NumberBoxTrack2.Value;
                    }
                }

                // [ID3] Comment
                if (TextBoxComment.Text.Length != 0) {
                    encParams += " --tc \"" + TextBoxComment.Text + "\"";
                }

                // [ID3] Cover art
                if (_coverArtPath != null) {
                    encParams += " --ti \"" + _coverArtPath + "\"";
                }

                if (_lameProc != null) { _lameProc.Dispose(); }

                _lameProc = new Process() {
                    StartInfo = new ProcessStartInfo() {
                        FileName = AppDomain.CurrentDomain.BaseDirectory + LameLocation,
                        Arguments = encParams + " \"" + inputFile + "\" \"" + TextBoxOutputFile.Text + "\"",
                        CreateNoWindow = true,
                        UseShellExecute = false,
                        RedirectStandardError = true
                    }
                };

                _lameProc.ErrorDataReceived += Lame_ErrorDataReceived;
                try {
                    _lameProc.Start();
                    _lameProc.BeginErrorReadLine();
                } catch {
                    _lameProc = null;
                    MessageBox.Show("The LAME encoder was not found at \"" + AppDomain.CurrentDomain.BaseDirectory + LameLocation + "\"." + Environment.NewLine +
                        "Please download it, and then try again.",
                        "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    ButtonStart.Visibility = Visibility.Visible;
                    GridProgress.Visibility = Visibility.Hidden;
                }

            } else {
                MessageBox.Show("The input file does not exist.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                ButtonStart.Visibility = Visibility.Visible;
                GridProgress.Visibility = Visibility.Hidden;
            }
        }

        private void Lame_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            var outputText = e.Data;

            if (outputText != null && outputText.Contains("%)")) {
                var tmp1 = outputText.Substring(0, outputText.IndexOf('(') - 1).Replace(" ", string.Empty);
                var tmp2 = tmp1.Split('/');

                double value = double.Parse(tmp2[0], InvariantCulture);
                _syncContext.Send(_ => ProgressBarEncoding.Value = value, null);

                double maximum;
                if (double.TryParse(tmp2[1], out maximum)) {
                    _syncContext.Send(_ => ProgressBarEncoding.Maximum = maximum, null);
                    if (value == maximum) {
                        _syncContext.Send(_ => ButtonStop.Content = "Ok", null);
                    }
                }
            }
        }

        private void ButtonStop_Click(object sender, RoutedEventArgs e)
        {
            if (_lameProc != null && !_lameProc.HasExited){
                _lameProc.Kill();

                var tmp = TextBoxOutputFile.Text;
                new Thread(() => TryDeleteOutput(tmp)).Start();
            }
            
            ButtonStart.Visibility = Visibility.Visible;
            GridProgress.Visibility = Visibility.Hidden;
            ButtonStop.Content = "Cancel";
            ProgressBarEncoding.Reset();
        }

        private void TryDeleteOutput(string fileName)
        {
            do {
                if (_lameProc == null || _lameProc.HasExited || _lameProc.WaitForExit(3000)) {
                    try {
                        File.Delete(fileName);
                        return;
                    } catch { }
                }

            } while (
                MessageBox.Show("Could not delete the unfinished output file at \"" + fileName + "\"." + Environment.NewLine +
                    "Do you want to retry?",
                    "Warning", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes
            );
        }

        private void ButtonInput_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog() {
                Filter = "All files|*|Waveform Audio File (*.wav, *.wave)|*.wav;*.wave"
            };

            if (dlg.ShowDialog() == true) {
                TextBoxInputFile.Text = dlg.FileName;
            }
        }

        private void ButtonOutput_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new SaveFileDialog() {
                Filter = "All files|*|MP3 File (*.mp3)|*.mp3",
                FilterIndex = 2,
                OverwritePrompt = false
            };

            if (dlg.ShowDialog() == true) {
                TextBoxOutputFile.Text = dlg.FileName;
            }
        }

        private void NumberBoxTrack1_ValueChanged(object sender, RoutedEventArgs e)
        {
            if (NumberBoxTrack1.Value != null) {
                NumberBoxTrack2.IsEnabled = true;

            } else {
                NumberBoxTrack2.IsEnabled = false;
                NumberBoxTrack2.Clear();
            }
        }

        private void ButtonChangeCoverArt_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();
            dlg.Filter = "All files|*|Recommended image files (*.jpg, *.jpeg, *.png)|*.jpg;*.jpeg;*.png|JPEG files (*.jpg, *.jpeg, *.jpe)|*.jpg;*.jpeg;*.jpe|PNG files (*.png)|*.png|GIF files (*.gif)|*.gif";
            dlg.FilterIndex = 2;

            if (dlg.ShowDialog() == true) {
                try {
                    ImageCoverArt.Source = new BitmapImage(new Uri(dlg.FileName));
                    _coverArtPath = dlg.FileName;
                    ButtonChangeCoverArt.Content = "Change";
                    ButtonRemoveCoverArt.IsEnabled = true;
                } catch { }
            }
        }

        private void ButtonRemoveCoverArt_Click(object sender, RoutedEventArgs e)
        {
            ImageCoverArt.Source = null;
            _coverArtPath = null;
            ButtonRemoveCoverArt.IsEnabled = false;
            ButtonChangeCoverArt.Content = "Add";
        }
    }
}
