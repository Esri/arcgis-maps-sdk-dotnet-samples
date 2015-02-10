using Esri.ArcGISRuntime.Controls;
using Esri.ArcGISRuntime.Layers;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.Tasks.Geoprocessing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows;
using System.Windows.Controls;

namespace ArcGISRuntime.Samples.Desktop
{
    /// <summary>
    /// This sample demonstrates use of the Geoprocessor to call a DriveTimes geoprocessing service. To use the sample, click a point in the map. Drive time polygons of 1, 2, and 3 minutes will be calculated and displayed on the map.
    /// </summary>
    /// <title>Drive Times</title>
    /// <category>Tasks</category>
    /// <subcategory>Geoprocessing</subcategory>
    public partial class DriveTimes : UserControl
    {
        private const string DriveTimeServiceUrl =
            "http://sampleserver1.arcgisonline.com/ArcGIS/rest/services/Network/ESRI_DriveTime_US/GPServer/CreateDriveTimePolygons";

        private GraphicsOverlay _resultsOverlay;
        private GraphicsOverlay _inputOverlay;
        private List<Symbol> _bufferSymbols;
        private Geoprocessor _gpTask;
        private CancellationTokenSource _cancellationTokenSource;


        /// <summary>Constructs Drive Times sample control</summary>
        public DriveTimes()
        {
            InitializeComponent();

            _resultsOverlay = MyMapView.GraphicsOverlays["resultsOverlay"];
            _inputOverlay = MyMapView.GraphicsOverlays["inputOverlay"];

            _bufferSymbols = new List<Symbol>()
            {
                layoutGrid.Resources["FillSymbol1"] as Symbol, 
                layoutGrid.Resources["FillSymbol2"] as Symbol, 
                layoutGrid.Resources["FillSymbol3"] as Symbol
            };

            _gpTask = new Geoprocessor(new Uri(DriveTimeServiceUrl));
        }

        // Use geoprocessor to call drive times gp service and display results
        private async void MyMapView_MapViewTapped(object sender, MapViewInputEventArgs e)
        {
            try
            {
               
                progress.Visibility = Visibility.Visible;

                // If the _gpTask has completed successfully (or before it is run the first time), the _cancellationTokenSource is set to null. 
                // If it has not completed and the map is clicked/tapped again, set the _cancellationTokenSource to cancel the initial task. 
                if (_cancellationTokenSource != null)
                {
                    _cancellationTokenSource.Cancel(); // Cancel previous operation
                }

                _cancellationTokenSource = new CancellationTokenSource();

                _inputOverlay.Graphics.Clear();
                _resultsOverlay.Graphics.Clear();

                _inputOverlay.Graphics.Add(new Graphic(e.Location));

                var parameter = new GPInputParameter();
                parameter.GPParameters.Add(new GPFeatureRecordSetLayer("Input_Location", e.Location));
                parameter.GPParameters.Add(new GPString("Drive_Times", "1 2 3"));

                var result = await _gpTask.ExecuteAsync(parameter, _cancellationTokenSource.Token);

                _cancellationTokenSource = null; // null out to show there are no pending operations

                if (result != null)
                {
                    var features = result.OutParameters.OfType<GPFeatureRecordSetLayer>().First().FeatureSet.Features;
                    _resultsOverlay.Graphics.AddRange(features.Select((fs, idx) => new Graphic(fs.Geometry, _bufferSymbols[idx])));
                }
            }
            catch (OperationCanceledException)
            {
                // Catch this exception because it is expected (when task cancellation is successful).
                // Catching it here prevents the message from being propagated to a MessageBox. 

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Sample Error");
            }
            finally
            {
                progress.Visibility = Visibility.Collapsed;
            }
        }
    }
}
