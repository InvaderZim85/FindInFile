using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using MahApps.Metro.Controls.Dialogs;
using Serilog;
using ZimLabs.WpfBase.NetCore;

namespace FindInFile.Ui.ViewModel
{
    /// <summary>
    /// Provides the base functions of a view model
    /// </summary>
    internal class ViewModelBase : ObservableObject
    {
        /// <summary>
        /// The instance of the mah apps dialog coordinator
        /// </summary>
        private readonly IDialogCoordinator _dialogCoordinator;

        /// <summary>
        /// Creates a new instance of the <see cref="ViewModelBase"/>
        /// </summary>
        protected ViewModelBase()
        {
            _dialogCoordinator = DialogCoordinator.Instance;
        }

        /// <summary>
        /// Shows a message dialog
        /// </summary>
        /// <param name="title">The title of the dialog</param>
        /// <param name="message">The message of the dialog</param>
        /// <returns>The awaitable task</returns>
        protected async Task ShowMessage(string title, string message)
        {
            await _dialogCoordinator.ShowMessageAsync(this, title, message);
        }

        /// <summary>
        /// Shows a question dialog with two buttons
        /// </summary>
        /// <param name="title">The title of the dialog</param>
        /// <param name="message">The message of the dialog</param>
        /// <param name="okButtonText">The text of the ok button (optional)</param>
        /// <param name="cancelButtonText">The text of the cancel button (optional)</param>
        /// <returns>The dialog result</returns>
        protected async Task<MessageDialogResult> ShowQuestion(string title, string message, string okButtonText = "OK",
            string cancelButtonText = "Cancel")
        {
            var result = await _dialogCoordinator.ShowMessageAsync(this, title, message,
                MessageDialogStyle.AffirmativeAndNegative, new MetroDialogSettings
                {
                    AffirmativeButtonText = okButtonText,
                    NegativeButtonText = cancelButtonText
                });

            return result;
        }

        /// <summary>
        /// Shows an error message
        /// </summary>
        /// <param name="ex">The exception which was thrown</param>
        /// <param name="caller">The caller of the method (auto filled)</param>
        /// <returns>The awaitable task</returns>
        protected async Task ShowError(Exception ex, [CallerMemberName] string caller = "")
        {
            await _dialogCoordinator.ShowMessageAsync(this, "Error", $"An error has occurred: {ex.Message}");
            Log.Error(ex, "An error has occurred in method '{method}'", caller);
        }

        /// <summary>
        /// Shows a progress dialog
        /// </summary>
        /// <param name="title">The title of the dialog</param>
        /// <param name="message">The message of the dialog</param>
        /// <param name="setIndeterminate">true to set the controller to indeterminate, otherwise false (optional)</param>
        /// <param name="isCancelable">the value which indicates if the progress can be cancelled</param>
        /// <returns>The progress controller</returns>
        protected async Task<ProgressDialogController> ShowProgress(string title, string message, bool setIndeterminate = true, bool isCancelable = false)
        {
            var controller = await _dialogCoordinator.ShowProgressAsync(this, title, message, isCancelable);
            if (setIndeterminate)
                controller.SetIndeterminate();

            return controller;
        }

        /// <summary>
        /// Shows a input dialog
        /// </summary>
        /// <param name="title">The title of the dialog</param>
        /// <param name="message">The message of the dialog</param>
        /// <returns>The result of the input</returns>
        protected async Task<string> ShowInput(string title, string message)
        {
            var result = await _dialogCoordinator.ShowInputAsync(this, title, message);

            return result;
        }

        /// <summary>
        /// Copies the given content to the clipboard
        /// </summary>
        /// <param name="content">The content which should be copied</param>
        protected static void CopyToClipboard(string content)
        {
            Clipboard.SetText(content);
        }
    }
}
