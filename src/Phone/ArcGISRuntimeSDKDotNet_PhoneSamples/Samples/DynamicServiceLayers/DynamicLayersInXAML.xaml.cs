using Esri.ArcGISRuntime.Controls;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Layers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
    /// <category>Dynamic Service Layers</category>
	public sealed partial class DynamicLayersInXAML : Page
    {
        public DynamicLayersInXAML()
        {
            this.InitializeComponent();
			mapView1.Map.InitialViewpoint = new Viewpoint(new Envelope(
				-3170138,
				-1823795, 
				2850785, 
				1766663, 
				SpatialReference.Create(102009)));

           (mapView1.Map.Layers["USA"] as ArcGISDynamicMapServiceLayer).VisibleLayers = new System.Collections.ObjectModel.ObservableCollection<int> { 0, 2, 4 };
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.  The Parameter
        /// property is typically used to configure the page.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
        }
    }
}
