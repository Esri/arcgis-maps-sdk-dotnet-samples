// Copyright 2021 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using ArcGISRuntime;
using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.UI.Controls;
using Foundation;
using System;
using UIKit;
using Color = System.Drawing.Color;

namespace ArcGISRuntimeXamarin.Samples.DisplayOACollection
{
    [Register("DisplayOACollection")]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        name: "Display OGC API collection",
        category: "Layers",
        description: "Display an OGC API feature collection and query features while navigating the map view.",
        instructions: "Pan the map and observe how new features are loaded from the OGC API feature service.",
        tags: new[] { "OGC", "OGC API", "feature", "feature layer", "feature table", "service", "table", "web" })]
    public class DisplayOACollection : UIViewController
    {
        // Hold references to UI controls.
        private MapView _myMapView;
        private UIActivityIndicatorView _loadingProgressBar;

        // Hold a reference to the OGC feature collection table.
        private OgcFeatureCollectionTable _featureTable;

        // Constants for the service URL and collection id.
        private const string ServiceUrl = "https://demo.ldproxy.net/daraa";

        // Note that the service defines the collection id which can be accessed via OgcFeatureCollectionInfo.CollectionId.
        private const string CollectionId = "TransportationGroundCrv";

        public DisplayOACollection()
        {
            Title = "Display OGC API collection";
        }

        private async void Initialize()
        {
            // Create the map with topographic basemap.
            _myMapView.Map = new Map(BasemapStyle.ArcGISTopographic);

            try
            {
                // Create the feature table from URI and collection id.
                _featureTable = new OgcFeatureCollectionTable(new Uri(ServiceUrl), CollectionId);

                // Set the feature request mode to manual (only manual is currently supported).
                // In this mode, you must manually populate the table - panning and zooming won't request features automatically.
                _featureTable.FeatureRequestMode = FeatureRequestMode.ManualCache;

                // Load the table.
                await _featureTable.LoadAsync();

                // Create a feature layer to visualize the OAFeat features.
                FeatureLayer ogcFeatureLayer = new FeatureLayer(_featureTable);

                // Apply a renderer.
                ogcFeatureLayer.Renderer = new SimpleRenderer(new SimpleLineSymbol(SimpleLineSymbolStyle.Solid, Color.Blue, 3));

                // Add the layer to the map.
                _myMapView.Map.OperationalLayers.Add(ogcFeatureLayer);

                // Use the navigation completed event to populate the table with the features needed for the current extent.
                _myMapView.NavigationCompleted += MapView_NavigationCompleted;

                // Zoom to a small area within the dataset by default.
                Envelope datasetExtent = _featureTable.Extent;
                if (datasetExtent != null && !datasetExtent.IsEmpty)
                {
                    await _myMapView.SetViewpointGeometryAsync(new Envelope(datasetExtent.GetCenter(), datasetExtent.Width / 3, datasetExtent.Height / 3));
                }
            }
            catch (Exception e)
            {
                new UIAlertView("Error.", e.Message, (IUIAlertViewDelegate)null, "OK", null).Show();
            }
        }

        private void MapView_NavigationCompleted(object sender, EventArgs e) => UpdateForExtent();

        private async void UpdateForExtent()
        {
            // Show the loading bar.
            _loadingProgressBar.StartAnimating();

            // Get the current extent.
            Envelope currentExtent = _myMapView.VisibleArea.Extent;

            // Create a query based on the current visible extent.
            QueryParameters visibleExtentQuery = new QueryParameters();
            visibleExtentQuery.Geometry = currentExtent;
            visibleExtentQuery.SpatialRelationship = SpatialRelationship.Intersects;

            // Set a limit of 5000 on the number of returned features per request,
            // because the default on some services could be as low as 10.
            visibleExtentQuery.MaxFeatures = 5000;

            try
            {
                // Populate the table with the query, leaving existing table entries intact.
                // Setting outFields to null requests all fields.
                await _featureTable.PopulateFromServiceAsync(visibleExtentQuery, false, null);
            }
            catch (Exception exception)
            {
                new UIAlertView("Couldn't populate table.", exception.Message, (IUIAlertViewDelegate)null, "OK", null).Show();
            }
            finally
            {
                // Hide the loading bar.
                _loadingProgressBar.StopAnimating();
            }
        }

        public override void LoadView()
        {
            // Create the views.
            View = new UIView() { BackgroundColor = ApplicationTheme.BackgroundColor };

            _myMapView = new MapView();
            _myMapView.TranslatesAutoresizingMaskIntoConstraints = false;

            _loadingProgressBar = new UIActivityIndicatorView(UIActivityIndicatorViewStyle.WhiteLarge)
            {
                HidesWhenStopped = true,
                BackgroundColor = UIColor.FromWhiteAlpha(0, .6f),
                TranslatesAutoresizingMaskIntoConstraints = false
            };

            UILabel helpLabel = new UILabel
            {
                Text = "Pan and zoom to see features.",
                AdjustsFontSizeToFitWidth = true,
                TextAlignment = UITextAlignment.Center,
                BackgroundColor = UIColor.FromWhiteAlpha(0, .6f),
                TextColor = UIColor.White,
                Lines = 1,
                TranslatesAutoresizingMaskIntoConstraints = false
            };

            // Add the views.
            View.AddSubviews(_myMapView, helpLabel, _loadingProgressBar);

            // Lay out the views.
            NSLayoutConstraint.ActivateConstraints(new[]
            {
                _myMapView.TopAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.TopAnchor),
                _myMapView.BottomAnchor.ConstraintEqualTo(View.BottomAnchor),
                _myMapView.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor),
                _myMapView.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor),

                helpLabel.TopAnchor.ConstraintEqualTo(_myMapView.TopAnchor),
                helpLabel.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor),
                helpLabel.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor),
                helpLabel.HeightAnchor.ConstraintEqualTo(40),

                _loadingProgressBar.TopAnchor.ConstraintEqualTo(helpLabel.BottomAnchor),
                _loadingProgressBar.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor),
                _loadingProgressBar.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor),
                _loadingProgressBar.HeightAnchor.ConstraintEqualTo(40)
            });
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            Initialize();
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            // Subscribe to events.
            _myMapView.NavigationCompleted += MapView_NavigationCompleted;
        }

        public override void ViewDidDisappear(bool animated)
        {
            base.ViewDidDisappear(animated);

            // Unsubscribe from events, per best practice.
            _myMapView.NavigationCompleted -= MapView_NavigationCompleted;
        }
    }
}