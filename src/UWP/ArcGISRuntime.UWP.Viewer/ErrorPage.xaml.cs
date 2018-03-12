using System;
using Windows.UI.Xaml.Controls;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace ArcGISRuntime.UWP.Viewer
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ErrorPage
    {
        public ErrorPage()
        {
            InitializeComponent();
        }

        public ErrorPage(Exception ex)
        {
            InitializeComponent();

            // TODO - do this with binding instead
            ErrorText.Text = string.Format("{0}\n\nMessage: {1}\n\nStack Trace:\n{2}", ex.ToString(), ex.Message, ex.StackTrace);
        }
    }
}