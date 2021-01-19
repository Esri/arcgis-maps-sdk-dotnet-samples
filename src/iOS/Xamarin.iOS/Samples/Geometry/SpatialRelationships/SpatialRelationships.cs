// Copyright 2018 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.UI;
using Esri.ArcGISRuntime.UI.Controls;
using Foundation;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using UIKit;

namespace ArcGISRuntime.Samples.ListTransformations
{
    [Register("SpatialRelationships")]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        name: "Spatial relationships",
        category: "Geometry",
        description: "Determine spatial relationships between two geometries.",
        instructions: "Select one of the three graphics. The tree view will list the relationships the selected graphic has to the other graphic geometries.",
        tags: new[] { "geometries", "relationship", "spatial analysis" })]
    public class SpatialRelationships : UIViewController
    {
        // Hold references to UI controls.
        private MapView _myMapView;
        private UITextView _resultTextView;
        private UIStackView _stackView;

        // References to the graphics and graphics overlay.
        private GraphicsOverlay _graphicsOverlay;
        private Graphic _polygonGraphic;
        private Graphic _polylineGraphic;
        private Graphic _pointGraphic;

        public SpatialRelationships()
        {
            Title = "Spatial relationships";
        }

        private async void Initialize()
        {
            // Configure the basemap.
            _myMapView.Map = new Map(BasemapStyle.ArcGISTopographic);

            // Create the graphics overlay.
            _graphicsOverlay = new GraphicsOverlay();

            // Add the overlay to the MapView.
            _myMapView.GraphicsOverlays.Add(_graphicsOverlay);

            // Update the selection color.
            _myMapView.SelectionProperties.Color = Color.Yellow;

            // Create the point collection that defines the polygon.
            PointCollection polygonPoints = new PointCollection(SpatialReferences.WebMercator)
            {
                new MapPoint(-5991501.677830, 5599295.131468),
                new MapPoint(-6928550.398185, 2087936.739807),
                new MapPoint(-3149463.800709, 1840803.011362),
                new MapPoint(-1563689.043184, 3714900.452072),
                new MapPoint(-3180355.516764, 5619889.608838)
            };

            // Create the polygon.
            Polygon polygonGeometry = new Polygon(polygonPoints);

            // Define the symbology of the polygon.
            SimpleLineSymbol polygonOutlineSymbol = new SimpleLineSymbol(SimpleLineSymbolStyle.Solid, System.Drawing.Color.Green, 2);
            SimpleFillSymbol polygonFillSymbol = new SimpleFillSymbol(SimpleFillSymbolStyle.ForwardDiagonal, System.Drawing.Color.Green, polygonOutlineSymbol);

            // Create the polygon graphic and add it to the graphics overlay.
            _polygonGraphic = new Graphic(polygonGeometry, polygonFillSymbol);
            _graphicsOverlay.Graphics.Add(_polygonGraphic);

            // Create the point collection that defines the polyline.
            PointCollection polylinePoints = new PointCollection(SpatialReferences.WebMercator)
            {
                new MapPoint(-4354240.726880, -609939.795721),
                new MapPoint(-3427489.245210, 2139422.933233),
                new MapPoint(-2109442.693501, 4301843.057130),
                new MapPoint(-1810822.771630, 7205664.366363)
            };

            // Create the polyline.
            Polyline polylineGeometry = new Polyline(polylinePoints);

            // Create the polyline graphic and add it to the graphics overlay.
            _polylineGraphic = new Graphic(polylineGeometry, new SimpleLineSymbol(SimpleLineSymbolStyle.Dash, System.Drawing.Color.Red, 4));
            _graphicsOverlay.Graphics.Add(_polylineGraphic);

            // Create the point geometry that defines the point graphic.
            MapPoint pointGeometry = new MapPoint(-4487263.495911, 3699176.480377, SpatialReferences.WebMercator);

            // Define the symbology for the point.
            SimpleMarkerSymbol locationMarker = new SimpleMarkerSymbol(SimpleMarkerSymbolStyle.Circle, System.Drawing.Color.Blue, 10);

            // Create the point graphic and add it to the graphics overlay.
            _pointGraphic = new Graphic(pointGeometry, locationMarker);
            _graphicsOverlay.Graphics.Add(_pointGraphic);

            try
            {
                // Set the viewpoint to center on the point.
                await _myMapView.SetViewpointAsync(new Viewpoint(pointGeometry, 200000000));
            }
            catch (Exception e)
            {
                new UIAlertView("Error", e.ToString(), (IUIAlertViewDelegate)null, "OK", null).Show();
            }
        }

        private async void MyMapView_GeoViewTapped(object sender, GeoViewInputEventArgs e)
        {
            // Identify the tapped graphics.
            IdentifyGraphicsOverlayResult result = null;

            try
            {
                result = await _myMapView.IdentifyGraphicsOverlayAsync(_graphicsOverlay, e.Position, 5, false);
            }
            catch (Exception ex)
            {
                new UIAlertView("Error", ex.ToString(), (IUIAlertViewDelegate)null, "OK", null).Show();
            }

            // Return if there are no results.
            if (result == null || result.Graphics.Count < 1)
            {
                return;
            }

            // Get the smallest identified graphic.
            Graphic identifiedGraphic = result.Graphics.OrderBy(graphic => GeometryEngine.Area(graphic.Geometry)).First();

            // Clear any existing selection, then select the tapped graphic.
            _graphicsOverlay.ClearSelection();
            identifiedGraphic.IsSelected = true;

            // Get the selected graphic's geometry.
            Geometry selectedGeometry = identifiedGraphic.Geometry;

            // Perform the calculation and show the results.
            _resultTextView.Text = GetOutputText(selectedGeometry);
        }

        private string GetOutputText(Geometry selectedGeometry)
        {
            string output = "";

            // Get the relationships.
            List<SpatialRelationship> polygonRelationships = GetSpatialRelationships(selectedGeometry, _polygonGraphic.Geometry);
            List<SpatialRelationship> polylineRelationships = GetSpatialRelationships(selectedGeometry, _polylineGraphic.Geometry);
            List<SpatialRelationship> pointRelationships = GetSpatialRelationships(selectedGeometry, _pointGraphic.Geometry);

            // Add the point relationships to the output.
            if (selectedGeometry.GeometryType != GeometryType.Point)
            {
                output += "Point:\n";
                foreach (SpatialRelationship relationship in pointRelationships)
                {
                    output += $"\t{relationship}\n";
                }
            }

            // Add the polygon relationships to the output.
            if (selectedGeometry.GeometryType != GeometryType.Polygon)
            {
                output += "Polygon:\n";
                foreach (SpatialRelationship relationship in polygonRelationships)
                {
                    output += $"\t{relationship}\n";
                }
            }

            // Add the polyline relationships to the output.
            if (selectedGeometry.GeometryType != GeometryType.Polyline)
            {
                output += "Polyline:\n";
                foreach (SpatialRelationship relationship in polylineRelationships)
                {
                    output += $"\t{relationship}\n";
                }
            }

            return output;
        }

        /// <summary>
        /// Returns a list of spatial relationships between two geometries.
        /// </summary>
        /// <param name="a">The 'a' in "a contains b".</param>
        /// <param name="b">The 'b' in "a contains b".</param>
        /// <returns>A list of spatial relationships that are true for a and b.</returns>
        private static List<SpatialRelationship> GetSpatialRelationships(Geometry a, Geometry b)
        {
            List<SpatialRelationship> relationships = new List<SpatialRelationship>();
            if (GeometryEngine.Crosses(a, b))
            {
                relationships.Add(SpatialRelationship.Crosses);
            }

            if (GeometryEngine.Contains(a, b))
            {
                relationships.Add(SpatialRelationship.Contains);
            }

            if (GeometryEngine.Disjoint(a, b))
            {
                relationships.Add(SpatialRelationship.Disjoint);
            }

            if (GeometryEngine.Intersects(a, b))
            {
                relationships.Add(SpatialRelationship.Intersects);
            }

            if (GeometryEngine.Overlaps(a, b))
            {
                relationships.Add(SpatialRelationship.Overlaps);
            }

            if (GeometryEngine.Touches(a, b))
            {
                relationships.Add(SpatialRelationship.Touches);
            }

            if (GeometryEngine.Within(a, b))
            {
                relationships.Add(SpatialRelationship.Within);
            }

            return relationships;
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            Initialize();
        }

        public override void LoadView()
        {
            // Create the views.
            View = new UIView() { BackgroundColor = ApplicationTheme.BackgroundColor };

            _myMapView = new MapView();
            _myMapView.TranslatesAutoresizingMaskIntoConstraints = false;

            _resultTextView = new UITextView
            {
                TextColor = ApplicationTheme.ForegroundColor,
                Text = "Tap a shape to see its relationship with the others.",
                Editable = false,
                ScrollEnabled = false,
            };

            _stackView = new UIStackView(new UIView[] { _myMapView, _resultTextView });
            _stackView.Distribution = UIStackViewDistribution.FillEqually;
            _stackView.TranslatesAutoresizingMaskIntoConstraints = false;
            _stackView.Axis = UILayoutConstraintAxis.Vertical;

            // Add the views.
            View.AddSubview(_stackView);

            // Lay out the views.
            NSLayoutConstraint.ActivateConstraints(new[]
            {
                _stackView.TopAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.TopAnchor),
                _stackView.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor),
                _stackView.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor),
                _stackView.BottomAnchor.ConstraintEqualTo(View.BottomAnchor)
            });
        }

        public override void TraitCollectionDidChange(UITraitCollection previousTraitCollection)
        {
            base.TraitCollectionDidChange(previousTraitCollection);
            if (View.TraitCollection.VerticalSizeClass == UIUserInterfaceSizeClass.Compact)
            {
                // Landscape
                _stackView.Axis = UILayoutConstraintAxis.Horizontal;
            }
            else
            {
                // Portrait
                _stackView.Axis = UILayoutConstraintAxis.Vertical;
            }
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            // Subscribe to events.
            _myMapView.GeoViewTapped += MyMapView_GeoViewTapped;
        }

        public override void ViewDidDisappear(bool animated)
        {
            base.ViewDidDisappear(animated);

            // Unsubscribe from events, per best practice.
            _myMapView.GeoViewTapped -= MyMapView_GeoViewTapped;
        }
    }
}