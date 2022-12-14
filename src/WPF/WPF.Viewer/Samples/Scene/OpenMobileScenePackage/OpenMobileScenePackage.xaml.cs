// Copyright 2019 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using ArcGIS.Samples.Managers;
using Esri.ArcGISRuntime.Mapping;
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace ArcGIS.WPF.Samples.OpenMobileScenePackage
{
    [ArcGIS.Samples.Shared.Attributes.Sample(
        name: "Open mobile scene package",
        category: "Scene",
        description: "Opens and displays a scene from a Mobile Scene Package (.mspk).",
        instructions: "When the sample opens, it will automatically display the Scene in the Mobile Map Package.",
        tags: new[] { "offline", "scene" })]
    [ArcGIS.Samples.Shared.Attributes.OfflineData("7dd2f97bb007466ea939160d0de96a9d")]
    public partial class OpenMobileScenePackage
    {
        public OpenMobileScenePackage()
        {
            InitializeComponent();
            _ = Initialize();
        }

        private async Task Initialize()
        {
            // Get the path to the scene package.
            string scenePath = DataManager.GetDataFolder("7dd2f97bb007466ea939160d0de96a9d", "philadelphia.mspk");

            try
            {
                // Open the package.
                MobileScenePackage package = await MobileScenePackage.OpenAsync(scenePath);

                // Load the package.
                await package.LoadAsync();

                // Show the first scene.
                MySceneView.Scene = package.Scenes.First();
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "Couldn't open scene");
                Debug.WriteLine(e);
            }
        }
    }
}