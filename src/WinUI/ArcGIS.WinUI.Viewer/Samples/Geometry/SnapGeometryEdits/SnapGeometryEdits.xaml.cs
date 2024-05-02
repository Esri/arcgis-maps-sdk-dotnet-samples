// Copyright 2024 Esri.
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
using Esri.ArcGISRuntime.Portal;
using Esri.ArcGISRuntime.UI;
using Esri.ArcGISRuntime.UI.Controls;
using Esri.ArcGISRuntime.UI.Editing;
using Microsoft.UI.Xaml;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace ArcGIS.WinUI.Samples.SnapGeometryEdits
{
    [ArcGIS.Samples.Shared.Attributes.Sample(
        "Snap geometry edits",
        "Geometry",
        "Use the Geometry Editor to edit a geometry and align it to existing geometries on a map.",
        "")]
    [ArcGIS.Samples.Shared.Attributes.OfflineData("b95fe18073bc4f7788f0375af2bb445e")]
    public partial class SnapGeometryEdits : INotifyPropertyChanged
    {
        private GeometryEditor _geometryEditor;
        private GraphicsOverlay _graphicsOverlay;
        private Graphic _selectedGraphic;

        public SnapGeometryEdits()
        {
            InitializeComponent();
            _ = Initialize();
        }

        private async Task Initialize()
        {
            ArcGISPortal portal = await ArcGISPortal.CreateAsync();
            PortalItem portalItem = await PortalItem.CreateAsync(portal, "b95fe18073bc4f7788f0375af2bb445e");

            // Create a map using a portal item.
            var myMap = new Map(portalItem);

            // Set the map load setting feature tiling mode.
            // Enabled with full resolution when supported is used to ensure that we are snapping to geometries in full resolution to improve snapping accuracy. 
            myMap.LoadSettings.FeatureTilingMode = FeatureTilingMode.EnabledWithFullResolutionWhenSupported;

            // Set the initial viewpoint.
            myMap.InitialViewpoint = new Viewpoint(new MapPoint(-9812798, 5126406, SpatialReferences.WebMercator), 2000);

            // Create a graphic, graphics overlay and geometry editor.
            _graphicsOverlay = new GraphicsOverlay();
            MyMapView.GraphicsOverlays.Add(_graphicsOverlay);

            // Add the map to the map view.
            MyMapView.Map = myMap;

            _geometryEditor = new GeometryEditor();
            MyMapView.GeometryEditor = _geometryEditor;

            await myMap.LoadAsync();

            // Ensure all layers are loaded before setting the snap settings.
            await Task.WhenAll(MyMapView.Map.OperationalLayers.ToList().Select(layer => layer.LoadAsync()).ToList());

            SetSnapSettings();
            SnappingControls.Visibility = Visibility.Visible;

            MyMapView.GeoViewTapped += MyMapView_GeoViewTapped;
        }

        private void SetSnapSettings()
        {
            if (!_geometryEditor.SnapSettings.SourceSettings.Any())
            {
                _geometryEditor.SnapSettings.SyncSourceSettings();
                _geometryEditor.SnapSettings.IsEnabled = true;
            }

            var snapSourceSettingsVMs = _geometryEditor.SnapSettings.SourceSettings.Select(sourceSettings => new SnapSourceSettingsVM(sourceSettings) { IsEnabled = false }).ToList();

            PointSnapSettingsList.ItemsSource = snapSourceSettingsVMs.Where(snapSourceSettingVM => snapSourceSettingVM.GeometryType == GeometryType.Point).ToList();
            PolylineSnapSettingsList.ItemsSource = snapSourceSettingsVMs.Where(snapSourceSettingVM => snapSourceSettingVM.GeometryType == GeometryType.Polyline).ToList();
        }

        private void CreateNewGraphic()
        {
            var geometry = _geometryEditor.Stop();
            var graphic = new Graphic(geometry);
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
            if (_geometryEditor.IsStarted) return;

            try
            {
                IReadOnlyList<IdentifyGraphicsOverlayResult> results = await MyMapView.IdentifyGraphicsOverlaysAsync(e.Position, 10, false);

                _selectedGraphic = results.FirstOrDefault()?.Graphics?.FirstOrDefault();

                if (results.Any() && results[0].Graphics.Any())
                {
                    _selectedGraphic = results.First().Graphics.First();
                    _selectedGraphic.IsSelected = true;
                    _geometryEditor.Start(_selectedGraphic.Geometry);
                }
                else
                {
                    _selectedGraphic = null;
                }
            }
            catch (Exception ex)
            {
                await new MessageDialog2(ex.Message).ShowAsync();

                ResetFromEditingSession();
            }

            if (_selectedGraphic == null) return;

            _selectedGraphic.IsSelected = true;

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

            _selectedGraphic = null;
        }

        private void EnableAllPointSnapSourceButton_Click(object sender, RoutedEventArgs e)
        {
            foreach (var item in PointSnapSettingsList.Items.ToList())
            {
                if (item is SnapSourceSettingsVM snapSourceSettingsVM)
                {
                    snapSourceSettingsVM.IsEnabled = true;
                }
            }
        }

        private void EnableAllPolylineSnapSourceButton_Click(object sender, RoutedEventArgs e)
        {
            foreach (var item in PolylineSnapSettingsList.Items.ToList())
            {
                if (item is SnapSourceSettingsVM snapSourceSettingsVM)
                {
                    snapSourceSettingsVM.IsEnabled = true;
                }
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            _geometryEditor.DeleteSelectedElement();
        }

        private void UndoButton_Click(object sender, RoutedEventArgs e)
        {
            _geometryEditor.Undo();
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
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

        private void DiscardButton_Click(object sender, RoutedEventArgs e)
        {
            _geometryEditor.Stop();

            ResetFromEditingSession();
        }

        private void PointButton_Click(object sender, RoutedEventArgs e)
        {
            if (_geometryEditor.IsStarted)
            {
                _geometryEditor.Stop();
            }

            ResetFromEditingSession();

            _geometryEditor.Start(GeometryType.Point);
        }

        private void MultipointButton_Click(object sender, RoutedEventArgs e)
        {
            if (_geometryEditor.IsStarted)
            {
                _geometryEditor.Stop();
            }

            ResetFromEditingSession();

            _geometryEditor.Start(GeometryType.Multipoint);
        }
        private void PolylineButton_Click(object sender, RoutedEventArgs e)
        {
            if (_geometryEditor.IsStarted)
            {
                _geometryEditor.Stop();
            }

            ResetFromEditingSession();

            _geometryEditor.Start(GeometryType.Polyline);
        }
        private void PolygonButton_Click(object sender, RoutedEventArgs e)
        {
            if (_geometryEditor.IsStarted)
            {
                _geometryEditor.Stop();
            }

            ResetFromEditingSession();

            _geometryEditor.Start(GeometryType.Polygon);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class SnapSourceSettingsVM : INotifyPropertyChanged
    {
        public SnapSourceSettings SnapSourceSettings { get; set; }

        public SnapSourceSettingsVM(SnapSourceSettings snapSourceSettings)
        {
            SnapSourceSettings = snapSourceSettings;

            if (snapSourceSettings.Source is FeatureLayer flayer && flayer.FeatureTable != null)
            {
                Name = flayer.Name;
                GeometryType = flayer.FeatureTable.GeometryType;
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