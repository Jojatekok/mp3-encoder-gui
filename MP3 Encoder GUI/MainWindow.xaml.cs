using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace Mp3EncoderGui
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        const string LameLocation = @"lame\lame.exe";
        const string DefaultEncodingParams = "--replaygain-accurate --strictly-enforce-ISO --id3v2-latin1 -q 0 -b 320";

        private Process _lameProc;
        private SynchronizationContext _syncContext;

        private string _coverArtPath;

        public MainWindow()
        {
            InitializeComponent();

            _syncContext = SynchronizationContext.Current;

            ComboBoxGenre.ItemsSource = MusicGenres.GenreDictionary.Keys;
            DataObject.AddPastingHandler(TextBoxTrack1, new DataObjectPastingEventHandler(TextBoxTracks_OnPaste));
            DataObject.AddPastingHandler(TextBoxTrack2, new DataObjectPastingEventHandler(TextBoxTracks_OnPaste));

            ProgressBarEncoding.Text = "Encoding...";
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
            if (TextBoxInputFile.Text != string.Empty) {
                RecommendOutputFileName();
            } else {
                TextBoxOutputFile.Text = string.Empty;
            }
        }

        private void TextBoxOutputFile_LostFocus(object sender, RoutedEventArgs e)
        {
            if (TextBoxOutputFile.Text == string.Empty && TextBoxInputFile.Text != string.Empty) {
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

            if (inputFile != string.Empty && File.Exists(inputFile)) {
                var encParams = DefaultEncodingParams;

                // [ID3] Title
                if (TextBoxTitle.Text != string.Empty) {
                    encParams += " --tt \"" + TextBoxTitle.Text + "\"";
                }

                // [ID3] Artist
                if (TextBoxArtist.Text != string.Empty) {
                    encParams += " --ta \"" + TextBoxArtist.Text + "\"";
                }

                // [ID3] Album
                if (TextBoxAlbum.Text != string.Empty) {
                    encParams += " --tl \"" + TextBoxAlbum.Text + "\"";
                }

                // [ID3] Genre
                if (ComboBoxGenre.Text != string.Empty) {
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
                if (TextBoxTrack1.Text != string.Empty) {
                    encParams += " --tn " + TextBoxTrack1.Text;
                    if (TextBoxTrack2.Text != string.Empty) {
                        encParams += "/" + TextBoxTrack2.Text;
                    }
                }

                // [ID3] Comment
                if (TextBoxComment.Text != string.Empty) {
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
                
                double value = double.Parse(tmp2[0]);
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

        private void TextBoxTracks_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            ValidateTrack(e.Text, sender as TextBox);
            e.Handled = true;
        }

        private void TextBoxTracks_OnPaste(object sender, DataObjectPastingEventArgs e)
        {
            if (e.SourceDataObject.GetDataPresent(System.Windows.DataFormats.Text)) {
                ValidateTrack(e.SourceDataObject.GetData(DataFormats.Text) as string, sender as TextBox);
            }

            e.CancelCommand();
        }

        private bool StringIsNumber(string input)
        {
            for (var i = 0; i != input.Length; i++) {
                var tmp = input[i];
                if (tmp < '0' || tmp > '9') {
                    return false;
                }
            }

            return true;
        }

        private void ValidateTrack(string input, TextBox trackTextBox)
        {
            if (!StringIsNumber(input) || trackTextBox.Text == "255"){
                return;
            }

            byte tmp1;
            var tmp2 = trackTextBox.SelectionStart;
            var combinedText = trackTextBox.Text + input;

            if (!byte.TryParse(combinedText, out tmp1)) {
                trackTextBox.Text = "255";

            } else if (combinedText == "0") {
                trackTextBox.Text = "1";

            } else {
                trackTextBox.Text = tmp1.ToString();
            }

            trackTextBox.SelectionStart = tmp2 + 1;
        }

        private void TextBoxTrack1_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (TextBoxTrack1.Text != string.Empty) {
                TextBoxTrack2.IsEnabled = true;
            } else {
                TextBoxTrack2.IsEnabled = false;
                TextBoxTrack2.Text = string.Empty;
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
