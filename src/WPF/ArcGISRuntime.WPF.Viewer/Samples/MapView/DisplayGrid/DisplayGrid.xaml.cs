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
using Colors = System.Drawing.Color;

namespace ArcGISRuntime.WPF.Samples.DisplayGrid
{
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Display a grid",
        "MapView",
        "Display and work with coordinate grid systems such as Latitude/Longitude, MGRS, UTM and USNG on a map view. This includes toggling labels visibility, changing the color of the grid lines, and changing the color of the grid labels.",
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
            gridTypeCombo.ItemsSource = new[] { "LatLong", "MGRS", "UTM", "USNG" };
            var visibilityItemsSource = new[] { "Visible", "Invisible" };
            labelVisibilityCombo.ItemsSource = visibilityItemsSource;
            gridVisibilityCombo.ItemsSource = visibilityItemsSource;
            var colorItemsSource = new[] { Colors.Red, Colors.Green, Colors.Blue, Colors.White };
            gridColorCombo.ItemsSource = colorItemsSource;
            labelColorCombo.ItemsSource = colorItemsSource;
            labelPositionCombo.ItemsSource = Enum.GetNames(typeof(GridLabelPosition));
            labelFormatCombo.ItemsSource = Enum.GetNames(typeof(LatitudeLongitudeGridLabelFormat));
            foreach (var combo in new[] { gridTypeCombo, labelVisibilityCombo, gridVisibilityCombo, gridColorCombo, labelColorCombo, labelPositionCombo, labelFormatCombo })
            {
                combo.SelectedIndex = 0;
            }

            // Subscribe to the button click event.
            applySettingsButton.Click += ApplySettingsButton_Click;

            // Enable the action button.
            applySettingsButton.IsEnabled = true;
        }

        private void ApplySettingsButton_Click(object sender, RoutedEventArgs e)
        {
            // First, update the grid based on the type selected.
            switch (gridTypeCombo.SelectedValue.ToString())
            {
                case "LatLong":
                    MyMapView.Grid = new LatitudeLongitudeGrid();
                    // Apply the label format setting.
                    string selectedFormatString = labelFormatCombo.SelectedValue.ToString();
                    ((LatitudeLongitudeGrid)MyMapView.Grid).LabelFormat =
                        (LatitudeLongitudeGridLabelFormat)Enum.Parse(typeof(LatitudeLongitudeGridLabelFormat), selectedFormatString);
                    break;

                case "MGRS":
                    MyMapView.Grid = new MgrsGrid();
                    break;

                case "UTM":
                    MyMapView.Grid = new UtmGrid();
                    break;

                case "USNG":
                    MyMapView.Grid = new UsngGrid();
                    break;
            }

            // Next, apply the label visibility setting.
            switch (labelVisibilityCombo.SelectedValue.ToString())
            {
                case "Visible":
                    MyMapView.Grid.IsLabelVisible = true;
                    break;

                case "Invisible":
                    MyMapView.Grid.IsLabelVisible = false;
                    break;
            }

            // Next, apply the grid visibility setting.
            switch (gridVisibilityCombo.SelectedValue.ToString())
            {
                case "Visible":
                    MyMapView.Grid.IsVisible = true;
                    break;

                case "Invisible":
                    MyMapView.Grid.IsVisible = false;
                    break;
            }

            // Next, apply the grid color and label color settings for each zoom level.
            for (long level = 0; level < MyMapView.Grid.LevelCount; level++)
            {
                // Set the line symbol.
                Symbol lineSymbol = new SimpleLineSymbol(SimpleLineSymbolStyle.Solid, (Colors)gridColorCombo.SelectedValue, 2);
                MyMapView.Grid.SetLineSymbol(level, lineSymbol);

                // Set the text symbol.
                Symbol textSymbol = new TextSymbol
                {
                    Color = (Colors)labelColorCombo.SelectedValue,
                    OutlineColor = Colors.Purple,
                    Size = 16,
                    HaloColor = Colors.Purple,
                    HaloWidth = 3
                };
                MyMapView.Grid.SetTextSymbol(level, textSymbol);
            }

            // Next, apply the label position setting.
            MyMapView.Grid.LabelPosition = (GridLabelPosition)Enum.Parse(typeof(GridLabelPosition), labelPositionCombo.SelectedValue.ToString());
        }
    }
}