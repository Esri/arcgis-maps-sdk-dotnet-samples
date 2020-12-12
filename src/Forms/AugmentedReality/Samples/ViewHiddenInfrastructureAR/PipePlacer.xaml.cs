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
using System;
using System.Diagnostics;
using System.Linq;
using Xamarin.Forms;

#if XAMARIN_ANDROID
using ArcGISRuntime.Droid;
#endif

namespace ArcGISRuntimeXamarin.Samples.ViewHiddenInfrastructureAR
{
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        name: "View hidden infrastructure in AR",
        category: "Augmented reality",
        description: "Visualize hidden infrastructure in its real-world location using augmented reality.",
        instructions: "When you open the sample, you'll see a map centered on your current location. Use the tools to draw pipes around your location. An elevation surface will be queried to place the drawn infrastructure above or below ground. When you're ready, use the button to view the infrastructure you drew in AR.",
        tags: new[] { "augmented reality", "full-scale", "infrastructure", "lines", "mixed reality", "pipes", "real-scale", "underground", "visualization", "visualize", "world-scale" })]
    [ArcGISRuntime.Samples.Shared.Attributes.OfflineData()]
    public partial class PipePlacer : ContentPage
    {
        // Graphics overlays for showing pipes.
        private GraphicsOverlay _pipesOverlay = new GraphicsOverlay();

        // Sketch editor for drawing pipes onto the MapView.
        private SketchEditor _sketchEditor = new SketchEditor();

        // Elevation source for getting altitude value for points.
        private ArcGISTiledElevationSource _elevationSource;
        private Surface _elevationSurface;

        public PipePlacer()
        {
            InitializeComponent();
            Initialize();
        }

        private async void Initialize()
        {
            // Create and add the map.
            MyMapView.Map = new Map(Basemap.CreateImagery());

            MyMapView.PropertyChanged += async (o, e) =>
            {
                if (e.PropertyName == nameof(MyMapView.LocationDisplay) && MyMapView.LocationDisplay != null)
                {
                    // Start the location display on the mapview.
                    try
                    {
                        // Permission request only needed on Android.
#if XAMARIN_ANDROID
                        // See implementation in MainActivity.cs in the Android platform project.
                        bool permissionGranted = await MainActivity.Instance.AskForLocationPermission();
                        if (!permissionGranted)
                        {
                            throw new Exception("Location permission not granted.");
                        }
#endif
                        MyMapView.LocationDisplay.AutoPanMode = LocationDisplayAutoPanMode.Recenter;
                        await MyMapView.LocationDisplay.DataSource.StartAsync();
                        MyMapView.LocationDisplay.IsEnabled = true;
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex);
                        await Application.Current.MainPage.DisplayAlert("Couldn't start location", ex.Message, "OK");
                    }
                }
            };

            // Add a graphics overlay for the drawn pipes.
            MyMapView.GraphicsOverlays.Add(_pipesOverlay);
            _pipesOverlay.Renderer = new SimpleRenderer(new SimpleLineSymbol(SimpleLineSymbolStyle.Solid, System.Drawing.Color.Red, 2));

            // Set the SketchEditor for the map.
            MyMapView.SketchEditor = _sketchEditor;

            try
            {
                // Create an elevation source and Surface.
                _elevationSource = new ArcGISTiledElevationSource(new Uri("https://elevation3d.arcgis.com/arcgis/rest/services/WorldElevation3D/Terrain3D/ImageServer"));
                await _elevationSource.LoadAsync();
                _elevationSurface = new Surface();
                _elevationSurface.ElevationSources.Add(_elevationSource);
                await _elevationSurface.LoadAsync();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                await Application.Current.MainPage.DisplayAlert("Error", "Failed to load elevation.", "OK");
            }

            // Enable the add button.
            AddButton.IsEnabled = true;
        }

        private void DoneClicked(object sender, EventArgs e)
        {
            // Check if sketch can be completed.
            if (MyMapView.SketchEditor.CompleteCommand.CanExecute(null))
            {
                // Complete the sketch.
                MyMapView.SketchEditor.CompleteCommand.Execute(null);

                // Disable the editing buttons.
                DoneButton.IsEnabled = UndoButton.IsEnabled = RedoButton.IsEnabled = false;
            }
        }

        private async void AddSketch(object sender, EventArgs e)
        {
            // Enable the editing buttons.
            DoneButton.IsEnabled = UndoButton.IsEnabled = RedoButton.IsEnabled = true;

            // Prevent the user from adding pipes concurrently.
            AddButton.IsEnabled = false;

            // Get the geometry of the user drawn line.
            Geometry geometry = await _sketchEditor.StartAsync(SketchCreationMode.Polyline);
            AddButton.IsEnabled = true;

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
                double elevationOffset = ElevationSlider.Value;

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
                    HelpLabel.Text = string.Format("Pipe added {0:0.0}m below surface", elevationOffset * -1);
                }
                else if (elevationOffset == 0)
                {
                    HelpLabel.Text = "Pipe added at ground level";
                }
                else
                {
                    HelpLabel.Text = string.Format("Pipe added {0:0.0}m above the surface", elevationOffset);
                }

                // Enable the view button once a pipe has been added to the graphics overlay.
                ViewButton.IsEnabled = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
        private void UndoClicked(object sender, EventArgs e) { if (MyMapView.SketchEditor.UndoCommand.CanExecute(null)) MyMapView.SketchEditor.UndoCommand.Execute(null); }
        private void RedoClicked(object sender, EventArgs e) { if (MyMapView.SketchEditor.RedoCommand.CanExecute(null)) MyMapView.SketchEditor.RedoCommand.Execute(null); }

        private void ViewClicked(object sender, EventArgs e)
        {
            // Stop the current location source.
            MyMapView.LocationDisplay.DataSource.StopAsync();

            // Set the field for sharing data between views.
            PipeViewer.PipeGraphics = _pipesOverlay.Graphics.Select(x => new Graphic(x.Geometry));

            // Load the routeviewer as a new page on the navigation stack.
            Navigation.PopAsync();
            Navigation.PushAsync(new PipeViewer() { }, true);
        }
    }
}