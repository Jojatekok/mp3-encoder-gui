using System;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Windows;

namespace MP3EncoderGUI
{
    public sealed class LameProcess : IDisposable
    {
        #region Events

        public event EventHandler<ProgressChangedEventArgs> ProgressChanged;
        public event EventHandler Disposed;

        #endregion

        #region Declarations

        private static readonly string _lamePath = Helper.AppStartDirectory + @"lame\lame.exe";
        public static string LamePath {
            get { return _lamePath; }
        }

        public bool IsRunning {
            get { return !IsDisposed && !_lameProc.HasExited; }
        }

        private bool IsDisposed { get; set; }

        private string OutputFile { get; set; }

        private readonly Window _parentWindow;
        private readonly Process _lameProc;

        #endregion

        #region Methods

        public LameProcess(Window parentWindow, string inputFile, string outputFile, string arguments)
        {
            _parentWindow = parentWindow;
            OutputFile = outputFile;
            
            _lameProc = new Process {
                StartInfo = new ProcessStartInfo {
                    FileName = LamePath,
                    Arguments = arguments + " \"" + inputFile + "\" \"" + outputFile + "\"",
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardError = true
                }
            };
        }

        public void Start()
        {
            try {
                _lameProc.ErrorDataReceived += Lame_ErrorDataReceived;
                _lameProc.Start();
                _lameProc.BeginErrorReadLine();
            } catch {
                IsDisposed = true;
                _lameProc.Dispose();
                throw new FileNotFoundException(Messages.Errors.LameEncoderNotFound, LamePath);
            }
        }

        public void Cancel()
        {
            _lameProc.Kill();
            ProgressChanged = null;
            new Thread(TryDeleteOutput).Start();
        }

        private void Lame_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            var outputText = e.Data;

            if (outputText != null && outputText.Contains("%)")) {
                var tmp = outputText.Substring(0, outputText.IndexOf('(') - 1).Replace(" ", string.Empty).Split('/');

                uint maximum;
                if (uint.TryParse(tmp[1], out maximum)) {
                    var value = uint.Parse(tmp[0], Helper.InvariantCulture);
                    OnProgressChanged(new ProgressChangedEventArgs(value, maximum));
                }
            }
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
                if (DeleteOutput()) {
                    Dispose();
                    return;
                }
            } while (
                Messages.ShowWarningByDispatcher(_parentWindow, Messages.Warnings.OutputFileIsNotRemovable, OutputFile) == MessageBoxResult.Yes
            );
        }

        private void OnProgressChanged(ProgressChangedEventArgs e)
        {
            if (ProgressChanged != null)
                ProgressChanged(this, e);
        }

        private void OnDisposed()
        {
            if (Disposed != null)
                Disposed(this, null);
        }

        #endregion

        #region IDisposable support

        public void Dispose()
        {
            Dispose(true);
        }

        private void Dispose(bool disposing)
        {
            if (disposing && !IsDisposed) {
                if (!_lameProc.HasExited) {
                    _lameProc.Kill();
                    DeleteOutput();
                }

                IsDisposed = true;
                _lameProc.Dispose();
                OnDisposed();
            }
        }

        #endregion
    }

    public sealed class ProgressChangedEventArgs : EventArgs
    {
        public uint NewValue { get; private set; }
        public uint Maximum { get; private set; }

        public ProgressChangedEventArgs(uint newValue, uint maximum)
        {
            NewValue = newValue;
            Maximum = maximum;
        }
    }
}
