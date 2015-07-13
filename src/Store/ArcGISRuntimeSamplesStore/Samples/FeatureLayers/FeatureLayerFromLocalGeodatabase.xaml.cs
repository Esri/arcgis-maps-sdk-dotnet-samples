using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Layers;
using System;
using System.Linq;
using Windows.Storage;
using Windows.UI.Popups;
using Windows.UI.Xaml.Controls;

namespace ArcGISRuntime.Samples.Store.Samples
{
    /// <summary>
    /// Demonstrates adding a Feature Layer from a local geodatabase file to a map.
    /// </summary>
    /// <title>Feature Layer from Local Geodatabase</title>
    /// <category>Feature Layers</category>
    public sealed partial class FeatureLayerFromLocalGeodatabase : Page
    {
        private const string LOCAL_GDB_PATH = @"maps\usa.geodatabase";

        public FeatureLayerFromLocalGeodatabase()
        {
            this.InitializeComponent();

            MyMapView.ExtentChanged += MyMapView_ExtentChanged;
        }

        /// <summary>Add feature tables from a local geodatabase as layers on the map</summary>
        private async void MyMapView_ExtentChanged(object sender, EventArgs e)
        {
            MyMapView.ExtentChanged -= MyMapView_ExtentChanged;

            try
            {
                var gdbFile = await ApplicationData.Current.LocalFolder.TryGetItemAsync(LOCAL_GDB_PATH);
                if (gdbFile == null)
                    throw new Exception("Local geodatabase not found. Please download sample data from 'Sample Data Settings'");

                var gdb = await Geodatabase.OpenAsync(gdbFile.Path);

                Envelope extent = gdb.FeatureTables.First().Extent;
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

                    MyMapView.Map.Layers.Add(flayer);
                }

                if (!extent.IsEmpty)
                    await MyMapView.SetViewAsync(extent.Expand(1.10));
            }
            catch (Exception ex)
            {
                var _x = new MessageDialog(ex.Message, "Sample Error").ShowAsync();
            }
        }
    }
}
