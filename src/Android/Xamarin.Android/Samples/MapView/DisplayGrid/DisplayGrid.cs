// Copyright 2018 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using Android.App;
using Android.OS;
using Android.Widget;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.UI;
using Esri.ArcGISRuntime.UI.Controls;
using System;
using Esri.ArcGISRuntime.Geometry;
using Colors = System.Drawing.Color;

namespace ArcGISRuntime.Samples.DisplayGrid
{
    [Activity (ConfigurationChanges=Android.Content.PM.ConfigChanges.Orientation | Android.Content.PM.ConfigChanges.ScreenSize)]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        name: "Display grid",
        category: "MapView",
        description: "Display coordinate system grids including Latitude/Longitude, MGRS, UTM and USNG on a map view. Also, toggle label visibility and change the color of grid lines and grid labels.",
        instructions: "Select type of grid from the types (LatLong, MGRS, UTM and USNG) and modify its properties like label visibility, grid line color, and grid label color. Press the button to apply these settings.",
        tags: new[] { "MGRS", "USNG", "UTM", "coordinates", "degrees", "graticule", "grid", "latitude", "longitude", "minutes", "seconds" })]
    [ArcGISRuntime.Samples.Shared.Attributes.AndroidLayout("DisplayGrid.axml")]
    public class DisplayGrid : Activity
    {
        // Variables for referring to the UI controls.
        private MapView _myMapView;

        private Button _applySettingsButton;
        private Spinner _gridTypeSpinner;
        private Switch _labelVisibilitySwitch;
        private Switch _gridVisibilitySwitch;
        private Spinner _gridColorSpinner;
        private Spinner _labelColorSpinner;
        private Spinner _labelPositionSpinner;
        private Spinner _labelFormatSpinner;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            Title = "Display a grid";

            // Create the UI, setup the control references and execute initialization
            CreateLayout();
            Initialize();
        }

        private void Initialize()
        {
            // Set up the map view with a basemap.
            _myMapView.Map = new Map(Basemap.CreateImageryWithLabels());

            // Configure the UI options.
            _gridTypeSpinner.Adapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleSpinnerItem, new[] { "LatLong", "MGRS", "UTM", "USNG" });
            ArrayAdapter<string> colorItemsSource = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleSpinnerItem, new[] { "Red", "Green", "Blue", "White", "Purple" });
            _gridColorSpinner.Adapter = colorItemsSource;
            _labelColorSpinner.Adapter = colorItemsSource;
            _labelPositionSpinner.Adapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleSpinnerItem, Enum.GetNames(typeof(GridLabelPosition)));
            _labelFormatSpinner.Adapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleSpinnerItem, Enum.GetNames(typeof(LatitudeLongitudeGridLabelFormat)));
            foreach (Spinner spinner in new[] { _gridTypeSpinner, _gridColorSpinner, _labelColorSpinner, _labelPositionSpinner, _labelFormatSpinner })
            {
                spinner.SetSelection(0);
            }

            // Handle grid type changes so that the format option can be disabled for non-latlong grids.
            _gridTypeSpinner.ItemSelected += (o, e) =>
            {
                _labelFormatSpinner.Enabled = _gridTypeSpinner.SelectedItem.ToString() == "LatLong";
            };

            // Subscribe to the button click event.
            _applySettingsButton.Click += _applySettingsButton_Click;

            // Enable the action button.
            _applySettingsButton.Enabled = true;

            // Zoom to a default scale that will show the grid labels if they are enabled.
            _myMapView.SetViewpointCenterAsync(
                new MapPoint(-7702852.905619, 6217972.345771, SpatialReferences.WebMercator), 23227);

            // Apply default settings.
            _applySettingsButton_Click(this, null);
        }

        private void _applySettingsButton_Click(object sender, EventArgs e)
        {
            Grid grid;

            // First, update the grid based on the type selected.
            switch (_gridTypeSpinner.SelectedItem.ToString())
            {
                case "LatLong":
                   grid = new LatitudeLongitudeGrid();
                    // Apply the label format setting.
                    string selectedFormatString = _labelFormatSpinner.SelectedItem.ToString();
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
            grid.IsLabelVisible = _labelVisibilitySwitch.Checked;

            // Next, apply the grid visibility setting.
            grid.IsVisible = _gridVisibilitySwitch.Checked;

            // Next, apply the grid color and label color settings for each zoom level.
            for (long level = 0; level < grid.LevelCount; level++)
            {
                // Set the line symbol.
                string lineColor = ((ArrayAdapter<string>)_gridColorSpinner.Adapter).GetItem(_gridColorSpinner.SelectedItemPosition);
                Symbol lineSymbol = new SimpleLineSymbol(SimpleLineSymbolStyle.Solid, Colors.FromName(lineColor), 2);
                grid.SetLineSymbol(level, lineSymbol);

                // Set the text symbol.
                string labelColor = ((ArrayAdapter<String>)_labelColorSpinner.Adapter).GetItem(_labelColorSpinner.SelectedItemPosition);
                Symbol textSymbol = new TextSymbol
                {
                    Color = Colors.FromName(labelColor),
                    Size = 16,
                    FontWeight = FontWeight.Bold
                };
                grid.SetTextSymbol(level, textSymbol);
            }

            // Next, apply the label position setting.
            grid.LabelPosition = (GridLabelPosition)Enum.Parse(typeof(GridLabelPosition), _labelPositionSpinner.SelectedItem.ToString());

            // Apply the updated grid.
            _myMapView.Grid = grid;
        }

        private void CreateLayout()
        {
            // Load the layout for the sample from the .axml file.
            SetContentView(Resource.Layout.DisplayGrid);

            // Update control references to point to the controls defined in the layout.
            _applySettingsButton = FindViewById<Button>(Resource.Id.displayGrid_applySettingsButton);
            _gridTypeSpinner = FindViewById<Spinner>(Resource.Id.displayGrid_gridTypeSpinner);
            _labelVisibilitySwitch = FindViewById<Switch>(Resource.Id.displayGrid_labelVisibilitySwitch);
            _gridVisibilitySwitch = FindViewById<Switch>(Resource.Id.displayGrid_gridVisibilitySwitch);
            _gridColorSpinner = FindViewById<Spinner>(Resource.Id.displayGrid_gridColorSpinner);
            _labelColorSpinner = FindViewById<Spinner>(Resource.Id.displayGrid_labelColorSpinner);
            _labelPositionSpinner = FindViewById<Spinner>(Resource.Id.displayGrid_labelPositionSpinner);
            _labelFormatSpinner = FindViewById<Spinner>(Resource.Id.displayGrid_labelFormatSpinner);
            _myMapView = FindViewById<MapView>(Resource.Id.displayGrid_mapView);
        }
    }
}