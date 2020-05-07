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
using Android.Views;
using Android.Views.InputMethods;

namespace ArcGISRuntime.Samples.QueryFeatureCountAndExtent
{
    [Activity (ConfigurationChanges=Android.Content.PM.ConfigChanges.Orientation | Android.Content.PM.ConfigChanges.ScreenSize)]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Query feature count and extent",
        "Analysis",
        "Zoom to features matching a query and count the features in the current visible extent.",
        "Use the button to zoom to the extent of the state specified (by abbreviation) in the textbox or use the button to count the features in the current extent.",
        "count", "feature layer", "feature table", "features", "filter", "number", "query")]
    public class QueryFeatureCountAndExtent : Activity
    {
        // UI controls.
        private EditText _myStateEntry;
        private TextView _myResultsLabel;
        private Button _myQueryStateButton;
        private Button _myQueryExtentButton;
        private MapView _myMapView;

        // URL to the feature service.
        private readonly Uri _medicareHospitalSpendLayer =
            new Uri("https://services1.arcgis.com/4yjifSiIG17X0gW4/arcgis/rest/services/Medicare_Hospital_Spending_per_Patient/FeatureServer/0");

        // Feature table to query.
        private ServiceFeatureTable _featureTable;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            Title = "Query feature count and extent";

            CreateLayout();
            Initialize();
        }

        private async void Initialize()
        {
            // Create the map with a basemap.
            Map myMap = new Map(Basemap.CreateDarkGrayCanvasVector());

            // Create the feature table from the service URL.
            _featureTable = new ServiceFeatureTable(_medicareHospitalSpendLayer);

            // Create the feature layer from the table.
            FeatureLayer myFeatureLayer = new FeatureLayer(_featureTable);

            // Add the feature layer to the map.
            myMap.OperationalLayers.Add(myFeatureLayer);

            try
            {
                // Wait for the feature layer to load.
                await myFeatureLayer.LoadAsync();

                // Set the map initial extent to the extent of the feature layer.
                myMap.InitialViewpoint = new Viewpoint(myFeatureLayer.FullExtent);

                // Add the map to the MapView.
                _myMapView.Map = myMap;
            }
            catch (Exception e)
            {
                new AlertDialog.Builder(this).SetMessage(e.ToString()).SetTitle("Error").Show();
            }
        }

        private async void BtnZoomToFeatures_Click(object sender, EventArgs e)
        {
            // Create the query parameters.
            QueryParameters queryStates = new QueryParameters { WhereClause = $"upper(State) LIKE '%{_myStateEntry.Text.ToUpper()}%'" };

            try
            {
                // Get the extent from the query.
                Envelope resultExtent = await _featureTable.QueryExtentAsync(queryStates);

                // Return if there is no result (might happen if query is invalid).
                if (resultExtent?.SpatialReference == null)
                {
                    _myResultsLabel.Text = $"Couldn't zoom to features in {_myStateEntry.Text}.";
                    return;
                }

                // Create a viewpoint from the extent.
                Viewpoint resultViewpoint = new Viewpoint(resultExtent);

                // Zoom to the viewpoint.
                await _myMapView.SetViewpointAsync(resultViewpoint);

                // Update label.
                _myResultsLabel.Text = $"Zoomed to features in {_myStateEntry.Text}.";
            }
            catch (Exception ex)
            {
                new AlertDialog.Builder(this).SetMessage(ex.ToString()).SetTitle("Error").Show();
            }
        }

        private async void BtnCountFeatures_Click(object sender, EventArgs e)
        {
            try
            {
                // Get the current visible extent.
                Geometry currentExtent = _myMapView.GetCurrentViewpoint(ViewpointType.BoundingGeometry).TargetGeometry;

                // Create the query parameters.
                QueryParameters queryCityCount = new QueryParameters
                {
                    Geometry = currentExtent,
                    // Specify the interpretation of the Geometry query parameters.
                    SpatialRelationship = SpatialRelationship.Intersects
                };

                // Get the count of matching features.
                long count = await _featureTable.QueryFeatureCountAsync(queryCityCount);

                // Update the UI.
                _myResultsLabel.Text = $"{count} features in extent";
            }
            catch (NullReferenceException exception)
            {
                // Sample wasn't ready.
                System.Diagnostics.Debug.WriteLine(exception);
            }
            catch (Exception)
            {
                // Uncaught exception in async void will crash application.
            }
        }

        private void CreateLayout()
        {
            // Create a new vertical layout for the app.
            LinearLayout layout = new LinearLayout(this) { Orientation = Orientation.Vertical };

            // Create the entry.
            _myStateEntry = new EditText(this) { Hint = "State abbreviation (e.g. NY)" };

            // Hide the keyboard on enter.
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

            // Create the results label.
            _myResultsLabel = new TextView(this);

            // Create the two buttons.
            _myQueryStateButton = new Button(this) { Text = "Zoom to matching features" };
            _myQueryExtentButton = new Button(this) { Text = "Count features in extent" };

            // Subscribe to button events.
            _myQueryExtentButton.Click += BtnCountFeatures_Click;
            _myQueryStateButton.Click += BtnZoomToFeatures_Click;

            // Add the views to the layout.
            layout.AddView(_myStateEntry);
            layout.AddView(_myQueryStateButton);
            layout.AddView(_myQueryExtentButton);
            layout.AddView(_myResultsLabel);
            _myMapView = new MapView(this);
            layout.AddView(_myMapView);

            // Show the layout in the app.
            SetContentView(layout);
        }
    }
}