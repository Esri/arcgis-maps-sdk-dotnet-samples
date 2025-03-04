// Copyright 2018 Esri.
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
using System.Windows;
using System.Windows.Controls;
using Colors = System.Drawing.Color;
using Grid = Esri.ArcGISRuntime.UI.Grid;

namespace ArcGIS.WPF.Samples.DisplayGrid
{
    [ArcGIS.Samples.Shared.Attributes.Sample(
        name: "Display grid",
        category: "MapView",
        description: "Display and customize coordinate system grids including Latitude/Longitude, MGRS, UTM and USNG on a map view or scene view.",
        instructions: "Use the controls to change the grid settings. You can change the view from 2D or 3D, select the type of grid from `Grid Type` (LatLong, MGRS, UTM, and USNG) and modify its properties like label visibility, grid line color, grid label color, label formatting, and label offset.",
        tags: new[] { "MGRS", "USNG", "UTM", "coordinates", "degrees", "graticule", "grid", "latitude", "longitude", "minutes", "seconds" })]
    public partial class DisplayGrid
    {
        public DisplayGrid()
        {
            InitializeComponent();

            Initialize();
        }

        private void Initialize()
        {
            // Set up map and scene with basemaps.
            MyMapView.Map = new Map(BasemapStyle.ArcGISImagery);
            MySceneView.Scene = new Scene(BasemapStyle.ArcGISImagery);

            // Add an elevation source to the scene.
            var elevationSource = new ArcGISTiledElevationSource(new Uri(
                "https://elevation3d.arcgis.com/arcgis/rest/services/WorldElevation3D/Terrain3D/ImageServer"));
            MySceneView.Scene.BaseSurface.ElevationSources.Add(elevationSource);

            // Configure the UI options.
            GridTypeCombo.ItemsSource = new[] { "LatLong", "MGRS", "UTM", "USNG" };
            Colors[] colorItemsSource = { Colors.Red, Colors.Green, Colors.Blue, Colors.White, Colors.Purple };
            GridColorCombo.ItemsSource = colorItemsSource;
            LabelColorCombo.ItemsSource = colorItemsSource;
            HaloColorCombo.ItemsSource = colorItemsSource;
            LabelPositionCombo.ItemsSource = Enum.GetNames(typeof(GridLabelPosition));
            LabelFormatCombo.ItemsSource = Enum.GetNames(typeof(LatitudeLongitudeGridLabelFormat));
            ComboBox[] boxes = { GridTypeCombo, GridColorCombo, LabelColorCombo, HaloColorCombo, LabelPositionCombo, LabelFormatCombo };
            foreach (ComboBox combo in boxes)
            {
                combo.SelectedIndex = 0;
            }

            // Update the halo color so it isn't the same as the text color.
            HaloColorCombo.SelectedIndex = 3;

            // Subscribe to change events so the label format combo can be disabled as necessary.
            GridTypeCombo.SelectionChanged += (o, e) =>
            {
                LabelFormatCombo.IsEnabled = GridTypeCombo.SelectedItem.ToString() == "LatLong";
            };

            // Subscribe to the button click event.
            ApplySettingsButton.Click += ApplySettingsButton_Click;

            // Enable the action button.
            ApplySettingsButton.IsEnabled = true;

            // Zoom to a default scale that will show the grid labels if they are enabled.
            MyMapView.SetViewpointCenterAsync(
                new MapPoint(-7702852.905619, 6217972.345771, SpatialReferences.WebMercator), 23227);

            // Apply default settings.
            ApplySettingsButton_Click(this, null);
        }

        private void ApplySettingsButton_Click(object sender, RoutedEventArgs e)
        {
            Grid grid;

            // First, update the grid based on the type selected.
            switch (GridTypeCombo.SelectedValue.ToString())
            {
                case "LatLong":
                    grid = new LatitudeLongitudeGrid();
                    // Apply the label format setting.
                    string selectedFormatString = LabelFormatCombo.SelectedValue.ToString();
                    ((LatitudeLongitudeGrid)grid).LabelFormat =
                        (LatitudeLongitudeGridLabelFormat)Enum.Parse(typeof(LatitudeLongitudeGridLabelFormat),
                            selectedFormatString);
                    break;

                case "MGRS":
                    grid = new MgrsGrid();
                    break;

                case "UTM":
                    grid = new UtmGrid();
                    break;

                case "USNG":
                default:
                    grid = new UsngGrid();
                    break;
            }

            // Next, apply the label visibility setting.
            grid.IsLabelVisible = LabelVisibilityCheckbox.IsChecked.Value;
            grid.IsVisible = GridVisibilityCheckbox.IsChecked.Value;

            // Next, apply the grid color and label color settings for each zoom level.
            for (long level = 0; level < grid.LevelCount; level++)
            {
                // Set the line symbol.
                Symbol lineSymbol = new SimpleLineSymbol(SimpleLineSymbolStyle.Solid,
                    (Colors)GridColorCombo.SelectedValue, 2);
                grid.SetLineSymbol(level, lineSymbol);

                // Set the text symbol.
                Symbol textSymbol = new TextSymbol
                {
                    Color = (Colors)LabelColorCombo.SelectedValue,
                    OutlineColor = (Colors)HaloColorCombo.SelectedValue,
                    Size = 16,
                    HaloColor = (Colors)HaloColorCombo.SelectedValue,
                    HaloWidth = 3
                };
                grid.SetTextSymbol(level, textSymbol);
            }

            // Next, apply the label position setting.
            grid.LabelPosition =
                (GridLabelPosition)Enum.Parse(typeof(GridLabelPosition), LabelPositionCombo.SelectedValue.ToString());

            // Set the label offset.
            grid.LabelOffset = LabelOffsetSlider.Value;

            // Apply the updated grid.
            // Show the correct GeoView.
            if (MapViewRadioButton.IsChecked == true)
            {
                MyMapView.Grid = grid;
                MyMapView.Visibility = Visibility.Visible;
                MySceneView.Visibility = Visibility.Collapsed;
            }
            else
            {
                MySceneView.Grid = grid;
                MySceneView.Visibility = Visibility.Visible;
                MyMapView.Visibility = Visibility.Collapsed;
            }
        }
    }
}