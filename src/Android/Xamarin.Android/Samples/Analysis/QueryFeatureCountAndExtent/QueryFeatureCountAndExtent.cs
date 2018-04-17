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
using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.UI.Controls;
using System;
using Android.Content;
using Android.Views;
using Android.Views.InputMethods;

namespace ArcGISRuntime.Samples.QueryFeatureCountAndExtent
{
    [Activity]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Query feature count and extent",
        "Analysis",
        "This sample demonstrates how to query a feature table, in this case returning a count, for features that are within the visible extent or that meet specified criteria.",
        "Use the button to zoom to the extent of the state specified (by abbreviation) in the textbox or use the button to count the features in the current extent.")]
    public class QueryFeatureCountAndExtent : Activity
    {
        // Search box for state entry
        private EditText _myStateEntry;

        // Label for results
        private TextView _myResultsLabel;

        // Button for querying cities by state
        private Button _myQueryStateButton;

        // Button for querying cities within the current extent
        private Button _myQueryExtentButton;

        // Create and hold reference to the used MapView
        private readonly MapView _myMapView = new MapView();

        // URL to the feature service
        private readonly Uri _usaCitiesSource = new Uri("https://sampleserver6.arcgisonline.com/arcgis/rest/services/USA/MapServer/0");

        // Feature table to query
        private ServiceFeatureTable _myFeatureTable;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            Title = "Query feature count and extent";

            // Create the UI, setup the control references and execute initialization
            CreateLayout();
            Initialize();
        }

        private async void Initialize()
        {
            // Create the map with a vector street basemap
            Map myMap = new Map(Basemap.CreateStreetsVector());

            // Create the feature table from the service URL
            _myFeatureTable = new ServiceFeatureTable(_usaCitiesSource);

            // Create the feature layer from the table
            FeatureLayer myFeatureLayer = new FeatureLayer(_myFeatureTable);

            // Add the feature layer to the map
            myMap.OperationalLayers.Add(myFeatureLayer);

            // Wait for the feature layer to load
            await myFeatureLayer.LoadAsync();

            // Set the map initial extent to the extent of the feature layer
            myMap.InitialViewpoint = new Viewpoint(myFeatureLayer.FullExtent);

            // Add the map to the MapView
            _myMapView.Map = myMap;
        }

        private async void BtnZoomToFeatures_Click(object sender, EventArgs e)
        {
            // Create the query parameters
            QueryParameters queryStates = new QueryParameters { WhereClause = $"upper(ST) LIKE '%{_myStateEntry.Text.ToUpper()}%'" };

            // Get the extent from the query
            Envelope resultExtent = await _myFeatureTable.QueryExtentAsync(queryStates);

            // Return if there is no result (might happen if query is invalid)
            if (resultExtent?.SpatialReference == null)
            {
                return;
            }

            // Create a viewpoint from the extent
            Viewpoint resultViewpoint = new Viewpoint(resultExtent);

            // Zoom to the viewpoint
            await _myMapView.SetViewpointAsync(resultViewpoint);

            // Update label
            _myResultsLabel.Text = $"Zoomed to features in {_myStateEntry.Text}";
        }

        private async void BtnCountFeatures_Click(object sender, EventArgs e)
        {
            // Create the query parameters
            QueryParameters queryCityCount = new QueryParameters
            {
                // Get the current view extent and use that as a query parameters
                Geometry = _myMapView.GetCurrentViewpoint(ViewpointType.BoundingGeometry).TargetGeometry,

                // Specify the interpretation of the Geometry query parameters
                SpatialRelationship = SpatialRelationship.Intersects
            };

            // Get the count of matching features
            long count = await _myFeatureTable.QueryFeatureCountAsync(queryCityCount);

            // Update the UI
            _myResultsLabel.Text = $"{count} features in extent";
        }

        private void CreateLayout()
        {
            // Create a new vertical layout for the app
            var layout = new LinearLayout(this) { Orientation = Orientation.Vertical };

            // Create the entry
            _myStateEntry = new EditText(this) { Hint = "State abbreviation (e.g. NY)" };

            // Hide the keyboard on enter
            _myStateEntry.KeyPress += (sender, args) =>
            {
                if (args.Event.Action == KeyEventActions.Down && args.KeyCode == Keycode.Enter)
                {
                    InputMethodManager imm = (InputMethodManager)GetSystemService(InputMethodService);
                    imm.HideSoftInputFromWindow(_myStateEntry.WindowToken, 0);
                    BtnZoomToFeatures_Click(_myStateEntry, null);
                }
                else
                {
                    args.Handled = false;
                }
            };

            // Create the results label
            _myResultsLabel = new TextView(this);

            // Create the two buttons
            _myQueryStateButton = new Button(this) { Text = "Zoom to matching features" };
            _myQueryExtentButton = new Button(this) { Text = "Count features in extent" };

            // Subscribe to button events
            _myQueryExtentButton.Click += BtnCountFeatures_Click;
            _myQueryStateButton.Click += BtnZoomToFeatures_Click;

            // Add the views to the layout
            layout.AddView(_myStateEntry);
            layout.AddView(_myQueryStateButton);
            layout.AddView(_myQueryExtentButton);
            layout.AddView(_myResultsLabel);
            layout.AddView(_myMapView);

            // Show the layout in the app
            SetContentView(layout);
        }
    }
}