﻿using Android.App;
using Android.OS;
using Android.Widget;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.UI;
using Esri.ArcGISRuntime.UI.Controls;
using System;

namespace ArcGISRuntimeXamarin.Samples.ViewHiddenInfrastructureAR
{
    [Activity(ConfigurationChanges = Android.Content.PM.ConfigChanges.Orientation | Android.Content.PM.ConfigChanges.ScreenSize)]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "View hidden infrastructure in AR",
        "Augmented reality",
        "",
        "")]
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

        // Graphics overlay to host sketch graphics
        private GraphicsOverlay _pipesOverlay = new GraphicsOverlay();

        private SketchEditor _sketchEditor = new SketchEditor();

        private ArcGISTiledElevationSource _elevationSource;
        private Surface _elevationSurface;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            Title = "Sketch on map";

            // Create the UI
            CreateLayout();

            // Initialize controls, set up event handlers, etc.
            Initialize();
        }

        private async void Initialize()
        {
            // Create and add the map.
            _mapView.Map = new Map(Basemap.CreateImagery());

            // Configure location display.
            _mapView.LocationDisplay.AutoPanMode = LocationDisplayAutoPanMode.Recenter;
            await _mapView.LocationDisplay.DataSource.StartAsync();
            _mapView.LocationDisplay.IsEnabled = true;

            _mapView.GraphicsOverlays.Add(_pipesOverlay);
            _pipesOverlay.Renderer = new SimpleRenderer(new SimpleLineSymbol(SimpleLineSymbolStyle.Solid, System.Drawing.Color.Red, 2));

            _mapView.SketchEditor = _sketchEditor;

            _elevationSource = new ArcGISTiledElevationSource(new Uri("https://elevation3d.arcgis.com/arcgis/rest/services/WorldElevation3D/Terrain3D/ImageServer"));

            await _elevationSource.LoadAsync();
            _elevationSurface = new Surface();
            _elevationSurface.ElevationSources.Add(_elevationSource);
            await _elevationSurface.LoadAsync();
            _addButton.Enabled = true;
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

            _addButton.Click += AddClicked;
            _undoButton.Click += (s, e) => { if (_mapView.SketchEditor.UndoCommand.CanExecute(null)) _mapView.SketchEditor.UndoCommand.Execute(null); };
            _redoButton.Click += (s, e) => { if (_mapView.SketchEditor.RedoCommand.CanExecute(null)) _mapView.SketchEditor.RedoCommand.Execute(null); };
            _doneButton.Click += DoneClicked;
            _viewButton.Click += ViewClicked;
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

        private async void AddClicked(object sender, EventArgs e)
        {
            _doneButton.Enabled = _undoButton.Enabled = _redoButton.Enabled = true;

            _addButton.Enabled = false;
            Geometry geometry = await _sketchEditor.StartAsync(SketchCreationMode.Polyline);
            _addButton.Enabled = true;

            if (!(geometry is Polyline))
            {
                return;
            }

            MapPoint firstPoint = ((Polyline)geometry).Parts[0].StartPoint;
            try
            {
                // Get the users selected elevation offset.
                double elevationOffset = _elevationSlider.Progress;

                // Get the elevation of the geometry.
                double elevation = await _elevationSurface.GetElevationAsync(firstPoint);

                // Create a polyline for the pipe.
                Polyline elevatedLine = GeometryEngine.SetZ(geometry, elevation + elevationOffset) as Polyline;

                // Create a graphic for the pipe.
                Graphic linegraphic = new Graphic(elevatedLine);
                _pipesOverlay.Graphics.Add(linegraphic);

                // Display a message with the pipes offset from the surface.
                if (elevationOffset < 0)
                {
                    _helpLabel.Text = $"Pipe added {elevationOffset * -1}m below surface";
                }
                else if (elevationOffset == 0)
                {
                    _helpLabel.Text = "Pipe added at ground level";
                }
                else
                {
                    _helpLabel.Text = $"Pipe added {elevationOffset}m above the surface";
                }
            }
            catch (Exception)
            {
            }
            finally
            {
                _viewButton.Enabled = true;
            }
        }

        private void ViewClicked(object sender, EventArgs e)
        {
        }
    }
}