using LameEncoderInterface;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
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
using System.Windows.Shell;
using WpfCustomControls;

namespace MP3EncoderGUI
{
    public sealed partial class MainWindow : IDisposable
    {
        #region Declarations

        private readonly SynchronizationContext _syncContext = SynchronizationContext.Current;
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
                    TaskbarItemInfo.ProgressState = TaskbarItemProgressState.Indeterminate;
                    ProgressBarEncoding.Text = "Checking for updates...";
                    GridProgress.Visibility = Visibility.Visible;
                    ButtonStart.Visibility = Visibility.Hidden;

                } else {
                    TaskbarItemInfo.ProgressState = TaskbarItemProgressState.None;
                    ButtonStart.Visibility = Visibility.Visible;
                    GridProgress.Visibility = Visibility.Hidden;
                    ProgressBarEncoding_ResetText();
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

            #region Arguments support

            var cmdArgs = new List<string>(Environment.GetCommandLineArgs());
            cmdArgs.RemoveAt(0);

            // -f: Force starting the application, without checking for updates
            if (cmdArgs.Contains("-f")) {
                ProgressBarEncoding_ResetText();
                return;
            }

            #endregion

            // Throw error and exit if the required version of the .NET Framework is not installed
            if (!Helper.IsNetFramework45Installed()) {
                Messages.ShowError(this, Messages.Errors.NetFramework45NotFound);
                Application.Current.Shutdown();
                return;
            }

            // Prepare the icon, and parse the pre-defined genres' list
            Icon = System.Drawing.Icon.ExtractAssociatedIcon(Assembly.GetExecutingAssembly().Location).ToImageSource();
            ComboBoxGenre.ItemsSource = Dictionaries.MusicGenres.Keys;
            TextBoxExtraCmdArgs.SelectionStart = TextBoxExtraCmdArgs.Text.Length;

#if DEBUG
            // Do not check for updates
            ProgressBarEncoding_ResetText();
#else
            if (Helper.IsConnectedToInternet()) {
                ThreadPool.QueueUserWorkItem(CheckForUpdates);
            } else {
                ProgressBarEncoding_ResetText();
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

        private void TaskbarItemInfo_Changed(object sender, EventArgs e)
        {
            ProgressBarEncoding.IsIndeterminate = TaskbarItemInfo.ProgressState == TaskbarItemProgressState.Indeterminate;
            ProgressBarEncoding.Value = (byte)(TaskbarItemInfo.ProgressValue * 100D);
        }

        #endregion

        #region Check for updates

        private void CheckForUpdates(object sender)
        {
            _syncContext.Send(_ => IsCheckingForUpdates = true, null);

            using (var webClient = new WebClient()) {
                try {
                    var currentVersion = Assembly.GetExecutingAssembly().GetName().Version;
                    var latestVersionString = webClient.DownloadString(new Uri("http://jojatekok.github.io/mp3-encoder-gui/version.txt"));

                    if (new Version(latestVersionString).CompareTo(currentVersion) > 0) {
                        var updateName = "Update (v" + latestVersionString + ")";
                        var updatePath = Helper.AppStartDirectory + updateName;

                        // Start downloading the update and notify the user about it
                        using (var downloadTask = webClient.DownloadFileTaskAsync(new Uri("http://jojatekok.github.io/mp3-encoder-gui/latest.zip"), updatePath + ".zip")) {
                            if (IsCheckingForUpdates && Messages.ShowQuestionByDispatcher(this, Messages.Questions.UpdateAvailable, latestVersionString) == MessageBoxResult.Yes) {
                                _syncContext.Send(_ => {
                                    ButtonStop.IsEnabled = false;
                                    ProgressBarEncoding.Text = "Applying update...";
                                }, null);

                            } else {
                                _syncContext.Send(_ => IsCheckingForUpdates = false, null);
                                try {
                                    File.Delete(updatePath + ".zip");
                                } catch { }
                                return;
                            }

                            downloadTask.Wait();
                        }

                        // Extract the downloaded update
                        ZipFile.ExtractToDirectory(updatePath + ".zip", updatePath);

                        // Write a batch file which applies the update
                        using (var writer = new StreamWriter(Helper.AppStartDirectory + "Updater.bat")) {
                            writer.Write("XCOPY /V /Q /R /Y \"" + updateName + "\"" + Environment.NewLine +
                                         "START \"\" \"MP3 Encoder GUI.exe\" -f" + Environment.NewLine +
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

                        _syncContext.Send(_ => Application.Current.Shutdown(), null);
                    }

                } catch {
                    Messages.ShowErrorByDispatcher(this, Messages.Errors.UpdateCheckFailed);
                }
            }

            _syncContext.Send(_ => IsCheckingForUpdates = false, null);
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

        #region General

        private void NumberBoxTrack1_ValueChanged(object sender, WpfCustomControls.NuintValueChangedEventArgs e)
        {
            if (e.NewValue != null) {
                NumberBoxTrack2.IsEnabled = true;

            } else {
                NumberBoxTrack2.IsEnabled = false;
                NumberBoxTrack2.Clear();
            }
        }

        #endregion

        #region Quality

        private void RadioButtonBitrateNonVbr_Checked(object sender, RoutedEventArgs e)
        {
            if (!IsInitialized) return;

            if (Equals(sender, RadioButtonBitrateConstant)) {
                BitrateSelectorNonVbr.UpdateValidValues(SamplingFrequencySelectorNonVbr.GetAvailableMp3Types());
                SamplingFrequencySelectorNonVbr.UpdateValidValues(BitrateSelectorNonVbr.GetAvailableMp3Types());

            } else {
                const Mp3Type allMp3Types = Mp3Type.Mpeg10 | Mp3Type.Mpeg20 | Mp3Type.Mpeg25;
                BitrateSelectorNonVbr.UpdateValidValues(allMp3Types);
                SamplingFrequencySelectorNonVbr.UpdateValidValues(allMp3Types);
            }

            GridQualityOptionsNonVbr.Visibility = Visibility.Visible;
            GridQualityOptionsVbr.Visibility = Visibility.Hidden;
        }

        private void RadioButtonBitrateVbr_Checked(object sender, RoutedEventArgs e)
        {
            if (!IsInitialized) return;

            GridQualityOptionsVbr.Visibility = Visibility.Visible;
            GridQualityOptionsNonVbr.Visibility = Visibility.Hidden;
        }

        private void BitrateSelectorNonVbr_ValueChanged(object sender, UshortValueChangedEventArgs e)
        {
            if (RadioButtonBitrateConstant.IsChecked != null && RadioButtonBitrateConstant.IsChecked.Value) {
                SamplingFrequencySelectorNonVbr.UpdateValidValues(BitrateSelectorNonVbr.GetAvailableMp3Types());
            }
        }

        private void SamplingFrequencySelectorNonVbr_ValueChanged(object sender, UshortValueChangedEventArgs e)
        {
            if (RadioButtonBitrateConstant.IsChecked != null && RadioButtonBitrateConstant.IsChecked.Value) {
                BitrateSelectorNonVbr.UpdateValidValues(SamplingFrequencySelectorNonVbr.GetAvailableMp3Types());
            }
        }

        #endregion

        #region Cover art

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

        #endregion

        #region Start/stop encoding

        private void ButtonStart_Click(object sender, RoutedEventArgs e)
        {
            LockOptionControls(true);

            var inputFile = TextBoxInputFile.Text;
            var outputFile = TextBoxOutputFile.Text;

            // Check whether the output file already exists
            if (File.Exists(outputFile)) {
                if (Messages.ShowWarning(this, Messages.Warnings.OutputFileAlreadyExists) == MessageBoxResult.No) {
                    ButtonStart.Visibility = Visibility.Visible;
                    GridProgress.Visibility = Visibility.Hidden;
                    return;
                }
            }

            // Dispose the previous LAME process if necessary
            if (_lameProc != null) _lameProc.Dispose();

            try {
                _lameProc = new LameProcess(inputFile, outputFile, Helper.GetEncodingParams(this));
                _lameProc.ProgressChanged += LameProc_ProgressChanged;
                _lameProc.InputFileRemovalFailed += LameProc_InputFileRemovalFailed;
                _lameProc.Disposed += LameProc_Disposed;
                _lameProc.Start();

            } catch (Exception ex) {
                Messages.ShowError(this, ex.Message);
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
        }

        public void LockOptionControls(bool doLock)
        {
            if (doLock) {
                TaskbarItemInfo.ProgressState = TaskbarItemProgressState.Normal;

                GridProgress.Visibility = Visibility.Visible;
                ButtonStart.Visibility = Visibility.Hidden;

                var tmp = ComboBoxGenre.Text;
                ComboBoxGenre.ItemsSource = null;
                if (Dictionaries.MusicGenres.ContainsKey(tmp)) {
                    ComboBoxGenre.SelectedValue = tmp;
                }
                ComboBoxGenre.Text = tmp;

                ButtonChangeCoverArt.IsEnabled = false;
                ButtonRemoveCoverArt.IsEnabled = false;

            } else {
                TaskbarItemInfo.ProgressState = TaskbarItemProgressState.None;
                TaskbarItemInfo.ProgressValue = 0D;

                ButtonStart.Visibility = Visibility.Visible;
                GridProgress.Visibility = Visibility.Hidden;

                ComboBoxGenre.ItemsSource = Dictionaries.MusicGenres.Keys;

                ButtonChangeCoverArt.IsEnabled = true;
                if (ImageCoverArt.Source != null) {
                    ButtonRemoveCoverArt.IsEnabled = true;
                }
            }

            ButtonInputFile.IsEnabled = !doLock;
            ButtonOutputFile.IsEnabled = !doLock;

            ComboBoxGenre.IsReadOnly = doLock;
            NumericUpDownYear.IsReadOnly = doLock;
            NumberBoxTrack1.IsReadOnly = doLock;
            NumberBoxTrack2.IsReadOnly = doLock;

            TextBoxInputFile.IsReadOnly = doLock;
            TextBoxOutputFile.IsReadOnly = doLock;

            TextBoxExtraCmdArgs.IsReadOnly = doLock;

            foreach (var textBox in Helper.FindVisualChildren<TextBox>(TabControlEncodingOptions)) {
                textBox.IsReadOnly = doLock;
            }

            foreach (var control in Helper.FindVisualChildren<Control>(GridEncodingOptionsQuality)) {
                control.IsEnabled = !doLock;
            }
        }

        private void LameProc_ProgressChanged(object sender, LameEncoderInterface.ProgressChangedEventArgs e)
        {
            _syncContext.Send(_ => {
                TaskbarItemInfo.ProgressValue = (double)e.NewValue / e.Maximum;
            }, null);
            
            if (e.NewValue == e.Maximum) {
                _syncContext.Send(_ => ButtonStop.Content = "Ok", null);
            }
        }

        private void LameProc_InputFileRemovalFailed(object sender, FileLocationEventArgs e)
        {
            var file = e.FilePath;
            if (Messages.ShowWarning(this, Messages.Warnings.OutputFileIsNotRemovable, file) == MessageBoxResult.Yes) {
                if (File.Exists(file))
                    File.Delete(file);
            }
        }

        private void LameProc_Disposed(object sender, EventArgs e)
        {
            _lameProc = null;
        }

        #endregion

        #region Miscellaneous

        private void ProgressBarEncoding_ResetText()
        {
            ProgressBarEncoding.Text = "Encoding...";
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
