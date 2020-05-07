// Copyright 2016 Esri.
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
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.UI.Controls;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using Android.Text;

namespace ArcGISRuntime.Samples.FeatureLayerQuery
{
    [Activity (ConfigurationChanges=Android.Content.PM.ConfigChanges.Orientation | Android.Content.PM.ConfigChanges.ScreenSize)]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Feature layer query",
        "Data",
        "Find features in a feature table which match an SQL query.",
        "Input the name of a U.S. state into the text field. When you tap the button, a query is performed and the matching features are highlighted or an error is returned.",
        "query", "search")]
    public class FeatureLayerQuery : Activity
    {
        // Create reference to service of US States
        private string _statesUrl = "https://services.arcgis.com/jIL9msH9OI208GCb/arcgis/rest/services/USA_Daytime_Population_2016/FeatureServer/0";

        // Hold a reference to the map view
        private MapView _myMapView;

        // Create globally available text box for easy referencing
        private EditText _queryTextBox;

        // Create globally available feature table for easy referencing
        private ServiceFeatureTable _featureTable;

        // Create globally available feature layer for easy referencing
        private FeatureLayer _featureLayer;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            Title = "Feature layer  query";

            // Create the UI, setup the control references and execute initialization
            CreateLayout();
            Initialize();
        }

        private void Initialize()
        {
            // Create new Map with basemap
            Map myMap = new Map(Basemap.CreateTopographic());

            // Create and set initial map location
            MapPoint initialLocation = new MapPoint(
                -11000000, 5000000, SpatialReferences.WebMercator);
            myMap.InitialViewpoint = new Viewpoint(initialLocation, 100000000);

            // Create feature table using a url
            _featureTable = new ServiceFeatureTable(new Uri(_statesUrl));

            // Create feature layer using this feature table
            _featureLayer = new FeatureLayer(_featureTable)
            {
                // Set the Opacity of the Feature Layer
                Opacity = 0.6,
                // Work around service setting
                MaxScale = 10
            };

            // Create a new renderer for the States Feature Layer.
            SimpleLineSymbol lineSymbol = new SimpleLineSymbol(SimpleLineSymbolStyle.Solid, Color.Black, 1);
            SimpleFillSymbol fillSymbol = new SimpleFillSymbol(SimpleFillSymbolStyle.Solid, Color.Transparent, lineSymbol);

            // Set States feature layer renderer
            _featureLayer.Renderer = new SimpleRenderer(fillSymbol);

            // Add feature layer to the map
            myMap.OperationalLayers.Add(_featureLayer);

            // Update the selection color
            _myMapView.SelectionProperties.Color = Color.Cyan;

            // Assign the map to the MapView
            _myMapView.Map = myMap;
        }

        private async void OnQueryClicked(object sender, EventArgs e)
        {
            // Remove any previous feature selections that may have been made
            _featureLayer.ClearSelection();

            // Begin query process
            await QueryStateFeature(_queryTextBox.Text);
        }

        private async Task QueryStateFeature(string stateName)
        {
            // Create dialog to display alert information
            AlertDialog.Builder alert = new AlertDialog.Builder(this);

            try
            {
                // Create a query parameters that will be used to Query the feature table
                QueryParameters queryParams = new QueryParameters
                {

                    // Construct and assign the where clause that will be used to query the feature table
                    WhereClause = "upper(STATE_NAME) LIKE '%" + stateName.Trim().ToUpper() + "%'"
                };

                // Query the feature table
                FeatureQueryResult queryResult = await _featureTable.QueryFeaturesAsync(queryParams);

                // Cast the QueryResult to a List so the results can be interrogated.
                List<Feature> features = queryResult.ToList();

                if (features.Any())
                {
                    // Create an envelope.
                    EnvelopeBuilder envBuilder = new EnvelopeBuilder(SpatialReferences.WebMercator);

                    // Loop over each feature from the query result.
                    foreach (Feature feature in features)
                    {
                        // Add the extent of each matching feature to the envelope.
                        envBuilder.UnionOf(feature.Geometry.Extent);

                        // Select each feature.
                        _featureLayer.SelectFeature(feature);
                    }

                    // Zoom to the extent of the selected feature(s).
                    await _myMapView.SetViewpointGeometryAsync(envBuilder.ToGeometry(), 50);
                }
                else
                {
                    alert.SetTitle("State Not Found!");
                    alert.SetMessage("Add a valid state name.");
                    alert.Show();
                }
            }
            catch (Exception ex)
            {
                alert.SetTitle("Sample Error");
                alert.SetMessage(ex.Message);
                alert.Show();
            }
        }

        private void CreateLayout()
        {
            // Create a new vertical layout for the app
            LinearLayout layout = new LinearLayout(this) { Orientation = Orientation.Vertical };

            // Create new Text box that will take the query parameter
            _queryTextBox = new EditText(this)
            {
                InputType = InputTypes.ClassText | InputTypes.TextVariationNormal
            };
            _queryTextBox.SetMaxLines(1);

            // Create Button that will start the Feature Query
            Button queryButton = new Button(this)
            {
                Text = "Query"
            };
            queryButton.Click += OnQueryClicked;

            // Create and add a help label
            TextView helpLabel = new TextView(this)
            {
                Text = "Enter the name of a state, then press 'Query' to search."
            };
            layout.AddView(helpLabel);

            // Add TextBox to the layout
            layout.AddView(_queryTextBox);

            // Add Button to the layout
            layout.AddView(queryButton);

            // Add the map view to the layout
            _myMapView = new MapView(this);
            layout.AddView(_myMapView);

            // Show the layout in the app
            SetContentView(layout);
        }
    }
}