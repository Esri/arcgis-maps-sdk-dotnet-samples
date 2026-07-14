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
using Esri.ArcGISRuntime.UI.Editing;
using Microsoft.Maui.ApplicationModel;
using Map = Esri.ArcGISRuntime.Mapping.Map;

namespace ArcGIS.Samples.DisplayGeometryEditorInformationDuringInteraction
{
    [ArcGIS.Samples.Shared.Attributes.Sample(
        name: "Display geometry editor information during interaction",
        category: "Geometry",
        description: "Use the geometry editor to see information about the geometry editor's previewed geometry during an editing interaction.",
        instructions: "Tap a graphic to edit its geometry by moving, rotating, or scaling the geometry. During the interaction, information about the geometry will be displayed to provide feedback to the user.",
        tags: new[] { "draw", "edit", "geometry editor", "interaction preview" })]
    [ArcGIS.Samples.Shared.Attributes.OfflineData()]
    public partial class DisplayGeometryEditorInformationDuringInteraction
    {
        // Create a geometry editor instance.
        private readonly GeometryEditor _geometryEditor = new();

        // Create a variable to hold the graphic being edited.
        private Graphic _editingGraphic;

        // Create the initial viewpoint.
        private readonly Viewpoint _initialViewpoint = Viewpoint.FromJson(@"{""rotation"":0.0,""scale"":17000,""targetGeometry"":{""spatialReference"":{""wkid"":3857},""x"":-13045202.018086127,""y"":4035612.571361517}}");

        // Create a polygon, polyline, and multipoint in Redlands, California.
        private readonly Polygon _redlandsPolygon = Polygon.FromJson(@"{""rings"":[[[-13046991.222211758,4034618.5047884779],[-13046991.222211758,4035962.0723415823],[-13045677.652220398,4035962.0723415823],[-13045677.652220398,4034618.5047884779],[-13046991.222211758,4034618.5047884779]]],""spatialReference"":{""wkid"":3857}}") as Polygon;

        private readonly Polyline _redlandsPolyline = Polyline.FromJson(@"{""paths"":[[[-13044533.805088846,4034221.5100018946],[-13043597.938505623,4034197.1337576872],[-13043597.938505623,4035135.572073034],[-13044522.634505576,4035170.5449295067]]],""spatialReference"":{""wkid"":3857}}") as Polyline;

        private readonly Multipoint _redlandsMultipoint = Multipoint.FromJson(@"{""points"":[[-13045283.292102993,4035739.1925106063],[-13045314.922186911,4036533.8852012255],[-13044798.24723932,4036138.7808295386],[-13044354.514637273,4035719.3623426706],[-13044281.57229173,4036473.0999132735]],""spatialReference"":{""wkid"":3857}}") as Multipoint;

        public DisplayGeometryEditorInformationDuringInteraction()
        {
            InitializeComponent();
            _ = Initialize();
        }

        private async Task Initialize()
        {
            // Create a new map with the streets basemap.
            Map myMap = new(BasemapStyle.ArcGISStreets);

            // Create a new graphics overlay and add it to the map view.
            GraphicsOverlay myGraphicsOverlay = new();
            MyMapView.GraphicsOverlays.Add(myGraphicsOverlay);

            // Create a symbols for the geometry editor.
            SimpleLineSymbol lineSymbol = new(SimpleLineSymbolStyle.Solid, System.Drawing.Color.Red, 2);
            SimpleMarkerSymbol markerSymbol = new SimpleMarkerSymbol(SimpleMarkerSymbolStyle.Circle, System.Drawing.Color.Blue, 8);

            // Create graphics for the polygon, polyline, and multipoint.
            var polygonGraphic = new Graphic(_redlandsPolygon, lineSymbol);
            var polylineGraphic = new Graphic(_redlandsPolyline, lineSymbol);
            var multipointGraphic = new Graphic(_redlandsMultipoint, markerSymbol);

            // Add the graphics to the graphics overlay.
            myGraphicsOverlay.Graphics.AddRange([multipointGraphic, polygonGraphic, polylineGraphic]);

            // Set the map's initial viewpoint.
            myMap.InitialViewpoint = _initialViewpoint;

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

            if (originalExtent.Width != 0 && originalExtent.Height != 0)
            {
                // Calculate the scale factors for X and Y.
                double scaleX = previewExtent.Width / originalExtent.Width;
                double scaleY = previewExtent.Height / originalExtent.Height;

                // Update the UI labels with the scale factor information.
                InteractionPreviewDescriptionLabel.Text = "Scale Factor (X, Y):";
                InteractionPreviewValueLabel.Text = $"({scaleX:F2}, {scaleY:F2})";
                InteractionPreviewGrid.IsVisible = true;
            }
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
                case Multipart previewMultipart:
                    originalPoint = ((Multipart)_geometryEditor.Geometry).Parts[0].Points.Where(point => point != center).FirstOrDefault();
                    previewPoint = previewMultipart.Parts[0].Points.Where(point => point != center).FirstOrDefault();
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
                double clockwiseNormalized = ((-angle % 360) + 360) % 360;
                InteractionPreviewValueLabel.Text = $"{clockwiseNormalized:F2}°";
            }

            // Update the UI label for rotation angle.
            InteractionPreviewDescriptionLabel.Text = "Rotation Angle (degrees):";
            InteractionPreviewGrid.IsVisible = true;
        }

        // Set the moving geometry label based on the interaction preview.
        private void SetMovingGeometryLabel(GeometryEditorInteractionPreview interactionPreview)
        {
            // Get the center point of the preview geometry.
            MapPoint previewCenter = interactionPreview.PreviewGeometry.Extent.GetCenter();

            // Update the UI labels with the center point information.
            InteractionPreviewDescriptionLabel.Text = "Center (X, Y):";
            InteractionPreviewValueLabel.Text = $"({previewCenter.X:F2}, {previewCenter.Y:F2})";
            InteractionPreviewGrid.IsVisible = true;
        }

        // Handle the InteractionPreviewChanged event to update the UI based on the type of interaction.
        private void GeometryEditor_InteractionPreviewChanged(object sender, GeometryEditorInteractionPreviewEventArgs e)
        {
            // Check if the interaction preview is not null.
            if (e.InteractionPreview != null)
            {
                // Use a switch statement to determine the type of interaction and update the UI accordingly.
                switch (e.InteractionPreview.InteractionType)
                {
                    case GeometryEditorInteractionType.Scale:
                        MainThread.BeginInvokeOnMainThread(() => SetScaleFactorLabel(e.InteractionPreview));
                        break;
                    case GeometryEditorInteractionType.Rotate:
                        MainThread.BeginInvokeOnMainThread(() => SetRotationAngleLabel(e.InteractionPreview));
                        break;
                    case GeometryEditorInteractionType.Move:
                        MainThread.BeginInvokeOnMainThread(() => SetMovingGeometryLabel(e.InteractionPreview));
                        break;
                }
            }
            else
            {
                // If the interaction preview is null the interaction has finished.
                MainThread.BeginInvokeOnMainThread(() => InteractionPreviewGrid.IsVisible = false);
            }
        }

        #region UI Event Handlers
        private async void MyMapView_GeoViewTapped(object sender, Esri.ArcGISRuntime.Maui.GeoViewInputEventArgs e)
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

        private void SaveButton_Click(object sender, EventArgs e)
        {
            if (_geometryEditor.IsStarted)
            {
                _editingGraphic.Geometry = _geometryEditor.Stop();
                _editingGraphic.IsVisible = true; // Show the graphic again after editing.
                InteractionPreviewGrid.IsVisible = false;
            }
        }

        private void CancelButton_Click(object sender, EventArgs e)
        {
            if (_geometryEditor.IsStarted)
            {
                _geometryEditor.Stop();
                _editingGraphic.IsVisible = true; // Show the graphic again after canceling.
                InteractionPreviewGrid.IsVisible = false;
            }
        }

        private void RedoButton_Click(object sender, EventArgs e)
        {
            _geometryEditor.Redo();
        }

        private void UndoButton_Click(object sender, EventArgs e)
        {
            _geometryEditor.Undo();
        }
        #endregion
    }
}