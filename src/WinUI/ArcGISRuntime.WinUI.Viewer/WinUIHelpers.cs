using System;
using System.Collections.Generic;
using System.Net.Mime;
using System.Runtime.InteropServices;
using Windows.ApplicationModel.Appointments.DataProvider;
using Windows.Foundation;
using Windows.UI.Popups;
using WinRT;

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
            var handle = GetActiveWindow();
            if (handle == IntPtr.Zero)
                throw new InvalidOperationException();
            dialog.As<IInitializeWithWindow>().Initialize(handle);
            return dialog.ShowAsync();
        }

        [ComImport]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        [Guid("3E68D4BD-7135-4D10-8018-9FB6D9F33FA1")]
        internal interface IInitializeWithWindow
        {
            void Initialize(IntPtr hwnd);
        }

        [DllImport("user32.dll")]
        private static extern IntPtr GetActiveWindow();
    }
}
