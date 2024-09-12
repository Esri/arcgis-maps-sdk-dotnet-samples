﻿// Copyright 2024 Esri.
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
using Esri.ArcGISRuntime.Maui;
using Esri.ArcGISRuntime.Toolkit.Maui;
using Esri.ArcGISRuntime.UI;
using Esri.ArcGISRuntime.UI.Editing;
using Microsoft.Maui.ApplicationModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Map = Esri.ArcGISRuntime.Mapping.Map;

namespace ArcGIS.Samples.SnapGeometryEdits
{
    [ArcGIS.Samples.Shared.Attributes.Sample(
        name: "Snap geometry edits",
        category: "Geometry",
        description: "Use the Geometry Editor to edit a geometry and align it to existing geometries on a map.",
        instructions: "To create a geometry, press the create button to choose the geometry type you want to create (i.e. points, multipoints, polyline, or polygon) and interactively tap and drag on the map view to create the geometry.",
        tags: new[] { "edit", "feature", "geometry editor", "graphics", "layers", "map", "snapping" })]
    public partial class SnapGeometryEdits : ContentPage
    {
        // Hold references for use in event handlers.
        private GeometryEditor _geometryEditor;
        private GraphicsOverlay _graphicsOverlay;
        private Graphic _selectedGraphic;
        private List<Button> _geometryEditorToolButtons;

        public SnapGeometryEdits()
        {
            InitializeComponent();
            _ = Initialize();
        }

        private async Task Initialize()
        {
            // Create a map using a Uri.
            var myMap = new Map(new Uri("https://www.arcgis.com/home/item.html?id=b95fe18073bc4f7788f0375af2bb445e"));

            // Set the map load setting feature tiling mode.
            // Enabled with full resolution when supported is used to ensure that snapping to geometries occurs in full resolution.
            // Snapping in full resolution improves snapping accuracy.
            myMap.LoadSettings.FeatureTilingMode = FeatureTilingMode.EnabledWithFullResolutionWhenSupported;

            // Set the initial viewpoint.
            myMap.InitialViewpoint = new Viewpoint(new MapPoint(-9812798, 5126406, SpatialReferences.WebMercator), 2000);

            // Create a graphics overlay and add it to the map view.
            _graphicsOverlay = new GraphicsOverlay();
            MyMapView.GraphicsOverlays.Add(_graphicsOverlay);

            // Add the map to the map view.
            MyMapView.Map = myMap;

            // Create and add a geometry editor to the map view.
            _geometryEditor = new GeometryEditor();
            MyMapView.GeometryEditor = _geometryEditor;

            // Load the map.
            await myMap.LoadAsync();

            // Ensure all layers are loaded before setting the snap settings.
            // If this is not awaited there is a risk that operational layers may not have loaded and therefore would not have been included in the snap sources.
            await Task.WhenAll(MyMapView.Map.OperationalLayers.ToList().Select(layer => layer.LoadAsync()).ToList());

            // Set the snap source settings.
            SetSnapSettings();

            // Show the create and edit geometries UI.
            CreateEditGeometriesPanel.IsVisible = true;

            // Store a reference to the geometry editor tool buttons to update their background color when selected.
            _geometryEditorToolButtons = new List<Button>()
            {
                PointButton,
                PolylineButton,
                PolygonButton,
                MultipointButton
            };

            // Hide the popup when user taps screen.
            SnappingSettingsPopup.GestureRecognizers.Add(new TapGestureRecognizer
            {
                Command = new Command(() =>
                {
                    SnappingSettingsPopup.IsVisible = false;
                })
            });

            // Add an event handler to detect geoview tapped events.
            MyMapView.GeoViewTapped += MyMapView_GeoViewTapped;
        }

        private void SetSnapSettings()
        {
            // Synchronize the snap source collection with the map's operational layers. 
            // Note that layers that have not been loaded will not synchronize.
            _geometryEditor.SnapSettings.SyncSourceSettings();

            // Enable snapping on the geometry layer.
            _geometryEditor.SnapSettings.IsEnabled = true;

            // Create a list of snap source settings with snapping disabled.
            List<SnapSourceSettingsVM> snapSourceSettingsVMs = _geometryEditor.SnapSettings.SourceSettings.Select(sourceSettings => new SnapSourceSettingsVM(sourceSettings) { IsEnabled = false }).ToList();

            // Populate lists of snap source settings for point and polyline layers.
            PointSnapSettingsList.ItemsSource = snapSourceSettingsVMs.Where(snapSourceSettingVM => snapSourceSettingVM.GeometryType == GeometryType.Point).ToList();
            PolylineSnapSettingsList.ItemsSource = snapSourceSettingsVMs.Where(snapSourceSettingVM => snapSourceSettingVM.GeometryType == GeometryType.Polyline).ToList();

            // Populate a list of snap source settings for graphics overlays.
            GraphicsOverlaySnapSettingsList.ItemsSource = snapSourceSettingsVMs.Where(snapSourceSettingsVMs => snapSourceSettingsVMs.SnapSourceSettings.Source is GraphicsOverlay).ToList();
        }

        private void CreateNewGraphic()
        {
            // Get the new geometry from the geometry editor.
            Geometry geometry = _geometryEditor.Stop();

            // Create a graphic.
            var graphic = new Graphic(geometry);

            // Create a geometry editor style to get symbols for the new graphic.
            var geometryEditorStyle = new GeometryEditorStyle();

            switch (geometry.GeometryType)
            {
                case GeometryType.Point:
                    graphic.Symbol = geometryEditorStyle.VertexSymbol;
                    break;
                case GeometryType.Envelope:
                    graphic.Symbol = geometryEditorStyle.LineSymbol;
                    break;
                case GeometryType.Polyline:
                    graphic.Symbol = geometryEditorStyle.LineSymbol;
                    break;
                case GeometryType.Polygon:
                    graphic.Symbol = geometryEditorStyle.FillSymbol;
                    break;
                case GeometryType.Multipoint:
                    graphic.Symbol = geometryEditorStyle.VertexSymbol;
                    break;
            }

            // Add the graphic to the GraphicsOverlay and unselect it.
            _graphicsOverlay.Graphics.Add(graphic);
            graphic.IsSelected = false;
        }

        private async void MyMapView_GeoViewTapped(object sender, GeoViewInputEventArgs e)
        {
            // If the geometry editor is active then stop.
            if (_geometryEditor.IsStarted) return;

            try
            {
                // Get the list of identified graphics overlay results based on tap position.
                IReadOnlyList<IdentifyGraphicsOverlayResult> results = await MyMapView.IdentifyGraphicsOverlaysAsync(e.Position, 10, false);

                // If a graphics overlay result has been tapped and contains a corresponding graphic,
                // set the selected graphic and start the geometry editor.
                if (results.Any() && results[0].Graphics.Any())
                {
                    _selectedGraphic = results[0].Graphics[0];
                    _selectedGraphic.IsSelected = true;
                }
                else
                {
                    // No results have been found, update the selected graphic.
                    _selectedGraphic = null;
                }
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error editing", ex.Message, "OK");

                // Reset the UI.
                ResetFromEditingSession();
                return;
            }

            if (_selectedGraphic == null) return;

            // Hide the selected graphic and start an editing session with a copy of it.
            _geometryEditor.Start(_selectedGraphic.Geometry);
            _selectedGraphic.IsVisible = false;
        }

        // Reset the UI after the editor stops.
        private void ResetFromEditingSession()
        {
            // Reset the selected graphic.
            if (_selectedGraphic != null)
            {
                _selectedGraphic.IsSelected = false;
                _selectedGraphic.IsVisible = true;
            }

            // Reset the geometry tool button colors.
            foreach (var button in _geometryEditorToolButtons)
            {
                switch (Application.Current.RequestedTheme)
                {
                    case AppTheme.Dark:
                        button.BackgroundColor = Color.FromArgb("#3C3C3C");
                        break;
                    default:
                        button.BackgroundColor = Colors.White;
                        break;
                }
            }

            // Update the selected graphic.
            _selectedGraphic = null;
        }

        #region Enable Snapping Button Handlers
        // Enable all point layer snap sources.
        private void EnableAllSnapSettingsButton_Click(object sender, EventArgs e)
        {
            _geometryEditor.SnapSettings.IsEnabled = true;
            _geometryEditor.SnapSettings.IsGeometryGuidesEnabled = true;
            _geometryEditor.SnapSettings.IsFeatureSnappingEnabled = true;
        }

        // Enable all point layer snap sources.
        private void EnableAllPointSnapSourceButton_Click(object sender, EventArgs e)
        {
            foreach (var item in PointSnapSettingsList.ItemsSource)
            {
                if (item is SnapSourceSettingsVM snapSourceSettingsVM)
                {
                    snapSourceSettingsVM.IsEnabled = true;
                }
            }
        }

        // Enable all polyline layer snap sources.
        private void EnableAllPolylineSnapSourceButton_Click(object sender, EventArgs e)
        {
            foreach (var item in PolylineSnapSettingsList.ItemsSource)
            {
                if (item is SnapSourceSettingsVM snapSourceSettingsVM)
                {
                    snapSourceSettingsVM.IsEnabled = true;
                }
            }
        }
        #endregion

        #region Geometry Management Button Handlers
        private void DeleteButton_Click(object sender, EventArgs e)
        {
            _geometryEditor.DeleteSelectedElement();
        }

        private void UndoButton_Click(object sender, EventArgs e)
        {
            _geometryEditor.Undo();
        }

        private void SaveButton_Click(object sender, EventArgs e)
        {
            if (_selectedGraphic?.Geometry != null)
            {
                _selectedGraphic.Geometry = _geometryEditor.Stop();
                _selectedGraphic.IsSelected = false;
            }
            else if (_geometryEditor.IsStarted)
            {
                CreateNewGraphic();
            }

            ResetFromEditingSession();
        }

        private void DiscardButton_Click(object sender, EventArgs e)
        {
            _geometryEditor.Stop();

            ResetFromEditingSession();
        }
        #endregion

        #region Geometry Tool Buttons Handlers
        private void ReticleVertexToolSwitch_Toggled(object sender, ToggledEventArgs e)
        {
            if (ReticleVertexToolSwitch.IsToggled)
            {
                _geometryEditor.Tool = new ReticleVertexTool();
            }
            else
            {
                _geometryEditor.Tool = new VertexTool();
            }
        }

        private void PointButton_Click(object sender, EventArgs e)
        {
            if (_geometryEditor.IsStarted)
            {
                _geometryEditor.Stop();
            }

            ResetFromEditingSession();

            PointButton.BackgroundColor = Application.Current.RequestedTheme == AppTheme.Dark ? Colors.Gray : Colors.LightGray;
            _geometryEditor.Start(GeometryType.Point);
        }

        private void MultipointButton_Click(object sender, EventArgs e)
        {
            if (_geometryEditor.IsStarted)
            {
                _geometryEditor.Stop();
            }

            ResetFromEditingSession();

            MultipointButton.BackgroundColor = Application.Current.RequestedTheme == AppTheme.Dark ? Colors.Gray : Colors.LightGray;
            _geometryEditor.Start(GeometryType.Multipoint);
        }

        private void PolylineButton_Click(object sender, EventArgs e)
        {
            if (_geometryEditor.IsStarted)
            {
                _geometryEditor.Stop();
            }

            ResetFromEditingSession();

            PolylineButton.BackgroundColor = Application.Current.RequestedTheme == AppTheme.Dark ? Colors.Gray : Colors.LightGray;
            _geometryEditor.Start(GeometryType.Polyline);
        }
        private void PolygonButton_Click(object sender, EventArgs e)
        {
            if (_geometryEditor.IsStarted)
            {
                _geometryEditor.Stop();
            }

            ResetFromEditingSession();

            PolygonButton.BackgroundColor = Application.Current.RequestedTheme == AppTheme.Dark ? Colors.Gray : Colors.LightGray;
            _geometryEditor.Start(GeometryType.Polygon);
        }
        #endregion

        #region Snap Settings Visibility Button Handlers
        private void ShowSnapSettingsButton_Clicked(object sender, EventArgs e)
        {
            SnappingSettingsPopup.IsVisible = true;
        }
        #endregion
    }

    public class SnapSourceSettingsVM : INotifyPropertyChanged
    {
        public SnapSourceSettings SnapSourceSettings { get; set; }

        // Wrap the snap source settings in a view model to expose them to the UI. 
        public SnapSourceSettingsVM(SnapSourceSettings snapSourceSettings)
        {
            SnapSourceSettings = snapSourceSettings;

            if (snapSourceSettings.Source is FeatureLayer featureLayer)
            {
                Name = featureLayer.Name;

                if (featureLayer.FeatureTable != null)
                {
                    GeometryType = featureLayer.FeatureTable.GeometryType;
                }
            }
            else if (snapSourceSettings.Source is GraphicsOverlay graphicsOverlay)
            {
                Name = "Editor graphics overlay";
            }

            IsEnabled = snapSourceSettings.IsEnabled;
        }

        private string _name;
        public string Name
        {
            get
            {
                return _name;
            }
            set
            {
                _name = value;
                OnPropertyChanged();
            }
        }

        private bool _isEnabled;
        public bool IsEnabled
        {
            get
            {
                return _isEnabled;
            }
            set
            {
                _isEnabled = value;
                SnapSourceSettings.IsEnabled = value;
                OnPropertyChanged();
            }
        }

        public GeometryType GeometryType { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}