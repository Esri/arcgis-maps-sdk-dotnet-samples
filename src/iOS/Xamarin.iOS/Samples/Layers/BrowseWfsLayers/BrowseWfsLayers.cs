﻿// Copyright 2019 Esri.
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
        "Browse WFS service for layers",
        "Layers",
        "Browse a WFS service for layers and add them to the map.",
        "")]
    public class BrowseWfsLayers : UIViewController
    {
        // Hold references to UI controls.
        private MapView _myMapView;
        private UISwitch _toggleAxisOrderSwitch;
        private UIActivityIndicatorView _loadingProgressBar;
        private UIBarButtonItem _chooseLayersButton;
        private UIBarButtonItem _loadServiceButton;

        // Hold a reference to the WFS service info - used to get list of available layers.
        private WfsServiceInfo _serviceInfo;

        // URL to the WFS service.
        private const string ServiceUrl = "https://dservices2.arcgis.com/ZQgQTuoyBrtmoGdP/arcgis/services/Seattle_Downtown_Features/WFSServer?service=wfs&request=getcapabilities";

        public BrowseWfsLayers()
        {
            Title = "Browse WFS service for layers";
        }

        private void Initialize()
        {
            // Update the UI.
            _loadServiceButton.Title = ServiceUrl;

            // Create the map with imagery basemap.
            _myMapView.Map = new Map(Basemap.CreateImagery());

            LoadService();
        }

        private async void LoadService()
        {
            try
            {
                _loadingProgressBar.StartAnimating();
                _loadServiceButton.Enabled = false;
                _chooseLayersButton.Enabled = false;

                // Create the WFS service.
                WfsService service = new WfsService(new Uri(_loadServiceButton.Title));

                // Load the WFS service.
                await service.LoadAsync();

                // Get the service metadata.
                _serviceInfo = service.ServiceInfo;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                new UIAlertView("Error", ex.Message, (IUIAlertViewDelegate)null, "Error loading service", null).Show();
            }
            finally
            {
                // Update the UI.
                _loadingProgressBar.StopAnimating();
                _loadServiceButton.Enabled = true;
                _chooseLayersButton.Enabled = true;
            }
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

                // Set the feature request mode to manual - only manual is supported at v100.5.
                // In this mode, you must manually populate the table - panning and zooming won't request features automatically.
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
                wfsFeatureLayer.Renderer = GetRendererForTable(table) ?? wfsFeatureLayer.Renderer;

                // Add the layer to the map.
                _myMapView.Map.OperationalLayers.Add(wfsFeatureLayer);

                // Zoom to the extent of the selected layer.
                await _myMapView.SetViewpointGeometryAsync(selectedLayerInfo.Extent, 50);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex);
                new UIAlertView("Error", ex.Message, (IUIAlertViewDelegate)null, "Couldn't load layer.", null).Show();
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

        private Renderer GetRendererForTable(FeatureTable table)
        {
            switch (table.GeometryType)
            {
                case GeometryType.Point:
                case GeometryType.Multipoint:
                    return new SimpleRenderer(new SimpleMarkerSymbol(SimpleMarkerSymbolStyle.Circle, Color.Blue, 4));

                case GeometryType.Polygon:
                case GeometryType.Envelope:
                    return new SimpleRenderer(new SimpleFillSymbol(SimpleFillSymbolStyle.Solid, Color.Blue, null));

                case GeometryType.Polyline:
                    return new SimpleRenderer(new SimpleLineSymbol(SimpleLineSymbolStyle.Solid, Color.Blue, 1));
            }

            return null;
        }

        public override void LoadView()
        {
            // Create the views.
            View = new UIView { BackgroundColor = UIColor.White };

            _myMapView = new MapView();
            _myMapView.TranslatesAutoresizingMaskIntoConstraints = false;

            _toggleAxisOrderSwitch = new UISwitch();

            UILabel axisOrderLabel = new UILabel();
            axisOrderLabel.Text = "Swap coordinates";

            _chooseLayersButton = new UIBarButtonItem();
            _chooseLayersButton.Title = "Choose layer";
            _chooseLayersButton.Enabled = false;

            _loadServiceButton = new UIBarButtonItem();

            UIToolbar loadBar = new UIToolbar();
            loadBar.TranslatesAutoresizingMaskIntoConstraints = false;
            loadBar.Items = new[]
            {
                _loadServiceButton
            };

            UIToolbar toolbar = new UIToolbar();
            toolbar.TranslatesAutoresizingMaskIntoConstraints = false;
            toolbar.Items = new[]
            {
                new UIBarButtonItem(_toggleAxisOrderSwitch),
                new UIBarButtonItem(UIBarButtonSystemItem.FixedSpace) {Width = 8},
                new UIBarButtonItem(axisOrderLabel),
                new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace),
                _chooseLayersButton
            };

            _loadingProgressBar = new UIActivityIndicatorView(UIActivityIndicatorViewStyle.WhiteLarge);
            _loadingProgressBar.TranslatesAutoresizingMaskIntoConstraints = false;
            _loadingProgressBar.HidesWhenStopped = true;
            _loadingProgressBar.BackgroundColor = UIColor.FromWhiteAlpha(0, .6f);

            // Add the views.
            View.AddSubviews(_myMapView, loadBar, toolbar, _loadingProgressBar);

            // Lay out the views.
            NSLayoutConstraint.ActivateConstraints(new[]
            {
                _myMapView.TopAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.TopAnchor),
                _myMapView.BottomAnchor.ConstraintEqualTo(loadBar.TopAnchor),
                _myMapView.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor),
                _myMapView.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor),

                loadBar.TopAnchor.ConstraintEqualTo(_myMapView.BottomAnchor),
                loadBar.BottomAnchor.ConstraintEqualTo(toolbar.TopAnchor),
                loadBar.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor),
                loadBar.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor),

                toolbar.TopAnchor.ConstraintEqualTo(loadBar.BottomAnchor),
                toolbar.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor),
                toolbar.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor),
                toolbar.BottomAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.BottomAnchor),

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

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            // Subscribe to events.
            _chooseLayersButton.Clicked += ShowLayerOptions;
            _loadServiceButton.Clicked += ServiceLinkClick;
        }

        private void ServiceLinkClick(object sender, EventArgs e)
        {
            UIAlertView alert = new UIAlertView()
            {
                Message = "Enter WFS URL.",
                AlertViewStyle = UIAlertViewStyle.PlainTextInput,
                CancelButtonIndex = 1
            };
            alert.GetTextField(0).Text = _loadServiceButton.Title;
            alert.AddButton("Load");
            alert.AddButton("Cancel");

            alert.Clicked += (object s, UIButtonEventArgs a) =>
            {
                if (a.ButtonIndex != alert.CancelButtonIndex)
                {
                    _loadServiceButton.Title = alert.GetTextField(0).Text;
                    LoadService();
                }
            };
            alert.Show();
        }

        public override void ViewDidDisappear(bool animated)
        {
            base.ViewDidDisappear(animated);

            // Unsubscribe from events, per best practice.
            _chooseLayersButton.Clicked -= ShowLayerOptions;
            _loadServiceButton.Clicked -= ServiceLinkClick;
        }
    }
}