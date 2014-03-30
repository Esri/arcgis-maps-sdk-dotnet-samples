using Esri.ArcGISRuntime.Layers;
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

namespace ArcGISRuntimeSDKDotNet_StoreSamples.Samples
{
    /// <summary>
    /// 
    /// </summary>
    /// <category>Mapping</category>
    public sealed partial class SwitchBasemaps : Page
    {
        public SwitchBasemaps()
        {
            this.InitializeComponent();
        }

        private void RadioButton_Click(object sender, RoutedEventArgs e)
        {
            mapView1.Map.Layers.RemoveAt(0);

            mapView1.Map.Layers.Add(new ArcGISTiledMapServiceLayer()
            {
                ServiceUri = ((RadioButton)sender).Tag as string
            });
        }
    }
}
