// Copyright 2018 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.UI;
using System;
using Microsoft.UI.Xaml.Controls;
using Esri.ArcGISRuntime.Geometry;
using Colors = System.Drawing.Color;
using Grid = Esri.ArcGISRuntime.UI.Grid;
using Symbol = Esri.ArcGISRuntime.Symbology.Symbol;

namespace ArcGISRuntime.WinUI.Samples.DisplayGrid
{
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        name: "Display grid",
        category: "MapView",
        description: "Display coordinate system grids including Latitude/Longitude, MGRS, UTM and USNG on a map view. Also, toggle label visibility and change the color of grid lines and grid labels.",
        instructions: "Select type of grid from the types (LatLong, MGRS, UTM and USNG) and modify its properties like label visibility, grid line color, and grid label color. Press the button to apply these settings.",
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
            // Set up the map view with a basemap.
            MyMapView.Map = new Map(BasemapStyle.ArcGISImagery);

            // Configure the UI options.
            gridTypeCombo.ItemsSource = new[] { "LatLong", "MGRS", "UTM", "USNG" };
            string[] colorItemsSource = { "Red", "Green", "Blue", "White", "Purple" };
            gridColorCombo.ItemsSource = colorItemsSource;
            labelColorCombo.ItemsSource = colorItemsSource;
            haloColorCombo.ItemsSource = colorItemsSource;
            labelPositionCombo.ItemsSource = Enum.GetNames(typeof(GridLabelPosition));
            labelFormatCombo.ItemsSource = Enum.GetNames(typeof(LatitudeLongitudeGridLabelFormat));
            foreach (ComboBox combo in new[] { gridTypeCombo, gridColorCombo, labelColorCombo, labelPositionCombo, labelFormatCombo })
            {
                combo.SelectedIndex = 0;
            }

            // Apply a good default halo color selection.
            haloColorCombo.SelectedIndex = 3;

            // Subscribe to grid type change events in order to disable the format change option when it doesn't apply.
            gridTypeCombo.SelectionChanged += (o, e) =>
            {
                labelFormatCombo.IsEnabled = gridTypeCombo.SelectedItem.ToString() == "LatLong";
            };

            // Subscribe to the button click event.
            applySettingsButton.Click += ApplySettingsButton_Click;

            // Enable the action button.
            applySettingsButton.IsEnabled = true;

            // Zoom to a default scale that will show the grid labels if they are enabled.
            MyMapView.SetViewpointCenterAsync(
                new MapPoint(-7702852.905619, 6217972.345771, SpatialReferences.WebMercator), 23227);

            // Apply default settings.
            ApplySettingsButton_Click(this, null);
        }

        private void ApplySettingsButton_Click(object sender, Microsoft.UI.Xaml.RoutedEventArgs e)
        {
            Grid grid;

            // First, update the grid based on the type selected.
            switch (gridTypeCombo.SelectedValue.ToString())
            {
                case "LatLong":
                    grid = new LatitudeLongitudeGrid();
                    // Apply the label format setting.
                    string selectedFormatString = labelFormatCombo.SelectedValue.ToString();
                    ((LatitudeLongitudeGrid)grid).LabelFormat =
                        (LatitudeLongitudeGridLabelFormat)Enum.Parse(typeof(LatitudeLongitudeGridLabelFormat), selectedFormatString);
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
            grid.IsLabelVisible = labelVisibilityCheckbox.IsChecked.Value;

            // Next, apply the grid visibility setting.
            grid.IsVisible = gridVisibilityCheckbox.IsChecked.Value;

            // Next, apply the grid color and label color settings for each zoom level.
            for (long level = 0; level < grid.LevelCount; level++)
            {
                // Set the line symbol.
                Symbol lineSymbol = new SimpleLineSymbol(SimpleLineSymbolStyle.Solid, Colors.FromName(gridColorCombo.SelectedItem.ToString()), 2);
                grid.SetLineSymbol(level, lineSymbol);

                // Set the text symbol.
                Symbol textSymbol = new TextSymbol
                {
                    Color = Colors.FromName(labelColorCombo.SelectedItem.ToString()),
                    OutlineColor = Colors.FromName(haloColorCombo.SelectedItem.ToString()),
                    Size = 16,
                    HaloColor = Colors.FromName(haloColorCombo.SelectedItem.ToString()),
                    HaloWidth = 3
                };
                grid.SetTextSymbol(level, textSymbol);
            }

            // Next, apply the label position setting.
            grid.LabelPosition = (GridLabelPosition)Enum.Parse(typeof(GridLabelPosition), labelPositionCombo.SelectedValue.ToString());

            // Apply the updated grid.
            MyMapView.Grid = grid;
        }
    }
}