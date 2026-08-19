// Copyright 2026 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Mapping.PointCloud;
using Microsoft.Maui.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ArcGIS.Samples.ApplyPointCloudRendererAndFilter
{
    [ArcGIS.Samples.Shared.Attributes.Sample(
        name: "Apply point cloud renderer and filter",
        category: "Scene",
        description: "Visualize point cloud data using different renderers and filters.",
        instructions: "The sample initially displays a point cloud layer using its RGB values. Select a renderer to visualize the points by RGB color, elevation, or LAS classification code.",
        tags: new[] { "3D", "classification", "filter", "lidar", "point cloud", "renderer", "scene layer", "visualization" })]
    public partial class ApplyPointCloudRendererAndFilter : ContentPage
    {
        private const string ElevationServiceUrl = "https://elevation3d.arcgis.com/arcgis/rest/services/WorldElevation3D/Terrain3D/ImageServer";
        private const string PointCloudServiceUrl = "https://tiles.arcgis.com/tiles/z2tnIkrLQ2BRzr6P/arcgis/rest/services/SONOMA_AREA1_LiDAR_RGB/SceneServer";
        // Bit 6 in the FLAGS attribute stores the LAS scan direction flag.
        private const uint ScanDirectionFlagBit = 6;

        private PointCloudLayer _pointCloudLayer;
        private PointCloudRenderer[] _renderers;

        private bool _isUpdatingBitfieldControls;

        public ApplyPointCloudRendererAndFilter()
        {
            InitializeComponent();
            _ = Initialize();
        }

        private async Task Initialize()
        {
            // Create a local scene with elevation.
            Scene scene = new Scene(SceneViewingMode.Local, BasemapStyle.ArcGISStreets);
            scene.BaseSurface.ElevationSources.Add(new ArcGISTiledElevationSource(new Uri(ElevationServiceUrl)));

            // Create the point cloud layer from the scene service.
            _pointCloudLayer = new PointCloudLayer(new Uri(PointCloudServiceUrl));

            // Load the layer to populate the attribute schema used by the renderers and filters.
            await _pointCloudLayer.LoadAsync();

            // Read each point's packed color from the RGB attribute.
            PointCloudRGBRenderer rgbRenderer = new PointCloudRGBRenderer("RGB");

            // Map continuous ELEVATION values through a blue-to-red color ramp.
            List<PointCloudColorStop> colorStops = new List<PointCloudColorStop>
            {
                new PointCloudColorStop(System.Drawing.Color.FromArgb(31, 79, 255), 0),
                new PointCloudColorStop(System.Drawing.Color.FromArgb(33, 163, 102), 30),
                new PointCloudColorStop(System.Drawing.Color.FromArgb(229, 57, 53), 90)
            };
            PointCloudStretchRenderer stretchRenderer = new PointCloudStretchRenderer("ELEVATION", colorStops);

            // Group ELEVATION values into discrete ranges with a color for each range.
            List<PointCloudColorClassBreak> classBreaks = new List<PointCloudColorClassBreak>
            {
                new PointCloudColorClassBreak(System.Drawing.Color.FromArgb(96, 67, 151), 0, 20),
                new PointCloudColorClassBreak(System.Drawing.Color.FromArgb(65, 145, 136), 20, 40),
                new PointCloudColorClassBreak(System.Drawing.Color.FromArgb(216, 155, 77), 40, float.MaxValue)
            };
            PointCloudClassBreaksRenderer classBreaksRenderer = new PointCloudClassBreaksRenderer("ELEVATION", classBreaks);

            // Assign a color to each standard LAS classification code in CLASS_CODE.
            List<PointCloudColorUniqueValue> uniqueValues = new List<PointCloudColorUniqueValue>
            {
                new PointCloudColorUniqueValue(System.Drawing.Color.FromArgb(139, 178, 194), new[] { "1" }),
                new PointCloudColorUniqueValue(System.Drawing.Color.FromArgb(212, 223, 160), new[] { "2" }),
                new PointCloudColorUniqueValue(System.Drawing.Color.FromArgb(168, 208, 141), new[] { "3" }),
                new PointCloudColorUniqueValue(System.Drawing.Color.FromArgb(112, 173, 71), new[] { "4" }),
                new PointCloudColorUniqueValue(System.Drawing.Color.FromArgb(47, 107, 47), new[] { "5" }),
                new PointCloudColorUniqueValue(System.Drawing.Color.FromArgb(200, 62, 62), new[] { "6" }),
                new PointCloudColorUniqueValue(System.Drawing.Color.FromArgb(187, 185, 220), new[] { "7" }),
                new PointCloudColorUniqueValue(System.Drawing.Color.FromArgb(187, 225, 228), new[] { "8" }),
                new PointCloudColorUniqueValue(System.Drawing.Color.FromArgb(155, 191, 177), new[] { "9" }),
                new PointCloudColorUniqueValue(System.Drawing.Color.FromArgb(75, 85, 99), new[] { "10" }),
                new PointCloudColorUniqueValue(System.Drawing.Color.FromArgb(107, 114, 128), new[] { "11" }),
                new PointCloudColorUniqueValue(System.Drawing.Color.FromArgb(209, 213, 219), new[] { "12" }),
                new PointCloudColorUniqueValue(System.Drawing.Color.FromArgb(245, 158, 11), new[] { "13" }),
                new PointCloudColorUniqueValue(System.Drawing.Color.FromArgb(234, 179, 8), new[] { "14" }),
                new PointCloudColorUniqueValue(System.Drawing.Color.FromArgb(124, 58, 237), new[] { "15" }),
                new PointCloudColorUniqueValue(System.Drawing.Color.FromArgb(236, 72, 153), new[] { "16" }),
                new PointCloudColorUniqueValue(System.Drawing.Color.FromArgb(139, 90, 43), new[] { "17" }),
                new PointCloudColorUniqueValue(System.Drawing.Color.FromArgb(17, 24, 39), new[] { "18" })
            };
            PointCloudUniqueValueRenderer uniqueValueRenderer = new PointCloudUniqueValueRenderer("CLASS_CODE", uniqueValues);

            // PointsPerInch controls display density; the splat scale controls each point's size.
            _renderers = new PointCloudRenderer[]
            {
                rgbRenderer,
                stretchRenderer,
                classBreaksRenderer,
                uniqueValueRenderer
            };
            foreach (PointCloudRenderer renderer in _renderers)
            {
                renderer.PointsPerInch = 25;
                renderer.SizeAlgorithm = new PointCloudSplatAlgorithm(1.0);
            }

            // Apply the RGB renderer initially.
            _pointCloudLayer.Renderer = rgbRenderer;

            // Add the point cloud layer and show a close street-level view.
            scene.OperationalLayers.Add(_pointCloudLayer);
            MySceneView.Scene = scene;
            MapPoint cameraLocation = new MapPoint(
                -13631735.748425495,
                4621846.155726249,
                117.0263783885166,
                SpatialReferences.WebMercator);
            MySceneView.SetViewpointCamera(new Camera(cameraLocation, 140.19101175677667, 62.60556759594034, 0));
        }

        private void RendererRadioButton_CheckedChanged(object sender, CheckedChangedEventArgs e)
        {
            if (!e.Value || _pointCloudLayer == null || _renderers == null)
            {
                return;
            }

            int rendererIndex;
            if (sender == RgbRendererRadioButton)
            {
                rendererIndex = 0;
            }
            else if (sender == StretchRendererRadioButton)
            {
                rendererIndex = 1;
            }
            else if (sender == ClassBreaksRendererRadioButton)
            {
                rendererIndex = 2;
            }
            else if (sender == UniqueValueRendererRadioButton)
            {
                rendererIndex = 3;
            }
            else
            {
                return;
            }

            // Apply the selected renderer to the point cloud layer.
            _pointCloudLayer.Renderer = _renderers[rendererIndex];

            if (_pointCloudLayer.Renderer.SizeAlgorithm is PointCloudSplatAlgorithm splatAlgorithm)
            {
                PointSizeSlider.Value = splatAlgorithm.ScaleFactor;
            }
        }

        private void PointSizeSlider_ValueChanged(object sender, ValueChangedEventArgs e)
        {
            if (_pointCloudLayer?.Renderer?.SizeAlgorithm is PointCloudSplatAlgorithm splatAlgorithm)
            {
                // Update the active renderer's existing splat algorithm.
                splatAlgorithm.ScaleFactor = PointSizeSlider.Value;
            }
        }

        private void ValueFilterModeRadioButton_CheckedChanged(object sender, CheckedChangedEventArgs e)
        {
            if (!e.Value)
            {
                return;
            }

            UpdateValueFilter();
        }

        private void ValueFilterCheckBox_CheckedChanged(object sender, CheckedChangedEventArgs e)
        {
            UpdateValueFilter();
        }

        private void UpdateValueFilter()
        {
            if (_pointCloudLayer == null)
            {
                return;
            }

            // These values are the LAS classification codes for ground, high vegetation, and buildings.
            List<double> selectedClassCodes = new List<double>();
            if (GroundCheckBox.IsChecked)
            {
                selectedClassCodes.Add(2);
            }
            if (HighVegetationCheckBox.IsChecked)
            {
                selectedClassCodes.Add(5);
            }
            if (BuildingCheckBox.IsChecked)
            {
                selectedClassCodes.Add(6);
            }

            PointCloudValueFilter valueFilter = _pointCloudLayer.Filters.OfType<PointCloudValueFilter>().FirstOrDefault();
            if (selectedClassCodes.Count == 0)
            {
                // Each filter is independent, so remove only the value filter when it has no selections.
                if (valueFilter != null)
                {
                    _pointCloudLayer.Filters.Remove(valueFilter);
                }
                return;
            }

            PointCloudValueFilterMode mode = ExcludeValueFilterRadioButton.IsChecked
                ? PointCloudValueFilterMode.Exclude
                : PointCloudValueFilterMode.Include;

            if (valueFilter == null)
            {
                // Add the value filter on first use.
                valueFilter = new PointCloudValueFilter("CLASS_CODE", selectedClassCodes, mode);
                _pointCloudLayer.Filters.Add(valueFilter);
            }
            else
            {
                // Update the existing value filter.
                valueFilter.Values.Clear();
                foreach (double classCode in selectedClassCodes)
                {
                    valueFilter.Values.Add(classCode);
                }
                valueFilter.Mode = mode;
            }
        }

        private void ClearValueFilterButton_Clicked(object sender, EventArgs e)
        {
            GroundCheckBox.IsChecked = false;
            HighVegetationCheckBox.IsChecked = false;
            BuildingCheckBox.IsChecked = false;
            UpdateValueFilter();
        }

        private void ReturnFilterCheckBox_CheckedChanged(object sender, CheckedChangedEventArgs e)
        {
            UpdateReturnFilter();
        }

        private void UpdateReturnFilter()
        {
            if (_pointCloudLayer == null)
            {
                return;
            }

            // RETURNS encodes where a lidar pulse return occurs in a sequence of returns.
            List<PointCloudReturnType> selectedReturnTypes = new List<PointCloudReturnType>();
            if (FirstOfManyCheckBox.IsChecked)
            {
                selectedReturnTypes.Add(PointCloudReturnType.FirstOfMany);
            }
            if (LastCheckBox.IsChecked)
            {
                selectedReturnTypes.Add(PointCloudReturnType.Last);
            }
            if (LastOfManyCheckBox.IsChecked)
            {
                selectedReturnTypes.Add(PointCloudReturnType.LastOfMany);
            }
            if (SingleCheckBox.IsChecked)
            {
                selectedReturnTypes.Add(PointCloudReturnType.Single);
            }

            PointCloudReturnFilter returnFilter = _pointCloudLayer.Filters.OfType<PointCloudReturnFilter>().FirstOrDefault();
            if (selectedReturnTypes.Count == 0)
            {
                if (returnFilter != null)
                {
                    _pointCloudLayer.Filters.Remove(returnFilter);
                }
                return;
            }

            if (returnFilter == null)
            {
                // Add the return filter on first use.
                returnFilter = new PointCloudReturnFilter("RETURNS", selectedReturnTypes);
                _pointCloudLayer.Filters.Add(returnFilter);
            }
            else
            {
                // Update the existing return filter.
                returnFilter.IncludedReturns.Clear();
                foreach (PointCloudReturnType returnType in selectedReturnTypes)
                {
                    returnFilter.IncludedReturns.Add(returnType);
                }
            }
        }

        private void ClearReturnFilterButton_Clicked(object sender, EventArgs e)
        {
            FirstOfManyCheckBox.IsChecked = false;
            LastCheckBox.IsChecked = false;
            LastOfManyCheckBox.IsChecked = false;
            SingleCheckBox.IsChecked = false;
            UpdateReturnFilter();
        }

        private void BitfieldFilterCheckBox_CheckedChanged(object sender, CheckedChangedEventArgs e)
        {
            if (_pointCloudLayer == null || _isUpdatingBitfieldControls)
            {
                return;
            }

            _isUpdatingBitfieldControls = true;
            if (sender == RequireSetCheckBox && RequireSetCheckBox.IsChecked)
            {
                RequireClearCheckBox.IsChecked = false;
            }
            else if (sender == RequireClearCheckBox && RequireClearCheckBox.IsChecked)
            {
                RequireSetCheckBox.IsChecked = false;
            }
            _isUpdatingBitfieldControls = false;

            UpdateBitfieldFilter();
        }

        private void UpdateBitfieldFilter()
        {
            if (_pointCloudLayer == null)
            {
                return;
            }

            // Bitfield filters contain bit positions, not numeric mask values.
            List<uint> requiredClearBits = new List<uint>();
            List<uint> requiredSetBits = new List<uint>();
            if (RequireClearCheckBox.IsChecked)
            {
                requiredClearBits.Add(ScanDirectionFlagBit);
            }
            if (RequireSetCheckBox.IsChecked)
            {
                requiredSetBits.Add(ScanDirectionFlagBit);
            }

            PointCloudBitfieldFilter bitfieldFilter = _pointCloudLayer.Filters.OfType<PointCloudBitfieldFilter>().FirstOrDefault();
            if (requiredClearBits.Count == 0 && requiredSetBits.Count == 0)
            {
                if (bitfieldFilter != null)
                {
                    _pointCloudLayer.Filters.Remove(bitfieldFilter);
                }
                return;
            }

            if (bitfieldFilter == null)
            {
                // Add the bitfield filter on first use.
                bitfieldFilter = new PointCloudBitfieldFilter("FLAGS", requiredClearBits, requiredSetBits);
                _pointCloudLayer.Filters.Add(bitfieldFilter);
            }
            else
            {
                // Update the existing bitfield filter.
                bitfieldFilter.RequiredClearBits.Clear();
                foreach (uint bit in requiredClearBits)
                {
                    bitfieldFilter.RequiredClearBits.Add(bit);
                }
                bitfieldFilter.RequiredSetBits.Clear();
                foreach (uint bit in requiredSetBits)
                {
                    bitfieldFilter.RequiredSetBits.Add(bit);
                }
            }
        }

        private void ClearBitfieldFilterButton_Clicked(object sender, EventArgs e)
        {
            _isUpdatingBitfieldControls = true;
            RequireSetCheckBox.IsChecked = false;
            RequireClearCheckBox.IsChecked = false;
            _isUpdatingBitfieldControls = false;
            UpdateBitfieldFilter();
        }
    }
}
