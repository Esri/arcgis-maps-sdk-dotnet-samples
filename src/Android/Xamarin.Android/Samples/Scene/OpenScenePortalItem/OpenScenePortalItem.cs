// Copyright 2018 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific 
// language governing permissions and limitations under the License.

using Android.App;
using Android.OS;
using Android.Views;
using Android.Widget;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Portal;
using Esri.ArcGISRuntime.UI.Controls;
using System;

namespace ArcGISRuntime.Samples.OpenScenePortalItem
{
    [Activity (ConfigurationChanges=Android.Content.PM.ConfigChanges.Orientation | Android.Content.PM.ConfigChanges.ScreenSize)]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        name: "Open scene (portal item)",
        category: "Scene",
        description: "Open a web scene from a portal item.",
        instructions: "When the sample opens, it will automatically display the scene from ArcGIS Online. Pan and zoom to explore the scene.",
        tags: new[] { "portal", "scene", "web scene" })]
    public class OpenScenePortalItem : Activity
    {
        // Hold the ID of the portal item, which is a web scene.
        private const string ItemId = "31874da8a16d45bfbc1273422f772270";

        // Hold a reference to the scene view.
        private SceneView _mySceneView;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            Title = "Open scene (Portal item)";

            CreateLayout();
            Initialize();
        }

        private async void Initialize()
        {
            try
            {
                // Try to load the default portal, which will be ArcGIS Online.
                ArcGISPortal portal = await ArcGISPortal.CreateAsync();

                // Create the portal item.
                PortalItem websceneItem = await PortalItem.CreateAsync(portal, ItemId);

                // Create and show the scene.
                _mySceneView.Scene = new Scene(websceneItem);
            }
            catch (Exception e)
            {
                new AlertDialog.Builder(this).SetMessage(e.ToString()).SetTitle("Error").Show();
            }
        }

        private void CreateLayout()
        {
            // Create a new vertical layout for the app
            LinearLayout layout = new LinearLayout(this) {Orientation = Orientation.Vertical};

            // Add the map view to the layout
            _mySceneView = new SceneView(this);
            layout.AddView(_mySceneView);

            // Show the layout in the app
            SetContentView(layout);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            // Remove the sceneview
            (_mySceneView.Parent as ViewGroup).RemoveView(_mySceneView);
            _mySceneView.Dispose();
            _mySceneView = null;
        }
    }
}