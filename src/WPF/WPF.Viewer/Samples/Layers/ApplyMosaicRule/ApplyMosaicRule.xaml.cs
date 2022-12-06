// Copyright 2021 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Rasters;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ArcGIS.WPF.Samples.ApplyMosaicRule
{
    [ArcGIS.Samples.Shared.Attributes.Sample(
        name: "Apply mosaic rule to rasters",
        category: "Layers",
        description: "Apply mosaic rule to a mosaic dataset of rasters.",
        instructions: "When the rasters are loaded, choose from a list of preset mosaic rules to apply to the rasters.",
        tags: new[] { "image service", "mosaic method", "mosaic rule", "raster" })]
    public partial class ApplyMosaicRule
    {
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

        public ApplyMosaicRule()
        {
            InitializeComponent();
            _ = Initialize();
        }

        private async Task Initialize()
        {
            // Create a raster layer using an image service.
            _imageServiceRaster = new ImageServiceRaster(new Uri("https://sampleserver7.arcgisonline.com/server/rest/services/amberg_germany/ImageServer"));
            RasterLayer rasterLayer = new RasterLayer(_imageServiceRaster);
            await rasterLayer.LoadAsync();

            // Create a map with the raster layer.
            MyMapView.Map = new Map(BasemapStyle.ArcGISTopographic);
            MyMapView.Map.OperationalLayers.Add(rasterLayer);
            await MyMapView.SetViewpointAsync(new Viewpoint(rasterLayer.FullExtent));

            // Populate the combo box.
            MosaicRulesBox.ItemsSource = _mosaicRules.Keys;
            MosaicRulesBox.SelectedIndex = 0;
        }

        private void MosaicRulesBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            // Change the mosaic rule used for the image service raster.
            _imageServiceRaster.MosaicRule = _mosaicRules[MosaicRulesBox.SelectedItem as string];
        }
    }
}