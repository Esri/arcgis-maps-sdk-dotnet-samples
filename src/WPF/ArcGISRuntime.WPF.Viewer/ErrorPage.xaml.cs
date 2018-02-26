using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ArcGISRuntime.WPF.Viewer
{
    /// <summary>
    /// Interaction logic for ErrorPage.xaml
    /// </summary>
    public partial class ErrorPage : UserControl
    {
        public ErrorPage()
        {
            InitializeComponent();
        }

        public ErrorPage(Exception ex)
        {
            InitializeComponent();
            // TODO - do this with binding instead
            txtError.Text = String.Format("{0}\n\nMessage: {1}\n\nStack Trace:\n{2}", ex.ToString(), ex.Message, ex.StackTrace);
        }
    }
}
