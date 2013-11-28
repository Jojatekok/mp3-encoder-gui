using System;
using System.Collections.Generic;
using System.Windows;

namespace MP3EncoderGUI
{
    public static class Messages
    {
        #region Methods

        internal static void ShowError(Window window, IList<string> error, params string[] arguments)
        {
            MessageBox.Show(window, string.Format(Helper.InvariantCulture, error[1], arguments), error[0], MessageBoxButton.OK, MessageBoxImage.Error);

            var mainWindow = window as MainWindow;
            if (mainWindow != null) {
                mainWindow.LockOptionControls(false);
            }
        }

        internal static void ShowError(Window window, string errorMessage, params string[] arguments)
        {
            ShowError(window, new[] { Errors.DefaultTitle, errorMessage }, arguments);
        }

        internal static MessageBoxResult ShowWarning(Window window, IList<string> warning, params string[] arguments)
        {
            return MessageBox.Show(window, string.Format(Helper.InvariantCulture, warning[1], arguments), warning[0], MessageBoxButton.YesNo, MessageBoxImage.Warning);
        }

        internal static MessageBoxResult ShowWarning(Window window, string warningMessage, params string[] arguments)
        {
            return ShowWarning(window, new[] { Errors.DefaultTitle, warningMessage }, arguments);
        }

        internal static MessageBoxResult ShowWarningByDispatcher(Window window, IList<string> warning, params string[] arguments)
        {
            return window.Dispatcher.Invoke(() =>
                ShowWarning(window, warning, arguments)
            );
        }

        internal static MessageBoxResult ShowWarningByDispatcher(Window window, string warningMessage, params string[] arguments)
        {
            return ShowWarningByDispatcher(window, new[] { Warnings.DefaultTitle, warningMessage }, arguments);
        }

        internal static MessageBoxResult ShowQuestion(Window window, IList<string> question, params string[] arguments)
        {
            return MessageBox.Show(window, string.Format(Helper.InvariantCulture, question[1], arguments), question[0], MessageBoxButton.YesNo, MessageBoxImage.Question);
        }

        //internal static MessageBoxResult ShowQuestion(Window window, string questionMessage, params string[] arguments)
        //{
        //    return ShowWarning(window, new[] { Questions.DefaultTitle, questionMessage }, arguments);
        //}

        #endregion

        #region Classes

        internal static class Errors
        {
            public const string DefaultTitle = "Error";

            private const string _updateCheckFailed = "Failed to check for updates.";
            public static string UpdateCheckFailed {
                get { return _updateCheckFailed; }
            }

            private const string _inputFileNotFound = "The input file does not exist.";
            public static string InputFileNotFound {
                get { return _inputFileNotFound; }
            }

            private const string _outputFilePathInvalid = "The output file's path is invalid.";
            public static string OutputFilePathInvalid {
                get { return _outputFilePathInvalid; }
            }

            private static readonly string _lameEncoderNotFound = "The LAME encoder was not found at \"" + LameProcess.LamePath + "\"." + Environment.NewLine +
                "Please download it, and then try again.";
            public static string LameEncoderNotFound {
                get { return _lameEncoderNotFound; }
            }
        }

        internal static class Warnings
        {
            public const string DefaultTitle = "Warning";

            private static readonly IList<string> _exitConfirmation = new[] {
                "The encoding is still in progress",
                "Are you sure you want to exit?"
            };
            public static IList<string> ExitConfirmation {
                get { return _exitConfirmation; }
            }

            private const string _outputFileAlreadyExists = "A file already exists at the desired location of the output file. Do you want to overwrite it?";
            public static string OutputFileAlreadyExists {
                get { return _outputFileAlreadyExists; }
            }

            private static readonly string _outputFileIsNotRemovable = "Could not delete the unfinished output file at \"{0}\"." + Environment.NewLine +
                "Do you want to retry?";
            public static string OutputFileIsNotRemovable {
                get { return _outputFileIsNotRemovable; }
            }
        }

        internal static class Questions
        {
            public const string DefaultTitle = "Question";

            private static readonly IList<string> _updateAvailable = new[] {
                "Update available",
                "An update is available (v{0}). Do you want to update now?"
            };
            public static IList<string> UpdateAvailable
            {
                get { return _updateAvailable; }
            }
        }

        #endregion
    }
}
