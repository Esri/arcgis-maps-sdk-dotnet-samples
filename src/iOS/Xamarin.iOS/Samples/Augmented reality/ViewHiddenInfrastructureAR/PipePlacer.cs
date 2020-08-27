// Copyright 2019 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using ArcGISRuntime;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.UI;
using Esri.ArcGISRuntime.UI.Controls;
using Foundation;
using System;
using System.Linq;
using UIKit;

namespace ArcGISRuntimeXamarin.Samples.ViewHiddenInfrastructureAR
{
    [Register("ViewHiddenInfrastructureAR")]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        name: "View hidden infrastructure in AR",
        category: "Augmented reality",
        description: "Visualize hidden infrastructure in its real-world location using augmented reality.",
        instructions: "When you open the sample, you'll see a map centered on your current location. Use the tools to draw pipes around your location. An elevation surface will be queried to place the drawn infrastructure above or below ground. When you're ready, use the button to view the infrastructure you drew in AR.",
        tags: new[] { "augmented reality", "full-scale", "infrastructure", "lines", "mixed reality", "pipes", "real-scale", "underground", "visualization", "visualize", "world-scale" })]
    [ArcGISRuntime.Samples.Shared.Attributes.OfflineData()]
    public class PipePlacer : UIViewController
    {
        // Hold references to the UI controls.
        private MapView _mapView;
        private UILabel _helpLabel;
        private UIBarButtonItem _addButton;
        private UIBarButtonItem _undoButton;
        private UIBarButtonItem _redoButton;
        private UIBarButtonItem _viewButton;
        private UIBarButtonItem _doneButton;
        private UIBarButtonItem _elevationSliderButton;
        private UISlider _elevationSlider;

        // Graphics overlays for showing pipes.
        private GraphicsOverlay _pipesOverlay = new GraphicsOverlay();

        // Sketch editor for drawing pipes onto the MapView.
        private SketchEditor _sketchEditor = new SketchEditor();

        // Elevation source for getting altitude value for points.
        private ArcGISTiledElevationSource _elevationSource;
        private Surface _elevationSurface;

        private async void Initialize()
        {
            // Create and add the map.
            _mapView.Map = new Map(Basemap.CreateImagery());

            // Configure location display.
            _mapView.LocationDisplay.AutoPanMode = LocationDisplayAutoPanMode.Recenter;
            await _mapView.LocationDisplay.DataSource.StartAsync();
            _mapView.LocationDisplay.IsEnabled = true;
            
            // Add a graphics overlay for the drawn pipes.
            _mapView.GraphicsOverlays.Add(_pipesOverlay);
            _pipesOverlay.Renderer = new SimpleRenderer(new SimpleLineSymbol(SimpleLineSymbolStyle.Solid, System.Drawing.Color.Red, 2));

            // Set the SketchEditor for the map.
            _mapView.SketchEditor = _sketchEditor;

            // Create an elevation source and Surface.
            _elevationSource = new ArcGISTiledElevationSource(new Uri("https://elevation3d.arcgis.com/arcgis/rest/services/WorldElevation3D/Terrain3D/ImageServer"));
            await _elevationSource.LoadAsync();
            _elevationSurface = new Surface();
            _elevationSurface.ElevationSources.Add(_elevationSource);
            await _elevationSurface.LoadAsync();

            // Enable the add button.
            _addButton.Enabled = true;

            // Update the help message.
            _helpLabel.Text = "Draw pipes on the map, then tap 📷 to view in AR";
        }

        private void DoneButton_Clicked(object sender, EventArgs e)
        {
            // Check if sketch can be completed.
            if (_mapView.SketchEditor.CompleteCommand.CanExecute(null))
            {
                // Complete the sketch.
                _mapView.SketchEditor.CompleteCommand.Execute(null);

                // Disable the editing buttons.
                _doneButton.Enabled = _undoButton.Enabled = _redoButton.Enabled = false;
            }
        }

        private void ViewButton_Clicked(object sender, EventArgs e)
        {
            // Transition to the view the pipes in augmented reality.
            NavigationController.PopViewController(true);
            NavigationController.PushViewController(new PipeViewerAR() { _pipeGraphics = _pipesOverlay.Graphics.Select(x => new Graphic(x.Geometry)) }, true);
        }

        private void RedoButton_Clicked(object sender, EventArgs e)
        {
            // Redo if possible.
            if (_sketchEditor.RedoCommand.CanExecute(null)) _sketchEditor.RedoCommand.Execute(null);
        }

        private void UndoButton_Clicked(object sender, EventArgs e)
        {
            // Undo if possible.
            if (_sketchEditor.UndoCommand.CanExecute(null)) _sketchEditor.UndoCommand.Execute(null);
        }

        private async void AddSketch(object sender, EventArgs e)
        {
            // Enable the editing buttons.
            _doneButton.Enabled = _undoButton.Enabled = _redoButton.Enabled = true;

            // Prevent the user from adding pipes concurrently.
            _addButton.Enabled = false;

            // Get the geometry of the user drawn line.
            Geometry geometry = await _sketchEditor.StartAsync(SketchCreationMode.Polyline);
            _addButton.Enabled = true;

            // Verify that the user has drawn a polyline.
            if (!(geometry is Polyline))
            {
                return;
            }

            // Get the first point of the pipe.
            MapPoint firstPoint = ((Polyline)geometry).Parts[0].StartPoint;
            try
            {
                // Get the users selected elevation offset.
                double elevationOffset = _elevationSlider.Value;

                // Get the elevation of the geometry from the first point of the pipe.
                double elevation = await _elevationSurface.GetElevationAsync(firstPoint);

                // Create a polyline for the pipe at the selected altitude.
                Polyline elevatedLine = GeometryEngine.SetZ(geometry, elevation + elevationOffset) as Polyline;

                // Create a graphic for the pipe.
                Graphic linegraphic = new Graphic(elevatedLine);
                _pipesOverlay.Graphics.Add(linegraphic);

                // Display a message with the pipes offset from the surface.
                if (elevationOffset < 0)
                {
                    _helpLabel.Text = string.Format("Pipe added {0:0.0}m below surface", elevationOffset * -1);
                }
                else if (elevationOffset == 0)
                {
                    _helpLabel.Text = "Pipe added at ground level";
                }
                else
                {
                    _helpLabel.Text = string.Format("Pipe added {0:0.0}m above the surface", elevationOffset);
                }

                // Enable the view button once a pipe has been added to the graphics overlay.
                _viewButton.Enabled = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }

        public override void LoadView()
        {
            // Create the views.
            View = new UIView() { BackgroundColor = ApplicationTheme.BackgroundColor };

            _mapView = new MapView();
            _mapView.TranslatesAutoresizingMaskIntoConstraints = false;

            _helpLabel = new UILabel();
            _helpLabel.TranslatesAutoresizingMaskIntoConstraints = false;
            _helpLabel.TextAlignment = UITextAlignment.Center;
            _helpLabel.TextColor = UIColor.White;
            _helpLabel.BackgroundColor = UIColor.FromWhiteAlpha(0f, 0.6f);
            _helpLabel.Text = "Preparing services...";

            UIToolbar elevToolbar = new UIToolbar();
            elevToolbar.TranslatesAutoresizingMaskIntoConstraints = false;

            UIBarButtonItem elevLabel = new UIBarButtonItem() { CustomView = new UILabel() { Text = "Elevation:" } };

            _elevationSlider = new UISlider() { MinValue = -10, MaxValue = 10, Value = 0 };
            _elevationSliderButton = new UIBarButtonItem() { CustomView = _elevationSlider };

            elevToolbar.Items = new[]
            {
                 elevLabel,
                new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace),
                _elevationSliderButton
            };

            UIToolbar buttonToolbar = new UIToolbar();
            buttonToolbar.TranslatesAutoresizingMaskIntoConstraints = false;

            _addButton = new UIBarButtonItem(UIBarButtonSystemItem.Add, AddSketch) { Enabled = false };
            _undoButton = new UIBarButtonItem(UIBarButtonSystemItem.Undo, UndoButton_Clicked) { Enabled = false };
            _redoButton = new UIBarButtonItem(UIBarButtonSystemItem.Redo, RedoButton_Clicked) { Enabled = false };
            _doneButton = new UIBarButtonItem(UIBarButtonSystemItem.Done, DoneButton_Clicked) { Enabled = false };
            _viewButton = new UIBarButtonItem(UIBarButtonSystemItem.Camera, ViewButton_Clicked) { Enabled = false };

            buttonToolbar.Items = new[]
            {
                 _addButton,
                new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace),
                _undoButton,
                new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace),
                _redoButton,
                new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace),
                _doneButton,
                new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace),
                _viewButton,
                new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace)
            };

            // Add the views.
            View.AddSubviews(_mapView, _helpLabel, elevToolbar, buttonToolbar);

            // Lay out the views.
            NSLayoutConstraint.ActivateConstraints(new[]{
                _mapView.TopAnchor.ConstraintEqualTo(View.TopAnchor),
                _mapView.BottomAnchor.ConstraintEqualTo(elevToolbar.TopAnchor),
                _mapView.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor),
                _mapView.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor),
                _helpLabel.TopAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.TopAnchor),
                _helpLabel.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor),
                _helpLabel.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor),
                _helpLabel.HeightAnchor.ConstraintEqualTo(40),
                elevToolbar.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor),
                elevToolbar.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor),
                elevToolbar.BottomAnchor.ConstraintEqualTo(buttonToolbar.TopAnchor),
                buttonToolbar.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor),
                buttonToolbar.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor),
                buttonToolbar.BottomAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.BottomAnchor)
            });
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            Initialize();
        }
    }
}