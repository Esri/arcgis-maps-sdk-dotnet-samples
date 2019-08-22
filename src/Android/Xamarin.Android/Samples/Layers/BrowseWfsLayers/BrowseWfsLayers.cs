// Copyright 2019 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific 
// language governing permissions and limitations under the License.

using Android.App;
using Android.OS;
using Android.Views;
using Android.Widget;
using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Ogc;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.UI.Controls;
using System;
using System.Drawing;

namespace ArcGISRuntimeXamarin.Samples.BrowseWfsLayers
{
    [Activity]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Browse a WFS service for layers",
        "Layers",
        "Browse for layers in a WFS service.",
        "")]
    public class BrowseWfsLayers : Activity
    {
        // Hold references to the UI controls.
        private MapView _myMapView;
        private Switch _axisOrderSwitch;
        private ProgressBar _loadingProgressBar;
        private Button _loadLayerButton;

        // Hold a reference to the WFS service info.
        private WfsServiceInfo _serviceInfo;

        // URL to the WFS service.
        private const string ServiceUrl = "https://dservices2.arcgis.com/ZQgQTuoyBrtmoGdP/arcgis/services/Seattle_Downtown_Features/WFSServer?service=wfs&request=getcapabilities";

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            Title = "Browse a WFS service for layers";

            CreateLayout();
            Initialize();
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
            _loadLayerButton.Enabled = true;
        }

        private async void LayerMenu_LayerSelected(object sender, PopupMenu.MenuItemClickEventArgs e)
        {
            // Show the progress bar.
            _loadingProgressBar.Visibility = ViewStates.Visible;

            // Clear the existing layers.
            _myMapView.Map.OperationalLayers.Clear();

            try
            {
                // Get the selected layer info.
                WfsLayerInfo selectedLayerInfo = _serviceInfo.LayerInfos[e.Item.Order];

                // Create the WFS feature table.
                WfsFeatureTable table = new WfsFeatureTable(selectedLayerInfo);

                // Set the feature request mode to manual - only manual is supported at v100.5.
                // In this mode, you must manually populate the table - panning and zooming won't request features automatically.
                table.FeatureRequestMode = FeatureRequestMode.ManualCache;

                // Set the axis order based on the UI.
                if (_axisOrderSwitch.Checked)
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
                System.Diagnostics.Debug.WriteLine(exception);
                new AlertDialog.Builder(this).SetMessage(exception.ToString()).SetTitle("Couldn't load layer.").Show();
            }
            finally
            {
                // Hide the progress bar.
                _loadingProgressBar.Visibility = ViewStates.Gone;
            }
        }

        private void ChooseLayer_Clicked(object sender, EventArgs e)
        {
            // Get a reference to the button.
            Button loadButton = (Button)sender;

            // Create menu to show layer options.
            PopupMenu layerMenu = new PopupMenu(this, loadButton);
            layerMenu.MenuItemClick += LayerMenu_LayerSelected;

            // Create menu options.
            int index = 0;
            foreach (WfsLayerInfo layerInfo in _serviceInfo.LayerInfos)
            {
                layerMenu.Menu.Add(0, index, index, layerInfo.Title);
                index++;
            }

            // Show menu in the view.
            layerMenu.Show();
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
                    return new SimpleRenderer(new SimpleMarkerSymbol(SimpleMarkerSymbolStyle.Circle, GetRandomColor(), 4));
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

        private void CreateLayout()
        {
            // Create a new vertical layout for the app.
            var layout = new LinearLayout(this) { Orientation = Orientation.Vertical };

            // Add the axis order switch.
            _axisOrderSwitch = new Switch(this);
            _axisOrderSwitch.Text = "Swap coordinates";
            layout.AddView(_axisOrderSwitch);

            // Add the button.
            _loadLayerButton = new Button(this);
            _loadLayerButton.Text = "Choose a layer";
            _loadLayerButton.Click += ChooseLayer_Clicked;
            _loadLayerButton.Enabled = false;
            layout.AddView(_loadLayerButton);

            // Add the loading indicator.
            _loadingProgressBar = new ProgressBar(this);
            _loadingProgressBar.Indeterminate = true;
            _loadingProgressBar.Visibility = ViewStates.Gone;
            layout.AddView(_loadingProgressBar);

            // Add the map view to the layout.
            _myMapView = new MapView(this);
            layout.AddView(_myMapView);

            // Show the layout in the app.
            SetContentView(layout);
        }
    }
}
