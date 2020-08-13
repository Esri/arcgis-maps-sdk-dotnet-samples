// Copyright 2018 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific 
// language governing permissions and limitations under the License.

using System;
using ArcGISRuntime;
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
        name: "Display KML",
        category: "Layers",
        description: "Display KML from a URL, portal item, or local KML file.",
        instructions: "Use the UI to select a source. A KML file from that source will be loaded and displayed in the scene.",
        tags: new[] { "KML", "KMZ", "OGC", "keyhole" })]
    [ArcGISRuntime.Samples.Shared.Attributes.OfflineData("324e4742820e46cfbe5029ff2c32cb1f")]
    public class DisplayKml : UIViewController
    {
        // Hold references to UI controls.
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

        private async void DataChoiceButtonOnValueChanged(object sender, EventArgs e)
        {
            // Clear existing layers.
            _mySceneView.Scene.OperationalLayers.Clear();

            // Get the name of the selected layer.
            string name = _sources[((UISegmentedControl) sender).SelectedSegment];

            try
            {
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
            catch (Exception ex)
            {
                new UIAlertView("Error", ex.ToString(), (IUIAlertViewDelegate) null, "OK", null).Show();
            }
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            Initialize();
        }

        public override void LoadView()
        {
            // Create the views.
            View = new UIView() { BackgroundColor = ApplicationTheme.BackgroundColor };

            _mySceneView = new SceneView();
            _mySceneView.TranslatesAutoresizingMaskIntoConstraints = false;

            _dataChoiceButton = new UISegmentedControl(_sources)
            {
                BackgroundColor = ApplicationTheme.BackgroundColor,
                TintColor = ApplicationTheme.ForegroundColor,
                TranslatesAutoresizingMaskIntoConstraints = false
            };

            // Clean up borders of segmented control - avoid corner pixels.
            _dataChoiceButton.ClipsToBounds = true;
            _dataChoiceButton.Layer.CornerRadius = 5;

            // Add the views.
            View.AddSubviews(_mySceneView, _dataChoiceButton);

            // Lay out the views.
            NSLayoutConstraint.ActivateConstraints(new[]
            {
                _mySceneView.TopAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.TopAnchor),
                _mySceneView.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor),
                _mySceneView.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor),
                _mySceneView.BottomAnchor.ConstraintEqualTo(View.BottomAnchor),

                _dataChoiceButton.LeadingAnchor.ConstraintEqualTo(View.LayoutMarginsGuide.LeadingAnchor),
                _dataChoiceButton.TrailingAnchor.ConstraintEqualTo(View.LayoutMarginsGuide.TrailingAnchor),
                _dataChoiceButton.TopAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.TopAnchor, 8)
            });
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            // Subscribe to events.
            _dataChoiceButton.ValueChanged += DataChoiceButtonOnValueChanged;
        }

        public override void ViewDidDisappear(bool animated)
        {
            base.ViewDidDisappear(animated);

            // Unsubscribe from events, per best practice.
            _dataChoiceButton.ValueChanged -= DataChoiceButtonOnValueChanged;
        }
    }
}