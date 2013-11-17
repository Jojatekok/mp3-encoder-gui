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

        private LameProcess _lameProc;
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
            if (_lameProc.IsRunning) {
                if (Messages.ShowWarning(this, Warnings.ExitConfirmation) == MessageBoxResult.No) {
                    e.Cancel = true;
                    return;
                }
            }

            Dispose();
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
                            Messages.ShowError(this, Errors.OutputFilePathInvalid);
                            return;
                        }

                        // Check whether the output file already exists
                        if (File.Exists(outputFileInfo.FullName)) {
                            if (Messages.ShowWarning(this, Warnings.OutputFileAlreadyExists) == MessageBoxResult.No) {
                                ButtonStart.Visibility = Visibility.Visible;
                                GridProgress.Visibility = Visibility.Hidden;
                                return;
                            }
                        } else {
                            outputFileInfo.Directory.Create();
                        }

                        // Dispose the previous LAME process if necessary
                        if (_lameProc != null) { _lameProc.Dispose(); }

                        _lameProc = new LameProcess(this, inputFile, outputFileInfo.FullName, Helper.GetEncodingParams(this));
                        _lameProc.ProgressChanged += LameProc_ProgressChanged;

                        try {
                            _lameProc.Start();
                        } catch {
                            _lameProc.Dispose();
                            _lameProc = null;
                            Messages.ShowError(this, Errors.LameEncoderNotFound);
                        }
                    }
                } catch {
                    Messages.ShowError(this, Errors.InputFileNotFound);
                }

            } else {
                Messages.ShowError(this, Errors.InputFileNotFound);
            }
        }

        private void LameProc_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            _syncContext.Send(_ => {
                ProgressBarEncoding.Maximum = e.Maximum;
                ProgressBarEncoding.Value = e.NewValue;
            }, null);

            if (e.NewValue == e.Maximum) {
                _syncContext.Send(_ => ButtonStop.Content = "Ok", null);
            }
        }

        private void ButtonStop_Click(object sender, RoutedEventArgs e)
        {
            if (_lameProc.IsRunning) {
                _lameProc.Cancel();
            }
            
            ButtonStart.Visibility = Visibility.Visible;
            GridProgress.Visibility = Visibility.Hidden;
            ButtonStop.Content = "Cancel";
            ProgressBarEncoding.Reset();
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

                if (_lameProc != null) {
                    _lameProc.Dispose();
                    _lameProc = null;
                }
            }
        }

        #endregion
    }
}
