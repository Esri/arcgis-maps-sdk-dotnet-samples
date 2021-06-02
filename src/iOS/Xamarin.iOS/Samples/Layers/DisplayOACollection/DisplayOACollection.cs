// Copyright 2021 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific 
// language governing permissions and limitations under the License.

using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.Tasks;
using Esri.ArcGISRuntime.Tasks.Offline;
using Esri.ArcGISRuntime.UI;
using Esri.ArcGISRuntime.ArcGISServices;
using Esri.ArcGISRuntime.UI.Controls;
using Foundation;
using UIKit;

namespace ArcGISRuntimeXamarin.Samples.DisplayOACollection
{
    [Register("DisplayOACollection")]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        name: "Display OGC API collection",
        category: "Layers",
        description: "Display an OGC API feature collection and query features while navigating the map view.",
        instructions: "Pan the map and observe how new features are loaded from the OGC API feature service.",
        tags: new[] { "OGC", "OGC API", "feature", "feature layer", "feature table", "service", "table", "web" })]
    [ArcGISRuntime.Samples.Shared.Attributes.OfflineData()]
    public class DisplayOACollection : UIViewController
    {
        // Hold references to UI controls.
        private MapView _myMapView;

        public DisplayOACollection()
        {
            Title = "Display OGC API collection";
        }

        private void Initialize()
        {
        }

        public override void LoadView()
        {
            // Create the views.
            View = new UIView { BackgroundColor = UIColor.White };

            _myMapView = new MapView();
            _myMapView.TranslatesAutoresizingMaskIntoConstraints = false;

            // Add the views.
            View.AddSubviews(_myMapView);

            // Lay out the views.
            NSLayoutConstraint.ActivateConstraints(new []{
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
