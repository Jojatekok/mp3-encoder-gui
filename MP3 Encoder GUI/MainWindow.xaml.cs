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
    public partial class MainWindow : Window, IDisposable
    {
        #region Declarations

        public static readonly string LameEncoderLocation = AppDomain.CurrentDomain.BaseDirectory + @"lame\lame.exe";

        private Process _lameProc;
        private SynchronizationContext _syncContext;

        public string CoverArtPath { get; private set; }
        private FileStream _coverArtFileStream;
        private MemoryStream _coverArtMemStream;

        #endregion

        #region Methods

        #region Window-related

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
                    if (Messages.ShowWarning(this, Messages.ExitConfirmationMessage, Messages.ExitConfirmationTitle) == MessageBoxResult.Yes) {
                        _lameProc.Kill();
                    } else {
                        e.Cancel = true;
                        return;
                    }
                }

                _lameProc.Dispose();
            }
        }

        #endregion

        #region File locations

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

        #endregion

        #region Encoding options

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
                    // Dispose the previous cover art streams if necessary
                    DisposeCoverArtStreams();
                    
                    _coverArtFileStream = new FileStream(dlg.FileName, FileMode.Open, FileAccess.Read, FileShare.Read, 4096);
                    _coverArtMemStream = new MemoryStream(Helper.ReadAllBytes(_coverArtFileStream));
                    
                    var image = new BitmapImage();
                    image.BeginInit();
                    image.StreamSource = _coverArtMemStream;
                    image.EndInit();

                    ImageCoverArt.Source = image;

                    CoverArtPath = dlg.FileName;
                    ButtonChangeCoverArt.Content = "Change";
                    ButtonRemoveCoverArt.IsEnabled = true;
                } catch { }
            }
        }

        private void DisposeCoverArtStreams()
        {
            ImageCoverArt.Source = null;
            CoverArtPath = null;

            if (_coverArtFileStream != null) {
                if (_coverArtMemStream != null) {
                    _coverArtMemStream.Dispose();
                    _coverArtMemStream = null;
                }

                _coverArtFileStream.Dispose();
                _coverArtFileStream = null;
            }
        }

        private void ButtonRemoveCoverArt_Click(object sender, RoutedEventArgs e)
        {
            // Dispose the previous cover art streams if necessary
            DisposeCoverArtStreams();

            ButtonRemoveCoverArt.IsEnabled = false;
            ButtonChangeCoverArt.Content = "Add";
        }

        #endregion

        #region Start/stop encoding

        private void ButtonStart_Click(object sender, RoutedEventArgs e)
        {
            GridProgress.Visibility = Visibility.Visible;
            ButtonStart.Visibility = Visibility.Hidden;

            var inputFile = TextBoxInputFile.Text;
            FileInfo outputFileInfo;

            if (inputFile.Length != 0) {
                try {
                    using (var stream = new FileStream(inputFile, FileMode.Open, FileAccess.Read, FileShare.Read)) {
                        try {
                            outputFileInfo = new FileInfo(TextBoxOutputFile.Text);
                        } catch {
                            Messages.ShowError(this, Messages.OutputFilePathInvalid);
                            return;
                        }

                        // Dispose the previous LAME process if necessary
                        if (_lameProc != null) { _lameProc.Dispose(); }

                        // Check whether the output file already exists
                        if (File.Exists(outputFileInfo.FullName)) {
                            if (Messages.ShowWarning(this, Messages.OutputFileAlreadyExists) == MessageBoxResult.No) {
                                ButtonStart.Visibility = Visibility.Visible;
                                GridProgress.Visibility = Visibility.Hidden;
                                return;
                            }
                        } else {
                            outputFileInfo.Directory.Create();
                        }
                        
                        _lameProc = new Process() {
                            StartInfo = new ProcessStartInfo() {
                                FileName = LameEncoderLocation,
                                Arguments = Helper.GetEncodingParams(this) + " \"" + inputFile + "\" \"" + outputFileInfo.FullName + "\"",
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
                            _lameProc.Dispose();
                            _lameProc = null;
                            Messages.ShowError(this, Messages.LameEncoderNotFound);
                        }
                    }
                } catch {
                    Messages.ShowError(this, Messages.InputFileNotFound);
                }

            } else {
                Messages.ShowError(this, Messages.InputFileNotFound);
            }
        }

        private void Lame_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            var outputText = e.Data;

            if (outputText != null && outputText.Contains("%)")) {
                var tmp1 = outputText.Substring(0, outputText.IndexOf('(') - 1).Replace(" ", string.Empty);
                var tmp2 = tmp1.Split('/');

                double value = double.Parse(tmp2[0], Helper.InvariantCulture);
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
                Messages.ShowWarningByDispatcher(this, Messages.OutputFileIsNotRemovable, Messages.DefaultWarningTitle, fileName) == MessageBoxResult.Yes
            );
        }

        #endregion

        #endregion

        #region IDisposable support

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing) {
                DisposeCoverArtStreams();
            }
        }

        #endregion
    }
}
