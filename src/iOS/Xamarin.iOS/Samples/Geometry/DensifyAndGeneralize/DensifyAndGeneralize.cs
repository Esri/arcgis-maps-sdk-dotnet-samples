// Copyright 2020 Esri.
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
using UIKit;

namespace ArcGISRuntime.Samples.DensifyAndGeneralize
{
    [Register("DensifyAndGeneralize")]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        name: "Densify and generalize",
        category: "Geometry",
        description: "A multipart geometry can be densified by adding interpolated points at regular intervals. Generalizing multipart geometry simplifies it while preserving its general shape. Densifying a multipart geometry adds more vertices at regular intervals.",
        instructions: "Use the sliders to control the parameters of the densify and generalize methods.",
        tags: new[] { "data", "densify", "generalize", "simplify" })]
    public class DensifyAndGeneralize : UIViewController
    {
        // Hold references to UI controls.
        private MapView _myMapView;
        private UISegmentedControl _operationPicker;
        private UISlider _slider;
        private UILabel _resultLabel;

        // Graphic used to refer to the original geometry.
        private Polyline _originalPolyline;

        // Graphics used to show the densify or generalize result.
        private Graphic _resultPolylineGraphic;
        private Graphic _resultPointGraphic;

        public DensifyAndGeneralize()
        {
            Title = "Densify and generalize";
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

            // Center the map.
            _myMapView.SetViewpointGeometryAsync(_originalPolyline.Extent, 100);
        }

        private void OnUIChanged(object sender, EventArgs e)
        {
            // Start with the original polyline.
            Polyline polyline = _originalPolyline;

            // Reset the slider if the operation is changed.
            if (sender is UISegmentedControl) _slider.Value = 100;

            // Apply the selected operation.
            if (_operationPicker.SelectedSegment == 0)
            {
                polyline = (Polyline)GeometryEngine.Densify(polyline, _slider.Value);

                // Update the result label.
                _resultLabel.Text = $"Densify - Segment length: {_slider.Value:f}";
            }
            else
            {
                polyline = (Polyline)GeometryEngine.Generalize(polyline, _slider.Value, true);

                // Update the result label.
                _resultLabel.Text = $"Generalize - Deviation: {_slider.Value:f}";
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

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            Initialize();
        }

        public override void LoadView()
        {
            // Create the views.
            View = new UIView { BackgroundColor = UIColor.White };

            _myMapView = new MapView();
            _myMapView.TranslatesAutoresizingMaskIntoConstraints = false;

            _resultLabel = new UILabel
            {
                Text = "Adjust the slider to start.",
                AdjustsFontSizeToFitWidth = true,
                TextAlignment = UITextAlignment.Center,
                BackgroundColor = UIColor.FromWhiteAlpha(0, .6f),
                TextColor = UIColor.White,
                Lines = 1,
                TranslatesAutoresizingMaskIntoConstraints = false
            };

            _operationPicker = new UISegmentedControl("Densify", "Generalize");
            _operationPicker.TranslatesAutoresizingMaskIntoConstraints = false;
            _operationPicker.SelectedSegment = 0;

            _slider = new UISlider();
            _slider.TranslatesAutoresizingMaskIntoConstraints = false;
            _slider.MinValue = 100;
            _slider.MaxValue = 500;

            UIToolbar toolbar = new UIToolbar();
            toolbar.TranslatesAutoresizingMaskIntoConstraints = false;
            toolbar.Items = new[]
            {
                new UIBarButtonItem {CustomView = _operationPicker},
                new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace),
                new UIBarButtonItem {CustomView = _slider, Width = 150}
            };

            // Add the views.
            View.AddSubviews(_myMapView, toolbar, _resultLabel);

            // Lay out the views.
            NSLayoutConstraint.ActivateConstraints(new[]
            {
                _myMapView.TopAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.TopAnchor),
                _myMapView.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor),
                _myMapView.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor),
                _myMapView.BottomAnchor.ConstraintEqualTo(toolbar.TopAnchor),

                toolbar.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor),
                toolbar.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor),
                toolbar.BottomAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.BottomAnchor),

                _resultLabel.TopAnchor.ConstraintEqualTo(_myMapView.TopAnchor),
                _resultLabel.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor),
                _resultLabel.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor),
                _resultLabel.HeightAnchor.ConstraintEqualTo(40)
            });
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            // Subscribe to events.
            _slider.ValueChanged += OnUIChanged;
            _operationPicker.ValueChanged += OnUIChanged;
        }

        public override void ViewDidDisappear(bool animated)
        {
            base.ViewDidDisappear(animated);

            // Unsubscribe from events, per best practice.
            _slider.ValueChanged -= OnUIChanged;
            _operationPicker.ValueChanged -= OnUIChanged;
        }
    }
}