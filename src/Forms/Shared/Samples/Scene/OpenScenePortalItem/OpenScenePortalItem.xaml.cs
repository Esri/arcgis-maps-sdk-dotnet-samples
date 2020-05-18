// Copyright 2018 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific 
// language governing permissions and limitations under the License.

using Esri.ArcGISRuntime.Mapping;
using System;
using Esri.ArcGISRuntime.Portal;
using Xamarin.Forms;

namespace ArcGISRuntime.Samples.OpenScenePortalItem
{
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        name: "Open a scene (portal item)",
        category: "Map",
        description: "Open a web scene from a portal item.",
        instructions: "When the sample opens, it will automatically display the scene from ArcGIS Online. Pan and zoom to explore the scene.",
        tags: new[] { "portal", "scene", "web scene" })]
    public partial class OpenScenePortalItem : ContentPage
    {
        // Hold the ID of the portal item, which is a web scene.
        private const string ItemId = "c6f90b19164c4283884361005faea852";

        public OpenScenePortalItem()
        {
            InitializeComponent();

            // Create the UI, setup the control references and execute initialization 
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
                MySceneView.Scene = new Scene(websceneItem);
            }
            catch (Exception e)
            {
                await Application.Current.MainPage.DisplayAlert("Error", e.ToString(), "OK");
            }
        }
    }
}