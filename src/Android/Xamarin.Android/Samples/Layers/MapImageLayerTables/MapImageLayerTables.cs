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
using System.Drawing;
using System.Linq;

namespace ArcGISRuntime.Samples.MapImageLayerTables
{
    [Activity (ConfigurationChanges=Android.Content.PM.ConfigChanges.Orientation | Android.Content.PM.ConfigChanges.ScreenSize)]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        name: "Map image layer tables",
        category: "Layers",
        description: "Find features in a spatial table related to features in a non-spatial table.",
        instructions: "Once the map image layer loads, a list view will be populated with comment data from non-spatial features. Tap on one of the comments to query related spatial features and display the first result on the map.",
        tags: new[] { "features", "query", "related features", "search" })]
    [ArcGISRuntime.Samples.Shared.Attributes.AndroidLayoutAttribute("MapImageLayerTables.axml")]
    public class MapImageLayerTables : Activity
    {
        // Hold a reference to the map view.
        private MapView _myMapView;

        // A graphics overlay for showing selected features.
        private GraphicsOverlay _selectedFeaturesOverlay;

        // A list view to show all service request comments.
        private ListView _commentsListBox;

        // A list to hold service request comments.
        private List<Feature> _commentFeatures = new List<Feature>();

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            Title = "Query map image layer tables";

            // Create the UI.
            CreateLayout();

            // Initialize the map and show the list of comments.
            Initialize();
        }

        private async void Initialize()
        {
            try
            {
                // Create a new Map with a streets basemap.
                Map myMap = new Map(BasemapStyle.ArcGISStreets);

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
                    WhereClause = "requestid <> '' AND comments <> ''"
                };

                // Query the comments table to get the non-null records.
                FeatureQueryResult commentQueryResult = await commentsTable.QueryFeaturesAsync(queryToGetNonNullComments, QueryFeatureFields.LoadAll);

                // Store the comments in a list.
                _commentFeatures = commentQueryResult.ToList();

                // Show the comment text from the service request comments records in the list view control.
                IEnumerable<object> comments = _commentFeatures.Select(c => c.Attributes["comments"]);
                ArrayAdapter commentsAdapter = new ArrayAdapter(this, Android.Resource.Layout.SimpleSpinnerItem, comments.ToArray());
                _commentsListBox.Adapter = commentsAdapter;

                // Create a graphics overlay to show selected features and add it to the map view.
                _selectedFeaturesOverlay = new GraphicsOverlay();
                _myMapView.GraphicsOverlays.Add(_selectedFeaturesOverlay);

                // Assign the map to the MapView.
                _myMapView.Map = myMap;
            }
            catch (Exception e)
            {
                new AlertDialog.Builder(this).SetMessage(e.ToString()).SetTitle("Error").Show();
            }
        }

        private void CreateLayout()
        {
            // Load the UI layout from an .axml file in the project resources.
            SetContentView(Resource.Layout.MapImageLayerTables);

            // Update control references to point to the controls defined in the layout.
            _myMapView = FindViewById<MapView>(Resource.Id.mapImageLayerTables_MyMapView);

            // List view for displaying rows from the comments table.
            _commentsListBox = FindViewById<ListView>(Resource.Id.mapImageLayerTables_commentListView);

            // Handle an item (comment) being clicked in the list.
            _commentsListBox.ItemClick += CommentsListBox_ItemClick;
        }

        // When a comment is clicked, get the related feature (service request) and select it on the map.
        private async void CommentsListBox_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            // Clear selected features from the graphics overlay.
            _selectedFeaturesOverlay.Graphics.Clear();

            // Get the clicked record (ArcGISFeature) using the list position. If one is not found, return.
            ArcGISFeature selectedComment = _commentFeatures[e.Position] as ArcGISFeature;
            if (selectedComment == null) { return; }

            // Get the map image layer that contains the service request sublayer and the service request comments table.
            ArcGISMapImageLayer serviceRequestsMapImageLayer = (ArcGISMapImageLayer)_myMapView.Map.OperationalLayers[0];

            // Get the (non-spatial) table that contains the service request comments.
            ServiceFeatureTable commentsTable = serviceRequestsMapImageLayer.Tables[0];

            // Get the relationship that defines related service request features to features in the comments table (this is the first and only relationship).
            RelationshipInfo commentsRelationshipInfo = commentsTable.LayerInfo.RelationshipInfos.FirstOrDefault();

            // Create query parameters to get the related service request for features in the comments table.
            RelatedQueryParameters relatedQueryParams = new RelatedQueryParameters(commentsRelationshipInfo)
            {
                ReturnGeometry = true
            };

            try
            {
                // Query the comments table to get the related service request feature for the selected comment.
                IReadOnlyList<RelatedFeatureQueryResult> relatedRequestsResult = await commentsTable.QueryRelatedFeaturesAsync(selectedComment, relatedQueryParams);

                // Get the first result. 
                RelatedFeatureQueryResult result = relatedRequestsResult.FirstOrDefault();

                // Get the first feature from the result. If it's null, warn the user and return.
                ArcGISFeature serviceRequestFeature = result.FirstOrDefault() as ArcGISFeature;
                if (serviceRequestFeature == null)
                {
                    // Report to the user that a related feature was not found, then return.
                    AlertDialog.Builder alertBuilder = new AlertDialog.Builder(this);
                    AlertDialog alert = alertBuilder.Create();
                    alert.SetMessage("Related feature not found.");
                    alert.Show();

                    return;
                }

                // Load the related service request feature (so its geometry is available).
                await serviceRequestFeature.LoadAsync();

                // Get the service request geometry (point).
                MapPoint serviceRequestPoint = serviceRequestFeature.Geometry as MapPoint;

                // Create a cyan marker symbol to display the related feature.
                Symbol selectedRequestSymbol = new SimpleMarkerSymbol(SimpleMarkerSymbolStyle.Circle, Color.Cyan, 14);

                // Create a graphic using the service request point and marker symbol.
                Graphic requestGraphic = new Graphic(serviceRequestPoint, selectedRequestSymbol);

                // Add the graphic to the graphics overlay and zoom the map view to its extent.
                _selectedFeaturesOverlay.Graphics.Add(requestGraphic);
                await _myMapView.SetViewpointCenterAsync(serviceRequestPoint, 150000);
            }
            catch (Exception ex)
            {
                new AlertDialog.Builder(this).SetMessage(ex.ToString()).SetTitle("Error").Show();
            }
        }
    }
}