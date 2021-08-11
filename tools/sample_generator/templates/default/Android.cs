// Copyright sample_year Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific 
// language governing permissions and limitations under the License.

using Android.App;
using Android.OS;
using Android.Widget;
using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.Tasks;
using Esri.ArcGISRuntime.Tasks.Offline;
using Esri.ArcGISRuntime.UI;
using Esri.ArcGISRuntime.ArcGISServices;
using Esri.ArcGISRuntime.UI.Controls;
using System.Threading.Tasks;

namespace ArcGISRuntimeXamarin.Samples.sample_name
{
    [Activity (ConfigurationChanges=Android.Content.PM.ConfigChanges.Orientation | Android.Content.PM.ConfigChanges.ScreenSize)]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "friendly_name",
        "sample_category",
        "sample_description",
        "")]
    [offline_data_attr]
    public class sample_name : Activity
    {
        // Hold references to the UI controls.
        private Geo_View _myGeo_View;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            Title = "friendly_name";

            CreateLayout();
            _ = Initialize();
        }

        private async Task Initialize()
        {
        }

        private void CreateLayout()
        {
            // Create a new vertical layout for the app.
            var layout = new LinearLayout(this) { Orientation = Orientation.Vertical };

            // Add the map view to the layout.
            _myGeo_View = new Geo_View(this);
            layout.AddView(_myGeo_View);

            // Show the layout in the app.
            SetContentView(layout);
        }
    }
}
