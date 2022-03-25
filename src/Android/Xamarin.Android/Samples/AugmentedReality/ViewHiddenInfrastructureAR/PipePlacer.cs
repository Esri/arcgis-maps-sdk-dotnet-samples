// Copyright 2019 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using Android.App;
using Android.Content;
using Android.OS;
using Android.Widget;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.UI;
using Esri.ArcGISRuntime.UI.Controls;
using System;
using System.Linq;

namespace ArcGISRuntimeXamarin.Samples.ViewHiddenInfrastructureAR
{
    [Activity(ConfigurationChanges = Android.Content.PM.ConfigChanges.Orientation | Android.Content.PM.ConfigChanges.ScreenSize)]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        name: "View hidden infrastructure in AR",
        category: "Augmented reality",
        description: "Visualize hidden infrastructure in its real-world location using augmented reality.",
        instructions: "When you open the sample, you'll see a map centered on your current location. Use the tools to draw pipes around your location. An elevation surface will be queried to place the drawn infrastructure above or below ground. When you're ready, use the button to view the infrastructure you drew in AR.",
        tags: new[] { "augmented reality", "full-scale", "infrastructure", "lines", "mixed reality", "pipes", "real-scale", "underground", "visualization", "visualize", "world-scale" })]
    public class PipePlacer : Activity
    {
        // Hold references to the UI controls.
        private MapView _mapView;
        private Button _addButton;
        private Button _undoButton;
        private Button _redoButton;
        private Button _doneButton;
        private Button _viewButton;
        private SeekBar _elevationSlider;
        private TextView _helpLabel;

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
            _mapView.Map = new Map(BasemapStyle.ArcGISImageryStandard);

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
        }

        private void DoneClicked(object sender, EventArgs e)
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
                int elevationOffset = _elevationSlider.Progress;

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

        private void ViewClicked(object sender, EventArgs e)
        {
            // Set the field for sharing data between views.
            PipeViewer.PipeGraphics = _pipesOverlay.Graphics.Select(x => new Graphic(x.Geometry));

            // Start the AR viewer activity.
            Intent myIntent = new Intent(this, typeof(PipeViewer));
            StartActivity(myIntent);
        }

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            Title = "View hidden infrastructure in AR";

            // Create the UI
            CreateLayout();

            // Initialize controls, set up event handlers, etc.
            Initialize();
        }

        private void CreateLayout()
        {
            // Create and show the layout.
            SetContentView(ArcGISRuntime.Resource.Layout.ViewHiddenARPipePlacer);

            // Set up control references.
            _addButton = FindViewById<Button>(ArcGISRuntime.Resource.Id.addButton);
            _undoButton = FindViewById<Button>(ArcGISRuntime.Resource.Id.undoButton);
            _redoButton = FindViewById<Button>(ArcGISRuntime.Resource.Id.redoButton);
            _doneButton = FindViewById<Button>(ArcGISRuntime.Resource.Id.doneButton);
            _viewButton = FindViewById<Button>(ArcGISRuntime.Resource.Id.viewButton);
            _elevationSlider = FindViewById<SeekBar>(ArcGISRuntime.Resource.Id.elevationSlider);

            _helpLabel = FindViewById<TextView>(ArcGISRuntime.Resource.Id.statusLabel);
            _mapView = FindViewById<MapView>(ArcGISRuntime.Resource.Id.MapView);

            _addButton.Click += AddSketch;
            _undoButton.Click += (s, e) => { if (_mapView.SketchEditor.UndoCommand.CanExecute(null)) _mapView.SketchEditor.UndoCommand.Execute(null); };
            _redoButton.Click += (s, e) => { if (_mapView.SketchEditor.RedoCommand.CanExecute(null)) _mapView.SketchEditor.RedoCommand.Execute(null); };
            _doneButton.Click += DoneClicked;
            _viewButton.Click += ViewClicked;
        }
    }
}