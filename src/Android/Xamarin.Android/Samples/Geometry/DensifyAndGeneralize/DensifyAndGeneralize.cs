// Copyright 2020 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific 
// language governing permissions and limitations under the License.

using Android.App;
using Android.OS;
using Android.Widget;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.UI;
using Esri.ArcGISRuntime.UI.Controls;
using System.Drawing;
using System.Linq;

namespace ArcGISRuntime.Samples.DensifyAndGeneralize
{
    [Activity (ConfigurationChanges=Android.Content.PM.ConfigChanges.Orientation | Android.Content.PM.ConfigChanges.ScreenSize)]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        name: "Densify and generalize",
        category: "Geometry",
        description: "A multipart geometry can be densified by adding interpolated points at regular intervals. Generalizing multipart geometry simplifies it while preserving its general shape. Densifying a multipart geometry adds more vertices at regular intervals.",
        instructions: "Use the sliders to control the parameters of the densify and generalize methods.",
        tags: new[] { "data", "densify", "generalize", "simplify" })]
    [ArcGISRuntime.Samples.Shared.Attributes.AndroidLayout("DensifyAndGeneralize.axml")]
    public class DensifyAndGeneralize : Activity
    {
        // UI controls.
        private TextView _resultLabel;
        private MapView _myMapView;
        private SeekBar _segmentLengthSlider;
        private SeekBar _deviationSlider;

        // Graphic used to refer to the original geometry.
        private Polyline _originalPolyline;

        // Graphics used to show the densify or generalize result.
        private Graphic _resultPolylineGraphic;
        private Graphic _resultPointGraphic;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            Title = "Densify and generalize";

            // Create the UI, setup the control references and execute initialization.
            CreateLayout();
            Initialize();
        }

        private void Initialize()
        {
            // Create the map with a default basemap.
            _myMapView.Map = new Map(Basemap.CreateLightGrayCanvas());

            // Create and add a graphics overlay.
            GraphicsOverlay overlay = new GraphicsOverlay();
            _myMapView.GraphicsOverlays.Add(overlay);

            // Create the original geometry: some points along a river.
            PointCollection points = CreateShipPoints();

            // Show the original geometry as red dots on the map.
            Multipoint originalMultipoint = new Multipoint(points);
            Graphic originalPointsGraphic = new Graphic(originalMultipoint,
                new SimpleMarkerSymbol(SimpleMarkerSymbolStyle.Circle, Color.Red, 7));
            overlay.Graphics.Add(originalPointsGraphic);

            // Show a dotted red line connecting the original points.
            _originalPolyline = new Polyline(points);
            Graphic originalPolylineGraphic = new Graphic(_originalPolyline,
                new SimpleLineSymbol(SimpleLineSymbolStyle.Dot, Color.Red, 3));
            overlay.Graphics.Add(originalPolylineGraphic);

            // Show the result (densified or generalized) points as magenta dots on the map.
            _resultPointGraphic = new Graphic
            {
                Symbol = new SimpleMarkerSymbol(SimpleMarkerSymbolStyle.Circle, Color.Magenta, 7),
                ZIndex = 999
            };
            overlay.Graphics.Add(_resultPointGraphic);

            // Connect the result points with a magenta polyline.
            _resultPolylineGraphic = new Graphic
            {
                Symbol = new SimpleLineSymbol(SimpleLineSymbolStyle.Solid, Color.Magenta, 3),
                ZIndex = 1000
            };
            overlay.Graphics.Add(_resultPolylineGraphic);

            // Listen for changes in state.
            _segmentLengthSlider.ProgressChanged += (o, e) =>
                UpdateGeometry("Densify", _segmentLengthSlider.Progress + 100, _deviationSlider.Progress + 1);
            _deviationSlider.ProgressChanged += (o, e) =>
                UpdateGeometry("Generalize", _segmentLengthSlider.Progress + 100, _deviationSlider.Progress + 1);

            // Center the map.
            _myMapView.SetViewpointGeometryAsync(_originalPolyline.Extent, 100);
        }

        private void CreateLayout()
        {
            // Load the layout for the sample from the .axml file.
            SetContentView(Resource.Layout.DensifyAndGeneralize);

            // Update control references to point to the controls defined in the layout.
            _myMapView = FindViewById<MapView>(Resource.Id.densifyAndGeneralize_MyMapView);
            _resultLabel = FindViewById<TextView>(Resource.Id.densifyAndGeneralize_ResultLabel);
            _deviationSlider = FindViewById<SeekBar>(Resource.Id.densifyAndGeneralize_deviationSlider);
            _deviationSlider.Max = 249;
            _segmentLengthSlider = FindViewById<SeekBar>(Resource.Id.densifyAndGeneralize_segmentLengthSlider);
            _segmentLengthSlider.Max = 400;
        }

        private void UpdateGeometry(string operation, double segmentLength, double deviation)
        {
            // Start with the original polyline.
            Polyline polyline = _originalPolyline;

            // Apply the selected operation.
            if (operation == "Generalize")
            {
                // Reset the other slider.
                _segmentLengthSlider.Progress = 0;

                polyline = (Polyline) GeometryEngine.Generalize(polyline, deviation, true);

                // Update the result label.
                _resultLabel.Text = $"Operation: Generalize, Deviation: {deviation:f}";
            }
            else
            {
                // Reset the other slider.
                _deviationSlider.Progress = 0;

                polyline = (Polyline) GeometryEngine.Densify(polyline, segmentLength);

                // Update the result label.
                _resultLabel.Text = $"Operation: Densify, Segment length: {segmentLength:f}";
            }

            // Update the graphic geometries to show the results.
            _resultPolylineGraphic.Geometry = polyline;
            _resultPointGraphic.Geometry = new Multipoint(polyline.Parts.SelectMany(m => m.Points));
        }

        private static PointCollection CreateShipPoints()
        {
            return new PointCollection(SpatialReference.Create(32126))
            {
                new MapPoint(2330611.130549, 202360.002957, 0.000000),
                new MapPoint(2330583.834672, 202525.984012, 0.000000),
                new MapPoint(2330574.164902, 202691.488009, 0.000000),
                new MapPoint(2330689.292623, 203170.045888, 0.000000),
                new MapPoint(2330696.773344, 203317.495798, 0.000000),
                new MapPoint(2330691.419723, 203380.917080, 0.000000),
                new MapPoint(2330435.065296, 203816.662457, 0.000000),
                new MapPoint(2330369.500800, 204329.861789, 0.000000),
                new MapPoint(2330400.929891, 204712.129673, 0.000000),
                new MapPoint(2330484.300447, 204927.797132, 0.000000),
                new MapPoint(2330514.469919, 205000.792463, 0.000000),
                new MapPoint(2330638.099138, 205271.601116, 0.000000),
                new MapPoint(2330725.315888, 205631.231308, 0.000000),
                new MapPoint(2330755.640702, 206433.354860, 0.000000),
                new MapPoint(2330680.644719, 206660.240923, 0.000000),
                new MapPoint(2330386.957926, 207340.947204, 0.000000),
                new MapPoint(2330485.861737, 207742.298501, 0.000000)
            };
        }
    }
}