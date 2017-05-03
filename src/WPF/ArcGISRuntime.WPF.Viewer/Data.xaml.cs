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
using ArcGISRuntime.Samples.Managers;
using System.Threading;

namespace ArcGISRuntime.WPF.Viewer
{
    /// <summary>
    /// Interaction logic for Data.xaml
    /// </summary>
    public partial class Data : UserControl
    {
        public Data()
        {
            InitializeComponent();
        }

        private async void DownloadData_Click(object sender, RoutedEventArgs e)
        {

            // TODO loading screen Start
            if(SampleManager.Current.SelectedSample.DataItemIds != null)
            {
                foreach (string id in SampleManager.Current.SelectedSample.DataItemIds)
                {
                    await DataManager.GetData(id, SampleManager.Current.SelectedSample.Name);
                }
            }
           
            // TODO loading screen Stop        
        }
    }
}
