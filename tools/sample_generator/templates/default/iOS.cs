// Copyright sample_year Esri.
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

namespace ArcGISRuntimeXamarin.Samples.sample_name
{
    [Register("sample_name")]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "friendly_name",
        "sample_category",
        "sample_description",
        "")]
    [offline_data_attr]
    public class sample_name : UIViewController
    {
        // Hold references to UI controls.
        private Geo_View _myGeo_View;

        public sample_name()
        {
            Title = "friendly_name";
        }

        private void Initialize()
        {
        }

        public override void LoadView()
        {
            // Create the views.
            View = new UIView { BackgroundColor = UIColor.White };

            _myGeo_View = new Geo_View();
            _myGeo_View.TranslatesAutoresizingMaskIntoConstraints = false;

            // Add the views.
            View.AddSubviews(_myGeo_View);

            // Lay out the views.
            NSLayoutConstraint.ActivateConstraints(new []{
                _myGeo_View.TopAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.TopAnchor),
                _myGeo_View.BottomAnchor.ConstraintEqualTo(View.BottomAnchor),
                _myGeo_View.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor),
                _myGeo_View.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor)
            });            
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            Initialize();
        }
    }
}
