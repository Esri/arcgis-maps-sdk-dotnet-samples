// Copyright 2018 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific 
// language governing permissions and limitations under the License.

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Esri.ArcGISRuntime.ArcGISServices;
using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.UI;
using Esri.ArcGISRuntime.UI.Controls;
using Foundation;
using UIKit;

namespace ArcGISRuntime.Samples.MapImageLayerTables
{
    [Register("MapImageLayerTables")]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        name: "Map image layer tables",
        category: "Layers",
        description: "Find features in a spatial table related to features in a non-spatial table.",
        instructions: "Once the map image layer loads, a list view will be populated with comment data from non-spatial features. Tap on one of the comments to query related spatial features and display the first result on the map.",
        tags: new[] { "features", "query", "related features", "search" })]
    public class MapImageLayerTables : UIViewController
    {
        // Hold references to UI controls.
        private MapView _myMapView;
        private UITableView _tableView;
        private UIStackView _stackView;

        // A graphics overlay for showing selected features.
        private GraphicsOverlay _selectedFeaturesOverlay;

        // A list of all service request comment records (non-spatial features).
        private readonly List<ArcGISFeature> _serviceRequestComments = new List<ArcGISFeature>();

        // Hold a reference to the comments table source.
        private ServiceRequestCommentsTableSource _commentsTableSource;

        public MapImageLayerTables()
        {
            Title = "Query map image layer tables";
        }

        private async void Initialize()
        {
            // Create a new Map with a vector streets basemap.
            Map myMap = new Map(BasemapStyle.ArcGISStreets);

            // Create the URI to the Service Requests map service.
            Uri serviceRequestUri = new Uri("https://sampleserver6.arcgisonline.com/arcgis/rest/services/ServiceRequest/MapServer");

            // Create a new ArcGISMapImageLayer that uses the service URI.
            ArcGISMapImageLayer serviceRequestsMapImageLayer = new ArcGISMapImageLayer(serviceRequestUri);

            try
            {
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

                // Show the records from the service request comments table in the UITableView control.
                foreach (ArcGISFeature commentFeature in commentQueryResult)
                {
                    _serviceRequestComments.Add(commentFeature);
                }

                // Create the table view source that uses the list of features.
                _commentsTableSource = new ServiceRequestCommentsTableSource(_serviceRequestComments);

                // Assign the table view source to the table view control.
                _tableView.Source = _commentsTableSource;

                // Subscribe to event.
                _commentsTableSource.ServiceRequestCommentSelected += CommentsTableSource_ServiceRequestCommentSelected;

                // Create a graphics overlay to show selected features and add it to the map view.
                _selectedFeaturesOverlay = new GraphicsOverlay();
                _myMapView.GraphicsOverlays.Add(_selectedFeaturesOverlay);

                // Assign the map to the MapView.
                _myMapView.Map = myMap;

                // Reload the table view data to refresh the display.
                _tableView.ReloadData();
            }
            catch (Exception e)
            {
                new UIAlertView("Error", e.ToString(), (IUIAlertViewDelegate) null, "OK", null).Show();
            }
        }

        // Handle a new selected comment record in the table view.
        private async void CommentsTableSource_ServiceRequestCommentSelected(object sender, ServiceRequestCommentSelectedEventArgs e)
        {
            // Clear selected features from the graphics overlay.
            _selectedFeaturesOverlay.Graphics.Clear();

            // Get the map image layer that contains the service request sublayer and the service request comments table.
            ArcGISMapImageLayer serviceRequestsMapImageLayer = (ArcGISMapImageLayer) _myMapView.Map.OperationalLayers[0];

            // Get the (non-spatial) table that contains the service request comments.
            ServiceFeatureTable commentsTable = serviceRequestsMapImageLayer.Tables[0];

            // Get the relationship that defines related service request features for features in the comments table (this is the first and only relationship).
            RelationshipInfo commentsRelationshipInfo = commentsTable.LayerInfo.RelationshipInfos.FirstOrDefault();

            // Create query parameters to get the related service request for features in the comments table.
            RelatedQueryParameters relatedQueryParams = new RelatedQueryParameters(commentsRelationshipInfo)
            {
                ReturnGeometry = true
            };

            try
            {
                // Query the comments table to get the related service request feature for the selected comment.
                IReadOnlyList<RelatedFeatureQueryResult> relatedRequestsResult = await commentsTable.QueryRelatedFeaturesAsync(e.SelectedComment, relatedQueryParams);

                // Get the first result. 
                RelatedFeatureQueryResult result = relatedRequestsResult.FirstOrDefault();

                // Get the first feature from the result. If it's null, warn the user and return.
                ArcGISFeature serviceRequestFeature = result.FirstOrDefault() as ArcGISFeature;
                if (serviceRequestFeature == null)
                {
                    UIAlertController alert = UIAlertController.Create("No Feature", "Related feature not found.", UIAlertControllerStyle.Alert);
                    alert.AddAction(UIAlertAction.Create("OK", UIAlertActionStyle.Default, null));
                    PresentViewController(alert, true, null);

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
            View = new UIView() { BackgroundColor = ApplicationTheme.BackgroundColor };

            _stackView = new UIStackView
            {
                Axis = UILayoutConstraintAxis.Vertical,
                Alignment = UIStackViewAlignment.Fill,
                Distribution = UIStackViewDistribution.FillEqually,
                TranslatesAutoresizingMaskIntoConstraints = false
            };

            _myMapView = new MapView();
            _stackView.AddArrangedSubview(_myMapView);

            _tableView = new UITableView();
            _stackView.AddArrangedSubview(_tableView);

            // Add the views.
            View.AddSubviews(_stackView);

            // Lay out the views.
            NSLayoutConstraint.ActivateConstraints(new[]
            {
                _stackView.TopAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.TopAnchor),
                _stackView.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor),
                _stackView.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor),
                _stackView.BottomAnchor.ConstraintEqualTo(View.BottomAnchor),
            });
        }

        public override void TraitCollectionDidChange(UITraitCollection previousTraitCollection)
        {
            base.TraitCollectionDidChange(previousTraitCollection);

            if (View.TraitCollection.VerticalSizeClass == UIUserInterfaceSizeClass.Compact)
            {
                // Update layout for landscape.
                _stackView.Axis = UILayoutConstraintAxis.Horizontal;
            }
            else
            {
                // Update layout for portrait.
                _stackView.Axis = UILayoutConstraintAxis.Vertical;
            }
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            // Subscribe to events, removing any existing subscriptions.
            if (_commentsTableSource != null)
            {
                // Avoid duplicate subscription.
                _commentsTableSource.ServiceRequestCommentSelected -= CommentsTableSource_ServiceRequestCommentSelected;
                _commentsTableSource.ServiceRequestCommentSelected += CommentsTableSource_ServiceRequestCommentSelected;
            }
        }

        public override void ViewDidDisappear(bool animated)
        {
            base.ViewDidDisappear(animated);

            // Unsubscribe from events, per best practice.
            _commentsTableSource.ServiceRequestCommentSelected -= CommentsTableSource_ServiceRequestCommentSelected;
        }
    }

    // A class to define a custom table view source for displaying the ID and description from service request comment features.
    internal class ServiceRequestCommentsTableSource : UITableViewSource
    {
        // An event that's fired when a new service request comment row is selected in the source.
        public event NewCommentSelectedHandler ServiceRequestCommentSelected;

        // A delegate to fire the ServiceRequestCommentSelected event and return the selected feature.
        public delegate void NewCommentSelectedHandler(object sender, ServiceRequestCommentSelectedEventArgs e);

        // A list of service request comment features.
        private readonly List<ArcGISFeature> _comments;

        // An identifier for the table cell (for cell reuse).
        private const string CellIdentifier = "TableCell";

        // Store the selected row.
        private ArcGISFeature _selectedCommentRecord;

        // The constructor takes a list of service request comment features.
        public ServiceRequestCommentsTableSource(List<ArcGISFeature> comments)
        {
            _comments = comments;
        }

        // Number of sections (groups) to display.
        public override nint NumberOfSections(UITableView tableView)
        {
            return 1;
        }

        // When the selected row changes, raise the ServiceRequestCommentSelected event to return the selected feature.
        public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
        {
            _selectedCommentRecord = _comments[indexPath.Row];
            ServiceRequestCommentSelected?.Invoke(this, new ServiceRequestCommentSelectedEventArgs(_selectedCommentRecord));
        }

        // Number of rows in the specified section (group).
        public override nint RowsInSection(UITableView tableview, nint section)
        {
            // If the feature list is null, return 0. Otherwise the number of features in the internal comments list.
            return _comments?.Count ?? 0;
        }

        // Get the cell to display for the specified row.
        public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
        {
            // Get the reused cell from the table view.
            UITableViewCell cell = tableView.DequeueReusableCell(CellIdentifier);

            // Get the comment feature at this cell index.
            Feature commentRow = _comments[indexPath.Row];

            // If there wasn't a cell for reuse, create a new one.
            if (cell == null)
            {
                // Value1 provides a text label (for the request ID) and detail text label (for the comment text).
                cell = new UITableViewCell(UITableViewCellStyle.Value1, CellIdentifier);
            }

            // Fill the cell text with attributes from the comment feature.
            cell.DetailTextLabel.Text = commentRow.Attributes["comments"].ToString();

            // Return the populated cell for display.
            return cell;
        }
    }

    // A custom class to define the event arguments to return when a new row is selected.
    internal class ServiceRequestCommentSelectedEventArgs : EventArgs
    {
        // Store the selected service request comment feature.
        internal ArcGISFeature SelectedComment { get; set; }

        // Constructor takes the service request comment feature that was selected.
        public ServiceRequestCommentSelectedEventArgs(ArcGISFeature selectedComment)
        {
            SelectedComment = selectedComment;
        }
    }
}