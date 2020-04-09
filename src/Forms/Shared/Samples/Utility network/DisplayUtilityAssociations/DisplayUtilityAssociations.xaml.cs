// Copyright 2020 Esri.
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
using Esri.ArcGISRuntime.Symbology;
using Esri.ArcGISRuntime.UI;
using Esri.ArcGISRuntime.UtilityNetworks;
using System;
using System.Collections.Generic;
using System.Linq;
using Xamarin.Forms;
using Color = System.Drawing.Color;

namespace ArcGISRuntimeXamarin.Samples.DisplayUtilityAssociations
{
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Display utility associations",
        "Utility network",
        "Create graphics for utility associations in a utility network.",
        "")]
    [ArcGISRuntime.Samples.Shared.Attributes.OfflineData()]
    public partial class DisplayUtilityAssociations : ContentPage
    {
        // Feature server for the utility network.
        private const string FeatureServerUrl = "https://sampleserver7.arcgisonline.com/arcgis/rest/services/UtilityNetwork/NapervilleElectric/FeatureServer";

        // This viewpoint shows several associations clearly in the utility network.
        private readonly Viewpoint InitialViewpoint = new Viewpoint(new MapPoint(-9812697.41544719, 5131928.28213524, SpatialReferences.WebMercator), 73.4352610787649);

        // Max scale at which to create graphics for the associations.
        private const double _maxScale = 2000;

        // Symbols for the associations.
        private Symbol _attachmentSymbol = new SimpleLineSymbol(SimpleLineSymbolStyle.Dot, Color.Green, 5d);
        private Symbol _connectivitySymbol = new SimpleLineSymbol(SimpleLineSymbolStyle.Dot, Color.Red, 5d);

        // Overlay to hold graphics for all of the associations.
        private GraphicsOverlay _associationsOverlay;

        private UtilityNetwork _utilityNetwork;

        public DisplayUtilityAssociations()
        {
            InitializeComponent();
            Initialize();
        }

        private async void Initialize()
        {
            try
            {
                // Create the utility network.
                _utilityNetwork = await UtilityNetwork.CreateAsync(new Uri(FeatureServerUrl));

                MyMapView.Map = new Map(Basemap.CreateTopographicVector());

                // Get all of the edges and junctions in the network.
                IEnumerable<UtilityNetworkSource> edges = _utilityNetwork.Definition.NetworkSources.Where(n => n.SourceType == UtilityNetworkSourceType.Edge);
                IEnumerable<UtilityNetworkSource> junctions = _utilityNetwork.Definition.NetworkSources.Where(n => n.SourceType == UtilityNetworkSourceType.Junction);

                // Add all edges that are not subnet lines to the map.
                foreach (UtilityNetworkSource source in edges)
                {
                    if (source.SourceUsageType != UtilityNetworkSourceUsageType.SubnetLine && source.FeatureTable != null)
                    {
                        MyMapView.Map.OperationalLayers.Add(new FeatureLayer(source.FeatureTable));
                    }
                }

                // Add all junctions to the map.
                foreach (UtilityNetworkSource source in junctions)
                {
                    if (source.FeatureTable != null)
                    {
                        MyMapView.Map.OperationalLayers.Add(new FeatureLayer(source.FeatureTable));
                    }
                }

                // Create a graphics overlay for associations.
                _associationsOverlay = new GraphicsOverlay();
                MyMapView.GraphicsOverlays.Add(_associationsOverlay);

                // Populate the legend in the UI.
                RuntimeImage attachmentSwatch = await _attachmentSymbol.CreateSwatchAsync();
                AttachmentImage.Source = await Esri.ArcGISRuntime.Xamarin.Forms.RuntimeImageExtensions.ToImageSourceAsync(attachmentSwatch);
                RuntimeImage connectSwatch = await _connectivitySymbol.CreateSwatchAsync();
                ConnectivityImage.Source = await Esri.ArcGISRuntime.Xamarin.Forms.RuntimeImageExtensions.ToImageSourceAsync(connectSwatch);

                // Set the starting viewpoint.
                await MyMapView.SetViewpointAsync(InitialViewpoint);
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert(ex.GetType().Name, ex.Message, "OK");
            }
        }

        private async void OnNavigationCompleted(object sender, EventArgs e)
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
                    Symbol symbol = null;
                    if (association.AssociationType == UtilityAssociationType.Attachment) symbol = _attachmentSymbol;
                    else if (association.AssociationType == UtilityAssociationType.Connectivity) symbol = _connectivitySymbol;
                    if (symbol != null)
                    {
                        Graphic graphic = new Graphic(association.Geometry, symbol);
                        graphic.Attributes["GlobalId"] = association.GlobalId;
                        _associationsOverlay.Graphics.Add(graphic);
                    }
                }
            }
            // This is thrown when there are too many associations in the extent.
            catch (TooManyAssociationsException)
            {
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert(ex.GetType().Name, ex.Message, "OK");
            }
        }
    }
}