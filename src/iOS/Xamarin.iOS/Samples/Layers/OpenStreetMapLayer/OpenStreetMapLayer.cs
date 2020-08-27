// Copyright 2019 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific 
// language governing permissions and limitations under the License.

using ArcGISRuntime;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.UI.Controls;
using Foundation;
using UIKit;

namespace ArcGISRuntimeXamarin.Samples.OpenStreetMapLayer
{
    [Register("OpenStreetMapLayer")]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        name: "OpenStreetMap layer",
        category: "Layers",
        description: "Add OpenStreetMap as a basemap layer.",
        instructions: "When the sample opens, it will automatically display the map with the OpenStreetMap basemap. Pan and zoom to observe the basemap.",
        tags: new[] { "OSM", "OpenStreetMap", "basemap", "layers", "map", "open", "street" })]
    public class OpenStreetMapLayer : UIViewController
    {
        // Hold references to UI controls.
        private MapView _myMapView;

        public OpenStreetMapLayer()
        {
            Title = "OpenStreetMap layer";
        }

        private void Initialize()
        {
            // Create the OpenStreetMap basemap.
            Basemap osmBasemap = Basemap.CreateOpenStreetMap();

            // Create the map with the OpenStreetMap basemap.
            Map osmMap = new Map(osmBasemap);

            // Show the map in the view.
            _myMapView.Map = osmMap;
        }

        public override void LoadView()
        {
            // Create the views.
            View = new UIView() { BackgroundColor = ApplicationTheme.BackgroundColor };
            _myMapView = new MapView();
            _myMapView.TranslatesAutoresizingMaskIntoConstraints = false;

            // Add the views.
            View.AddSubviews(_myMapView);

            // Lay out the views.
            NSLayoutConstraint.ActivateConstraints(new[]
            {
                _myMapView.TopAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.TopAnchor),
                _myMapView.BottomAnchor.ConstraintEqualTo(View.BottomAnchor),
                _myMapView.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor),
                _myMapView.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor)
            });
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            Initialize();
        }
    }
}