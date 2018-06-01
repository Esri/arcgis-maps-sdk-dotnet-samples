// Copyright 2018 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific 
// language governing permissions and limitations under the License.

using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Portal;
using System;

namespace ArcGISRuntime.UWP.Samples.OpenScene
{
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Open scene (Portal item)",
        "Map",
        "This sample demonstrates how to open a scene from a Portal item. Just like Web Maps are the ArcGIS format for maps, Web Scenes are the ArcGIS format for scenes. These scenes can be stored in ArcGIS Online or Portal.",
        "The sample will load the scene automatically.")]
    public partial class OpenScene
    {
        // Hold the ID of the portal item, which is a web scene.
        private const string ItemId = "a13c3c3540144967bc933cb5e498b8e4";

        public OpenScene()
        {
            InitializeComponent();

            // Setup the control references and execute initialization.
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
                System.Diagnostics.Debug.WriteLine(e.ToString());
            }
        }
    }
}
