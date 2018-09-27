// Copyright 2018 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific 
// language governing permissions and limitations under the License.

using System;
using ArcGISRuntime.Samples.Managers;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Portal;
using Esri.ArcGISRuntime.UI.Controls;
using Foundation;
using UIKit;

namespace ArcGISRuntimeXamarin.Samples.DisplayKml
{
    [Register("DisplayKml")]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Display KML",
        "Layers",
        "Display a KML file from URL, a local file, or a portal item.",
        "")]
    [ArcGISRuntime.Samples.Shared.Attributes.OfflineData("324e4742820e46cfbe5029ff2c32cb1f")]
    public class DisplayKml : UIViewController
    {
        // Hold a reference to the SceneView.
        private SceneView _mySceneView;
        private UISegmentedControl _dataChoiceButton;

        private readonly Envelope _usEnvelope = new Envelope(-144.619561355187, 18.0328662832097, -66.0903762761083, 67.6390975806745, SpatialReferences.Wgs84);
        private readonly string[] _sources = {"URL", "Local file", "Portal item"};

        public DisplayKml()
        {
            Title = "Display KML";
        }

        private void Initialize()
        {
            // Set up the basemap.
            _mySceneView.Scene = new Scene(Basemap.CreateImageryWithLabels());
        }

        public override void LoadView()
        {
            // Create the views.
            _mySceneView = new SceneView();
            _mySceneView.TranslatesAutoresizingMaskIntoConstraints = false;
            _dataChoiceButton = new UISegmentedControl(_sources)
            {
                BackgroundColor = UIColor.FromWhiteAlpha(0, .7f),
                TintColor = UIColor.White,
                TranslatesAutoresizingMaskIntoConstraints = false
            };
            // Clean up borders of segmented control - avoid corner pixels.
            _dataChoiceButton.ClipsToBounds = true;
            _dataChoiceButton.Layer.CornerRadius = 5;

            _dataChoiceButton.ValueChanged += DataChoiceButtonOnValueChanged;
            _dataChoiceButton.SelectedSegment = 0;

            // Add the views.
            View = new UIView();
            View.AddSubviews(_mySceneView, _dataChoiceButton);

            // Apply constraints.
            _mySceneView.TopAnchor.ConstraintEqualTo(View.TopAnchor).Active = true;
            _mySceneView.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor).Active = true;
            _mySceneView.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor).Active = true;
            _mySceneView.BottomAnchor.ConstraintEqualTo(View.BottomAnchor).Active = true;

            _dataChoiceButton.LeadingAnchor.ConstraintEqualTo(View.LayoutMarginsGuide.LeadingAnchor).Active = true;
            _dataChoiceButton.TrailingAnchor.ConstraintEqualTo(View.LayoutMarginsGuide.TrailingAnchor).Active = true;
            _dataChoiceButton.TopAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.TopAnchor, 8).Active = true;
        }

        private async void DataChoiceButtonOnValueChanged(object sender, EventArgs e)
        {
            // Clear existing layers.
            _mySceneView.Scene.OperationalLayers.Clear();

            // Get the name of the selected layer.
            string name = _sources[_dataChoiceButton.SelectedSegment];

            // Create the layer using the chosen constructor.
            KmlLayer layer;
            switch (name)
            {
                case "URL":
                default:
                    layer = new KmlLayer(new Uri("https://www.wpc.ncep.noaa.gov/kml/noaa_chart/WPC_Day1_SigWx.kml"));
                    break;
                case "Local file":
                    string filePath = DataManager.GetDataFolder("324e4742820e46cfbe5029ff2c32cb1f", "US_State_Capitals.kml");
                    layer = new KmlLayer(new Uri(filePath));
                    break;
                case "Portal item":
                    ArcGISPortal portal = await ArcGISPortal.CreateAsync();
                    PortalItem item = await PortalItem.CreateAsync(portal, "9fe0b1bfdcd64c83bd77ea0452c76253");
                    layer = new KmlLayer(item);
                    break;
            }

            // Add the selected layer to the map.
            _mySceneView.Scene.OperationalLayers.Add(layer);

            // Zoom to the extent of the United States.
            await _mySceneView.SetViewpointAsync(new Viewpoint(_usEnvelope));
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            Initialize();
        }
    }
}
