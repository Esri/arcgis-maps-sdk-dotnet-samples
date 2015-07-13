using Esri.ArcGISRuntime.Layers;
using Esri.ArcGISRuntime.LocalServices;
using System;
using System.Windows;
using System.Windows.Controls;

namespace ArcGISRuntime.Samples.Desktop
{
    /// <summary>
    /// Demonstrates creating an ArcGISDynamicMapServiceLayer which references a LocalMapService.
    /// </summary>
    /// <title>ArcGIS Local Dynamic Map Service Layer</title>
	/// <category>Layers</category>
	/// <subcategory>Dynamic Service Layers</subcategory>
	/// <requiresLocalServer>true</requiresLocalServer>
	public partial class ArcGISDynamicMapServiceLayerLocalSample : UserControl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ArcGISDynamicMapServiceLayerLocalSample"/> class.
        /// </summary>
        public ArcGISDynamicMapServiceLayerLocalSample()
        {
            InitializeComponent();
            CreateLocalServiceAndDynamicLayer();
        }

        public async void CreateLocalServiceAndDynamicLayer() 
        {
            try
            {
				LocalMapService localMapService = new LocalMapService(@"..\..\..\samples-data\maps\water-distribution-network.mpk");
                await localMapService.StartAsync();

                ArcGISDynamicMapServiceLayer arcGISDynamicMapServiceLayer = new ArcGISDynamicMapServiceLayer() 
                {
                    ID = "arcGISDynamicMapServiceLayer",
                    ServiceUri = localMapService.UrlMapService,
                };

				MyMapView.Map.Layers.Add(arcGISDynamicMapServiceLayer);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }
    }
}
