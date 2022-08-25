// Copyright 2022 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Mapping.Floor;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;


namespace ArcGISRuntimeMaui.Samples.BrowseBuildingFloors
{
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        name: "Browse building floors",
        category: "Map",
        description: "Display and browse through building floors from a floor-aware web map.",
        instructions: "Use the combo box to browse different floor levels in the facility. Only the selected floor will be displayed.",
        tags: new[] { "building", "facility", "floor", "floor-aware", "floors", "ground floor", "indoor", "level", "site", "story" })]
    public partial class BrowseBuildingFloors : ContentPage
    {
        private FloorManager _floorManager;

        // Collection of floors.
        private readonly Dictionary<string, FloorLevel> _floorOptions = new Dictionary<string, FloorLevel>();

        public BrowseBuildingFloors()
        {
            InitializeComponent();
            _ = Initialize();
        }

        private async Task Initialize()
        {
            try
            {
                // Gets the floor data from ArcGIS Online and creates a map with it.
                Map map = new Map(new Uri("https://ess.maps.arcgis.com/home/item.html?id=f133a698536f44c8884ad81f80b6cfc7"));

                MyMapView.Map = map;

                // Map needs to be loaded in order for floormanager to be used.
                await MyMapView.Map.LoadAsync();
                List<string> floorName = new List<string>();

                // Checks to see if the layer is floor aware.
                if (MyMapView.Map.FloorDefinition == null)
                {
                    await Application.Current.MainPage.DisplayAlert("Alert", "The layer is not floor aware.", "OK");
                    return;
                }

                await MyMapView.Map.FloorManager.LoadAsync();
                _floorManager = MyMapView.Map.FloorManager;

                // Use the dictionary to add the level's name as the key and the FloorLevel object with the associated level's name.
                foreach (FloorLevel level in _floorManager.Facilities[0].Levels)
                {
                    _floorOptions.Add(level.ShortName, level);
                    floorName.Add(level.ShortName);
                }

                FloorChooser.ItemsSource = floorName;
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Alert", ex.Message, "OK");
            }
        }

        private void OnFloorChooserSelectionChanged(object sender, EventArgs e)
        {
            // Get the name of the selected floor.
            string selectedFloorName = FloorChooser.SelectedItem.ToString();

            // Set all existing floors visibility to false.
            foreach (FloorLevel level in _floorManager.Levels)
            {
                level.IsVisible = false;
            }

            // Set the selected floor visibility to true.
            _floorOptions[selectedFloorName].IsVisible = true;

            MyMapView.SetViewpoint(new Viewpoint(_floorOptions[selectedFloorName].Facility.Geometry));
        }
    }
}