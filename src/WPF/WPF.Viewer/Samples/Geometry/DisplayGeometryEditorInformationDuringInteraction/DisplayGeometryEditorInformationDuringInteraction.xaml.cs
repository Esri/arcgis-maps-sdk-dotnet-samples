// Copyright 2026 Esri.
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
using Esri.ArcGISRuntime.UI.Editing;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace ArcGIS.WPF.Samples.DisplayGeometryEditorInformationDuringInteraction
{
    [ArcGIS.Samples.Shared.Attributes.Sample(
        name: "Display geometry editor information during interaction",
        category: "Geometry",
        description: "Use the geometry editor to see information about the geometry editor's previewed geometry during an editing interaction.",
        instructions: "Tap a graphic to edit its geometry by moving, rotating, or scaling the geometry. During the interaction, information about the geometry will be displayed to provide feedback to the user.",
        tags: new[] { "draw", "edit", "geometry editor" })]
    [ArcGIS.Samples.Shared.Attributes.OfflineData()]
    public partial class DisplayGeometryEditorInformationDuringInteraction
    {
        // Create a geometry editor instance.
        private readonly GeometryEditor _geometryEditor = new();

        // Create a variable to hold the graphic being edited.
        private Graphic _editingGraphic;

        // Create a polygon, polyline, and multipoint in Redlands, California.
        private readonly Polygon _redlandsPolygon = new(
        [
            new MapPoint(-117.195800, 34.046295),
            new MapPoint(-117.195800, 34.056295),
            new MapPoint(-117.184000, 34.056295),
            new MapPoint(-117.184000, 34.046295)
        ], SpatialReferences.Wgs84);

        private readonly Polyline _redlandsPolyline = new(
        [
            new MapPoint(-117.183200, 34.047200),
            new MapPoint(-117.180800, 34.049100),
            new MapPoint(-117.178300, 34.051000),
            new MapPoint(-117.175900, 34.052600)
        ], SpatialReferences.Wgs84);

        private readonly Multipoint _redlandsMultipoint = new(
        [
            new MapPoint(-117.186900, 34.048300),
            new MapPoint(-117.184700, 34.049700),
            new MapPoint(-117.182100, 34.050900),
            new MapPoint(-117.179800, 34.052100),
            new MapPoint(-117.177600, 34.053000)
        ], SpatialReferences.Wgs84);

        public DisplayGeometryEditorInformationDuringInteraction()
        {
            InitializeComponent();
            _ = Initialize();
        }

        private async Task Initialize()
        {
            // Create a new map with the streets basemap.
            Map myMap = new Map(BasemapStyle.ArcGISStreets);

            // Create a new graphics overlay and add it to the map view.
            GraphicsOverlay myGraphicsOverlay = new();
            MyMapView.GraphicsOverlays.Add(myGraphicsOverlay);

            // Create a simple line symbol for the geometry editor.
            SimpleLineSymbol lineSymbol = new(SimpleLineSymbolStyle.Solid, System.Drawing.Color.Red, 2);

            // TODO: Planned change to no longer require this projection, remove the projection after change has been implemented.
            var polygonGraphic = new Graphic(GeometryEngine.Project(_redlandsPolygon, SpatialReferences.WebMercator), lineSymbol);
            var polylineGraphic = new Graphic(GeometryEngine.Project(_redlandsPolyline, SpatialReferences.WebMercator), lineSymbol);
            var multipointGraphic = new Graphic(GeometryEngine.Project(_redlandsMultipoint, SpatialReferences.WebMercator), new SimpleMarkerSymbol(SimpleMarkerSymbolStyle.Circle, System.Drawing.Color.Blue, 8));

            // Add the graphics to the graphics overlay.
            myGraphicsOverlay.Graphics.AddRange([multipointGraphic, polygonGraphic, polylineGraphic]);

            // Set the map's initial viewpoint to the extent of the graphics, expanded by 20%.
            var graphicsEnvelope = GeometryEngine.Union([polylineGraphic.Geometry.Extent, polygonGraphic.Geometry.Extent, multipointGraphic.Geometry.Extent]);
            var envelopeBuilder = new EnvelopeBuilder(graphicsEnvelope.Extent);
            envelopeBuilder.Expand(1.2);
            var expandedEnvelope = envelopeBuilder.ToGeometry();
            myMap.InitialViewpoint = new Viewpoint(expandedEnvelope);

            // Assign the map to the MapView.
            MyMapView.Map = myMap;

            // Set the geometry editor tool to the vertex tool and configure it to not allow vertex creation, mid-vertex selection, deleting selected elements, vertex selection, or part creation.
            _geometryEditor.Tool = new VertexTool()
            {
                Configuration = new InteractionConfiguration()
                {
                    AllowVertexCreation = false,
                    AllowMidVertexSelection = false,
                    AllowDeletingSelectedElement = false,
                    AllowVertexSelection = false,
                    AllowPartCreation = false,
                }
            };

            // Assign the geometry editor to the MapView and subscribe to the InteractionPreviewChanged event.
            MyMapView.GeometryEditor = _geometryEditor;
            _geometryEditor.InteractionPreviewChanged += GeometryEditor_InteractionPreviewChanged;

            // Subscribe to the GeoViewTapped event to start editing a graphic when it is tapped.
            MyMapView.GeoViewTapped += MyMapView_GeoViewTapped;
        }

        // Set the scale factor label based on the interaction preview.
        private void SetScaleFactorLabel(GeometryEditorInteractionPreview interactionPreview)
        {
            // Get the original and preview geometry extents.
            Envelope originalExtent = _geometryEditor.Geometry.Extent;
            Envelope previewExtent = interactionPreview.PreviewGeometry.Extent;

            // Calculate the scale factors for X and Y.
            double scaleX = previewExtent.Width / originalExtent.Width;
            double scaleY = previewExtent.Height / originalExtent.Height;

            // Update the UI labels with the scale factor information.
            InteractionPreviewDescriptionLabel.Content = "Scale Factor (X, Y):";
            InteractionPreviewValueLabel.Content = $"({scaleX:F2}, {scaleY:F2})";
            InteractionPreviewGrid.Visibility = Visibility.Visible;
        }

        // Set the rotation angle label based on the interaction preview.
        private void SetRotationAngleLabel(GeometryEditorInteractionPreview interactionPreview)
        {
            // Get the center point of the original geometry.
            MapPoint center = _geometryEditor.Geometry.Extent.GetCenter();

            // Create variables to hold the original and preview points for rotation calculation.
            MapPoint originalPoint = null;
            MapPoint previewPoint = null;

            // Determine the type of geometry being previewed and extract the relevant points for rotation calculation.
            switch (interactionPreview.PreviewGeometry)
            {
                case Polyline previewPolyline:
                    originalPoint = ((Polyline)_geometryEditor.Geometry).Parts[0].Points.Where(point => point != center).FirstOrDefault();
                    previewPoint = previewPolyline.Parts[0].Points.Where(point => point != center).FirstOrDefault();
                    break;
                case Polygon previewPolygon:
                    originalPoint = ((Polygon)_geometryEditor.Geometry).Parts[0].Points.Where(point => point != center).FirstOrDefault();
                    previewPoint = previewPolygon.Parts[0].Points.Where(point => point != center).FirstOrDefault();
                    break;
                case Multipoint previewMultiPoint:
                    originalPoint = ((Multipoint)_geometryEditor.Geometry).Points.Where(point => point != center).FirstOrDefault();
                    previewPoint = previewMultiPoint.Points.Where(point => point != center).FirstOrDefault();
                    break;
            }

            // Calculate the rotation angle if both original and preview points are available and different from each other.
            if (originalPoint != previewPoint)
            {
                var vector1X = originalPoint.X - center.X;
                var vector2X = previewPoint.X - center.X;
                var vector1Y = originalPoint.Y - center.Y;
                var vector2Y = previewPoint.Y - center.Y;

                var cross = vector1X * vector2Y - vector1Y * vector2X;
                var dot = vector1X * vector2X + vector1Y * vector2Y;

                double angle = Math.Atan2(cross, dot) * (180.0 / Math.PI); // Convert to degrees
                InteractionPreviewValueLabel.Content = $"{angle:F2}°";
            }

            // Update the UI label for rotation angle.
            InteractionPreviewDescriptionLabel.Content = "Rotation Angle (degrees):";
            InteractionPreviewGrid.Visibility = Visibility.Visible;
        }

        // Set the moving geometry label based on the interaction preview.
        private void SetMovingGeometryLabel(GeometryEditorInteractionPreview interactionPreview)
        {
            // Get the center point of the preview geometry.
            MapPoint previewCenter = interactionPreview.PreviewGeometry.Extent.GetCenter();

            // Update the UI labels with the center point information.
            InteractionPreviewDescriptionLabel.Content = "Center (X, Y):";
            InteractionPreviewValueLabel.Content = $"({previewCenter.X:F2}, {previewCenter.Y:F2})";
            InteractionPreviewGrid.Visibility = Visibility.Visible;
        }

        // Handle the InteractionPreviewChanged event to update the UI based on the type of interaction.
        private void GeometryEditor_InteractionPreviewChanged(object sender, GeometryEditorInteractionPreviewEventArgs e)
        {
            // Check if the interaction preview and its geometry are not null.
            if (e.InteractionPreview != null && e.InteractionPreview.PreviewGeometry != null && e.InteractionPreview.InteractionElement != null)
            {
                // Use a switch statement to determine the type of interaction and update the UI accordingly.
                switch (e.InteractionPreview.InteractionType)
                {
                    case GeometryEditorInteractionType.Scale:
                        Application.Current.Dispatcher.BeginInvoke(new Action(() => SetScaleFactorLabel(e.InteractionPreview)));
                        break;
                    case GeometryEditorInteractionType.Rotate:
                        Application.Current.Dispatcher.BeginInvoke(new Action(() => SetRotationAngleLabel(e.InteractionPreview)));
                        break;
                    case GeometryEditorInteractionType.Move:
                        Application.Current.Dispatcher.BeginInvoke(new Action(() => SetMovingGeometryLabel(e.InteractionPreview)));
                        break;
                }
            }
        }

        #region UI Event Handlers
        private async void MyMapView_GeoViewTapped(object sender, GeoViewInputEventArgs e)
        {
            if (!_geometryEditor.IsStarted)
            {
                var result = await MyMapView.IdentifyGraphicsOverlayAsync(MyMapView.GraphicsOverlays.First(), e.Position, 10, false);

                if (result.Graphics.Count > 0)
                {
                    _editingGraphic = result.Graphics[0];

                    // Start the geometry editor with the identified graphic's geometry.
                    _geometryEditor.Start(_editingGraphic.Geometry);
                    _geometryEditor.SelectGeometry();
                    _editingGraphic.IsVisible = false; // Hide the original graphic while editing.
                }
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (_geometryEditor.IsStarted)
            {
                _editingGraphic.Geometry = _geometryEditor.Stop();
                _editingGraphic.IsVisible = true; // Show the graphic again after editing.
                InteractionPreviewGrid.Visibility = Visibility.Collapsed;
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            if (_geometryEditor.IsStarted)
            {
                _geometryEditor.Stop();
                _editingGraphic.IsVisible = true; // Show the graphic again after canceling.
                InteractionPreviewGrid.Visibility = Visibility.Collapsed;
            }
        }

        private void RedoButton_Click(object sender, RoutedEventArgs e)
        {
            _geometryEditor.Redo();
        }

        private void UndoButton_Click(object sender, RoutedEventArgs e)
        {
            _geometryEditor.Undo();
        }
        #endregion
    }
}