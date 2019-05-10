// Copyright 2019 Esri.
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
using ArcGISRuntime.Samples.Managers;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.UI.Controls;
using System;
using System.Linq;
using Debug = System.Diagnostics.Debug;

namespace ArcGISRuntimeXamarin.Samples.OpenMobileScenePackage
{
    [Activity (ConfigurationChanges=Android.Content.PM.ConfigChanges.Orientation | Android.Content.PM.ConfigChanges.ScreenSize)]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Open mobile scene package",
        "Map",
        "Display a scene from an offline mobile scene package (.mspk).",
        "")]
    [ArcGISRuntime.Samples.Shared.Attributes.OfflineData("7dd2f97bb007466ea939160d0de96a9d")]
    public class OpenMobileScenePackage : Activity
    {
        // Hold references to the UI control.
        private SceneView _mySceneView;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            Title = "Open mobile scene package";

            CreateLayout();
            Initialize();
        }

        private async void Initialize()
        {
            // Get the path to the scene package.
            string scenePath = DataManager.GetDataFolder("7dd2f97bb007466ea939160d0de96a9d", "philadelphia.mspk");

            try
            {
                // Determine if the package can be read directly or needs to be unpacked.
                if (await MobileScenePackage.IsDirectReadSupportedAsync(scenePath))
                {
                    // Open the package.
                    MobileScenePackage package = await MobileScenePackage.OpenAsync(scenePath);

                    // Load the package.
                    await package.LoadAsync();

                    // Show the first scene.
                    _mySceneView.Scene = package.Scenes.First();
                }
                else
                {
                    // Create a path for the unpacked package.
                    string unpackedPath = scenePath + "unpacked";

                    // Unpack the package.
                    await MobileScenePackage.UnpackAsync(scenePath, unpackedPath);

                    // Open the package.
                    MobileScenePackage package = await MobileScenePackage.OpenAsync(unpackedPath);

                    // Load the package.
                    await package.LoadAsync();

                    // Show the first scene.
                    _mySceneView.Scene = package.Scenes.First();
                }
            }
            catch (Exception e)
            {
                new AlertDialog.Builder(this).SetMessage(e.Message).SetTitle("Couldn't open scene").Show();
                Debug.WriteLine(e);
            }
        }

        private void CreateLayout()
        {
            // Create a new vertical layout for the app.
            var layout = new LinearLayout(this) {Orientation = Orientation.Vertical};

            // Add the scene view to the layout.
            _mySceneView = new SceneView();
            layout.AddView(_mySceneView);

            // Show the layout in the app.
            SetContentView(layout);
        }
    }
}