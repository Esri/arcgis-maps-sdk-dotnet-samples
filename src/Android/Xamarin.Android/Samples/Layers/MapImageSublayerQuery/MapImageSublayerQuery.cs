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
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.UI;
using Esri.ArcGISRuntime.UI.Controls;
using System;

namespace ArcGISRuntime.Samples.MapImageSublayerQuery
{
    [Activity]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Query a map image sublayer",
        "Layers",
        "This sample demonstrates how to execute an attribute and spatial query on the sublayers of an ArcGIS map image layer. Sublayers of an ArcGISMapImageLayer may expose a ServiceFeatureTable through a Table property.This allows you to perform the same queries available when working with a table from a FeatureLayer: attribute query, spatial query, statistics query, query for related features, and so on.",
        "1. Launch the sample, the map displays at an extent where individual states, counties, and cities can be seen clearly.\n2.Provide a numeric value for the population query(values under 1810000 will produce a selection in all layers).\n3.Click the `Query` button to find all features in the current map extent that have a population greater than the value entered.\n   - Any current selection is cleared from the map.\n   - If a non - numeric value was entered, an error message is displayed.\n4.All features(cities, counties, or states) meeting the query criteria are selected in the map.\n   - If no features meet the query criteria, a message displays stating zero features were selected.\n5.Experiment with different map extents and population values and see the results.",
        "Query", "Sublayer", "MapServer", "Table")]
    public class MapImageSublayerQuery : Activity
    {
        // MapView control for displaying the map.
        private MapView _myMapView = new MapView();

        // Use a private variable to reference the graphics overlay for showing selected features.
        private GraphicsOverlay _selectedFeaturesOverlay;

        // A text input for the population value to query with.
        private EditText _populationValueInput;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            Title = "Query a map image sublayer";

            // Create the UI and initialize the map.
            CreateLayout();
            Initialize();
        }

        private void Initialize()
        {
            // Create a new Map with a vector streets basemap.
            Map myMap = new Map(Basemap.CreateStreetsVector());

            // Create and set the map's initial view point.
            MapPoint initialLocation = new MapPoint(-12716000.00, 4170400.00, SpatialReferences.WebMercator);
            myMap.InitialViewpoint = new Viewpoint(initialLocation, 6000000);

            // Create the URI to the USA map service.
            Uri usaServiceUri = new Uri("https://sampleserver6.arcgisonline.com/arcgis/rest/services/USA/MapServer");

            // Create a new ArcGISMapImageLayer that uses the service URI.
            ArcGISMapImageLayer usaMapImageLayer = new ArcGISMapImageLayer(usaServiceUri);

            // Add the layer to the map.
            myMap.OperationalLayers.Add(usaMapImageLayer);

            // Assign the map to the MapView.
            _myMapView.Map = myMap;

            // Add a graphics overlay to show selected features.
            _selectedFeaturesOverlay = new GraphicsOverlay();
            _myMapView.GraphicsOverlays.Add(_selectedFeaturesOverlay);
        }

        private async void QuerySublayers_Click(object sender, EventArgs e)
        {
            // Clear selected features from the graphics overlay.
            _selectedFeaturesOverlay.Graphics.Clear();

            // If the population value entered is not numeric, warn the user and exit.
            double populationNumber = 0.0;
            if (!double.TryParse(_populationValueInput.Text.Trim(), out populationNumber))
            {
                Toast messageToast = Toast.MakeText(this.ApplicationContext, "Population value must be numeric.", ToastLength.Short);
                messageToast.Show();

                return;
            }

            // Get the USA map image layer (the first and only operational layer in the map).
            ArcGISMapImageLayer usaMapImageLayer = (ArcGISMapImageLayer)_myMapView.Map.OperationalLayers[0];

            // Use a utility method on the map image layer to load all the sublayers and tables.
            await usaMapImageLayer.LoadTablesAndLayersAsync();

            // Get the sublayers of interest (skip 'Highways' since it doesn't have the POP2000 field).
            ArcGISMapImageSublayer citiesSublayer = (ArcGISMapImageSublayer)usaMapImageLayer.Sublayers[0];
            ArcGISMapImageSublayer statesSublayer = (ArcGISMapImageSublayer)usaMapImageLayer.Sublayers[2];
            ArcGISMapImageSublayer countiesSublayer = (ArcGISMapImageSublayer)usaMapImageLayer.Sublayers[3];

            // Get the service feature table for each of the sublayers.
            ServiceFeatureTable citiesTable = citiesSublayer.Table;
            ServiceFeatureTable statesTable = statesSublayer.Table;
            ServiceFeatureTable countiesTable = countiesSublayer.Table;

            // Create the query parameters that will find features in the current extent with a population greater than the value entered.
            QueryParameters populationQuery = new QueryParameters
            {
                WhereClause = "POP2000 > " + _populationValueInput.Text,
                Geometry = _myMapView.GetCurrentViewpoint(ViewpointType.BoundingGeometry).TargetGeometry
            };

            // Query each of the sublayers with the query parameters.
            FeatureQueryResult citiesQueryResult = await citiesTable.QueryFeaturesAsync(populationQuery);
            FeatureQueryResult statesQueryResult = await statesTable.QueryFeaturesAsync(populationQuery);
            FeatureQueryResult countiesQueryResult = await countiesTable.QueryFeaturesAsync(populationQuery);

            // Display the selected cities in the graphics overlay.
            SimpleMarkerSymbol citySymbol = new SimpleMarkerSymbol(SimpleMarkerSymbolStyle.Circle, System.Drawing.Color.Red, 16);
            foreach (Feature city in citiesQueryResult)
            {
                Graphic cityGraphic = new Graphic(city.Geometry, citySymbol);

                _selectedFeaturesOverlay.Graphics.Add(cityGraphic);
            }

            // Display the selected counties in the graphics overlay.
            SimpleLineSymbol countyLineSymbol = new SimpleLineSymbol(SimpleLineSymbolStyle.Dash, System.Drawing.Color.Cyan, 2);
            SimpleFillSymbol countySymbol = new SimpleFillSymbol(SimpleFillSymbolStyle.DiagonalCross, System.Drawing.Color.Cyan, countyLineSymbol);
            foreach (Feature county in countiesQueryResult)
            {
                Graphic countyGraphic = new Graphic(county.Geometry, countySymbol);

                _selectedFeaturesOverlay.Graphics.Add(countyGraphic);
            }

            // Display the selected states in the graphics overlay.
            SimpleLineSymbol stateLineSymbol = new SimpleLineSymbol(SimpleLineSymbolStyle.Solid, System.Drawing.Color.DarkCyan, 6);
            SimpleFillSymbol stateSymbol = new SimpleFillSymbol(SimpleFillSymbolStyle.Null, System.Drawing.Color.Cyan, stateLineSymbol);
            foreach (Feature state in statesQueryResult)
            {
                Graphic stateGraphic = new Graphic(state.Geometry, stateSymbol);

                _selectedFeaturesOverlay.Graphics.Add(stateGraphic);
            }
        }

        private void CreateLayout()
        {
            // Create a new vertical layout for the app.
            LinearLayout layout = new LinearLayout(this) { Orientation = Orientation.Vertical };

            // Create a horizontal layout for the query controls.
            LinearLayout populationInputLayout = new LinearLayout(this) { Orientation = Orientation.Horizontal };

            // Create the population query controls: a label, a text input, and a button to execute the query.
            TextView populationLabel = new TextView(this) { Text = "[POP2000] > " };            
            _populationValueInput = new EditText(this) { Text = "1800000" };
            _populationValueInput.SetMinimumWidth(200);
            Button queryButton = new Button(this) { Text = "Query" };

            // Create some space.
            Space space = new Space(this);
            space.SetMinimumWidth(50);
            
            // Add the controls to the horizontal layout.
            populationInputLayout.AddView(space);
            populationInputLayout.AddView(populationLabel);
            populationInputLayout.AddView(_populationValueInput);
            populationInputLayout.AddView(queryButton);

            // Wire the event handler for the query button click.
            queryButton.Click += QuerySublayers_Click;

            // Add the query controls and map view to the app layout.
            layout.AddView(populationInputLayout); 
            layout.AddView(_myMapView);
         
            // Show the layout in the app.
            SetContentView(layout);
        }
    }
}