// Copyright 2017 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using System.Linq;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Widget;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.UI.Controls;

namespace ArcGISRuntimeXamarin.Samples.OpenMobileMap
{
    [Activity]
    public class OpenMobileMap : Activity
    {
        private MapView _myMapView;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            Title = "Open mobile map (map package)";

            // Create the UI, setup the control references and execute initialization
            CreateLayout();
            Initialize();
        }

        private async void Initialize()
        {
            // Load the Mobile Map Package
            //     The mobile map package is in the Assets folder
            //     Build action is 'AndroidAsset'; Do not copy to output directory
            var mmpkName = "Yellowstone.mmpk";
            var mmpkPath = GetFileStreamPath(mmpkName).AbsolutePath;
            using (var gdbAsset = Assets.Open(mmpkName))
            using (var gdbTarget = OpenFileOutput(mmpkName, FileCreationMode.WorldWriteable))
            {
                gdbAsset.CopyTo(gdbTarget);
            }

            MobileMapPackage myMapPackage = await MobileMapPackage.OpenAsync(mmpkPath);

            // Check that there is at least one map
            if (myMapPackage.Maps.Count > 0)
            {
                // Display the first map in the package
                _myMapView.Map = myMapPackage.Maps.First();
            }
        }

        private void CreateLayout()
        {
            // Create a new vertical layout for the app
            var layout = new LinearLayout(this) { Orientation = Orientation.Vertical };

            // Add a map view to the layout
            _myMapView = new MapView(this);
            layout.AddView(_myMapView);

            // Show the layout in the app
            SetContentView(layout);
        }
    }
}