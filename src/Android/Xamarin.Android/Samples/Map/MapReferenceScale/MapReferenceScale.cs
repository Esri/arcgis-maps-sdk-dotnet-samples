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
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Portal;
using Esri.ArcGISRuntime.UI.Controls;
using System;
using System.Linq;

namespace ArcGISRuntimeXamarin.Samples.MapReferenceScale
{
    [Activity (ConfigurationChanges=Android.Content.PM.ConfigChanges.Orientation | Android.Content.PM.ConfigChanges.ScreenSize)]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Map reference scale",
        "Map",
        "Set the map's reference scale and which feature layers should honor the reference scale.",
        "Use the control at the top to set the map's reference scale (1:500,000 1:250,000 1:100,000 1:50,000). Use the menu checkboxes in the layer menu to set which feature layers should honor the reference scale.",
        "map", "reference scale", "scene")]
    public class MapReferenceScale : Activity
    {
        // Hold references to the UI controls.
        private MapView _myMapView;
        private TextView _currentScaleLabel;
        private TextView _referenceScaleSelectionLabel;
        private Button _layersButton;

        // List of reference scale options.
        private readonly double[] _referenceScales =
        {
            50000,
            100000,
            250000,
            500000
        };

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            Title = "Map reference scale";

            CreateLayout();
            Initialize();
        }

        private async void Initialize()
        {
            // Create a portal and an item; the map will be loaded from portal item.
            ArcGISPortal portal = await ArcGISPortal.CreateAsync(new Uri("https://runtime.maps.arcgis.com"));
            PortalItem mapItem = await PortalItem.CreateAsync(portal, "3953413f3bd34e53a42bf70f2937a408");

            // Create the map from the item.
            Map webMap = new Map(mapItem);

            // Update the UI when the map navigates.
            _myMapView.ViewpointChanged += (o, e) => _currentScaleLabel.Text = $"Current map scale: 1:{_myMapView.MapScale:n0}";

            // Display the map.
            _myMapView.Map = webMap;

            // Wait for the map to load.
            await webMap.LoadAsync();

            // Enable the button now that the map is ready.
            _layersButton.Enabled = true;
        }

        private void ChooseScale_Clicked(object sender, EventArgs e)
        {
            try
            {
                Button scaleButton = (Button) sender;

                // Create menu for showing layer controls.
                PopupMenu sublayersMenu = new PopupMenu(this, scaleButton);
                sublayersMenu.MenuItemClick += ScaleSelection_Changed;

                // Create menu options.
                int index = 0;
                foreach (double scale in _referenceScales)
                {
                    // Add the menu item.
                    sublayersMenu.Menu.Add(0, index, index, $"1:{scale:n0}");
                    index++;
                }

                // Show menu in the view
                sublayersMenu.Show();
            }
            catch (Exception ex)
            {
                new AlertDialog.Builder(this).SetMessage(ex.ToString()).SetTitle("Error").Show();
            }
        }

        private void ScaleSelection_Changed(object sender, PopupMenu.MenuItemClickEventArgs e)
        {
            // Find the index of the selected scale.
            int selectionIndex = e.Item.Order;

            // Apply the selected scale.
            _myMapView.Map.ReferenceScale = _referenceScales[selectionIndex];

            // Update the UI.
            _referenceScaleSelectionLabel.Text = $"1:{_referenceScales[selectionIndex]:n0}";
        }

        private void ChooseLayers_Clicked(object sender, EventArgs e)
        {
            try
            {
                Button layersButton = (Button) sender;

                // Create menu for showing layer controls.
                PopupMenu sublayersMenu = new PopupMenu(this, layersButton);
                sublayersMenu.MenuItemClick += LayerScaleSelection_Changed;

                // Create menu options.
                int index = 0;
                foreach (FeatureLayer layer in _myMapView.Map.OperationalLayers.OfType<FeatureLayer>())
                {
                    // Add the menu item.
                    sublayersMenu.Menu.Add(0, index, index, layer.Name);

                    // Configure the menu item.
                    sublayersMenu.Menu.GetItem(index).SetCheckable(true).SetChecked(layer.ScaleSymbols);

                    index++;
                }

                // Show menu in the view
                sublayersMenu.Show();
            }
            catch (Exception ex)
            {
                new AlertDialog.Builder(this).SetMessage(ex.ToString()).SetTitle("Error").Show();
            }
        }

        private void LayerScaleSelection_Changed(object sender, PopupMenu.MenuItemClickEventArgs e)
        {
            // Get the checked item.
            int selectedLayerIndex = e.Item.Order;

            // Find the layer.
            FeatureLayer selectedLayer = _myMapView.Map.OperationalLayers.OfType<FeatureLayer>().ElementAt(selectedLayerIndex);

            // Set the symbol scale mode.
            selectedLayer.ScaleSymbols = !selectedLayer.ScaleSymbols;

            // Update the UI.
            e.Item.SetChecked(selectedLayer.ScaleSymbols);
        }

        private void CreateLayout()
        {
            // Create a new vertical layout for the app.
            var layout = new LinearLayout(this) {Orientation = Orientation.Vertical};

            // Button for choosing the map's reference scale.
            Button chooseScaleButton = new Button(this);
            chooseScaleButton.Text = "Choose map reference scale";
            chooseScaleButton.Click += ChooseScale_Clicked;
            layout.AddView(chooseScaleButton);

            // Label for showing current reference scale.
            _referenceScaleSelectionLabel = new TextView(this);
            _referenceScaleSelectionLabel.Text = "Choose a reference scale";
            layout.AddView(_referenceScaleSelectionLabel);

            // Button for selecting which layers will have symbol scaling enabled.
            _layersButton = new Button(this);
            _layersButton.Text = "Manage scaling for layers";
            _layersButton.Click += ChooseLayers_Clicked;
            layout.AddView(_layersButton);

            // Add a horizontal line separator.
            View separatorView = new View(this);
            LinearLayout.LayoutParams lp = new LinearLayout.LayoutParams(ViewGroup.LayoutParams.MatchParent, 1);
            separatorView.LayoutParameters = lp;
            layout.AddView(separatorView);

            // Add a label that shows the current map scale.
            _currentScaleLabel = new TextView(this);
            _currentScaleLabel.Text = "Current map scale: ";
            layout.AddView(_currentScaleLabel);

            // Add the map view to the layout.
            _myMapView = new MapView(this);
            layout.AddView(_myMapView);

            // Show the layout in the app.
            SetContentView(layout);
        }
    }
}