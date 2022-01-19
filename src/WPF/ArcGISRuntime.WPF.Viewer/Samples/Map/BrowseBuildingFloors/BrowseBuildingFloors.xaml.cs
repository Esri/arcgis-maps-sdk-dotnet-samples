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
        "Browse building floors",
        "Map",
        " Display and browse through building floors from a floor-aware web map.",
        "")]
    public partial class BrowseBuildingFloors
    {
        private const string _portalItem = "f133a698536f44c8884ad81f80b6cfc7";
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
                ArcGISPortal portal = await ArcGISPortal.CreateAsync();

                // Get the portal item for a web map using its unique item id.
                PortalItem mapItem = await PortalItem.CreateAsync(portal, _portalItem);
                Map map = new Map(mapItem);

                MyMapView.Map = map;

                // Map needs to be loaded in order for floormanager to be used.
                await MyMapView.Map.LoadAsync();
                await MyMapView.Map.FloorManager.LoadAsync();

                if (MyMapView.Map.FloorManager.LoadStatus.Equals(LoadStatus.Loaded))
                {
                    _floorManager = MyMapView.Map.FloorManager;
                    foreach (FloorLevel floors in _floorManager.Levels)
                    {
                        _floorOptions.Add(floors.ShortName, floors);
                    }
                }

                FloorChooser.ItemsSource = _floorOptions.Keys;
                FloorChooser.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, ex.GetType().Name, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void OnFloorChooserSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // Get the name of the selected floor.
            string selectedFloorName = e.AddedItems[0].ToString();

            // Set all existing floors visibility to false.
            foreach (FloorLevel level in _floorManager.Levels)
            {
                level.IsVisible = false;
            }

            // Set the selected floor visibility to true.
            _floorOptions[selectedFloorName].IsVisible = true;

            MyMapView.SetViewpoint(MyMapView.Map.InitialViewpoint);
        }
    }
}