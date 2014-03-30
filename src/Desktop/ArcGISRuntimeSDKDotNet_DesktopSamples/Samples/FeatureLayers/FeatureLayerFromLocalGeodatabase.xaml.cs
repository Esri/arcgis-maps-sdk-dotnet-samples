using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Layers;
using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace ArcGISRuntimeSDKDotNet_DesktopSamples.Samples
{
    /// <summary>
    /// This sample shows how to add a FeatureLayer from a local .geodatabase file to the map. A .geodatabase file may be created using the 'Create Runtime Content' tool in ArcMap.
    /// </summary>
    /// <title>Feature Layer from Local Geodatabase</title>
	/// <category>Layers</category>
	/// <subcategory>Feature Layers</subcategory>
	public partial class FeatureLayerFromLocalGeodatabase : UserControl
    {
        private const string GDB_PATH = @"..\..\..\..\..\samples-data\maps\usa.geodatabase";

        /// <summary>Construct FeatureLayerFromLocalGeodatabase sample control</summary>
        public FeatureLayerFromLocalGeodatabase()
        {
            InitializeComponent();
            var task = CreateFeatureLayersAsync();
        }

        private async Task CreateFeatureLayersAsync()
        {
            try
            {
                var gdb = await Geodatabase.OpenAsync(GDB_PATH);

                Envelope extent = new Envelope();
                foreach (var table in gdb.FeatureTables)
                {
                    var flayer = new FeatureLayer()
                    {
                        ID = table.Name,
                        DisplayName = table.Name,
                        FeatureTable = table
                    };

                    if (table.Extent != null)
                    {
                        if (extent == null)
                            extent = table.Extent;
                        else
                            extent = extent.Union(table.Extent);
                    }

                    mapView.Map.Layers.Add(flayer);
                }

                await mapView.SetViewAsync(extent.Expand(1.10));
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error creating feature layer: " + ex.Message, "Samples");
            }
        }
    }
}
