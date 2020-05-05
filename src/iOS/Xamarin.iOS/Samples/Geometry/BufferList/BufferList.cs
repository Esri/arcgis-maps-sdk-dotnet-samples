// Copyright 2018 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific 
// language governing permissions and limitations under the License.

using System;
using System.Collections.Generic;
using System.Drawing;
using CoreGraphics;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.UI;
using Esri.ArcGISRuntime.UI.Controls;
using Foundation;
using UIKit;

namespace ArcGISRuntime.Samples.BufferList
{
    [Register("BufferList")]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Buffer list",
        "Geometry",
        "This sample demonstrates how to use a planar (Euclidean) buffer operation by calling `GeometryEngine.Buffer` to generate one or more polygons from a collection of input geometries and a corresponding collection of buffer distances. The result polygons can be returned as individual geometries or unioned into a single polygon output.",
        "Tap on the map to add points. Each point will use the buffer distance entered when it was created. The envelope shows the area where you can expect reasonable results for planar buffer operations with the North Central Texas State Plane spatial reference.",
        "GeometryEngine, Geometry, Buffer, SpatialReference")]
    public class BufferList : UIViewController
    {
        // Hold references to UI controls.
        private MapView _myMapView;
        private UITextField _bufferDistanceEntry;
        private UIBarButtonItem _helpButton;
        private UIBarButtonItem _bufferButton;
        private UIBarButtonItem _resetButton;

        // A polygon that defines the valid area of the spatial reference used.
        private Polygon _spatialReferenceArea;

        // A Random object to create RGB color values.
        private readonly Random _random = new Random();

        public BufferList()
        {
            Title = "Buffer list";
        }

        private void Initialize()
        {
            // Create a spatial reference that's suitable for creating planar buffers in north central Texas (State Plane).
            SpatialReference statePlaneNorthCentralTexas = new SpatialReference(32038);

            // Define a polygon that represents the valid area of use for the spatial reference.
            // This information is available at https://developers.arcgis.com/net/latest/wpf/guide/pdf/projected_coordinate_systems_rt100_3_0.pdf
            List<MapPoint> spatialReferenceExtentCoords = new List<MapPoint>
            {
                new MapPoint(-103.070, 31.720, SpatialReferences.Wgs84),
                new MapPoint(-103.070, 34.580, SpatialReferences.Wgs84),
                new MapPoint(-94.000, 34.580, SpatialReferences.Wgs84),
                new MapPoint(-94.00, 31.720, SpatialReferences.Wgs84)
            };
            _spatialReferenceArea = new Polygon(spatialReferenceExtentCoords);
            _spatialReferenceArea = GeometryEngine.Project(_spatialReferenceArea, statePlaneNorthCentralTexas) as Polygon;

            // Create a map that uses the North Central Texas state plane spatial reference.
            Map bufferMap = new Map(statePlaneNorthCentralTexas);

            // Add some base layers (counties, cities, and highways).
            Uri usaLayerSource = new Uri("https://sampleserver6.arcgisonline.com/arcgis/rest/services/USA/MapServer");
            ArcGISMapImageLayer usaLayer = new ArcGISMapImageLayer(usaLayerSource);
            bufferMap.Basemap.BaseLayers.Add(usaLayer);

            // Use a new EnvelopeBuilder to expand the spatial reference extent 120%.
            EnvelopeBuilder envBuilder = new EnvelopeBuilder(_spatialReferenceArea.Extent);
            envBuilder.Expand(1.2);

            // Set the map's initial extent to the expanded envelope.
            Envelope startingEnvelope = envBuilder.ToGeometry();
            bufferMap.InitialViewpoint = new Viewpoint(startingEnvelope);

            // Assign the map to the MapView.
            _myMapView.Map = bufferMap;

            // Create a graphics overlay to show the buffer polygon graphics.
            GraphicsOverlay bufferGraphicsOverlay = new GraphicsOverlay
            {
                // Give the overlay an ID so it can be found later.
                Id = "buffers"
            };

            // Create a graphic to show the spatial reference's valid extent (envelope) with a dashed red line.
            SimpleLineSymbol lineSymbol = new SimpleLineSymbol(SimpleLineSymbolStyle.Dash, Color.Red, 5);
            Graphic spatialReferenceExtentGraphic = new Graphic(_spatialReferenceArea, lineSymbol);

            // Add the graphic to a new overlay.
            GraphicsOverlay spatialReferenceGraphicsOverlay = new GraphicsOverlay();
            spatialReferenceGraphicsOverlay.Graphics.Add(spatialReferenceExtentGraphic);

            // Add the graphics overlays to the MapView.
            _myMapView.GraphicsOverlays.Add(bufferGraphicsOverlay);
            _myMapView.GraphicsOverlays.Add(spatialReferenceGraphicsOverlay);
        }

        private void MyMapView_GeoViewTapped(object sender, GeoViewInputEventArgs e)
        {
            try
            {
                // Get the input map point (in the map's coordinate system, State Plane for North Central Texas).
                MapPoint tapMapPoint = e.Location;

                // Check if the point coordinates are within the spatial reference envelope.
                bool withInvalidExtent = GeometryEngine.Contains(_spatialReferenceArea, tapMapPoint);

                // If the input point is not within the valid extent for the spatial reference, warn the user and return.
                if (!withInvalidExtent)
                {
                    // Display a message to warn the user.
                    UIAlertController alertController = UIAlertController.Create("Out of bounds", "Location is not valid to buffer using the defined spatial reference.", UIAlertControllerStyle.Alert);
                    alertController.AddAction(UIAlertAction.Create("OK", UIAlertActionStyle.Default, null));
                    PresentViewController(alertController, true, null);

                    return;
                }

                // Get the buffer radius (in miles) from the text box.
                double bufferDistanceMiles = Convert.ToDouble(_bufferDistanceEntry.Text);

                // Use a helper method to get the buffer distance in feet (unit that's used by the spatial reference).
                double bufferDistanceFeet = LinearUnits.Miles.ConvertTo(LinearUnits.Feet, bufferDistanceMiles);

                // Create a simple marker symbol (red circle) to display where the user tapped/clicked on the map. 
                SimpleMarkerSymbol tapSymbol = new SimpleMarkerSymbol(SimpleMarkerSymbolStyle.Circle, Color.Red, 10);

                // Create a new graphic to show the tap location. 
                Graphic tapGraphic = new Graphic(tapMapPoint, tapSymbol)
                {
                    // Specify a z-index value on the point graphic to make sure it draws on top of the buffer polygons.
                    ZIndex = 2
                };

                // Store the specified buffer distance as an attribute with the graphic.
                tapGraphic.Attributes["distance"] = bufferDistanceFeet;

                // Add the tap point graphic to the buffer graphics overlay.
                _myMapView.GraphicsOverlays["buffers"].Graphics.Add(tapGraphic);
            }
            catch (Exception ex)
            {
                // Display an error message.
                UIAlertController alertController = UIAlertController.Create("Error creating buffer point", ex.Message, UIAlertControllerStyle.Alert);
                alertController.AddAction(UIAlertAction.Create("OK", UIAlertActionStyle.Default, null));
                PresentViewController(alertController, true, null);
            }
        }

        private void PromptForUnionChoice(object sender, EventArgs e)
        {
            var alert = UIAlertController.Create("Union buffers?", "", UIAlertControllerStyle.ActionSheet);
            if (alert.PopoverPresentationController != null)
            {
                alert.PopoverPresentationController.BarButtonItem = _bufferButton;
            }

            alert.AddAction(UIAlertAction.Create("Union", UIAlertActionStyle.Default, action => PerformUnion(true)));
            alert.AddAction(UIAlertAction.Create("Don't union", UIAlertActionStyle.Default, action => PerformUnion(false)));
            PresentViewController(alert, true, null);
        }

        private void PerformUnion(bool unionBuffers)
        {
            try
            {
                // Call a function to delete any existing buffer polygons so they can be recreated.
                ClearBufferPolygons();

                // Iterate all point graphics and create a list of map points and buffer distances for each.
                List<MapPoint> bufferMapPoints = new List<MapPoint>();
                List<double> bufferDistances = new List<double>();
                foreach (Graphic bufferGraphic in _myMapView.GraphicsOverlays["buffers"].Graphics)
                {
                    // Only use point graphics.
                    if (bufferGraphic.Geometry.GeometryType == GeometryType.Point)
                    {
                        // Get the geometry (map point) from the graphic.
                        MapPoint bufferLocation = bufferGraphic.Geometry as MapPoint;

                        // Read the "distance" attribute to get the buffer distance entered when the point was tapped.
                        double bufferDistanceFeet = (double) bufferGraphic.Attributes["distance"];

                        // Add the point and the corresponding distance to the lists.
                        bufferMapPoints.Add(bufferLocation);
                        bufferDistances.Add(bufferDistanceFeet);
                    }
                }

                // Call GeometryEngine.Buffer with a list of map points and a list of buffered distances.
                IEnumerable<Geometry> bufferPolygons = GeometryEngine.Buffer(bufferMapPoints, bufferDistances, unionBuffers);

                // Create the outline for the buffered polygons.
                SimpleLineSymbol bufferPolygonOutlineSymbol = new SimpleLineSymbol(SimpleLineSymbolStyle.Solid, Color.DarkBlue, 3);

                // Loop through all the geometries in the buffer results. There will be one buffered polygon if
                // the result geometries were unioned. Otherwise, there will be one buffer per input geometry.
                foreach (Geometry poly in bufferPolygons)
                {
                    // Create a random color to use for buffer polygon fill.
                    Color bufferPolygonColor = GetRandomColor();

                    // Create simple fill symbol for the buffered polygon using the fill color and outline.
                    SimpleFillSymbol bufferPolygonFillSymbol = new SimpleFillSymbol(SimpleFillSymbolStyle.Solid, bufferPolygonColor, bufferPolygonOutlineSymbol);

                    // Create a new graphic for the buffered polygon using the fill symbol.
                    Graphic bufferPolygonGraphic = new Graphic(poly, bufferPolygonFillSymbol)
                    {
                        // Specify a z-index of 0 to ensure the polygons draw below the tap points.
                        ZIndex = 0
                    };

                    // Add the buffered polygon graphic to the graphics overlay.                    
                    _myMapView.GraphicsOverlays[0].Graphics.Add(bufferPolygonGraphic);
                }
            }
            catch (Exception ex)
            {
                // Display an error message if there is a problem generating the buffers.
                UIAlertController alertController = UIAlertController.Create("Unable to create buffer polygons", ex.Message, UIAlertControllerStyle.Alert);
                alertController.AddAction(UIAlertAction.Create("OK", UIAlertActionStyle.Default, null));
                PresentViewController(alertController, true, null);
            }
        }

        private Color GetRandomColor()
        {
            // Get a byte array with three random values.
            byte[] colorBytes = new byte[3];
            _random.NextBytes(colorBytes);

            // Use the random bytes to define red, green, and blue values for a new color.
            return Color.FromArgb(155, colorBytes[0], colorBytes[1], colorBytes[2]);
        }

        private void ClearButton_Click(object sender, EventArgs e)
        {
            // Clear all graphics (tap points and buffer polygons).
            _myMapView.GraphicsOverlays["buffers"].Graphics.Clear();
        }

        private void ClearBufferPolygons()
        {
            // Get the collection of graphics in the graphics overlay (points and buffer polygons).
            GraphicCollection bufferGraphics = _myMapView.GraphicsOverlays["buffers"].Graphics;

            // Loop (backwards) through all graphics.
            for (int i = bufferGraphics.Count - 1; i >= 0; i--)
            {
                // If the graphic is a polygon, remove it from the overlay.
                Graphic thisGraphic = bufferGraphics[i];
                if (thisGraphic.Geometry.GeometryType == GeometryType.Polygon)
                {
                    bufferGraphics.RemoveAt(i);
                }
            }
        }

        private void ShowHelpAlert(object sender, EventArgs e)
        {
            const string message = "Tap to add points. Each point will use the buffer distance entered when it was created. " +
                                   "The envelope shows the area where you can expect reasonable results for planar buffer operations with the North Central Texas State Plane spatial reference.";
            new UIAlertView("Instructions", message, (IUIAlertViewDelegate) null, "OK", null).Show();
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            Initialize();
        }

        public override void LoadView()
        {
            // Create the views.
            View = new UIView {BackgroundColor = UIColor.White};

            _myMapView = new MapView();
            _myMapView.TranslatesAutoresizingMaskIntoConstraints = false;

            _helpButton = new UIBarButtonItem();
            _helpButton.Title = "Help";

            _resetButton = new UIBarButtonItem();
            _resetButton.Title = "Reset";

            _bufferButton = new UIBarButtonItem();
            _bufferButton.Title = "Buffer";

            UIToolbar controlsToolbar = new UIToolbar();
            controlsToolbar.TranslatesAutoresizingMaskIntoConstraints = false;
            controlsToolbar.Items = new[]
            {
                _helpButton,
                // Put a space between the buttons
                new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace),
                _bufferButton,
                _resetButton
            };

            UIToolbar entryToolbar = new UIToolbar();
            entryToolbar.TranslatesAutoresizingMaskIntoConstraints = false;

            _bufferDistanceEntry = new UITextField();
            _bufferDistanceEntry.TranslatesAutoresizingMaskIntoConstraints = false;
            _bufferDistanceEntry.Text = "10";
            _bufferDistanceEntry.BackgroundColor = UIColor.FromWhiteAlpha(1, .8f);
            _bufferDistanceEntry.Layer.CornerRadius = 4;
            _bufferDistanceEntry.LeftView = new UIView(new CGRect(0, 0, 5, 20));
            _bufferDistanceEntry.LeftViewMode = UITextFieldViewMode.Always;
            _bufferDistanceEntry.KeyboardType = UIKeyboardType.Default;

            UILabel bufferDistanceEntryLabel = new UILabel();
            bufferDistanceEntryLabel.TranslatesAutoresizingMaskIntoConstraints = false;
            bufferDistanceEntryLabel.Text = "Buffer size:";

            // Add the views.
            View.AddSubviews(_myMapView, controlsToolbar, entryToolbar, bufferDistanceEntryLabel, _bufferDistanceEntry);

            // Lay out the views.
            NSLayoutConstraint.ActivateConstraints(new[]
            {
                entryToolbar.TopAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.TopAnchor),
                entryToolbar.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor),
                entryToolbar.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor),

                bufferDistanceEntryLabel.LeadingAnchor.ConstraintEqualTo(entryToolbar.LayoutMarginsGuide.LeadingAnchor, 8),
                bufferDistanceEntryLabel.CenterYAnchor.ConstraintEqualTo(entryToolbar.CenterYAnchor),

                _bufferDistanceEntry.TrailingAnchor.ConstraintEqualTo(entryToolbar.LayoutMarginsGuide.TrailingAnchor),
                _bufferDistanceEntry.CenterYAnchor.ConstraintEqualTo(entryToolbar.CenterYAnchor),
                _bufferDistanceEntry.LeadingAnchor.ConstraintEqualTo(bufferDistanceEntryLabel.TrailingAnchor, 8),

                _myMapView.TopAnchor.ConstraintEqualTo(entryToolbar.BottomAnchor),
                _myMapView.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor),
                _myMapView.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor),
                _myMapView.BottomAnchor.ConstraintEqualTo(controlsToolbar.TopAnchor),

                controlsToolbar.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor),
                controlsToolbar.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor),
                controlsToolbar.BottomAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.BottomAnchor)
            });
        }

        private bool HandleTextField(UITextField textField)
        {
            // This method allows pressing 'return' to dismiss the software keyboard.
            textField.ResignFirstResponder();
            return true;
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            // Subscribe to events.
            _bufferDistanceEntry.ShouldReturn += HandleTextField;
            _myMapView.GeoViewTapped += MyMapView_GeoViewTapped;
            _helpButton.Clicked += ShowHelpAlert;
            _resetButton.Clicked += ClearButton_Click;
            _bufferButton.Clicked += PromptForUnionChoice;
        }

        public override void ViewDidDisappear(bool animated)
        {
            base.ViewDidDisappear(animated);

            // Unsubscribe from events, per best practice.
            _myMapView.GeoViewTapped -= MyMapView_GeoViewTapped;
            _bufferDistanceEntry.ShouldReturn -= HandleTextField;
            _helpButton.Clicked -= ShowHelpAlert;
            _resetButton.Clicked -= ClearButton_Click;
            _bufferButton.Clicked -= PromptForUnionChoice;
        }
    }
}