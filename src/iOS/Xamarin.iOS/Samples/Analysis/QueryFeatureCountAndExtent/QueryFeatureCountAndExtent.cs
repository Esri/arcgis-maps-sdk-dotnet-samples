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
using Esri.ArcGISRuntime.UI.Controls;
using Foundation;
using UIKit;

namespace ArcGISRuntime.Samples.QueryFeatureCountAndExtent
{
    [Register("QueryFeatureCountAndExtent")]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Query feature count and extent",
        "Analysis",
        "This sample demonstrates how to query a feature table, in this case returning a count, for features that are within the visible extent or that meet specified criteria.",
        "Use the button to zoom to the extent of the state specified (by abbreviation) in the textbox or use the button to count the features in the current extent.")]
    public class QueryFeatureCountAndExtent : UIViewController
    {
        // Create and hold references to the UI controls.
        private readonly MapView _myMapView = new MapView();
        private readonly UIToolbar _toolbar = new UIToolbar();
        private UIButton _queryExtentButton;
        private UIButton _queryStateButton;
        private UITextField _stateEntry;
        private UILabel _resultsLabel;
        private UILabel _helpLabel;

        // URL to the feature service.
        private readonly Uri _usaCitiesSource = new Uri("https://sampleserver6.arcgisonline.com/arcgis/rest/services/USA/MapServer/0");

        // Feature table to query.
        private ServiceFeatureTable _featureTable;

        public QueryFeatureCountAndExtent()
        {
            Title = "Query feature count and extent";
        }

        private async void Initialize()
        {
            // Create the map with a vector street basemap.
            Map map = new Map(Basemap.CreateStreetsVector());

            // Create the feature table from the service URL.
            _featureTable = new ServiceFeatureTable(_usaCitiesSource);

            // Create the feature layer from the table.
            FeatureLayer featureLayer = new FeatureLayer(_featureTable);

            // Add the feature layer to the map.
            map.OperationalLayers.Add(featureLayer);

            // Wait for the feature layer to load.
            await featureLayer.LoadAsync();

            // Set the map initial extent to the extent of the feature layer.
            map.InitialViewpoint = new Viewpoint(featureLayer.FullExtent);

            // Add the map to the MapView.
            _myMapView.Map = map;
        }

        private async void BtnZoomToFeatures_Click(object sender, EventArgs e)
        {
            // Create the query parameters.
            QueryParameters queryStates = new QueryParameters {WhereClause = $"upper(ST) LIKE '%{_stateEntry.Text.ToUpper()}%'"};

            // Get the extent from the query.
            Envelope resultExtent = await _featureTable.QueryExtentAsync(queryStates);

            // Return if there is no result (might happen if query is invalid).
            if (resultExtent?.SpatialReference == null)
            {
                return;
            }

            // Create a viewpoint from the extent.
            Viewpoint resultViewpoint = new Viewpoint(resultExtent);

            // Zoom to the viewpoint.
            await _myMapView.SetViewpointAsync(resultViewpoint);

            // Update the UI.
            _resultsLabel.Text = $"Zoomed to features in {_stateEntry.Text}";
        }

        private async void BtnCountFeatures_Click(object sender, EventArgs e)
        {
            // Create the query parameters.
            QueryParameters queryCityCount = new QueryParameters
            {
                // Get the current view extent and use that as a query parameters.
                Geometry = _myMapView.GetCurrentViewpoint(ViewpointType.BoundingGeometry).TargetGeometry,
                // Specify the interpretation of the Geometry query parameters.
                SpatialRelationship = SpatialRelationship.Intersects
            };

            // Get the count of matching features.
            long count = await _featureTable.QueryFeatureCountAsync(queryCityCount);

            // Update the UI.
            _resultsLabel.Text = $"{count} features in extent";
        }

        private void CreateLayout()
        {
            // Create the extent query button and subscribe to events.
            _queryExtentButton = new UIButton();
            _queryExtentButton.SetTitle("Count in extent", UIControlState.Normal);
            _queryExtentButton.SetTitleColor(View.TintColor, UIControlState.Normal);
            _queryExtentButton.TouchUpInside += BtnCountFeatures_Click;

            // Create the state query button and subscribe to events.
            _queryStateButton = new UIButton();
            _queryStateButton.SetTitle("Zoom to match", UIControlState.Normal);
            _queryStateButton.SetTitleColor(View.TintColor, UIControlState.Normal);
            _queryStateButton.TouchUpInside += BtnZoomToFeatures_Click;

            // Create the results label and the search bar.
            _resultsLabel = new UILabel
            {
                Text = "Enter a query to begin.",
                TextAlignment = UITextAlignment.Center
            };

            _stateEntry = new UITextField
            {
                Placeholder = "e.g. NH",
                BorderStyle = UITextBorderStyle.RoundedRect,
                BackgroundColor = UIColor.FromWhiteAlpha(1, .8f)
            };

            // Create the help label.
            _helpLabel = new UILabel
            {
                Text = "Tap 'Zoom to match' to zoom to features matching the given state abbreviation. Tap 'Count in extent' to count the features in the current extent, regardless of the query result.",
                Lines = 4,
                AdjustsFontSizeToFitWidth = true
            };

            // Allow the search bar to dismiss the keyboard.
            _stateEntry.ShouldReturn += sender =>
            {
                sender.ResignFirstResponder();
                return true;
            };

            // Add views to the page.
            View.AddSubviews(_myMapView, _toolbar, _helpLabel, _queryExtentButton, _queryStateButton, _resultsLabel, _stateEntry);
        }

        public override void ViewDidLoad()
        {
            CreateLayout();
            Initialize();

            base.ViewDidLoad();
        }

        public override void ViewDidLayoutSubviews()
        {
            try
            {
                nfloat topMargin = NavigationController.NavigationBar.Frame.Height + UIApplication.SharedApplication.StatusBarFrame.Height;
                nfloat margin = 5;
                nfloat controlHeight = 30;

                // Reposition the controls.
                _myMapView.Frame = new CGRect(0, 0, View.Bounds.Width, View.Bounds.Height);
                _toolbar.Frame = new CGRect(0, topMargin, View.Bounds.Width, controlHeight * 6 + margin * 5);
                _helpLabel.Frame = new CGRect(margin, topMargin + margin, View.Bounds.Width - 2 * margin, controlHeight * 3);
                _stateEntry.Frame = new CGRect(margin, topMargin + controlHeight * 3 + 2 * margin, View.Bounds.Width - 2 * margin, controlHeight);
                _queryExtentButton.Frame = new CGRect(margin, topMargin + 4 * controlHeight + 3 * margin, View.Bounds.Width / 2 - 2 * margin, controlHeight);
                _queryStateButton.Frame = new CGRect(View.Bounds.Width / 2 + margin, topMargin + 4 * controlHeight + 3 * margin, View.Bounds.Width / 2 - margin, controlHeight);
                _resultsLabel.Frame = new CGRect(margin, topMargin + 5 * controlHeight + 4 * margin, View.Bounds.Width - 2 * margin, controlHeight);
                _myMapView.ViewInsets = new UIEdgeInsets(_toolbar.Frame.Bottom, 0, 0, 0);

                base.ViewDidLayoutSubviews();
            }
            // Needed to prevent crash when NavigationController is null. This happens sometimes when switching between samples.
            catch (NullReferenceException)
            {
            }
        }
    }
}