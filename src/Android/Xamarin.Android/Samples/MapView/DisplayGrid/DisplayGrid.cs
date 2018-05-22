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
using Colors = System.Drawing.Color;

namespace ArcGISRuntime.Samples.DisplayGrid
{
    [Activity]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Display a grid",
        "MapView",
        "Display and work with coordinate grid systems such as Latitude/Longitude, MGRS, UTM and USNG on a map view. This includes toggling labels visibility, changing the color of the grid lines, and changing the color of the grid labels.",
        "Choose the grid settings and then tap 'Apply settings' to see them applied.")]
    [ArcGISRuntime.Samples.Shared.Attributes.AndroidLayout("DisplayGrid.axml")]
    public class DisplayGrid : Activity
    {
        // Variables for referring to the UI controls.
        private MapView _myMapView;

        private Button _applySettingsButton;
        private Spinner _gridTypeSpinner;
        private Spinner _labelVisibilitySpinner;
        private Spinner _gridVisibilitySpinner;
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
            _myMapView.Map = new Map(Basemap.CreateImageryWithLabelsVector());

            // Configure the UI options.
            _gridTypeSpinner.Adapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleSpinnerItem, new[] { "LatLong", "MGRS", "UTM", "USNG" });
            var visibilityItemsSource = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleSpinnerItem, new[] { "Visible", "Invisible" });
            _labelVisibilitySpinner.Adapter = visibilityItemsSource;
            _gridVisibilitySpinner.Adapter = visibilityItemsSource;
            var colorItemsSource = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleSpinnerItem, new[] { "Red", "Green", "Blue", "White" });
            _gridColorSpinner.Adapter = colorItemsSource;
            _labelColorSpinner.Adapter = colorItemsSource;
            _labelPositionSpinner.Adapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleSpinnerItem, Enum.GetNames(typeof(GridLabelPosition)));
            _labelFormatSpinner.Adapter = new ArrayAdapter<string>(this, Android.Resource.Layout.SimpleSpinnerItem, Enum.GetNames(typeof(LatitudeLongitudeGridLabelFormat)));
            foreach (Spinner spinner in new[] { _gridTypeSpinner, _labelVisibilitySpinner, _gridVisibilitySpinner, _gridColorSpinner, _labelColorSpinner, _labelPositionSpinner, _labelFormatSpinner })
            {
                spinner.SetSelection(0);
            }

            // Subscribe to the button click event.
            _applySettingsButton.Click += _applySettingsButton_Click;

            // Enable the action button.
            _applySettingsButton.Enabled = true;
        }

        private void _applySettingsButton_Click(object sender, EventArgs e)
        {
            // First, update the grid based on the type selected.
            switch (_gridTypeSpinner.SelectedItem.ToString())
            {
                case "LatLong":
                    _myMapView.Grid = new LatitudeLongitudeGrid();
                    // Apply the label format setting.
                    string selectedFormatString = _labelFormatSpinner.SelectedItem.ToString();
                    ((LatitudeLongitudeGrid)_myMapView.Grid).LabelFormat =
                        (LatitudeLongitudeGridLabelFormat)Enum.Parse(typeof(LatitudeLongitudeGridLabelFormat), selectedFormatString);
                    break;

                case "MGRS":
                    _myMapView.Grid = new MgrsGrid();
                    break;

                case "UTM":
                    _myMapView.Grid = new UtmGrid();
                    break;

                case "USNG":
                    _myMapView.Grid = new UsngGrid();
                    break;
            }

            // Next, apply the label visibility setting.
            switch (_labelVisibilitySpinner.SelectedItem.ToString())
            {
                case "Visible":
                    _myMapView.Grid.IsLabelVisible = true;
                    break;

                case "Invisible":
                    _myMapView.Grid.IsLabelVisible = false;
                    break;
            }

            // Next, apply the grid visibility setting.
            switch (_gridVisibilitySpinner.SelectedItem.ToString())
            {
                case "Visible":
                    _myMapView.Grid.IsVisible = true;
                    break;

                case "Invisible":
                    _myMapView.Grid.IsVisible = false;
                    break;
            }

            // Next, apply the grid color and label color settings for each zoom level.
            for (long level = 0; level < _myMapView.Grid.LevelCount; level++)
            {
                // Set the line symbol.
                string lineColor = ((ArrayAdapter<string>)_gridColorSpinner.Adapter).GetItem(_gridColorSpinner.SelectedItemPosition);
                Symbol lineSymbol = new SimpleLineSymbol(SimpleLineSymbolStyle.Solid, Colors.FromName(lineColor), 2);
                _myMapView.Grid.SetLineSymbol(level, lineSymbol);

                // Set the text symbol.
                string labelColor = ((ArrayAdapter<String>)_labelColorSpinner.Adapter).GetItem(_labelColorSpinner.SelectedItemPosition);
                Symbol textSymbol = new TextSymbol
                {
                    Color = Colors.FromName(labelColor),
                    OutlineColor = Colors.Purple,
                    Size = 16,
                    HaloColor = Colors.Purple,
                    HaloWidth = 3
                };
                _myMapView.Grid.SetTextSymbol(level, textSymbol);
            }

            // Next, apply the label position setting.
            _myMapView.Grid.LabelPosition = (GridLabelPosition)Enum.Parse(typeof(GridLabelPosition), _labelPositionSpinner.SelectedItem.ToString());
        }

        private void CreateLayout()
        {
            // Load the layout for the sample from the .axml file.
            SetContentView(Resource.Layout.DisplayGrid);

            // Update control references to point to the controls defined in the layout.
            _applySettingsButton = FindViewById<Button>(Resource.Id.displayGrid_applySettingsButton);
            _gridTypeSpinner = FindViewById<Spinner>(Resource.Id.displayGrid_gridTypeSpinner);
            _labelVisibilitySpinner = FindViewById<Spinner>(Resource.Id.displayGrid_labelVisibilitySpinner);
            _gridVisibilitySpinner = FindViewById<Spinner>(Resource.Id.displayGrid_gridVisibilitySpinner);
            _gridColorSpinner = FindViewById<Spinner>(Resource.Id.displayGrid_gridColorSpinner);
            _labelColorSpinner = FindViewById<Spinner>(Resource.Id.displayGrid_labelColorSpinner);
            _labelPositionSpinner = FindViewById<Spinner>(Resource.Id.displayGrid_labelPositionSpinner);
            _labelFormatSpinner = FindViewById<Spinner>(Resource.Id.displayGrid_labelFormatSpinner);
            _myMapView = FindViewById<MapView>(Resource.Id.displayGrid_mapView);
        }
    }
}