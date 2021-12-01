using ArcGISRuntime.WinUI.Viewer;
using System.Collections.Generic;
using Windows.Foundation;
using Windows.UI.Popups;

namespace ArcGISRuntime.WinUI
{
    /// <summary>
    /// Wrapper around MessageDialog that simplifies hooking the window to the current view.
    /// See https://github.com/microsoft/microsoft-ui-xaml/issues/4167 for more context
    /// </summary>
    public class MessageDialog2
    {
        private readonly MessageDialog dialog;

        public MessageDialog2(string content) : this(content, string.Empty) { }

        public MessageDialog2(string content, string title)
        {
            dialog = new MessageDialog(content, title);
        }

        public MessageDialogOptions Options
        {
            get => dialog.Options;
            set => dialog.Options = value;
        }

        public IList<IUICommand> Commands => dialog.Commands;

        public IAsyncOperation<IUICommand> ShowAsync()
        {
            WinRT.Interop.InitializeWithWindow.Initialize(dialog, App.CurrentWindowHandle);
            return dialog.ShowAsync();
        }
    }
}
