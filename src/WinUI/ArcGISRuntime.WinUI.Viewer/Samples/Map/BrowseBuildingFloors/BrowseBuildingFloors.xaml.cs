// Copyright 2022 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific 
// language governing permissions and limitations under the License.

using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.Tasks;
using Esri.ArcGISRuntime.Tasks.Offline;
using Esri.ArcGISRuntime.UI;
using Esri.ArcGISRuntime.ArcGISServices;
using Esri.ArcGISRuntime.UI.Controls;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Input;
using Esri.ArcGISRuntime.Portal;
using Esri.ArcGISRuntime.Mapping.Floor;
using System.Collections.Generic;
using Esri.ArcGISRuntime;
using Microsoft.UI.Xaml.Controls;

namespace ArcGISRuntime.WinUI.Samples.BrowseBuildingFloors
{
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        name: "Browse building floors",
        category: "Map",
        description: "Display and browse through building floors from a floor-aware web map.",
        instructions: "Use the spinner to browse different floor levels in the facility. Only the selected floor will be displayed.",
        tags: new[] { "building", "facility", "floor", "floor-aware", "floors", "ground floor", "indoor", "level", "site", "story" })]
    [ArcGISRuntime.Samples.Shared.Attributes.OfflineData()]
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
                await new MessageDialog2(ex.Message, "Error").ShowAsync();
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