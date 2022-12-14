// Copyright 2019 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using ArcGIS.Samples.Managers;
using Esri.ArcGISRuntime.Mapping;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media;

namespace ArcGIS.WPF.Samples.ControlAnnotationSublayerVisibility
{
    [ArcGIS.Samples.Shared.Attributes.Sample(
        name: "Control annotation sublayer visibility",
        category: "Layers",
        description: "Use annotation sublayers to gain finer control of annotation layer subtypes.",
        instructions: "Start the sample and take note of the visibility of the annotation. Zoom in and out to see the annotation turn on and off based on scale ranges set on the data.",
        tags: new[] { "annotation", "scale", "text", "utilities", "visualization" })]
    [ArcGIS.Samples.Shared.Attributes.OfflineData("b87307dcfb26411eb2e92e1627cb615b")]
    public partial class ControlAnnotationSublayerVisibility
    {
        // Mobile map package that contains annotation layers.
        private MobileMapPackage _mobileMapPackage;

        // Sub layers of the annotation layer.
        private AnnotationSublayer _openSublayer;
        private AnnotationSublayer _closedSublayer;

        public ControlAnnotationSublayerVisibility()
        {
            InitializeComponent();
            _ = Initialize();
        }

        private async Task Initialize()
        {
            try
            {
                // Load the mobile map package.
                _mobileMapPackage = new MobileMapPackage(DataManager.GetDataFolder("b87307dcfb26411eb2e92e1627cb615b", "GasDeviceAnno.mmpk"));
                await _mobileMapPackage.LoadAsync();

                // Set the mapview to display the map from the package.
                MyMapView.Map = _mobileMapPackage.Maps.First();

                // Get the annotation layer from the MapView operational layers.
                AnnotationLayer annotationLayer = (AnnotationLayer)MyMapView.Map.OperationalLayers.Where(layer => layer is AnnotationLayer).First();

                // Load the annotation layer.
                await annotationLayer.LoadAsync();

                // Get the annotation sub layers.
                _closedSublayer = (AnnotationSublayer)annotationLayer.SublayerContents[0];
                _openSublayer = (AnnotationSublayer)annotationLayer.SublayerContents[1];

                // Set the label content.
                OpenLabel.Content = $"{_openSublayer.Name} (1:{_openSublayer.MaxScale} - 1:{_openSublayer.MinScale})";
                ClosedLabel.Content = _closedSublayer.Name;

                // Enable the check boxes.
                OpenCheckBox.IsEnabled = true;
                ClosedCheckBox.IsEnabled = true;

                // Add event handler for changing the text to indicate whether the "open" sublayer is visible at the current scale.
                MyMapView.ViewpointChanged += (s, e) =>
                {
                    // Check if the sublayer is visible at the current map scale.
                    if (_openSublayer.IsVisibleAtScale(MyMapView.MapScale))
                    {
                        OpenLabel.Foreground = new SolidColorBrush(Colors.Black);
                    }
                    else
                    {
                        OpenLabel.Foreground = new SolidColorBrush(Colors.Gray);
                    }

                    // Set the current map scale text.
                    ScaleLabel.Content = "Current map scale: 1:" + (int)MyMapView.MapScale;
                };
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }

        private void OpenCheckBoxChanged(object sender, System.Windows.RoutedEventArgs e)
        {
            // Set the visibility of the sub layer.
            if (_openSublayer != null) _openSublayer.IsVisible = OpenCheckBox.IsChecked == true;
        }

        private void ClosedCheckBoxChanged(object sender, System.Windows.RoutedEventArgs e)
        {
            // Set the visibility of the sub layer.
            if (_closedSublayer != null) _closedSublayer.IsVisible = ClosedCheckBox.IsChecked == true;
        }
    }
}