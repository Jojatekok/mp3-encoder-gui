using System;
using System.Windows;

namespace MP3EncoderGUI
{
    public static class Messages
    {
        #region Error strings

        public const string DefaultErrorTitle = "Error";

        public const string InputFileNotFound = "The input file does not exist.";
        public const string OutputFilePathInvalid = "The output file's path is invalid.";

        public static readonly string LameEncoderNotFound = "The LAME encoder was not found at \"" + MainWindow.LameEncoderLocation + "\"." + Environment.NewLine +
            "Please download it, and then try again.";

        #endregion

        #region Warning strings

        public const string DefaultWarningTitle = "Warning";

        public const string ExitConfirmationTitle = "The encoding is still in progress";
        public const string ExitConfirmationMessage = "Are you sure you want to exit?";

        public const string OutputFileAlreadyExists = "A file already exists at the desired location of the output file. Do you want to overwrite it?";
        public static readonly string OutputFileIsNotRemovable = "Could not delete the unfinished output file at \"@Arg1\"." + Environment.NewLine +
            "Do you want to retry?";

        #endregion

        #region Methods

        public static void ShowError(MainWindow window, string message, string title = DefaultErrorTitle, params string[] arguments)
        {
            MessageBox.Show(window, AddArguments(message, arguments), title, MessageBoxButton.OK, MessageBoxImage.Error);
            window.ButtonStart.Visibility = Visibility.Visible;
            window.GridProgress.Visibility = Visibility.Hidden;
        }

        public static MessageBoxResult ShowWarning(MainWindow window, string message, string title = DefaultWarningTitle, params string[] arguments)
        {
            return MessageBox.Show(window, AddArguments(message, arguments), title, MessageBoxButton.YesNo, MessageBoxImage.Warning);
        }

        public static MessageBoxResult ShowWarningByDispatcher(MainWindow window, string message, string title = DefaultWarningTitle, params string[] arguments)
        {
            return (MessageBoxResult)window.Dispatcher.Invoke(new Func<MessageBoxResult>(() =>
                ShowWarning(window, message, title, arguments)
            ));
        }

        private static string AddArguments(string message, string[] arguments)
        {
            if (arguments.Length != 0) {
                for (var i = arguments.Length - 1; i >= 0; i--) {
                    message = message.Replace("@Arg" + (i + 1).ToString(Helper.InvariantCulture), arguments[i]);
                }
            }

            return message;
        }

        #endregion
    }
}
