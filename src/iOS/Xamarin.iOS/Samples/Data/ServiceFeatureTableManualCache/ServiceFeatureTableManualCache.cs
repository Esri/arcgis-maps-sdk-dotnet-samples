// Copyright 2016 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific 
// language governing permissions and limitations under the License.

using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.UI.Controls;
using Foundation;
using System;
using UIKit;

namespace ArcGISRuntime.Samples.ServiceFeatureTableManualCache
{
    [Register("ServiceFeatureTableManualCache")]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Service feature table (manual cache)",
        "Data",
        "Display a feature layer from a service using the **manual cache** feature request mode.",
        "Run the sample and pan and zoom around the map. Observe the features loaded from the table.",
        "cache", "feature request mode", "performance")]
    public class ServiceFeatureTableManualCache : UIViewController
    {
        // Hold references to UI controls.
        private MapView _myMapView;

        // Hold a reference to the service feature table.
        private ServiceFeatureTable _incidentsFeatureTable;

        public ServiceFeatureTableManualCache()
        {
            Title = "Service feature table (manual cache)";
        }

        private void Initialize()
        {
            // Create new Map with basemap.
            Map myMap = new Map(Basemap.CreateTopographic());

            // Create and set initial map location.
            MapPoint initialLocation = new MapPoint(-13630484, 4545415, SpatialReferences.WebMercator);
            myMap.InitialViewpoint = new Viewpoint(initialLocation, 500000);

            // Create URL to the used feature service.
            Uri serviceUri = new Uri("https://sampleserver6.arcgisonline.com/arcgis/rest/services/SF311/FeatureServer/0");

            // Create feature table for the incident feature service.
            _incidentsFeatureTable = new ServiceFeatureTable(serviceUri)
            {
                // Define the request mode.
                FeatureRequestMode = FeatureRequestMode.ManualCache
            };

            // When feature table is loaded, populate data.
            _incidentsFeatureTable.Loaded += OnLoadedPopulateData;

            // Create FeatureLayer that uses the created table.
            FeatureLayer incidentsFeatureLayer = new FeatureLayer(_incidentsFeatureTable);

            // Add created layer to the map.
            myMap.OperationalLayers.Add(incidentsFeatureLayer);

            // Assign the map to the MapView.
            _myMapView.Map = myMap;
        }

        private async void OnLoadedPopulateData(object sender, EventArgs e)
        {
            // Unsubscribe from event.
            _incidentsFeatureTable.Loaded -= OnLoadedPopulateData;

            // Create new query object that contains parameters to query specific request types.
            QueryParameters queryParameters = new QueryParameters
            {
                WhereClause = "req_Type = 'Tree Maintenance or Damage'"
            };

            // Create list of the fields that are returned from the service.
            string[] outputFields = {"*"};

            try
            {
                // Populate feature table with the data based on query.
                await _incidentsFeatureTable.PopulateFromServiceAsync(queryParameters, true, outputFields);
            }
            catch (Exception ex)
            {
                new UIAlertView("Error", ex.ToString(), (IUIAlertViewDelegate) null, "OK", null).Show();
            }
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            Initialize();
        }

        public override void LoadView()
        {
            // Create the views.
            _myMapView = new MapView();
            _myMapView.TranslatesAutoresizingMaskIntoConstraints = false;

            View = new UIView() { BackgroundColor = UIColor.White };

            // Add the views.
            View.AddSubviews(_myMapView);

            // Lay out the views.
            NSLayoutConstraint.ActivateConstraints(new[]
            {
                _myMapView.TopAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.TopAnchor),
                _myMapView.BottomAnchor.ConstraintEqualTo(View.BottomAnchor),
                _myMapView.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor),
                _myMapView.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor)
            });
        }
    }
}