using Esri.ArcGISRuntime.Controls;
using Esri.ArcGISRuntime.Layers;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.Tasks.Geoprocessing;
using System;
using System.Collections.Generic;
using System.Linq;
using Windows.UI.Popups;
using Windows.UI.Xaml;

namespace ArcGISRuntime.Samples.Store.Samples
{
    /// <summary>
    /// This sample demonstrates use of the Geoprocessor to call a DriveTimes geoprocessing service. To use the sample, click a point in the map. Drive time polygons of 1, 2, and 3 minutes will be calculated and displayed on the map.
    /// </summary>
    /// <title>Drive Times</title>
    /// <category>Geoprocessing Tasks</category>
    public partial class DriveTimes : Windows.UI.Xaml.Controls.Page
    {
        private const string DriveTimeServiceUrl =
            "http://sampleserver1.arcgisonline.com/ArcGIS/rest/services/Network/ESRI_DriveTime_US/GPServer/CreateDriveTimePolygons";

        private GraphicsOverlay _resultsOverlay;
        private GraphicsOverlay _inputOverlay;
        private List<Symbol> _bufferSymbols;
        private Geoprocessor _gpTask;

        /// <summary>Constructs Drive Times sample control</summary>
        public DriveTimes()
        {
            InitializeComponent();

			_resultsOverlay = MyMapView.GraphicsOverlays["resultsOverlay"];
			_inputOverlay = MyMapView.GraphicsOverlays["inputOverlay"];
                
            _bufferSymbols = new List<Symbol>()
            {
                LayoutRoot.Resources["FillSymbol1"] as Symbol, 
                LayoutRoot.Resources["FillSymbol2"] as Symbol, 
                LayoutRoot.Resources["FillSymbol3"] as Symbol
            };

            _gpTask = new Geoprocessor(new Uri(DriveTimeServiceUrl));
        }

        // Use geoprocessor to call drive times gp service and display results
        private async void MyMapView_MapViewTapped(object sender, MapViewInputEventArgs e)
        {
            try
            {
                progress.Visibility = Visibility.Visible;

                _inputOverlay.Graphics.Clear();

                _inputOverlay.Graphics.Add(new Graphic(e.Location));

                var parameter = new GPInputParameter();
                parameter.GPParameters.Add(new GPFeatureRecordSetLayer("Input_Location", e.Location));
                parameter.GPParameters.Add(new GPString("Drive_Times", "1 2 3"));

                var result = await _gpTask.ExecuteAsync(parameter);

                var features = result.OutParameters.OfType<GPFeatureRecordSetLayer>().First().FeatureSet.Features;

                _resultsOverlay.Graphics.Clear();

                _resultsOverlay.Graphics.AddRange(features.Select((fs, idx) => new Graphic(fs.Geometry, _bufferSymbols[idx])));
            }
            catch (Exception ex)
            {
                var _x = new MessageDialog(ex.Message, "Sample Error").ShowAsync();
            }
            finally
            {
                progress.Visibility = Visibility.Collapsed;
            }
        }
    }
}