// Copyright 2022 Esri.
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

using Colors = System.Drawing.Color;

namespace ArcGIS.Samples.DisplayGrid
{
    [ArcGIS.Samples.Shared.Attributes.Sample(
        name: "Display grid",
        category: "MapView",
        description: "Display and customize coordinate system grids including Latitude/Longitude, MGRS, UTM and USNG on a map view or scene view.",
        instructions: "Use the controls to change the grid settings. You can change the view from 2D or 3D, select the type of grid from `Grid Type` (LatLong, MGRS, UTM, and USNG) and modify its properties like label visibility, grid line color, grid label color, label formatting, and label offset.",
        tags: new[] { "MGRS", "USNG", "UTM", "coordinates", "degrees", "graticule", "grid", "latitude", "longitude", "minutes", "seconds" })]
    public partial class DisplayGrid : ContentPage
    {
        public DisplayGrid()
        {
            InitializeComponent();

            // Create the UI, setup the control references and execute initialization
            Initialize();
        }

        private void Initialize()
        {
            // Set up the map and scene with basemaps.
            MyMapView.Map = new Map(BasemapStyle.ArcGISImagery);
            MySceneView.Scene = new Scene(BasemapStyle.ArcGISImageryStandard);

            // Add an elevation source to the scene.
            var elevationSource = new ArcGISTiledElevationSource(new Uri(
                "https://elevation3d.arcgis.com/arcgis/rest/services/WorldElevation3D/Terrain3D/ImageServer"));
            MySceneView.Scene.BaseSurface.ElevationSources.Add(elevationSource);

            // Configure the UI options.
            GridTypePicker.ItemsSource = new[] { "LatLong", "MGRS", "UTM", "USNG" };
            string[] colorItemsSource = { "Red", "Green", "Blue", "White", "Purple" };
            GridColorPicker.ItemsSource = colorItemsSource;
            LabelColorPicker.ItemsSource = colorItemsSource;
            HaloColorPicker.ItemsSource = colorItemsSource;
            LabelPositionPicker.ItemsSource = Enum.GetNames(typeof(GridLabelPosition));
            LabelFormatPicker.ItemsSource = Enum.GetNames(typeof(LatitudeLongitudeGridLabelFormat));

            foreach (Picker combo in new Picker[] { GridTypePicker, GridColorPicker, LabelColorPicker, LabelPositionPicker, LabelFormatPicker })
            {
                combo.SelectedIndex = 0;
            }

            // Update the halo color to have a good default.
            HaloColorPicker.SelectedIndex = 3;

            // Handle grid type changes so that the format option can be disabled for non-latlong grids.
            GridTypePicker.SelectedIndexChanged += (o, e) =>
            {
                LabelFormatPicker.IsEnabled = GridTypePicker.SelectedItem.ToString() == "LatLong";
            };

            // Subscribe to the button click events.
            ApplySettingsButton.Clicked += ApplySettingsButton_Clicked;

            // Enable the action button.
            ApplySettingsButton.IsEnabled = true;

            // Zoom to a default scale that will show the grid labels if they are enabled.
            MyMapView.SetViewpointCenterAsync(
                new MapPoint(-7702852.905619, 6217972.345771, SpatialReferences.WebMercator), 23227);

            // Apply default settings.
            ApplySettingsButton_Clicked(this, null);

            // Close the settings window when user taps the greyed out GeoView.
            GridSettingsWindowBackground.GestureRecognizers.Add(new TapGestureRecognizer
            {
                Command = new Command(() =>
                {
                    CloseGridSettingsWindow();
                })
            });
        }

        private void ApplySettingsButton_Clicked(object sender, EventArgs e)
        {
            Esri.ArcGISRuntime.UI.Grid grid;

            // First, update the grid based on the type selected.
            switch (GridTypePicker.SelectedItem.ToString())
            {
                case "LatLong":
                    grid = new LatitudeLongitudeGrid();
                    // Apply the label format setting.
                    string selectedFormatString = LabelFormatPicker.SelectedItem.ToString();
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
            grid.IsLabelVisible = LabelVisibilitySwitch.IsToggled;

            // Next, apply the grid visibility setting.
            grid.IsVisible = GridVisibilitySwitch.IsToggled;

            // Next, apply the grid color and label color settings for each zoom level.
            for (long level = 0; level < grid.LevelCount; level++)
            {
                // Set the line symbol.
                Symbol lineSymbol = new SimpleLineSymbol(SimpleLineSymbolStyle.Solid,
                    Colors.FromName(GridColorPicker.SelectedItem.ToString()), 2);
                grid.SetLineSymbol(level, lineSymbol);

                // Set the text symbol.
                Symbol textSymbol = new TextSymbol
                {
                    Color = Colors.FromName(LabelColorPicker.SelectedItem.ToString()),
                    OutlineColor = Colors.FromName(HaloColorPicker.SelectedItem.ToString()),
                    Size = 16,
                    HaloColor = Colors.FromName(HaloColorPicker.SelectedItem.ToString()),
                    HaloWidth = 3
                };
                grid.SetTextSymbol(level, textSymbol);
            }

            // Next, apply the label position setting.
            grid.LabelPosition =
                (GridLabelPosition)Enum.Parse(typeof(GridLabelPosition), LabelPositionPicker.SelectedItem.ToString());

            // Set the label offset.
            grid.LabelOffset = LabelOffsetSlider.Value;

            // Apply the updated grid.
            // Show the correct GeoView.
            if (MapViewRadioButton.IsChecked == true)
            {
                MyMapView.Grid = grid;
                MyMapView.IsVisible = true;
                MySceneView.IsVisible = false;
            }
            else
            {
                MySceneView.Grid = grid;
                MySceneView.IsVisible = true;
                MyMapView.IsVisible = false;
            }

            CloseGridSettingsWindow();
        }

        private void ChangeSettingsButton_Clicked(object sender, EventArgs e)
        {
            GridSettingsWindow.IsVisible = true;
            GridSettingsWindowBackground.IsVisible = true;
        }

        private void CloseGridSettingsWindow()
        {
            GridSettingsWindowBackground.IsVisible = false;
            GridSettingsWindow.IsVisible = false;
        }
    }
}