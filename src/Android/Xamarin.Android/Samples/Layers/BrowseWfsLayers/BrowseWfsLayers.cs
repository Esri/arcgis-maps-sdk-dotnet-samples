// Copyright 2019 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using Android.App;
using Android.Content;
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
        "Browse WFS layers",
        "Layers",
        "Browse a WFS service for layers and add them to the map.",
        "A list of layers in the WFS service will be shown. Select a layer to display.",
        "OGC", "WFS", "browse", "catalog", "feature", "layers", "service", "web")]
    public class BrowseWfsLayers : Activity
    {
        // Hold references to the UI controls.
        private MapView _myMapView;
        private Switch _axisOrderSwitch;
        private ProgressBar _loadingProgressBar;
        private Button _loadLayerButton;
        private Button _loadServiceButton;
        private EditText _urlEntry;

        // Hold a reference to the WFS service info.
        private WfsServiceInfo _serviceInfo;

        // URL to the WFS service.
        private const string ServiceUrl = "https://dservices2.arcgis.com/ZQgQTuoyBrtmoGdP/arcgis/services/Seattle_Downtown_Features/WFSServer?service=wfs&request=getcapabilities";

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            Title = "Browse WFS service for layers";

            CreateLayout();
            Initialize();
        }

        private void Initialize()
        {
            // Update the UI.
            _loadServiceButton.Text = ServiceUrl;

            // Create the map with imagery basemap.
            _myMapView.Map = new Map(Basemap.CreateImagery());

            LoadService();
        }

        private async void LoadService()
        {
            try
            {
                _loadingProgressBar.Visibility = ViewStates.Visible;
                _loadLayerButton.Enabled = false;
                _loadServiceButton.Enabled = false;

                // Create the WFS service.
                WfsService service = new WfsService(new Uri(_loadServiceButton.Text));

                // Load the WFS service.
                await service.LoadAsync();

                // Get the service metadata.
                _serviceInfo = service.ServiceInfo;
            }
            catch (Exception ex)
            {
                new AlertDialog.Builder(this).SetMessage(ex.Message).SetTitle("Error loading service").Show();
            }
            finally
            {
                // Update the UI.
                _loadingProgressBar.Visibility = ViewStates.Gone;
                _loadLayerButton.Enabled = true;
                _loadServiceButton.Enabled = true;
            }
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
                wfsFeatureLayer.Renderer = GetRendererForTable(table) ?? wfsFeatureLayer.Renderer;

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

        private void ServiceClicked(object sender, EventArgs e)
        {
            // Create a text-entry prompt for changing the WFS url.
            AlertDialog.Builder builder = new AlertDialog.Builder(this);
            builder.SetTitle("Set WFS service URL");

            // Create the text entry.
            _urlEntry = new EditText(this);
            _urlEntry.Text = _loadServiceButton.Text;
            _urlEntry.InputType = Android.Text.InputTypes.TextVariationUri;
            builder.SetView(_urlEntry);

            // Finish building the dialog and display it.
            builder.SetPositiveButton("Load", ServicePressed);
            builder.SetCancelable(true);
            builder.Show();
        }

        private void ServicePressed(object sender, DialogClickEventArgs e)
        {
            _loadServiceButton.Text = _urlEntry.Text;
            LoadService();
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

        private void CreateLayout()
        {
            // Create a new vertical layout for the app.
            var layout = new LinearLayout(this) { Orientation = Orientation.Vertical };

            // Add the button.
            _loadServiceButton = new Button(this);
            _loadServiceButton.Text = "Choose a layer";
            _loadServiceButton.Click += ServiceClicked;
            _loadServiceButton.Enabled = false;
            layout.AddView(_loadServiceButton);

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