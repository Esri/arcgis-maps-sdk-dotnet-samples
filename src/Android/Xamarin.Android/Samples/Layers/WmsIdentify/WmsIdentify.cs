// Copyright 2017 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using Android.App;
using Android.OS;
using Android.Webkit;
using Android.Widget;
using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Ogc;
using Esri.ArcGISRuntime.UI.Controls;
using System;
using System.Collections.Generic;
using Android.Graphics;
using Android.Views;

namespace ArcGISRuntime.Samples.WmsIdentify
{
    [Activity (ConfigurationChanges=Android.Content.PM.ConfigChanges.Orientation | Android.Content.PM.ConfigChanges.ScreenSize)]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Identify WMS features",
        "Layers",
        "This sample demonstrates how to identify WMS features and display the associated content for an identified WMS feature.",
        "Tap to identify a feature. Note: the service returns HTML regardless of whether there was an identify result. See the Forms implementation for an example heuristic for identifying empty results.")]
    public class WmsIdentify : Activity
    {
        // Hold a reference to the map view
        private MapView _myMapView;

        // Create and hold the URL to the WMS service showing EPA water info
        private Uri _wmsUrl = new Uri("https://watersgeo.epa.gov/arcgis/services/OWPROGRAM/SDWIS_WMERC/MapServer/WMSServer?request=GetCapabilities&service=WMS");

        // Create and hold a list of uniquely-identifying WMS layer names to display
        private List<String> _wmsLayerNames = new List<string> { "4" };

        // Hold the WMS layer
        private WmsLayer _wmsLayer;

        // Hold the webview
        private WebView _htmlView;
        private LinearLayout _sampleLayout;
        private LinearLayout.LayoutParams _layoutParams;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            Title = "Identify WMS features";

            // Create the UI, setup the control references and execute initialization
            CreateLayout();
            Initialize();
        }

        private async void Initialize()
        {
            // Create new Map with basemap
            Map myMap = new Map(Basemap.CreateImagery());

            // Provide used Map to the MapView
            _myMapView.Map = myMap;

            // Create a new WMS layer displaying the specified layers from the service
            _wmsLayer = new WmsLayer(_wmsUrl, _wmsLayerNames);

            try
            {
                // Load the layer
                await _wmsLayer.LoadAsync();

                // Add the layer to the map
                _myMapView.Map.OperationalLayers.Add(_wmsLayer);

                // Zoom to the layer's extent
                _myMapView.SetViewpoint(new Viewpoint(_wmsLayer.FullExtent));

                // Subscribe to tap events - starting point for feature identification
                _myMapView.GeoViewTapped += _myMapView_GeoViewTapped;
            }
            catch (Exception e)
            {
                new AlertDialog.Builder(this).SetMessage(e.ToString()).SetTitle("Error").Show();
            }
        }

        private void CreateLayout()
        {
            // Create a new vertical layout for the app
            _sampleLayout = new LinearLayout(this)
            {
                Orientation = Orientation.Vertical
            };

            // Configuration for having the mapview and webview fill the screen.
            _layoutParams = new LinearLayout.LayoutParams(
                ViewGroup.LayoutParams.MatchParent,
                ViewGroup.LayoutParams.MatchParent,
                1.0f
            );

            // Create and add the webview
            _htmlView = new WebView(this)
            {
                LayoutParameters = _layoutParams
            };

            _myMapView = new MapView(this);

            _myMapView.LayoutParameters = _layoutParams;

            // Create and add a help label
            TextView helpLabel = new TextView(this)
            {
                Text = "Tap to identify features."
            };
            helpLabel.SetTextColor(Color.Black);
            _sampleLayout.AddView(helpLabel);

            // Add the map view to the layout
            _sampleLayout.AddView(_myMapView);
            _sampleLayout.AddView(_htmlView);

            // Make the background white to hide the flash when the webview is removed/re-created.
            _sampleLayout.SetBackgroundColor(Color.White);

            // Show the layout in the app
            SetContentView(_sampleLayout);
        }

        private async void _myMapView_GeoViewTapped(object sender, GeoViewInputEventArgs e)
        {
            try
            {
                // Perform the identify operation
                IdentifyLayerResult myIdentifyResult = await _myMapView.IdentifyLayerAsync(_wmsLayer, e.Position, 20, false);

                // Return if there's nothing to show
                if (myIdentifyResult.GeoElements.Count < 1)
                {
                    return;
                }

                // Retrieve the identified feature, which is always a WmsFeature for WMS layers
                WmsFeature identifiedFeature = (WmsFeature)myIdentifyResult.GeoElements[0];

                // Retrieve the WmsFeature's HTML content
                string htmlContent = identifiedFeature.Attributes["HTML"].ToString();

                // Note that the service returns a boilerplate HTML result if there is no feature found.
                //    This would be a good place to check if the result looks like it includes feature detail. 

                // Display the string content as an HTML document. 
                ShowResult(htmlContent);
            }
            catch (Exception ex)
            {
                new AlertDialog.Builder(this).SetMessage(ex.ToString()).SetTitle("Error").Show();
            }
        }

        private void ShowResult(string htmlContent)
        {
            // Display the content in a web view. Note that the web view needs to be re-created each time.
            _sampleLayout.RemoveView(_htmlView);
            _htmlView = new WebView(this)
            {
                LayoutParameters = _layoutParams
            };
            _htmlView.LoadData(htmlContent, "text/html", "UTF-8");
            _sampleLayout.AddView(_htmlView);
        }
    }
}