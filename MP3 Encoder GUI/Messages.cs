using System;
using System.Collections.Generic;
using System.Windows;

namespace MP3EncoderGUI
{
    internal static class Messages
    {
        internal static void ShowError(Window window, IList<string> error, params string[] arguments)
        {
            MessageBox.Show(window, AddArguments(error[1], arguments), error[0], MessageBoxButton.OK, MessageBoxImage.Error);

            var mainWindow = window as MainWindow;
            if (mainWindow != null) {
                mainWindow.ButtonStart.Visibility = Visibility.Visible;
                mainWindow.GridProgress.Visibility = Visibility.Hidden;
            }
        }

        internal static void ShowError(Window window, string errorMessage, params string[] arguments)
        {
            ShowError(window, new[] { Errors.DefaultTitle, errorMessage }, arguments);
        }

        internal static MessageBoxResult ShowWarning(Window window, IList<string> warning, params string[] arguments)
        {
            return MessageBox.Show(window, AddArguments(warning[1], arguments), warning[0], MessageBoxButton.YesNo, MessageBoxImage.Warning);
        }

        internal static MessageBoxResult ShowWarning(Window window, string warningMessage, params string[] arguments)
        {
            return ShowWarning(window, new[] { Errors.DefaultTitle, warningMessage }, arguments);
        }

        internal static MessageBoxResult ShowWarningByDispatcher(Window window, IList<string> warning, params string[] arguments)
        {
            return (MessageBoxResult)window.Dispatcher.Invoke(new Func<MessageBoxResult>(() =>
                ShowWarning(window, warning, arguments)
            ));
        }

        internal static MessageBoxResult ShowWarningByDispatcher(Window window, string warningMessage, params string[] arguments)
        {
            return ShowWarningByDispatcher(window, new[] { Warnings.DefaultTitle, warningMessage }, arguments);
        }

        private static string AddArguments(string message, string[] arguments)
        {
            if (arguments.Length != 0) {
                for (var i = arguments.Length - 1; i >= 0; i--) {
                    message = message.Replace("@Arg" + i, arguments[i]);
                }
            }

            return message;
        }
    }

    public static class Errors
    {
        private const string _defaultTitle = "Error";
        internal static string DefaultTitle {
            get { return _defaultTitle; }
        }

        private const string _inputFileNotFound = "The input file does not exist.";
        public static string InputFileNotFound {
            get { return _inputFileNotFound; }
        }

        private const string _outputFilePathInvalid = "The output file's path is invalid.";
        public static string OutputFilePathInvalid {
            get { return _outputFilePathInvalid; }
        }

        private static readonly string _lameEncoderNotFound = "The LAME encoder was not found at \"" + LameProcess.LameLocation + "\"." + Environment.NewLine +
            "Please download it, and then try again.";
        public static string LameEncoderNotFound {
            get { return _lameEncoderNotFound; }
        }
    }

    public static class Warnings
    {
        private const string _defaultTitle = "Warning";
        internal static string DefaultTitle {
            get { return _defaultTitle; }
        }

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

        private static readonly string _outputFileIsNotRemovable = "Could not delete the unfinished output file at \"@Arg0\"." + Environment.NewLine +
            "Do you want to retry?";
        public static string OutputFileIsNotRemovable {
            get { return _outputFileIsNotRemovable; }
        }
    }
}
