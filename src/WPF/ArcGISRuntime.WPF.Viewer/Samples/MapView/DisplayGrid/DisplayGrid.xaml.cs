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
using System.Windows;
using System.Windows.Controls;
using Esri.ArcGISRuntime.Geometry;
using Colors = System.Drawing.Color;
using Grid = Esri.ArcGISRuntime.UI.Grid;

namespace ArcGISRuntime.WPF.Samples.DisplayGrid
{
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Display a grid",
        "MapView",
        "This sample demonstrates how to display and work with coordinate grid systems such as Latitude/Longitude, MGRS, UTM and USNG on a map view. This includes toggling labels visibility, changing the color of the grid lines, and changing the color of the grid labels.",
        "Choose the grid settings and then tap 'Apply settings' to see them applied.")]
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
            MyMapView.Map = new Map(Basemap.CreateImageryWithLabelsVector());

            // Configure the UI options.
            GridTypeCombo.ItemsSource = new[] {"LatLong", "MGRS", "UTM", "USNG"};
            Colors[] colorItemsSource = {Colors.Red, Colors.Green, Colors.Blue, Colors.White, Colors.Purple};
            GridColorCombo.ItemsSource = colorItemsSource;
            LabelColorCombo.ItemsSource = colorItemsSource;
            HaloColorCombo.ItemsSource = colorItemsSource;
            LabelPositionCombo.ItemsSource = Enum.GetNames(typeof(GridLabelPosition));
            LabelFormatCombo.ItemsSource = Enum.GetNames(typeof(LatitudeLongitudeGridLabelFormat));
            ComboBox[] boxes = {GridTypeCombo, GridColorCombo, LabelColorCombo, HaloColorCombo, LabelPositionCombo, LabelFormatCombo};
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
                    ((LatitudeLongitudeGrid) grid).LabelFormat =
                        (LatitudeLongitudeGridLabelFormat) Enum.Parse(typeof(LatitudeLongitudeGridLabelFormat),
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
                    (Colors) GridColorCombo.SelectedValue, 2);
                grid.SetLineSymbol(level, lineSymbol);

                // Set the text symbol.
                Symbol textSymbol = new TextSymbol
                {
                    Color = (Colors) LabelColorCombo.SelectedValue,
                    OutlineColor = (Colors) HaloColorCombo.SelectedValue,
                    Size = 16,
                    HaloColor = (Colors) HaloColorCombo.SelectedValue,
                    HaloWidth = 3
                };
                grid.SetTextSymbol(level, textSymbol);
            }

            // Next, apply the label position setting.
            grid.LabelPosition =
                (GridLabelPosition) Enum.Parse(typeof(GridLabelPosition), LabelPositionCombo.SelectedValue.ToString());

            // Apply the updated grid.
            MyMapView.Grid = grid;
        }
    }
}