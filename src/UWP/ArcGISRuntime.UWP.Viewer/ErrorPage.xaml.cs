using System;

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
            ErrorText.Text = ex.ToString();
        }
    }
}