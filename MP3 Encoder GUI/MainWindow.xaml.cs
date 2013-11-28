using Microsoft.Win32;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Net;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace MP3EncoderGUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public sealed partial class MainWindow : IDisposable
    {
        #region Declarations

        private readonly SynchronizationContext _syncContext;
        private LameProcess _lameProc;

        public string CoverArtPath { get; private set; }
        private FileStream _coverArtFileStream;
        private MemoryStream _coverArtMemStream;

        private bool _isCheckingForUpdates;
        private bool IsCheckingForUpdates
        {
            get { return _isCheckingForUpdates; }

            set {
                if (value == IsCheckingForUpdates) return;

                if (value) {
                    ProgressBarEncoding.IsMarquee = true;
                    ProgressBarEncoding.Text = "Checking for updates...";
                    GridProgress.Visibility = Visibility.Visible;
                    ButtonStart.Visibility = Visibility.Hidden;

                } else {
                    ButtonStart.Visibility = Visibility.Visible;
                    GridProgress.Visibility = Visibility.Hidden;
                    ProgressBarEncoding.IsMarquee = false;
                    ProgressBarEncoding.Text = "Encoding...";
                }

                _isCheckingForUpdates = value;
            }
        }

        #endregion

        #region Methods

        #region Window-related

        public MainWindow()
        {
            InitializeComponent();
            _syncContext = SynchronizationContext.Current;

            ComboBoxGenre.ItemsSource = MusicGenres.GenreDictionary.Keys;

#if DEBUG
            // Do not check for updates
            ProgressBarEncoding.Text = "Encoding...";
#else
            if (IsConnectedToInternet()) {
                CheckForUpdates();
            } else {
                ProgressBarEncoding.Text = "Encoding...";
            }
#endif
        }

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            if (_lameProc != null && _lameProc.IsRunning) {
                if (Messages.ShowWarning(this, Messages.Warnings.ExitConfirmation) == MessageBoxResult.No) {
                    e.Cancel = true;
                    return;
                }
            }

            Dispose();
        }

        #endregion

        #region Check for updates

        private static bool IsConnectedToInternet()
        {
            int tmp;
            return NativeMethods.InternetGetConnectedState(out tmp, 0);
        }

        private async void CheckForUpdates()
        {
            IsCheckingForUpdates = true;

            using (var webClient = new WebClient()) {
                try {
                    var currentVersion = Assembly.GetExecutingAssembly().GetName().Version;
                    var latestVersionString = await webClient.DownloadStringTaskAsync(new Uri("http://jojatekok.github.io/mp3-encoder-gui/version.txt"));

                    if (new Version(latestVersionString).CompareTo(currentVersion) > 0) {
                        var updateName = "Update (v" + latestVersionString + ")";
                        var updatePath = Helper.AppStartDirectory + updateName;

                        // Start downloading the update and notify the user about it
                        using (var downloadTask = webClient.DownloadFileTaskAsync(new Uri("http://jojatekok.github.io/mp3-encoder-gui/latest.zip"), updatePath + ".zip")) {
                            if (IsCheckingForUpdates && Messages.ShowQuestion(this, Messages.Questions.UpdateAvailable, latestVersionString) == MessageBoxResult.Yes) {
                                ButtonStop.IsEnabled = false;
                                ProgressBarEncoding.Text = "Applying update...";

                            } else {
                                IsCheckingForUpdates = false;
                                try {
                                    File.Delete(updatePath + ".zip");
                                } catch { }
                                return;
                            }

                            await downloadTask;
                        }

                        // Extract the downloaded update
                        ZipFile.ExtractToDirectory(updatePath + ".zip", updatePath);

                        // Write a batch file which applies the update
                        using (var writer = new StreamWriter(Helper.AppStartDirectory + "Updater.bat")) {
                            await writer.WriteAsync("XCOPY /V /Q /R /Y \"" + updateName + "\"" + Environment.NewLine +
                                                    "START \"\" \"MP3 Encoder GUI.exe\"" + Environment.NewLine +
                                                    "RD /S /Q \"" + updateName + "\"" + Environment.NewLine +
                                                    "DEL /F /Q \"" + updateName + ".zip\"" + Environment.NewLine +
                                                    "DEL /F /Q %0");
                        }

                        new Process {
                            StartInfo = new ProcessStartInfo(Helper.AppStartDirectory + "Updater.bat") {
                                CreateNoWindow = true,
                                UseShellExecute = false
                            }
                        }.Start();

                        Application.Current.Shutdown();
                    }

                } catch {
                    Messages.ShowError(this, Messages.Errors.UpdateCheckFailed);
                }
            }

            IsCheckingForUpdates = false;
        }

        #endregion

        #region File locations

        private void ButtonInput_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog {
                Filter = "All files|*|Waveform Audio File (*.wav, *.wave)|*.wav;*.wave"
            };
            
            if (dlg.ShowDialog() == true) {
                TextBoxInputFile.Text = dlg.FileName;
            }
        }

        private void ButtonOutput_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new SaveFileDialog {
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
                // No input file extension
                TextBoxOutputFile.Text = TextBoxInputFile.Text + ".mp3";
            } else if (TextBoxInputFile.Text.Substring(tmp) != ".mp3") {
                // The input file's extension is not MP3
                TextBoxOutputFile.Text = TextBoxInputFile.Text.Substring(0, tmp) + ".mp3";
            } else {
                // The input file's extension is MP3
                TextBoxOutputFile.Text = TextBoxInputFile.Text.Substring(0, tmp) + " (Re-encoded).mp3";
            }
        }

        #endregion

        #region Encoding options

        private void NumberBoxTrack1_ValueChanged(object sender, WpfCustomControls.ValueChangedEventArgs e)
        {
            if (e.NewValue != null) {
                NumberBoxTrack2.IsEnabled = true;

            } else {
                NumberBoxTrack2.IsEnabled = false;
                NumberBoxTrack2.Clear();
            }
        }

        private void ButtonChangeCoverArt_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog {
                Filter = "All files|*|Recommended image files (*.jpg, *.jpeg, *.png)|*.jpg;*.jpeg;*.png|JPEG files (*.jpg, *.jpeg, *.jpe)|*.jpg;*.jpeg;*.jpe|PNG files (*.png)|*.png|GIF files (*.gif)|*.gif",
                FilterIndex = 2
            };

            if (dlg.ShowDialog() != true) return;

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

        private void ButtonRemoveCoverArt_Click(object sender, RoutedEventArgs e)
        {
            // Dispose the previous cover art streams if necessary
            DisposeCoverArtStreams();

            ButtonRemoveCoverArt.IsEnabled = false;
            ButtonChangeCoverArt.Content = "Add";
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

        #endregion

        #region Start/stop encoding

        private void ButtonStart_Click(object sender, RoutedEventArgs e)
        {
            LockOptionControls(true);

            var inputFile = TextBoxInputFile.Text;

            if (inputFile.Length == 0 || !File.Exists(inputFile)) {
                Messages.ShowError(this, Messages.Errors.InputFileNotFound);
                return;
            }

            // Lock the found input file (so it cannot be modified or removed)
            using (new FileStream(inputFile, FileMode.Open, FileAccess.Read, FileShare.Read, 1)) {
                if (!File.Exists(LameProcess.LamePath)) {
                    Messages.ShowError(this, Messages.Errors.LameEncoderNotFound);
                    return;
                }

                // Lock the found LAME encoder (so it cannot be modified or removed)
                using (new FileStream(LameProcess.LamePath, FileMode.Open, FileAccess.Read, FileShare.Read, 1)) {
                    FileInfo outputFileInfo;
                    try {
                        outputFileInfo = new FileInfo(TextBoxOutputFile.Text);
                    } catch {
                        Messages.ShowError(this, Messages.Errors.OutputFilePathInvalid);
                        return;
                    }

                    // Check whether the output file already exists
                    if (File.Exists(outputFileInfo.FullName)) {
                        if (Messages.ShowWarning(this, Messages.Warnings.OutputFileAlreadyExists) == MessageBoxResult.No) {
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
                    _lameProc.Disposed += LameProc_Disposed;
                    _lameProc.Start();
                }
            }
        }

        private void ButtonStop_Click(object sender, RoutedEventArgs e)
        {
            if (IsCheckingForUpdates) {
                // Cancel checking for updates
                IsCheckingForUpdates = false;
                return;
            }

            // Else, cancel encoding
            if (_lameProc.IsRunning) {
                _lameProc.Cancel();
            }

            LockOptionControls(false);
            
            ButtonStop.Content = "Cancel";
            ProgressBarEncoding.Reset();
        }

        public void LockOptionControls(bool doLock)
        {
            if (doLock) {
                GridProgress.Visibility = Visibility.Visible;
                ButtonStart.Visibility = Visibility.Hidden;

                ButtonChangeCoverArt.IsEnabled = false;
                ButtonRemoveCoverArt.IsEnabled = false;

                if (ComboBoxGenre.SelectedValue != null) {
                    var tmp = ComboBoxGenre.SelectedValue;
                    ComboBoxGenre.ItemsSource = null;
                    ComboBoxGenre.SelectedValue = tmp;
                    ComboBoxGenre.Text = tmp as string;

                } else {
                    var tmp = ComboBoxGenre.Text;
                    ComboBoxGenre.ItemsSource = null;
                    ComboBoxGenre.Text = tmp;
                }

            } else {
                ButtonStart.Visibility = Visibility.Visible;
                GridProgress.Visibility = Visibility.Hidden;

                ComboBoxGenre.ItemsSource = MusicGenres.GenreDictionary.Keys;

                ButtonChangeCoverArt.IsEnabled = true;
                if (ImageCoverArt.Source != null) {
                    ButtonRemoveCoverArt.IsEnabled = true;
                }
            }

            ComboBoxGenre.IsReadOnly = doLock;
            NumericUpDownYear.IsReadOnly = doLock;
            NumberBoxTrack1.IsReadOnly = doLock;
            NumberBoxTrack2.IsReadOnly = doLock;

            TextBoxInputFile.IsReadOnly = doLock;
            TextBoxOutputFile.IsReadOnly = doLock;

            foreach (var textBox in Helper.FindVisualChildren<TextBox>(TabControlEncodingOptions)) {
                textBox.IsReadOnly = doLock;
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

        private void LameProc_Disposed(object sender, EventArgs e)
        {
            _lameProc = null;
        }

        #endregion

        #endregion

        #region IDisposable support

        public void Dispose()
        {
            Dispose(true);
        }

        private void Dispose(bool disposing)
        {
            if (disposing) {
                DisposeCoverArtStreams();

                if (_lameProc != null) {
                    _lameProc.Dispose();
                }
            }
        }

        #endregion
    }
}
