// Copyright 2018 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific 
// language governing permissions and limitations under the License.

using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.UI;
using Esri.ArcGISRuntime.UI.Controls;
using Foundation;
using System;
using System.Drawing;
using System.Linq;
using CoreGraphics;
using UIKit;

namespace ArcGISRuntime.Samples.DensifyAndGeneralize
{
    [Register("DensifyAndGeneralize")]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Densify and generalize",
        "GeometryEngine",
        "This sample demonstrates how to densify or generalize a polyline geometry. In this example, points representing a ship's location are shown at irregular intervals. You can densify the polyline to interpolate points along the line at regular intervals. Generalizing the polyline can also simplify the geometry while preserving its general shape.",
        "Use the sliders to adjust the max deviation (for generalize) and the max segment length (for densify). The results will update automatically.",
        "Featured")]
    public class DensifyAndGeneralize : UIViewController
    {
        // UI controls.
        private MapView _myMapView;
        private UISlider _segmentLengthSlider;
        private UISlider _deviationSlider;
        private UILabel _segmentLengthLabel;
        private UILabel _deviationLabel;
        private UILabel _resultLabel;
        private UIToolbar _toolbar = new UIToolbar();

        // Graphic used to refer to the original geometry.
        private Polyline _originalPolyline;

        // Graphics used to show the densify or generalize result.
        private Graphic _resultPolylineGraphic;
        private Graphic _resultPointGraphic;

        public DensifyAndGeneralize()
        {
            Title = "Densify and generalize";
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            // Create the UI, setup the control references and execute initialization. 
            CreateLayout();
            Initialize();
        }

        public override void ViewDidLayoutSubviews()
        {
            nfloat topMargin = NavigationController.NavigationBar.Frame.Height + UIApplication.SharedApplication.StatusBarFrame.Height;
            nfloat controlHeight = 30;
            nfloat margin = 5;
            nfloat frameHeight = 3 * controlHeight + 4 * margin;
            nfloat colSplit = View.Bounds.Width * 2 / 3;
            _myMapView.Frame = new CGRect(0, 0, View.Bounds.Width, View.Bounds.Height);
            _myMapView.ViewInsets = new UIEdgeInsets(topMargin, 0, controlHeight * 3 + margin * 4, 0);

            // Place the toolbars.

            _toolbar.Frame = new CGRect(0, View.Bounds.Height - frameHeight, View.Bounds.Width, frameHeight);

            // Place the labels.
            _resultLabel.Frame = new CGRect(margin, View.Bounds.Height - 1 * controlHeight - margin,
                View.Bounds.Width - 2 * margin, controlHeight);
            _segmentLengthLabel.Frame = new CGRect(margin, View.Bounds.Height - 2 * controlHeight - 2 * margin,
                colSplit - 2 * margin, controlHeight);
            _deviationLabel.Frame = new CGRect(margin, View.Bounds.Height - 3 * controlHeight - 3 * margin,
                colSplit - 2 * margin, controlHeight);

            // Place the sliders.
            _segmentLengthSlider.Frame = new CGRect(colSplit + margin,
                View.Bounds.Height - 2 * controlHeight - 2 * margin, View.Bounds.Width * 1 / 3 - margin * 2,
                controlHeight);
            _deviationSlider.Frame = new CGRect(colSplit + margin, View.Bounds.Height - 3 * controlHeight - 3 * margin, View.Bounds.Width * 1 / 3- margin * 2, controlHeight);

            base.ViewDidLayoutSubviews();
        }

        private void Initialize()
        {
            // Create the map with a default basemap.
            _myMapView.Map = new Map(Basemap.CreateStreetsNightVector());

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
            _segmentLengthSlider.ValueChanged += (o, e) =>
                UpdateGeometry("Densify", _segmentLengthSlider.Value, _deviationSlider.Value);
            _deviationSlider.ValueChanged += (o, e) =>
                UpdateGeometry("Generalize", _segmentLengthSlider.Value, _deviationSlider.Value);

            // Center the map.
            _myMapView.SetViewpointGeometryAsync(_originalPolyline.Extent, 100);
        }

        private void UpdateGeometry(string operation, double segmentLength, double deviation)
        {
            // Start with the original polyline.
            Polyline polyline = _originalPolyline;

            // Apply the selected operation.
            if (operation == "Generalize")
            {
                polyline = (Polyline) GeometryEngine.Generalize(polyline, deviation, true);

                // Update the result label.
                _resultLabel.Text = $"Generalize - Deviation: {deviation:f}";
            }
            else
            {
                polyline = (Polyline) GeometryEngine.Densify(polyline, segmentLength);

                // Update the result label.
                _resultLabel.Text = $"Densify - Segment length: {segmentLength:f}";
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

        private void CreateLayout()
        {
            // Create the controls.
            _myMapView = new MapView();
            _deviationLabel = new UILabel();
            _segmentLengthLabel = new UILabel();
            _resultLabel = new UILabel();
            _segmentLengthSlider = new UISlider
            {
                MinValue = 100,
                MaxValue = 500
            };
            _deviationSlider = new UISlider
            {
                MinValue = 1,
                MaxValue = 250
            };

            // Configure the text.
            _deviationLabel.Text = "Max. Deviation: ";
            _segmentLengthLabel.Text = "Max. Segment length:";
            _resultLabel.Text = "Adjust a slider to start";
            _deviationLabel.TextAlignment = UITextAlignment.Right;
            _segmentLengthLabel.TextAlignment = UITextAlignment.Right;
            _resultLabel.TextAlignment = UITextAlignment.Center;
            _resultLabel.AdjustsFontSizeToFitWidth = true;

            // Add the MapView and other controls to the page.
            View.AddSubviews(_myMapView, _toolbar, _segmentLengthLabel,
                _deviationLabel, _deviationSlider, _segmentLengthSlider, _deviationLabel, _resultLabel);
        }
    }
}