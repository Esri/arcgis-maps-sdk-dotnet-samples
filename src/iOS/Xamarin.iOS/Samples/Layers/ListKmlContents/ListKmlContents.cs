// Copyright 2018 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific 
// language governing permissions and limitations under the License.

using ArcGISRuntime.Samples.Managers;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Ogc;
using Esri.ArcGISRuntime.UI.Controls;
using Foundation;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UIKit;

namespace ArcGISRuntimeXamarin.Samples.ListKmlContents
{
    [Register("ListKmlContents")]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        name: "List KML contents",
        category: "Layers",
        description: "List the contents of a KML file.",
        instructions: "The contents of the KML file are shown in a tree. Select a node to zoom to that node. Not all nodes can be zoomed to (e.g. screen overlays).",
        tags: new[] { "KML", "KMZ", "Keyhole", "OGC", "layers" })]
    [ArcGISRuntime.Samples.Shared.Attributes.OfflineData("da301cb122874d5497f8a8f6c81eb36e")]
    public class ListKmlContents : UIViewController
    {
        // Hold references to UI controls.
        private SceneView _mySceneView;
        private UITableView _myDisplayList;
        private LayerListSource _layerListSource;
        private UIStackView _stackView;

        // Hold a list of LayerDisplayVM; this is the ViewModel.
        private readonly List<LayerDisplayVM> _viewModelList = new List<LayerDisplayVM>();

        public ListKmlContents()
        {
            Title = "List KML contents";
        }

        private async void Initialize()
        {
            // Add a basemap.
            _mySceneView.Scene = new Scene(Basemap.CreateImageryWithLabels());
            _mySceneView.Scene.BaseSurface.ElevationSources.Add(new ArcGISTiledElevationSource(new Uri("https://elevation3d.arcgis.com/arcgis/rest/services/WorldElevation3D/Terrain3D/ImageServer")));

            // Get the URL to the data.
            Uri kmlUrl = new Uri(DataManager.GetDataFolder("da301cb122874d5497f8a8f6c81eb36e", "esri_test_data.kmz"));

            // Create the KML dataset and layer.
            KmlDataset dataset = new KmlDataset(kmlUrl);
            KmlLayer layer = new KmlLayer(dataset);

            // Add the layer to the map.
            _mySceneView.Scene.OperationalLayers.Add(layer);

            try
            {
                await dataset.LoadAsync();

                // Build the ViewModel from the expanded list of layer infos.
                foreach (KmlNode node in dataset.RootNodes)
                {
                    // LayerDisplayVM is a custom type made for this sample to serve as the ViewModel; it is not a part of ArcGIS Runtime.
                    LayerDisplayVM nodeVm = new LayerDisplayVM(node, null);
                    _viewModelList.Add(nodeVm);
                    LayerDisplayVM.BuildLayerInfoList(nodeVm, _viewModelList);
                }

                // Construct the layer list source.
                _layerListSource = new LayerListSource(_viewModelList, this);

                // Set the source for the table view (layer list).
                _myDisplayList.Source = _layerListSource;

                // Force an update of the list display.
                _myDisplayList.ReloadData();
            }
            catch (Exception e)
            {
                new UIAlertView("Error", e.ToString(), (IUIAlertViewDelegate) null, "OK", null).Show();
            }
        }


        /// <summary>
        /// Takes action once a new content selection is made.
        /// </summary>
        public void ContentSelectionChanged(int selectedIndex)
        {
            // Get the KML node.
            LayerDisplayVM selectedItem = _viewModelList[selectedIndex];

            NavigateToNode(selectedItem.Node);
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            Initialize();
        }

        public override void LoadView()
        {
            // Create the views.
            View = new UIView() { BackgroundColor = ApplicationTheme.BackgroundColor };

            _mySceneView = new SceneView();
            _mySceneView.TranslatesAutoresizingMaskIntoConstraints = false;

            _myDisplayList = new UITableView
            {
                RowHeight = 30
            };
            _myDisplayList.TranslatesAutoresizingMaskIntoConstraints = false;

            _stackView = new UIStackView(new UIView[] {_mySceneView, _myDisplayList});
            _stackView.TranslatesAutoresizingMaskIntoConstraints = false;
            _stackView.Axis = UILayoutConstraintAxis.Vertical;
            _stackView.Distribution = UIStackViewDistribution.FillEqually;

            // Add the views.
            View.AddSubviews(_stackView);

            // Lay out the views.
            NSLayoutConstraint.ActivateConstraints(new[]
            {
                _stackView.TopAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.TopAnchor),
                _stackView.BottomAnchor.ConstraintEqualTo(View.BottomAnchor),
                _stackView.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor),
                _stackView.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor)
            });
        }

        public override void TraitCollectionDidChange(UITraitCollection previousTraitCollection)
        {
            base.TraitCollectionDidChange(previousTraitCollection);
            if (View.TraitCollection.VerticalSizeClass == UIUserInterfaceSizeClass.Compact)
            {
                _stackView.Axis = UILayoutConstraintAxis.Horizontal;
            }
            else
            {
                _stackView.Axis = UILayoutConstraintAxis.Vertical;
            }
        }

        #region viewpoint_conversion

        private async void NavigateToNode(KmlNode node)
        {
            try
            {
                // Get a corrected Runtime viewpoint using the KmlViewpoint.
                bool viewpointNeedsAltitudeAdjustment;
                Viewpoint runtimeViewpoint = ViewpointFromKmlViewpoint(node, out viewpointNeedsAltitudeAdjustment);
                if (viewpointNeedsAltitudeAdjustment)
                {
                    runtimeViewpoint = await GetAltitudeAdjustedViewpointAsync(node, runtimeViewpoint);
                }

                // Set the viewpoint.
                if (runtimeViewpoint != null && !runtimeViewpoint.TargetGeometry.IsEmpty)
                {
                    await _mySceneView.SetViewpointAsync(runtimeViewpoint);
                }
            }
            catch (Exception e)
            {
                new UIAlertView("Error", e.ToString(), (IUIAlertViewDelegate) null, "OK", null).Show();
            }
        }

        private Viewpoint ViewpointFromKmlViewpoint(KmlNode node, out bool needsAltitudeFix)
        {
            KmlViewpoint kvp = node.Viewpoint;
            // If KmlViewpoint is specified, use it.
            if (kvp != null)
            {
                // Altitude adjustment is needed for everything except Absolute altitude mode.
                needsAltitudeFix = (kvp.AltitudeMode != KmlAltitudeMode.Absolute);
                switch (kvp.Type)
                {
                    case KmlViewpointType.LookAt:
                        return new Viewpoint(kvp.Location,
                            new Camera(kvp.Location, kvp.Range, kvp.Heading, kvp.Pitch, kvp.Roll));
                    case KmlViewpointType.Camera:
                        return new Viewpoint(kvp.Location,
                            new Camera(kvp.Location, kvp.Heading, kvp.Pitch, kvp.Roll));
                    default:
                        throw new InvalidOperationException("Unexpected KmlViewPointType: " + kvp.Type);
                }
            }

            if (node.Extent != null && !node.Extent.IsEmpty)
            {
                // When no altitude specified, assume elevation should be taken into account.
                needsAltitudeFix = true;

                // Workaround: it's possible for "IsEmpty" to be true but for width/height to still be zero.
                if (node.Extent.Width == 0 && node.Extent.Height == 0)
                {
                    // Defaults based on Google Earth.
                    return new Viewpoint(node.Extent, new Camera(node.Extent.GetCenter(), 1000, 0, 45, 0));
                }

                Envelope tx = node.Extent;
                // Add padding on each side.
                double bufferDistance = Math.Max(node.Extent.Width, node.Extent.Height) / 20;
                Envelope bufferedExtent = new Envelope(
                    tx.XMin - bufferDistance, tx.YMin - bufferDistance,
                    tx.XMax + bufferDistance, tx.YMax + bufferDistance,
                    tx.ZMin - bufferDistance, tx.ZMax + bufferDistance,
                    SpatialReferences.Wgs84);
                return new Viewpoint(bufferedExtent);
            }
            else
            {
                // Can't fly to.
                needsAltitudeFix = false;
                return null;
            }
        }

        // Asynchronously adjust the given viewpoint, taking into consideration elevation and KML altitude mode.
        private async Task<Viewpoint> GetAltitudeAdjustedViewpointAsync(KmlNode node, Viewpoint baseViewpoint)
        {
            // Get the altitude mode; assume clamp-to-ground if not specified.
            KmlAltitudeMode altMode = KmlAltitudeMode.ClampToGround;
            if (node.Viewpoint != null)
            {
                altMode = node.Viewpoint.AltitudeMode;
            }

            // If the altitude mode is Absolute, the base viewpoint doesn't need adjustment.
            if (altMode == KmlAltitudeMode.Absolute)
            {
                return baseViewpoint;
            }

            double altitude;
            Envelope lookAtExtent = baseViewpoint.TargetGeometry as Envelope;
            MapPoint lookAtPoint = baseViewpoint.TargetGeometry as MapPoint;

            if (lookAtExtent != null)
            {
                // Get the altitude for the extent.
                try
                {
                    altitude = await _mySceneView.Scene.BaseSurface.GetElevationAsync(lookAtExtent.GetCenter());
                }
                catch (Exception)
                {
                    altitude = 0;
                }

                // Apply elevation adjustment to the geometry.
                Envelope target;
                if (altMode == KmlAltitudeMode.ClampToGround)
                {
                    target = new Envelope(
                        lookAtExtent.XMin, lookAtExtent.YMin,
                        lookAtExtent.XMax, lookAtExtent.YMax,
                        altitude, lookAtExtent.Depth + altitude,
                        lookAtExtent.SpatialReference);
                }
                else
                {
                    target = new Envelope(
                        lookAtExtent.XMin, lookAtExtent.YMin,
                        lookAtExtent.XMax, lookAtExtent.YMax,
                        lookAtExtent.ZMin + altitude, lookAtExtent.ZMax + altitude,
                        lookAtExtent.SpatialReference);
                }

                if (node.Viewpoint != null)
                {
                    // Return adjusted geometry with adjusted camera if a viewpoint was specified on the node.
                    return new Viewpoint(target, baseViewpoint.Camera.Elevate(altitude));
                }
                else
                {
                    // Return adjusted geometry.
                    return new Viewpoint(target);
                }
            }
            else if (lookAtPoint != null)
            {
                // Get the altitude adjustment.
                try
                {
                    altitude = await _mySceneView.Scene.BaseSurface.GetElevationAsync(lookAtPoint);
                }
                catch (Exception)
                {
                    altitude = 0;
                }

                // Apply elevation adjustment to the geometry.
                MapPoint target;
                if (altMode == KmlAltitudeMode.ClampToGround)
                {
                    target = new MapPoint(lookAtPoint.X, lookAtPoint.Y, altitude, lookAtPoint.SpatialReference);
                }
                else
                {
                    target = new MapPoint(
                        lookAtPoint.X, lookAtPoint.Y, lookAtPoint.Z + altitude,
                        lookAtPoint.SpatialReference);
                }

                if (node.Viewpoint != null)
                {
                    // Return adjusted geometry with adjusted camera if a viewpoint was specified on the node.
                    return new Viewpoint(target, baseViewpoint.Camera.Elevate(altitude));
                }
                else
                {
                    // Google Earth defaults: 1000m away and 45-degree tilt.
                    return new Viewpoint(target, new Camera(target, 1000, 0, 45, 0));
                }
            }
            else
            {
                throw new InvalidOperationException("KmlNode has unexpected Geometry for its Extent: " +
                                                    baseViewpoint.TargetGeometry);
            }
        }

        #endregion viewpoint_conversion
    }

    public class LayerDisplayVM
    {
        public KmlNode Node { get; }

        private LayerDisplayVM Parent { get; set; }

        private int NestLevel
        {
            get
            {
                if (Parent == null)
                {
                    return 0;
                }

                return Parent.NestLevel + 1;
            }
        }

        public LayerDisplayVM(KmlNode info, LayerDisplayVM parent)
        {
            Node = info;
            Parent = parent;
        }

        public string Name => new string(' ', NestLevel * 3) + Node.GetType().Name + " - " + Node.Name;

        public static void BuildLayerInfoList(LayerDisplayVM root, IList<LayerDisplayVM> result)
        {
            // Add the root node to the result list.
            result.Add(root);

            // Make the node visible.
            root.Node.IsVisible = true;

            // Recursively add children. KmlContainers and KmlNetworkLinks can both have children.
            var containerNode = root.Node as KmlContainer;
            var networkLinkNode = root.Node as KmlNetworkLink;

            List<KmlNode> children = new List<KmlNode>();
            if (containerNode != null)
            {
                children.AddRange(containerNode.ChildNodes);
            }

            if (networkLinkNode != null)
            {
                children.AddRange(networkLinkNode.ChildNodes);
            }

            foreach (KmlNode node in children)
            {
                // Create the view model for the sublayer.
                LayerDisplayVM layerVm = new LayerDisplayVM(node, root);

                // Recursively add children.
                BuildLayerInfoList(layerVm, result);
            }
        }
    }

    /// <summary>
    /// Class defines how a UITableView renders its contents.
    /// This implements the list of KML content.
    /// </summary>
    public class LayerListSource : UITableViewSource
    {
        private readonly List<LayerDisplayVM> ViewModelList = new List<LayerDisplayVM>();

        // Used when re-using cells to ensure that a cell of the right type is used
        private const string CellId = "KmlContentCell";

        // Hold a reference to the owning view controller; this will be the active instance of ListKmlContents.
        [Weak] public ListKmlContents Owner;

        public LayerListSource(List<LayerDisplayVM> items, ListKmlContents owner)
        {
            // Set the items.
            if (items != null)
            {
                ViewModelList = items;
            }

            // Set the owner.
            Owner = owner;
        }

        /// <summary>
        /// This method gets a table view cell for the suggestion at the specified index.
        /// </summary>
        public override UITableViewCell GetCell(UITableView tableView, NSIndexPath indexPath)
        {
            // Try to get a re-usable cell (this is for performance).
            UITableViewCell cell = tableView.DequeueReusableCell(CellId);

            // If there are no cells, create a new one.
            if (cell == null)
            {
                cell = new UITableViewCell(UITableViewCellStyle.Default, CellId)
                {
                    BackgroundColor = UIColor.FromWhiteAlpha(0, 0f)
                };
                cell.TextLabel.TextColor = Owner.View.TintColor;
            }

            // Get the specific item to display.
            LayerDisplayVM item = ViewModelList[indexPath.Row];

            // Set the text on the cell.
            cell.TextLabel.Text = item.Name;

            // Return the cell.
            return cell;
        }

        /// <summary>
        /// This method allows the UITableView to know how many rows to render.
        /// </summary>
        public override nint RowsInSection(UITableView tableview, nint section)
        {
            return ViewModelList.Count;
        }

        /// <summary>
        /// Method called when a row is selected; notifies the primary view.
        /// </summary>
        public override void RowSelected(UITableView tableView, NSIndexPath indexPath)
        {
            // Deselect the row.
            tableView.DeselectRow(indexPath, true);

            // Select the content.
            Owner.ContentSelectionChanged(indexPath.Row);
        }
    }
}