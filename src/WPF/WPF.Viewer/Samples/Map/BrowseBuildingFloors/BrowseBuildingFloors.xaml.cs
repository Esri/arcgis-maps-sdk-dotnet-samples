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
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace ArcGIS.WPF.Samples.BrowseBuildingFloors
{
    [ArcGIS.Samples.Shared.Attributes.Sample(
        name: "Browse building floors",
        category: "Map",
        description: "Display and browse through building floors from a floor-aware web map.",
        instructions: "Use the combo box to browse different floor levels in the facility. Only the selected floor will be displayed.",
        tags: new[] { "building", "facility", "floor", "floor-aware", "floors", "ground floor", "indoor", "level", "site", "story" })]
    public partial class BrowseBuildingFloors
    {
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

                // Checks to see if the layer is floor aware.
                if (MyMapView.Map.FloorDefinition == null)
                {
                    MessageBox.Show("The layer is not floor aware.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }

                await MyMapView.Map.FloorManager.LoadAsync();
                FloorFacility selectedFacility = MyMapView.Map.FloorManager.Facilities[0];
                FloorChooser.ItemsSource = selectedFacility.Levels;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, ex.GetType().Name, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OnFloorChooserSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Set all existing floors visibility to false.
            foreach (FloorLevel level in MyMapView.Map.FloorManager.Facilities[0].Levels)
            {
                level.IsVisible = false;
            }

            // Set the selected floor to visible.
            FloorLevel selectedFloor = (FloorLevel)FloorChooser.SelectedItem;
            selectedFloor.IsVisible = true;

            MyMapView.SetViewpoint(new Viewpoint(selectedFloor.Geometry));
        }
    }
}