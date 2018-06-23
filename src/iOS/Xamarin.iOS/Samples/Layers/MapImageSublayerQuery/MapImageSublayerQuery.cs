// Copyright 2018 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific 
// language governing permissions and limitations under the License.

using System;
using CoreGraphics;
using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.UI;
using Esri.ArcGISRuntime.UI.Controls;
using Foundation;
using UIKit;

namespace ArcGISRuntime.Samples.MapImageSublayerQuery
{
    [Register("MapImageSublayerQuery")]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Query a map image sublayer",
        "Layers",
        "This sample demonstrates how to execute an attribute and spatial query on the sublayers of an ArcGIS map image layer. Sublayers of an ArcGISMapImageLayer may expose a ServiceFeatureTable through a Table property.This allows you to perform the same queries available when working with a table from a FeatureLayer: attribute query, spatial query, statistics query, query for related features, and so on.",
        "1. Launch the sample, the map displays at an extent where individual states, counties, and cities can be seen clearly.\n2.Provide a numeric value for the population query(values under 1810000 will produce a selection in all layers).\n3.Click the `Query` button to find all features in the current map extent that have a population greater than the value entered.\n   - Any current selection is cleared from the map.\n   - If a non - numeric value was entered, an error message is displayed.\n4.All features(cities, counties, or states) meeting the query criteria are selected in the map.\n   - If no features meet the query criteria, a message displays stating zero features were selected.\n5.Experiment with different map extents and population values and see the results.",
        "Query", "Sublayer", "MapServer", "Table")]
    public class MapImageSublayerQuery : UIViewController
    {
        // Hold references to the UI controls.
        private readonly MapView _myMapView = new MapView();
        private UILabel _populationLabel;
        private UIButton _queryButton;
        private UITextField _populationValueInput;
        private readonly UIToolbar _toolbar = new UIToolbar();

        // Graphics overlay for showing selected features.
        private GraphicsOverlay _selectedFeaturesOverlay;

        public MapImageSublayerQuery()
        {
            Title = "Query a map image sublayer";
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();

            CreateLayout();
            Initialize();
        }

        public override void ViewDidLayoutSubviews()
        {
            try
            {
                nfloat topMargin = NavigationController.NavigationBar.Frame.Height + UIApplication.SharedApplication.StatusBarFrame.Height;
                nfloat margin = 5;
                nfloat controlHeight = 30;
                nfloat toolbarHeight = controlHeight * 2 + margin * 3;

                // Reposition the controls.
                _myMapView.Frame = new CGRect(0, 0, View.Bounds.Width, View.Bounds.Height);
                _myMapView.ViewInsets = new UIEdgeInsets(topMargin + toolbarHeight, 0, 0, 0);
                _toolbar.Frame = new CGRect(0, topMargin, View.Bounds.Width, toolbarHeight);
                _populationLabel.Frame = new CGRect(margin, topMargin + margin, View.Bounds.Width / 2 - 2 * margin, controlHeight);
                _populationValueInput.Frame = new CGRect(View.Bounds.Width / 2 + margin, topMargin + margin, View.Bounds.Width / 2 - 2 * margin, controlHeight);
                _queryButton.Frame = new CGRect(margin, topMargin + controlHeight + 2 * margin, View.Bounds.Width - 2 * margin, controlHeight);

                base.ViewDidLayoutSubviews();
            }
            // Needed to prevent crash when NavigationController is null. This happens sometimes when switching between samples.
            catch (NullReferenceException)
            {
            }
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

        // Function to query map image sublayers when the query button is clicked.
        private async void QuerySublayers_Click(object sender, EventArgs e)
        {
            // Clear selected features from the graphics overlay.
            _selectedFeaturesOverlay.Graphics.Clear();

            // If the population value entered is not numeric, warn the user and exit.
            double populationNumber;
            if (!double.TryParse(_populationValueInput.Text.Trim(), out populationNumber))
            {
                UIAlertController alert = UIAlertController.Create("Invalid number", "Population value must be numeric.", UIAlertControllerStyle.Alert);
                alert.AddAction(UIAlertAction.Create("OK", UIAlertActionStyle.Default, null));
                PresentViewController(alert, true, null);

                return;
            }

            // Get the USA map image layer (the first and only operational layer in the map).
            ArcGISMapImageLayer usaMapImageLayer = _myMapView.Map.OperationalLayers[0] as ArcGISMapImageLayer;

            // Use a utility method on the map image layer to load all the sublayers and tables.
            await usaMapImageLayer.LoadTablesAndLayersAsync();

            // Get the sublayers of interest (skip 'Highways' since it doesn't have the POP2000 field).
            ArcGISMapImageSublayer citiesSublayer = usaMapImageLayer.Sublayers[0] as ArcGISMapImageSublayer;
            ArcGISMapImageSublayer statesSublayer = usaMapImageLayer.Sublayers[2] as ArcGISMapImageSublayer;
            ArcGISMapImageSublayer countiesSublayer = usaMapImageLayer.Sublayers[3] as ArcGISMapImageSublayer;

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
            // Create the population query controls: a label, a text input, and a button to execute the query.
            _populationLabel = new UILabel {Text = "[POP2000] > ", TextAlignment = UITextAlignment.Right};
            _populationValueInput = new UITextField
            {
                Text = "1800000",
                BackgroundColor = UIColor.FromWhiteAlpha(1, .8f),
                BorderStyle = UITextBorderStyle.RoundedRect
            };
            _populationValueInput.ShouldReturn += textField =>
            {
                textField.ResignFirstResponder();
                return true;
            };

            _queryButton = new UIButton(UIButtonType.Plain);
            _queryButton.SetTitle("Query", UIControlState.Normal);
            _queryButton.SetTitleColor(View.TintColor, UIControlState.Normal);

            // Wire the event handler for the query button click.
            _queryButton.TouchUpInside += QuerySublayers_Click;

            // Add the query controls and map view to the app layout.
            View.AddSubviews(_myMapView, _toolbar, _populationLabel, _populationValueInput, _queryButton);
        }
    }
}