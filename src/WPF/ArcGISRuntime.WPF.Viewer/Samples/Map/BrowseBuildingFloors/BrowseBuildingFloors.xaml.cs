// Copyright 2022 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using Esri.ArcGISRuntime;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Mapping.Floor;
using Esri.ArcGISRuntime.Portal;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace ArcGISRuntime.WPF.Samples.BrowseBuildingFloors
{
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        name: "Browse building floors",
        category: "Map",
        description: "Display and browse through building floors from a floor-aware web map.",
        instructions: "Use the combo box to browse different floor levels in the facility. Only the selected floor will be displayed.",
        tags: new[] { "building", "facility", "floor", "floor-aware", "floors", "ground floor", "indoor", "level", "site", "story" })]
    public partial class BrowseBuildingFloors
    {
        private const string _floorData = @"https://ess.maps.arcgis.com/home/item.html?id=f133a698536f44c8884ad81f80b6cfc7";
        private FloorManager _floorManager;
        private FloorFacility _selectedFacilities;

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
                Map map = new Map(new Uri(_floorData));

                MyMapView.Map = map;

                // Map needs to be loaded in order for floormanager to be used.
                await MyMapView.Map.LoadAsync();
                await MyMapView.Map.FloorManager.LoadAsync();

                if (MyMapView.Map.FloorManager.LoadStatus == LoadStatus.Loaded && MyMapView.Map.FloorManager != null)
                {
                    _floorManager = MyMapView.Map.FloorManager;
                    _selectedFacilities = _floorManager.Facilities[0];
                    FloorChooser.ItemsSource = _selectedFacilities.Levels;
                }

                // Provides an error message if the floor manager failed to load.
                else if (MyMapView.Map.FloorManager.LoadStatus == LoadStatus.FailedToLoad)
                {
                    MessageBox.Show("Floor manager failed to load.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, ex.GetType().Name, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OnFloorChooserSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Set all existing floors visibility to false.
            foreach (FloorLevel level in _floorManager.Facilities[0].Levels)
            {
                level.IsVisible = false;
            }

            // Get the selected floor to make visible.
            FloorLevel selectedFloor = (FloorLevel)FloorChooser.SelectedItem;
            selectedFloor.IsVisible = true;

            MyMapView.SetViewpoint(new Viewpoint(selectedFloor.Geometry));
        }
    }
}