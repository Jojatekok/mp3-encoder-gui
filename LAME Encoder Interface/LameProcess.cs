using System;
using System.Diagnostics;
using System.IO;
using System.Threading;

namespace LameEncoderInterface
{
    public sealed class LameProcess : IDisposable
    {
        #region Events

        public event EventHandler<ProgressChangedEventArgs> ProgressChanged;
        public event EventHandler InputFileRemovalFailed;
        public event EventHandler Disposed;

        #endregion

        #region Declarations

        private readonly string _lamePath;
        public string LamePath {
            get { return _lamePath; }
        }

        private readonly string _inputFile;
        public string InputFile {
            get { return _inputFile; }
        }

        private readonly string _outputFile;
        public string OutputFile {
            get { return _outputFile; }
        }

        private readonly LameArguments _arguments;
        public LameArguments Arguments {
            get { return _arguments; }
        }

        private bool IsDisposed { get; set; }
        public bool IsRunning {
            get { return !IsDisposed && !_lameProc.HasExited; }
        }

        private readonly Process _lameProc;

        #endregion

        #region Methods

        public LameProcess(string lamePath, string inputFile, string outputFile, LameArguments arguments)
        {
            FileInfo outputFileInfo;
            try {
                outputFileInfo = new FileInfo(outputFile);
            } catch {
                throw new UriFormatException(Messages.Errors.OutputFilePathInvalid);
            }

            if (!File.Exists(outputFileInfo.FullName)) {
                outputFileInfo.Directory.Create();
            }

            _outputFile = outputFileInfo.FullName;

            _lamePath = lamePath;
            _inputFile = inputFile;
            _arguments = arguments;
            
            _lameProc = new Process {
                StartInfo = new ProcessStartInfo {
                    FileName = lamePath,
                    Arguments = arguments + " \"" + inputFile + "\" \"" + outputFile + "\"",
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardError = true
                }
            };
            _lameProc.Disposed += LameProc_Disposed;
        }

        public LameProcess(string inputFile, string outputFile, LameArguments arguments) : this(Helper.DefaultLamePath, inputFile, outputFile, arguments)
        {
            
        }

        public LameProcess(string inputFile, string outputFile) : this(Helper.DefaultLamePath, inputFile, outputFile, null)
        {

        }

        public void Start()
        {
            _lameProc.ErrorDataReceived += LameProc_ErrorDataReceived;

            if (!File.Exists(LamePath)) {
                IsDisposed = true;
                _lameProc.Dispose();
                throw new FileNotFoundException(Messages.Errors.LameEncoderNotFound, LamePath);
            }

            if (!File.Exists(InputFile)) {
                IsDisposed = true;
                _lameProc.Dispose();
                throw new FileNotFoundException(Messages.Errors.InputFileNotFound, InputFile);
            }

            _lameProc.Start();
            _lameProc.BeginErrorReadLine();
        }

        public void Cancel()
        {
            _lameProc.Kill();
            ProgressChanged = null;
            ThreadPool.QueueUserWorkItem(TryDeleteOutput);
        }

        private void LameProc_ErrorDataReceived(object sender, DataReceivedEventArgs e)
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

        private void LameProc_Disposed(object sender, EventArgs e)
        {
            Dispose();
        }

        private bool DeleteOutput()
        {
            if (!IsRunning || _lameProc.WaitForExit(3000)) {
                if (File.Exists(OutputFile)) {
                    File.Delete(OutputFile);
                }
                return true; // Success
            }

            return false; // Fail
        }

        private void TryDeleteOutput(object sender)
        {
            while (DeleteOutput()) {
                Dispose();
                return;
            }
        }

        private void OnProgressChanged(ProgressChangedEventArgs e)
        {
            if (ProgressChanged != null)
                ProgressChanged(this, e);
        }

        private void OnInputFileRemovalFailed()
        {
            if (InputFileRemovalFailed != null)
                InputFileRemovalFailed(this, null);
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
                    if (!DeleteOutput()) OnInputFileRemovalFailed();
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
