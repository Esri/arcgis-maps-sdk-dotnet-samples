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
using Esri.ArcGISRuntime.ArcGISServices;
using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.UI;
using Esri.ArcGISRuntime.UI.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ArcGISRuntime.Samples.MapImageLayerTables
{
    [Activity]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Query map image layer tables",
        "Layers",
        "This sample demonstrates how to get a non-spatial table from an ArcGIS map image layer. It shows how to query such a table, as well as how to find related features in another table. The non-spatial tables contained by a map service may contain additional information about sublayer features. Such information can be accessed by traversing table relationships defined in the service.",
        "1. Launch the sample, the map displays at the extent of the `Service Requests` layer.\n2. The list is populated with service request comment records that have a valid(non-null) comment.\n3. Select one of the service request comments from the list to see the related service request feature selected in the map.",
        "Query", "Sublayer", "MapServer", "Related Tables")]
    public class MapImageLayerTables : Activity
    {
        // A map view control to show the map.
        private MapView _myMapView = new MapView();

        // A graphics overlay for showing selected features.
        private GraphicsOverlay _selectedFeaturesOverlay;

        // A list view to show all service request comments.
        ListView _commentsListBox;

        // A dictionary to hold service request comment info (id and feature).
        private Dictionary<string, object> _commentsInfo = new Dictionary<string, object>();

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            Title = "Query map image layer tables";

            // Create the UI.
            CreateLayout();

            // Initialize the map and show the list of comments.
            Initialize();
        }

        private async Task Initialize()
        {
            // Create a new Map with a streets basemap.
            Map myMap = new Map(Basemap.CreateStreets());

            // Create the URI to the Service Requests map service.
            Uri serviceRequestUri = new Uri("https://sampleserver6.arcgisonline.com/arcgis/rest/services/ServiceRequest/MapServer");

            // Create a new ArcGISMapImageLayer that uses the service URI.
            ArcGISMapImageLayer serviceRequestsMapImageLayer = new ArcGISMapImageLayer(serviceRequestUri);

            // Load all sublayers and tables contained by the map image layer.
            await serviceRequestsMapImageLayer.LoadTablesAndLayersAsync();

            // Set the initial map extent to the extent of all service request features.
            Envelope requestsExtent = serviceRequestsMapImageLayer.FullExtent;
            myMap.InitialViewpoint = new Viewpoint(requestsExtent);

            // Add the layer to the map.
            myMap.OperationalLayers.Add(serviceRequestsMapImageLayer);

            // Get the service request comments table from the map image layer.
            ServiceFeatureTable commentsTable = serviceRequestsMapImageLayer.Tables[0];

            // Create query parameters to get all non-null service request comment records (features) from the table.
            QueryParameters queryToGetNonNullComments = new QueryParameters
            {
                WhereClause = "requestid <> ''"
            };

            // Query the comments table to get the non-null records.
            FeatureQueryResult commentQueryResult = await commentsTable.QueryFeaturesAsync(queryToGetNonNullComments, QueryFeatureFields.LoadAll);

            // Store the comments in a dictionary (id and comment text).
            foreach (GeoElement row in commentQueryResult)
            {
                string id = row.Attributes["requestid"].ToString();
                //string comment = row.Attributes["comments"] != null ? row.Attributes["comments"].ToString() : "--";
                _commentsInfo.Add(id, row);
            }

            // Show the records from the service request comments table in the list view control.
            ArrayAdapter commentsAdapter = new ArrayAdapter(this, Android.Resource.Layout.SimpleSpinnerItem, _commentsInfo.Values.ToList());
            _commentsListBox.Adapter = commentsAdapter;

            // Create a graphics overlay to show selected features and add it to the map view.
            _selectedFeaturesOverlay = new GraphicsOverlay();
            _myMapView.GraphicsOverlays.Add(_selectedFeaturesOverlay);

            // Assign the map to the MapView.
            _myMapView.Map = myMap;
        }

        // Handle a new selected comment record in the table view.
        private async void CommentsListBox_SelectionChanged(object sender, AdapterView.ItemSelectedEventArgs e)
        {
            // Clear selected features from the graphics overlay.
            _selectedFeaturesOverlay.Graphics.Clear();

            // Get the selected comment feature. If there is no selection, return.
            ArcGISFeature selectedComment = _commentsInfo[e.ToString()] as ArcGISFeature;
            if (selectedComment == null) { return; }

            // Get the map image layer that contains the service request sublayer and the service request comments table.
            ArcGISMapImageLayer serviceRequestsMapImageLayer = _myMapView.Map.OperationalLayers[0] as ArcGISMapImageLayer;

            // Get the service requests sublayer.
            ArcGISMapImageSublayer requestsSublayer = serviceRequestsMapImageLayer.Sublayers[0] as ArcGISMapImageSublayer;

            // Get the (non-spatial) table that contains the service request comments.
            ServiceFeatureTable commentsTable = serviceRequestsMapImageLayer.Tables[0];

            // Get the relationship that defines related service request features for features in the comments table (this is the first and only relationship).
            RelationshipInfo commentsRelationshipInfo = commentsTable.LayerInfo.RelationshipInfos.FirstOrDefault();

            // Create query parameters to get the related service request for features in the comments table.
            RelatedQueryParameters relatedQueryParams = new RelatedQueryParameters(commentsRelationshipInfo)
            {
                ReturnGeometry = true
            };

            // Query the comments table to get the related service request feature for the selected comment.
            IReadOnlyList<RelatedFeatureQueryResult> relatedRequestsResult = await commentsTable.QueryRelatedFeaturesAsync(selectedComment, relatedQueryParams);

            // Get the first result. 
            RelatedFeatureQueryResult result = relatedRequestsResult.FirstOrDefault();

            // Get the first feature from the result. If it's null, warn the user and return.
            ArcGISFeature serviceRequestFeature = result.FirstOrDefault() as ArcGISFeature;
            if (serviceRequestFeature == null)
            {
                //TODO: DisplayAlert("No Feature", "Related feature not found.", "OK");

                return;
            }

            // Load the related service request feature (so its geometry is available).
            await serviceRequestFeature.LoadAsync();

            // Get the service request geometry (point).
            MapPoint serviceRequestPoint = serviceRequestFeature.Geometry as MapPoint;

            // Create a cyan marker symbol to display the related feature.
            Symbol selectedRequestSymbol = new SimpleMarkerSymbol(SimpleMarkerSymbolStyle.Circle, System.Drawing.Color.Cyan, 14);

            // Create a graphic using the service request point and marker symbol.
            Graphic requestGraphic = new Graphic(serviceRequestPoint, selectedRequestSymbol);

            // Add the graphic to the graphics overlay and zoom the map view to its extent.
            _selectedFeaturesOverlay.Graphics.Add(requestGraphic);
            await _myMapView.SetViewpointCenterAsync(serviceRequestPoint, 150000);
        }

        private void CreateLayout()
        {
            // Create a new vertical layout for the app
            var layout = new LinearLayout(this) { Orientation = Orientation.Vertical };

            // Create the list view for displaying rows from the comments table.
            _commentsListBox = new ListView(this);
            _commentsListBox.ItemSelected += CommentsListBox_SelectionChanged;
            // Create a scroll viewer for the list view.
            ScrollView commentsScrollView = new ScrollView(this);
            
            // Set the scroll view height so that it always appears on screen.
            commentsScrollView.SetMinimumHeight(Resources.DisplayMetrics.HeightPixels / 3);
            commentsScrollView.FillViewport = true;

            // Add the list view to the scroll view.
            commentsScrollView.AddView(_commentsListBox);

            // Add the map view and list view to the layout.
            layout.AddView(_myMapView);
            layout.AddView(commentsScrollView);

            // Show the layout in the app
            SetContentView(layout);
        }        
    }
}