using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace ArcGISRuntime.UWP.Viewer
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class ErrorPage : Page
    {
        public ErrorPage()
        {
            this.InitializeComponent();
        }

        public ErrorPage(Exception ex)
        {
            InitializeComponent();

            // TODO - do this with binding instead
            txtError.Text = String.Format("{0}\n\nMessage: {1}\n\nStack Trace:\n{2}", ex.ToString(), ex.Message, ex.StackTrace);
        }
    }
}
