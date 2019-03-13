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

namespace ArcGISRuntimeXamarin.Samples.BrowseWfsLayers
{
    [Register("BrowseWfsLayers")]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Browse a WFS service for layers",
        "Layers",
        "Browse for layers in a WFS service.",
        "")]
    public class BrowseWfsLayers : UIViewController
    {
        // Hold references to UI controls.
        private MapView _myMapView;
        private UISwitch _toggleAxisOrderSwitch;
        private UIActivityIndicatorView _loadingProgressBar;
        private UIBarButtonItem _chooseLayersButton;

        // Hold a reference to the WFS service info - used to get list of available layers.
        private WfsServiceInfo _serviceInfo;

        // URL to the WFS service.
        private const string ServiceUrl = "https://dservices2.arcgis.com/ZQgQTuoyBrtmoGdP/arcgis/services/Seattle_Downtown_Features/WFSServer?service=wfs&request=getcapabilities";

        public BrowseWfsLayers()
        {
            Title = "Browse a WFS service for layers";
        }

        private async void Initialize()
        {
            // Create the map with imagery basemap.
            _myMapView.Map = new Map(Basemap.CreateImagery());

            // Create the WFS service.
            WfsService service = new WfsService(new Uri(ServiceUrl));

            // Load the WFS service.
            await service.LoadAsync();

            // Store information about the WFS service for later.
            _serviceInfo = service.ServiceInfo;

            // Update the UI.
            _chooseLayersButton.Enabled = true;
        }

        private async void LoadSelectedLayer(WfsLayerInfo selectedLayerInfo)
        {
            // Show the progress bar.
            _loadingProgressBar.StartAnimating();

            // Clear the existing layers.
            _myMapView.Map.OperationalLayers.Clear();

            try
            {
                // Create the WFS feature table.
                WfsFeatureTable table = new WfsFeatureTable(selectedLayerInfo);

                // Set the WFS table's feature request mode.
                table.FeatureRequestMode = FeatureRequestMode.ManualCache;

                // Set the axis order based on the UI.
                if (_toggleAxisOrderSwitch.On)
                {
                    table.AxisOrder = OgcAxisOrder.Swap;
                }
                else
                {
                    table.AxisOrder = OgcAxisOrder.NoSwap;
                }

                // Populate the WFS table.
                await table.PopulateFromServiceAsync(new QueryParameters(), false, null);

                // Create a feature layer from the WFS table.
                FeatureLayer wfsFeatureLayer = new FeatureLayer(table);

                // Choose a renderer for the layer based on the table.
                wfsFeatureLayer.Renderer = GetRandomRendererForTable(table) ?? wfsFeatureLayer.Renderer;

                // Add the layer to the map.
                _myMapView.Map.OperationalLayers.Add(wfsFeatureLayer);

                // Zoom to the extent of the layer.
                await _myMapView.SetViewpointGeometryAsync(selectedLayerInfo.Extent, 50);
            }
            catch (Exception exception)
            {
                Debug.WriteLine(exception);
                new UIAlertView("Error", exception.ToString(), (IUIAlertViewDelegate) null, "Couldn't load layer.", null).Show();
            }
            finally
            {
                // Hide the progress bar.
                _loadingProgressBar.StopAnimating();
            }
        }

        private void ShowLayerOptions(object sender, EventArgs e)
        {
            // Create the view controller that will present the list of layers.
            UIAlertController layerSelectionAlert = UIAlertController.Create("Select a layer", "", UIAlertControllerStyle.ActionSheet);

            // Add an option for each layer.
            foreach (WfsLayerInfo layerInfo in _serviceInfo.LayerInfos)
            {
                // Selecting a layer will call the lambda method, which will show the layer.
                layerSelectionAlert.AddAction(UIAlertAction.Create(layerInfo.Title, UIAlertActionStyle.Default, action => LoadSelectedLayer(layerInfo)));
            }

            // Fix to prevent crash on iPad.
            var popoverPresentationController = layerSelectionAlert.PopoverPresentationController;
            if (popoverPresentationController != null)
            {
                popoverPresentationController.BarButtonItem = _chooseLayersButton;
            }

            // Show the alert.
            PresentViewController(layerSelectionAlert, true, null);
        }

        #region Random symbology

        // Random number generator used to generate random symbology.
        private static readonly Random _rand = new Random();

        private Renderer GetRandomRendererForTable(FeatureTable table)
        {
            switch (table.GeometryType)
            {
                case GeometryType.Point:
                case GeometryType.Multipoint:
                    return new SimpleRenderer(new SimpleMarkerSymbol(SimpleMarkerSymbolStyle.Circle, GetRandomColor(), 2));
                case GeometryType.Polygon:
                case GeometryType.Envelope:
                    return new SimpleRenderer(new SimpleFillSymbol(SimpleFillSymbolStyle.Solid, GetRandomColor(180), null));
                case GeometryType.Polyline:
                    return new SimpleRenderer(new SimpleLineSymbol(SimpleLineSymbolStyle.Solid, GetRandomColor(), 1));
            }

            return null;
        }

        private Color GetRandomColor(int alpha = 255)
        {
            return Color.FromArgb(alpha, _rand.Next(0, 255), _rand.Next(0, 255), _rand.Next(0, 255));
        }

        #endregion Random symbology

        public override void LoadView()
        {
            // Create the views.
            View = new UIView {BackgroundColor = UIColor.White};

            _myMapView = new MapView();
            _myMapView.TranslatesAutoresizingMaskIntoConstraints = false;

            _toggleAxisOrderSwitch = new UISwitch();

            UILabel axisOrderLabel = new UILabel();
            axisOrderLabel.Text = "Swap coordinates";

            _chooseLayersButton = new UIBarButtonItem();
            _chooseLayersButton.Title = "Choose layer";
            _chooseLayersButton.Enabled = false;
            _chooseLayersButton.Clicked += ShowLayerOptions;

            UIToolbar toolbar = new UIToolbar();
            toolbar.TranslatesAutoresizingMaskIntoConstraints = false;
            toolbar.Items = new[]
            {
                new UIBarButtonItem(_toggleAxisOrderSwitch),
                new UIBarButtonItem(UIBarButtonSystemItem.FixedSpace) { Width = 8 }, 
                new UIBarButtonItem(axisOrderLabel),
                new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace),
                _chooseLayersButton
            };

            _loadingProgressBar = new UIActivityIndicatorView(UIActivityIndicatorViewStyle.WhiteLarge);
            _loadingProgressBar.TranslatesAutoresizingMaskIntoConstraints = false;
            _loadingProgressBar.HidesWhenStopped = true;
            _loadingProgressBar.BackgroundColor = UIColor.FromWhiteAlpha(0, .6f);

            // Add the views.
            View.AddSubviews(_myMapView, toolbar, _loadingProgressBar);

            // Lay out the views.
            NSLayoutConstraint.ActivateConstraints(new[]
            {
                _myMapView.TopAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.TopAnchor),
                _myMapView.BottomAnchor.ConstraintEqualTo(toolbar.TopAnchor),
                _myMapView.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor),
                _myMapView.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor),
                toolbar.BottomAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.BottomAnchor),
                toolbar.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor),
                toolbar.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor),
                _loadingProgressBar.TopAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.TopAnchor),
                _loadingProgressBar.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor),
                _loadingProgressBar.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor),
                _loadingProgressBar.BottomAnchor.ConstraintEqualTo(View.BottomAnchor)
            });
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            Initialize();
        }
    }
}