// Copyright 2019 Esri.
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
using Esri.ArcGISRuntime.Ogc;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.UI.Controls;
using Foundation;
using System;
using System.Diagnostics;
using System.Drawing;
using UIKit;

namespace ArcGISRuntimeXamarin.Samples.DisplayWfs
{
    [Register("DisplayWfs")]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Display a WFS layer",
        "Layers",
        "Display a layer from a WFS service, requesting only features for the current extent.",
        "")]
    public class DisplayWfs : UIViewController
    {
        // Hold references to UI controls.
        private MapView _myMapView;
        private UIActivityIndicatorView _loadingProgressBar;

        // Hold a reference to the feature table.
        private WfsFeatureTable _featureTable;

        // Constants for the service URL and layer name.
        private const string ServiceUrl = "https://dservices2.arcgis.com/ZQgQTuoyBrtmoGdP/arcgis/services/Seattle_Downtown_Features/WFSServer?service=wfs&request=getcapabilities";
        private const string LayerName = "Seattle_Downtown_Features:Buildings";

        public DisplayWfs()
        {
            Title = "Display a WFS layer";
        }

        private async void Initialize()
        {
            // Create the map with topographic basemap.
            _myMapView.Map = new Map(Basemap.CreateTopographic());

            try
            {
                // Create the feature table from URI and layer name.
                _featureTable = new WfsFeatureTable(new Uri(ServiceUrl), LayerName);

                // Set the feature request mode to manual - only manual is supported at v100.5.
                _featureTable.FeatureRequestMode = FeatureRequestMode.ManualCache;

                // Set the axis order.
                _featureTable.AxisOrder = OgcAxisOrder.NoSwap;

                // Load the table.
                await _featureTable.LoadAsync();

                // Create a feature layer to visualize the WFS features.
                FeatureLayer wfsFeatureLayer = new FeatureLayer(_featureTable);

                // Apply a renderer.
                wfsFeatureLayer.Renderer = new SimpleRenderer(new SimpleLineSymbol(SimpleLineSymbolStyle.Solid, Color.Red, 3));

                // Add the layer to the map.
                _myMapView.Map.OperationalLayers.Add(wfsFeatureLayer);

                // Use the navigation completed event to populate the table with the features needed for the current extent.
                _myMapView.NavigationCompleted += MapView_NavigationCompleted;

                // Zoom to a small area within the dataset by default.
                MapPoint topLeft = new MapPoint(-122.341581, 47.617207, SpatialReferences.Wgs84);
                MapPoint bottomRight = new MapPoint(-122.332662, 47.613758, SpatialReferences.Wgs84);
                await _myMapView.SetViewpointGeometryAsync(new Envelope(topLeft, bottomRight));
            }
            catch (Exception e)
            {
                new UIAlertView("Error", e.ToString(), (IUIAlertViewDelegate) null, "Couldn't load sample.", null).Show();
                Debug.WriteLine(e);
            }
        }

        private async void MapView_NavigationCompleted(object sender, EventArgs e)
        {
            // Show the loading bar.
            _loadingProgressBar.StartAnimating();

            // Get the current extent.
            Envelope currentExtent = _myMapView.VisibleArea.Extent;

            // Create a query based on the current visible extent.
            QueryParameters visibleExtentQuery = new QueryParameters();
            visibleExtentQuery.Geometry = currentExtent;
            visibleExtentQuery.SpatialRelationship = SpatialRelationship.Intersects;

            try
            {
                // Populate the table with the query, leaving existing table entries intact.
                await _featureTable.PopulateFromServiceAsync(visibleExtentQuery, false, null);
            }
            catch (Exception exception)
            {
                new UIAlertView("Error", e.ToString(), (IUIAlertViewDelegate) null, "Couldn't populate table.", null).Show();
                Debug.WriteLine(exception);
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
            View = new UIView();

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
            NSLayoutConstraint.ActivateConstraints(new []
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
    }
}
