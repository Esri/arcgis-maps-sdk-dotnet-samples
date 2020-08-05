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
using Esri.ArcGISRuntime.UI.Controls;
using Esri.ArcGISRuntime.UtilityNetworks;
using Foundation;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using UIKit;

namespace ArcGISRuntimeXamarin.Samples.DisplayUtilityAssociations
{
    [Register("DisplayUtilityAssociations")]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        name: "Display utility associations",
        category: "Utility network",
        description: "Create graphics for utility associations in a utility network.",
        instructions: "Pan and zoom around the map. Observe graphics that show utility associations between junctions.",
        tags: new[] { "associating", "association", "attachment", "connectivity", "containment", "relationships" })]
    public class DisplayUtilityAssociations : UIViewController
    {
        // Hold references to UI controls.
        private MapView _myMapView;
        private UIImageView _attachmentImage;
        private UIImageView _connectivityImage;

        // Feature server for the utility network.
        private const string FeatureServerUrl = "https://sampleserver7.arcgisonline.com/arcgis/rest/services/UtilityNetwork/NapervilleElectric/FeatureServer";

        // This viewpoint shows several associations clearly in the utility network.
        private readonly Viewpoint InitialViewpoint = new Viewpoint(new MapPoint(-9812697.41544719, 5131928.28213524, SpatialReferences.WebMercator), 73.4352610787649);

        // Max scale at which to create graphics for the associations.
        private const double _maxScale = 2000;

        // Overlay to hold graphics for all of the associations.
        private GraphicsOverlay _associationsOverlay;

        // Utility network that will be created from the feature server.
        private UtilityNetwork _utilityNetwork;

        public DisplayUtilityAssociations()
        {
            Title = "Display utility associations";
        }

        private async void Initialize()
        {
            try
            {
                // Create the utility network.
                _utilityNetwork = await UtilityNetwork.CreateAsync(new Uri(FeatureServerUrl));

                // Create the map.
                _myMapView.Map = new Map(Basemap.CreateTopographicVector());

                // Get all of the edges and junctions in the network.
                IEnumerable<UtilityNetworkSource> edges = _utilityNetwork.Definition.NetworkSources.Where(n => n.SourceType == UtilityNetworkSourceType.Edge);
                IEnumerable<UtilityNetworkSource> junctions = _utilityNetwork.Definition.NetworkSources.Where(n => n.SourceType == UtilityNetworkSourceType.Junction);

                // Add all edges that are not subnet lines to the map.
                foreach (UtilityNetworkSource source in edges)
                {
                    if (source.SourceUsageType != UtilityNetworkSourceUsageType.SubnetLine && source.FeatureTable != null)
                    {
                        _myMapView.Map.OperationalLayers.Add(new FeatureLayer(source.FeatureTable));
                    }
                }

                // Add all junctions to the map.
                foreach (UtilityNetworkSource source in junctions)
                {
                    if (source.FeatureTable != null)
                    {
                        _myMapView.Map.OperationalLayers.Add(new FeatureLayer(source.FeatureTable));
                    }
                }

                // Create a graphics overlay for associations.
                _associationsOverlay = new GraphicsOverlay();
                _myMapView.GraphicsOverlays.Add(_associationsOverlay);

                // Symbols for the associations.
                Symbol attachmentSymbol = new SimpleLineSymbol(SimpleLineSymbolStyle.Dot, Color.Green, 5d);
                Symbol connectivitySymbol = new SimpleLineSymbol(SimpleLineSymbolStyle.Dot, Color.Red, 5d);

                // Create a renderer for the associations.
                var attachmentValue = new UniqueValue("Attachment", string.Empty, attachmentSymbol, UtilityAssociationType.Attachment.ToString());
                var connectivityValue = new UniqueValue("Connectivity", string.Empty, connectivitySymbol, UtilityAssociationType.Connectivity.ToString());
                _associationsOverlay.Renderer = new UniqueValueRenderer(new List<string> { "AssociationType" }, new List<UniqueValue> { attachmentValue, connectivityValue }, string.Empty, null);

                // Populate the legend in the UI.
                RuntimeImage attachmentSwatch = await attachmentSymbol.CreateSwatchAsync();
                _attachmentImage.Image = await attachmentSwatch?.ToImageSourceAsync();

                RuntimeImage connectSwatch = await connectivitySymbol.CreateSwatchAsync();
                _connectivityImage.Image = await connectSwatch?.ToImageSourceAsync();

                // Set the starting viewpoint.
                await _myMapView.SetViewpointAsync(InitialViewpoint);

                // Add the associations in the starting viewpoint.
                AddAssociations();
            }
            catch (Exception ex)
            {
                new UIAlertView(ex.GetType().Name, ex.Message, (IUIAlertViewDelegate)null, "OK", null).Show();
            }
        }

        private void OnNavigationCompleted(object sender, EventArgs e)
        {
            AddAssociations();
        }

        private async void AddAssociations()
        {
            try
            {
                // Check if the current viewpoint is outside of the max scale.
                if (_myMapView.GetCurrentViewpoint(ViewpointType.CenterAndScale)?.TargetScale >= _maxScale)
                {
                    return;
                }

                // Check if the current viewpoint has an extent.
                Envelope extent = _myMapView.GetCurrentViewpoint(ViewpointType.BoundingGeometry)?.TargetGeometry?.Extent;
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
                new UIAlertView(ex.GetType().Name, ex.Message, (IUIAlertViewDelegate)null, "OK", null).Show();
            }
        }

        public override void LoadView()
        {
            // Create the views.
            View = new UIView() { BackgroundColor = ApplicationTheme.BackgroundColor };

            _myMapView = new MapView();
            _myMapView.TranslatesAutoresizingMaskIntoConstraints = false;

            UIToolbar toolbar = new UIToolbar { TranslatesAutoresizingMaskIntoConstraints = false };

            _connectivityImage = new UIImageView(new CoreGraphics.CGRect(0, 0, 25, 25));
            _attachmentImage = new UIImageView(new CoreGraphics.CGRect(0, 0, 25, 25));

            UILabel connectivityLabel = new UILabel() { Text = "Connectivity" };
            UILabel attachmentLabel = new UILabel() { Text = "Attachment" };

            toolbar.Items = new[]
            {
                new UIBarButtonItem { CustomView = _attachmentImage},
                new UIBarButtonItem(UIBarButtonSystemItem.FixedSpace) { Width = 8 },
                new UIBarButtonItem { CustomView = attachmentLabel},
                new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace),
                new UIBarButtonItem { CustomView = _connectivityImage},
                new UIBarButtonItem(UIBarButtonSystemItem.FixedSpace) { Width = 8 },
                new UIBarButtonItem { CustomView = connectivityLabel},
                new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace),
            };

            // Add the views.
            View.AddSubviews(_myMapView, toolbar);

            // Lay out the views.
            NSLayoutConstraint.ActivateConstraints(new[]{
                _myMapView.TopAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.TopAnchor),
                _myMapView.BottomAnchor.ConstraintEqualTo(toolbar.TopAnchor),
                _myMapView.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor),
                _myMapView.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor),

                toolbar.TopAnchor.ConstraintEqualTo(_myMapView.BottomAnchor),
                toolbar.BottomAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.BottomAnchor),
                toolbar.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor),
                toolbar.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor)
            });
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            Initialize();
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            // Subscribe to events.
            _myMapView.NavigationCompleted += OnNavigationCompleted;
        }

        public override void ViewDidDisappear(bool animated)
        {
            base.ViewDidDisappear(animated);

            // Unsubscribe from events, per best practice.
            _myMapView.NavigationCompleted -= OnNavigationCompleted;
        }
    }
}