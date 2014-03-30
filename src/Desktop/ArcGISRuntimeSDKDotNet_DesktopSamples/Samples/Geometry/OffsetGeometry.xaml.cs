using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Layers;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.Tasks.Query;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ArcGISRuntimeSDKDotNet_DesktopSamples.Samples
{
    /// <summary>
    /// Demonstrates how to create an offset geometry using the Offset method of the GeometryEngine class. Here, the user selects a target polygon graphic and the application creates an offset polygon based on the selected graphic.
    /// </summary>
    /// <title>Offset</title>
	/// <category>Geometry</category>
	public partial class OffsetGeometry : UserControl
    {
        private SimpleMarkerSymbol _vertexSymbol;
        private SimpleMarkerSymbol _userPointSymbol;

        /// <summary>Construct Offset sample control</summary>
        public OffsetGeometry()
        {
            InitializeComponent();

            _vertexSymbol = new SimpleMarkerSymbol() { Color = Colors.LightGreen, Size = 8, Style = SimpleMarkerStyle.Circle };
            _userPointSymbol = new SimpleMarkerSymbol() { Color = Colors.Black, Size = 10, Style = SimpleMarkerStyle.Circle };

            mapView.Map.InitialExtent = (Envelope)GeometryEngine.Project(
                new Envelope(-83.3188395774275, 42.61428312652851, -83.31295664068958, 42.61670913269855, SpatialReferences.Wgs84),
                SpatialReferences.WebMercator);

            mapView.ExtentChanged += mapView_ExtentChanged;
        }

        private async void mapView_ExtentChanged(object sender, EventArgs e)
        {
            try
            {
                mapView.ExtentChanged -= mapView_ExtentChanged;

                await LoadParcelsAsync();
                await mapView.LayersLoadedAsync();

                layoutGrid.IsEnabled = true;
                await ProcessUserPointsAsync(false);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading parcels: " + ex.Message, "Offset Geometry Sample");
            }
        }

        // Process user selection and point clicks
        private async Task ProcessUserPointsAsync(bool isTargetSelected)
        {
            try
            {
                if (mapView.Editor.IsActive)
                    mapView.Editor.Cancel.Execute(null);

                while (mapView.Extent != null)
                {
                    await SelectTargetGeometryAsync();
                }
            }
            catch (TaskCanceledException)
            {
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message, "Offset Geometry Sample");
            }
        }

        // Load parcels from map service
        private async Task LoadParcelsAsync()
        {
            var queryTask = new QueryTask(new Uri("http://sampleserver1.arcgisonline.com/ArcGIS/rest/services/TaxParcel/AssessorsParcelCharacteristics/MapServer/1"));
            var query = new Query(mapView.Extent) { ReturnGeometry = true, OutSpatialReference = mapView.SpatialReference, OutFields = new OutFields(new List<string> { "Shape" }) };
            var result = await queryTask.ExecuteAsync(query);

            parcelLayer.Graphics.Clear();
            parcelLayer.Graphics.AddRange(result.FeatureSet.Features);
        }

        // Accept user click point, find a target parcel, and create an offset polygon graphic
        private async Task SelectTargetGeometryAsync()
        {
            Graphic graphic = null;
            while (graphic == null)
            {
                var point = await mapView.Editor.RequestPointAsync();

                graphic = await parcelLayer.HitTestAsync(mapView, mapView.LocationToScreen(point));
                if (graphic == null)
                    continue;

                double distance = (double)(int?)comboSize.SelectedItem;
                if ((bool)checkDirection.IsChecked)
                    distance *= -1;

                OffsetType offsetType = (OffsetType)comboOffsetType.SelectedValue;

                var offset = GeometryEngine.Offset(graphic.Geometry, distance, offsetType, 1.15, 0.0);

                offsetLayer.Graphics.Clear();
                offsetLayer.Graphics.Add(new Graphic(offset));
            }
        }
    }
}
