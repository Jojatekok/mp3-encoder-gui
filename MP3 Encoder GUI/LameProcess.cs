using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Windows;

namespace MP3EncoderGUI
{
    public class LameProcess : IDisposable
    {
        #region Events

        public event EventHandler<ProgressChangedEventArgs> ProgressChanged;

        #endregion

        #region Declarations

        public static readonly string LameLocation = AppDomain.CurrentDomain.BaseDirectory + @"lame\lame.exe";

        public bool IsRunning {
            get {
                if (IsDisposed || _lameProc.HasExited) { return false; }
                return true;
            }
        }

        public bool IsDisposed { get; private set; }

        private string OutputFile { get; set; }

        private Process _lameProc;
        private Window _parentWindow;

        #endregion

        #region Methods

        public LameProcess(Window parentWindow, string inputFile, string outputFile, string arguments)
        {
            _parentWindow = parentWindow;
            OutputFile = outputFile;

            _lameProc = new Process() {
                StartInfo = new ProcessStartInfo() {
                    FileName = LameLocation,
                    Arguments = arguments + " \"" + inputFile + "\" \"" + outputFile + "\"",
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardError = true
                }
            };
        }

        public void Start()
        {
            _lameProc.ErrorDataReceived += Lame_ErrorDataReceived;
            _lameProc.Start();
            _lameProc.BeginErrorReadLine();
        }

        public bool WaitForExit(int milliseconds)
        {
            if (!IsRunning) { return true; }
            return _lameProc.WaitForExit(milliseconds);
        }

        public void Cancel()
        {
            _lameProc.Kill();
            new Thread(TryDeleteOutput).Start();
        }

        private void Lame_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            var outputText = e.Data;

            if (outputText != null && outputText.Contains("%)")) {
                var tmp1 = outputText.Substring(0, outputText.IndexOf('(') - 1).Replace(" ", string.Empty);
                var tmp2 = tmp1.Split('/');
                
                double maximum;
                if (double.TryParse(tmp2[1], out maximum)) {
                    var value = double.Parse(tmp2[0], Helper.InvariantCulture);
                    OnProgressChanged(new ProgressChangedEventArgs(value, maximum));
                }
            }
        }

        protected virtual void OnProgressChanged(ProgressChangedEventArgs e)
        {
            if (ProgressChanged != null)
                ProgressChanged(this, e);
        }

        private bool DeleteOutput()
        {
            if (!IsRunning || _lameProc.WaitForExit(3000)) {
                try {
                    File.Delete(OutputFile);
                    return true; // Success
                } catch { }
            }

            return false; // Fail
        }

        private void TryDeleteOutput()
        {
            do {
                if (DeleteOutput() == true) {
                    return;
                }

            } while (
                Messages.ShowWarningByDispatcher(_parentWindow, Warnings.OutputFileIsNotRemovable, OutputFile) == MessageBoxResult.Yes
            );
        }

        #endregion

        #region IDisposable support

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (disposing && !IsDisposed) {
                if (_lameProc != null) {
                    if (!_lameProc.HasExited) {
                        _lameProc.Kill();
                        DeleteOutput();
                    }

                    IsDisposed = true;
                    _lameProc.Dispose();
                    _lameProc = null;
                    return;
                }

                IsDisposed = true;
            }
        }

        #endregion
    }

    public class ProgressChangedEventArgs : EventArgs
    {
        public double NewValue { get; private set; }
        public double Maximum { get; private set; }

        public ProgressChangedEventArgs(double newValue, double maximum)
        {
            NewValue = newValue;
            Maximum = maximum;
        }
    }
}
