﻿// Copyright 2021 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using Esri.ArcGISRuntime;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Security;
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.UI;
using Esri.ArcGISRuntime.UtilityNetworks;
using Microsoft.UI.Xaml.Media;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using Symbol = Esri.ArcGISRuntime.Symbology.Symbol;

namespace ArcGIS.WinUI.Samples.DisplayUtilityAssociations
{
    [ArcGIS.Samples.Shared.Attributes.Sample(
        name: "Display utility associations",
        category: "Utility network",
        description: "Create graphics for utility associations in a utility network.",
        instructions: "Pan and zoom around the map. Observe graphics that show utility associations between junctions.",
        tags: new[] { "associating", "association", "attachment", "connectivity", "containment", "relationships" })]
    public partial class DisplayUtilityAssociations
    {
        // This viewpoint shows several associations clearly in the utility network.
        private readonly Viewpoint InitialViewpoint = new Viewpoint(new MapPoint(-9812698.37297436, 5131928.33743317, SpatialReferences.WebMercator), 22d);

        // Max scale at which to create graphics for the associations.
        private const double _maxScale = 2000;

        // Overlay to hold graphics for all of the associations.
        private GraphicsOverlay _associationsOverlay;

        // Utility network that will be created from the feature server.
        private UtilityNetwork _utilityNetwork;

        public DisplayUtilityAssociations()
        {
            InitializeComponent();
            _ = Initialize();
        }

        private async Task Initialize()
        {
            // As of ArcGIS Enterprise 10.8.1, using utility network functionality requires a licensed user. The following login for the sample server is licensed to perform utility network operations.
            AuthenticationManager.Current.ChallengeHandler = new ChallengeHandler(async (info) =>
            {
                try
                {
                    // WARNING: Never hardcode login information in a production application. This is done solely for the sake of the sample.
                    string sampleServer7User = "viewer01";
                    string sampleServer7Pass = "I68VGU^nMurF";
                    return await AccessTokenCredential.CreateAsync(info.ServiceUri, sampleServer7User, sampleServer7Pass);
                }
                catch (Exception ex)
                {
                    await new MessageDialog2(ex.Message, ex.Message.GetType().Name).ShowAsync();
                    return null;
                }
            });

            try
            {
                // Create the map.
                MyMapView.Map = new Map(new Uri("https://sampleserver7.arcgisonline.com/portal/home/item.html?id=be0e4637620a453584118107931f718b"));
                await MyMapView.Map.LoadAsync();

                // Get the utility network from the map.
                _utilityNetwork = MyMapView.Map.UtilityNetworks.FirstOrDefault();
                await _utilityNetwork.LoadAsync();

                // Create a graphics overlay for associations.
                _associationsOverlay = new GraphicsOverlay();
                MyMapView.GraphicsOverlays.Add(_associationsOverlay);

                // Symbols for the associations.
                Symbol attachmentSymbol = new SimpleLineSymbol(SimpleLineSymbolStyle.Dot, Color.Green, 5d);
                Symbol connectivitySymbol = new SimpleLineSymbol(SimpleLineSymbolStyle.Dot, Color.Red, 5d);

                // Create a renderer for the associations.
                var attachmentValue = new UniqueValue("Attachment", string.Empty, attachmentSymbol, UtilityAssociationType.Attachment.ToString());
                var connectivityValue = new UniqueValue("Connectivity", string.Empty, connectivitySymbol, UtilityAssociationType.Connectivity.ToString());
                _associationsOverlay.Renderer = new UniqueValueRenderer(new List<string> { "AssociationType" }, new List<UniqueValue> { attachmentValue, connectivityValue }, string.Empty, null);

                // Populate the legend in the UI.
                Dictionary<UtilityAssociationType, ImageSource> legend;
                legend = new Dictionary<UtilityAssociationType, ImageSource>();

                RuntimeImage attachmentSwatch = await attachmentSymbol.CreateSwatchAsync();
                legend[UtilityAssociationType.Attachment] = await attachmentSwatch?.ToImageSourceAsync();

                RuntimeImage connectSwatch = await connectivitySymbol.CreateSwatchAsync();
                legend[UtilityAssociationType.Connectivity] = await connectSwatch?.ToImageSourceAsync();

                AssociationLegend.ItemsSource = legend;

                // Set the starting viewpoint.
                await MyMapView.SetViewpointAsync(InitialViewpoint);

                // Add the associations in the starting viewpoint.
                _ = AddAssociations();
            }
            catch (Exception ex)
            {
                await new MessageDialog2(ex.Message, ex.GetType().Name).ShowAsync();
            }
        }

        private void OnNavigationCompleted(object sender, EventArgs e)
        {
            _ = AddAssociations();
        }

        private async Task AddAssociations()
        {
            try
            {
                // Check if the current viewpoint is outside of the max scale.
                if (MyMapView.GetCurrentViewpoint(ViewpointType.CenterAndScale)?.TargetScale >= _maxScale)
                {
                    return;
                }

                // Check if the current viewpoint has an extent.
                Envelope extent = MyMapView.GetCurrentViewpoint(ViewpointType.BoundingGeometry)?.TargetGeometry?.Extent;
                if (extent == null)
                {
                    return;
                }

                // Get all of the associations in extent of the viewpoint.
                IEnumerable<UtilityAssociation> associations = await _utilityNetwork.GetAssociationsAsync(extent);
                foreach (UtilityAssociation association in associations)
                {
                    // Check if the graphics overlay already contains the association.
                    if (_associationsOverlay.Graphics.Any(g => g.Attributes.ContainsKey("GlobalId") && (Guid)g.Attributes["GlobalId"] == association.GlobalId))
                    {
                        continue;
                    }

                    // Add a graphic for the association.
                    Graphic graphic = new Graphic(association.Geometry);
                    graphic.Attributes["GlobalId"] = association.GlobalId;
                    graphic.Attributes["AssociationType"] = association.AssociationType.ToString();
                    _associationsOverlay.Graphics.Add(graphic);
                }
            }

            // This is thrown when there are too many associations in the extent.
            catch (TooManyAssociationsException)
            {
            }
            catch (Exception ex)
            {
                await new MessageDialog2(ex.Message, ex.GetType().Name).ShowAsync();
            }
        }
    }
}