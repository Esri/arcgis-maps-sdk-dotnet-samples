// Copyright 2021 Esri.
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
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Rasters;
using Esri.ArcGISRuntime.UI.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ArcGISRuntimeXamarin.Samples.ApplyMosaicRule
{
    [Activity(ConfigurationChanges = Android.Content.PM.ConfigChanges.Orientation | Android.Content.PM.ConfigChanges.ScreenSize)]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        name: "Apply mosaic rule to rasters",
        category: "Layers",
        description: "Apply mosaic rule to a mosaic dataset of rasters.",
        instructions: "When the rasters are loaded, choose from a list of preset mosaic rules to apply to the rasters.",
        tags: new[] { "image service", "mosaic method", "mosaic rule", "raster" })]
    public class ApplyMosaicRule : Activity
    {
        // Hold references to the UI controls.
        private MapView _myMapView;
        private Button _rulePickerButton;

        private ImageServiceRaster _imageServiceRaster;

        // Different mosaic rules to use with the image service raster.
        private Dictionary<string, MosaicRule> _mosaicRules = new Dictionary<string, MosaicRule>
        {
            { "None", new MosaicRule { MosaicMethod = MosaicMethod.None} },
            { "Northwest", new MosaicRule { MosaicMethod = MosaicMethod.Northwest, MosaicOperation = MosaicOperation.First} },
            { "Center", new MosaicRule { MosaicMethod = MosaicMethod.Center, MosaicOperation = MosaicOperation.Blend} },
            { "ByAttribute", new MosaicRule { MosaicMethod = MosaicMethod.Attribute, SortField = "OBJECTID"} },
            { "LockRaster", new MosaicRule { MosaicMethod = MosaicMethod.LockRaster, LockRasterIds = { 1, 7, 12 } } },
        };

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            Title = "Apply mosaic rule to rasters";

            CreateLayout();
            _ = Initialize();
        }

        private async Task Initialize()
        {
            // Create a raster layer using an image service.
            _imageServiceRaster = new ImageServiceRaster(new Uri("https://sampleserver7.arcgisonline.com/arcgis/rest/services/amberg_germany/ImageServer"));
            RasterLayer rasterLayer = new RasterLayer(_imageServiceRaster);
            await rasterLayer.LoadAsync();

            // Create a map with the raster layer.
            _myMapView.Map = new Map(BasemapStyle.ArcGISTopographic);
            _myMapView.Map.OperationalLayers.Add(rasterLayer);
            await _myMapView.SetViewpointAsync(new Viewpoint(rasterLayer.FullExtent));

            // Populate the combo box.
            _imageServiceRaster.MosaicRule = _mosaicRules["None"];
            _rulePickerButton.Text = "Rule: None";
        }

        private void RuleClick(object sender, EventArgs e)
        {
            // Create UI for terminal selection.
            AlertDialog.Builder builder = new AlertDialog.Builder(this);
            builder.SetTitle("Choose mosaic rule");
            builder.SetItems(_mosaicRules.Keys.ToList().ToArray(), RuleClick);
            builder.Show();
        }

        private void RuleClick(object sender, DialogClickEventArgs e)
        {
            string ruleName = _mosaicRules.Keys.ToList().ToArray()[e.Which];

            // Change the mosaic rule used for the image service raster.
            _imageServiceRaster.MosaicRule = _mosaicRules[ruleName];
            _rulePickerButton.Text = $"Rule: {ruleName}";
        }

        private void CreateLayout()
        {
            // Create a new vertical layout for the app.
            var layout = new LinearLayout(this) { Orientation = Orientation.Vertical };

            // Create the help label.
            TextView helpLabel = new TextView(this);
            helpLabel.Text = "Choose a mosaic rule to apply to the image service.";
            helpLabel.TextAlignment = TextAlignment.TextStart;
            helpLabel.Gravity = GravityFlags.Center;

            // Add the help label to the layout.
            layout.AddView(helpLabel);

            // Add the rule change button.
            _rulePickerButton = new Button(this);
            layout.AddView(_rulePickerButton);

            // Add the map view to the layout.
            _myMapView = new MapView(this);
            layout.AddView(_myMapView);

            // Show the layout in the app.
            SetContentView(layout);

            // Add an event handler for the map view.
            _rulePickerButton.Click += RuleClick;
        }
    }
}