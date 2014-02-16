using System;
using System.Collections.Generic;
using System.Windows;

namespace MP3EncoderGUI
{
    public static class Messages
    {
        #region Methods

        #region Error

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

        internal static void ShowErrorByDispatcher(Window window, IList<string> error, params string[] arguments)
        {
            window.Dispatcher.Invoke(() =>
                ShowError(window, error, arguments)
            );
        }

        internal static void ShowErrorByDispatcher(Window window, string errorMessage, params string[] arguments)
        {
            ShowErrorByDispatcher(window, new[] { Warnings.DefaultTitle, errorMessage }, arguments);
        }

        #endregion

        #region Warning

        internal static MessageBoxResult ShowWarning(Window window, IList<string> warning, params string[] arguments)
        {
            return MessageBox.Show(window, string.Format(Helper.InvariantCulture, warning[1], arguments), warning[0], MessageBoxButton.YesNo, MessageBoxImage.Warning);
        }

        internal static MessageBoxResult ShowWarning(Window window, string warningMessage, params string[] arguments)
        {
            return ShowWarning(window, new[] { Errors.DefaultTitle, warningMessage }, arguments);
        }

        //internal static MessageBoxResult ShowWarningByDispatcher(Window window, IList<string> warning, params string[] arguments)
        //{
        //    return window.Dispatcher.Invoke(() =>
        //        ShowWarning(window, warning, arguments)
        //    );
        //}

        //internal static MessageBoxResult ShowWarningByDispatcher(Window window, string warningMessage, params string[] arguments)
        //{
        //    return ShowWarningByDispatcher(window, new[] { Warnings.DefaultTitle, warningMessage }, arguments);
        //}

        #endregion

        #region Question

        internal static MessageBoxResult ShowQuestion(Window window, IList<string> question, params string[] arguments)
        {
            return MessageBox.Show(window, string.Format(Helper.InvariantCulture, question[1], arguments), question[0], MessageBoxButton.YesNo, MessageBoxImage.Question);
        }

        //internal static MessageBoxResult ShowQuestion(Window window, string questionMessage, params string[] arguments)
        //{
        //    return ShowWarning(window, new[] { Questions.DefaultTitle, questionMessage }, arguments);
        //}

        internal static MessageBoxResult ShowQuestionByDispatcher(Window window, IList<string> question, params string[] arguments)
        {
            return window.Dispatcher.Invoke(() =>
                ShowQuestion(window, question, arguments)
            );
        }

        //internal static MessageBoxResult ShowQuestionByDispatcher(Window window, string questionMessage, params string[] arguments)
        //{
        //    return ShowQuestionByDispatcher(window, new[] { Warnings.DefaultTitle, questionMessage }, arguments);
        //}

        #endregion

        #endregion

        #region Classes

        internal static class Errors
        {
            public const string DefaultTitle = "Error";

            private const string _netFramework45NotFound = "Microsoft .NET Framework 4.5 could not be found on your system. Please download and install the latest version of the Framework, and then try launching the application again.";
            public static string NetFramework45NotFound {
                get { return _netFramework45NotFound; }
            }

            private const string _updateCheckFailed = "Failed to check for updates.";
            public static string UpdateCheckFailed {
                get { return _updateCheckFailed; }
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
