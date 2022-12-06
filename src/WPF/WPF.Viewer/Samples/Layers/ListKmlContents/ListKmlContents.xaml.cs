// Copyright 2018 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using ArcGIS.Samples.Managers;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Ogc;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using System.Windows;

namespace ArcGIS.WPF.Samples.ListKmlContents
{
    [ArcGIS.Samples.Shared.Attributes.Sample(
        name: "List KML contents",
        category: "Layers",
        description: "List the contents of a KML file.",
        instructions: "The contents of the KML file are shown in a tree. Select a node to zoom to that node. Not all nodes can be zoomed to (e.g. screen overlays).",
        tags: new[] { "KML", "KMZ", "Keyhole", "OGC", "layers" })]
    [ArcGIS.Samples.Shared.Attributes.OfflineData("da301cb122874d5497f8a8f6c81eb36e")]
    public partial class ListKmlContents
    {
        // Hold a list of LayerDisplayVM; this is the ViewModel.
        private readonly ObservableCollection<LayerDisplayVM> _viewModelList = new ObservableCollection<LayerDisplayVM>();

        public ListKmlContents()
        {
            InitializeComponent();
            _ = Initialize();
        }

        private async Task Initialize()
        {
            // Add a basemap.
            MySceneView.Scene = new Scene(BasemapStyle.ArcGISImagery);
            MySceneView.Scene.BaseSurface.ElevationSources.Add(new ArcGISTiledElevationSource(new Uri("https://elevation3d.arcgis.com/arcgis/rest/services/WorldElevation3D/Terrain3D/ImageServer")));

            // Get the URL to the data.
            Uri kmlUrl = new Uri(DataManager.GetDataFolder("da301cb122874d5497f8a8f6c81eb36e", "esri_test_data.kmz"));

            // Create the KML dataset and layer.
            KmlDataset dataset = new KmlDataset(kmlUrl);
            KmlLayer layer = new KmlLayer(dataset);

            // Add the layer to the map.
            MySceneView.Scene.OperationalLayers.Add(layer);

            try
            {
                await dataset.LoadAsync();

                // Build the ViewModel from the expanded list of layer infos.
                foreach (KmlNode node in dataset.RootNodes)
                {
                    // LayerDisplayVM is a custom type made for this sample to serve as the ViewModel; it is not a part of ArcGIS Maps SDK for .NET.
                    LayerDisplayVM nodeVm = new LayerDisplayVM(node, null);
                    _viewModelList.Add(nodeVm);
                    LayerDisplayVM.BuildLayerInfoList(nodeVm, _viewModelList);
                }

                // Update the list of layers, using the root node from the list.
                LayerTreeView.ItemsSource = _viewModelList;
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString(), "Error");
            }
        }

        private void LayerTreeView_OnSelectedItemChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            // Get the KML node.
            LayerDisplayVM selectedItem = (LayerDisplayVM)e.NewValue;

            _ = NavigateToNode(selectedItem.Node);
        }

        #region viewpoint_conversion

        private async Task NavigateToNode(KmlNode node)
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
                    await MySceneView.SetViewpointAsync(runtimeViewpoint);
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.ToString(), "Error");
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
                else
                {
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
                    altitude = await MySceneView.Scene.BaseSurface.GetElevationAsync(lookAtExtent.GetCenter());
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
                    altitude = await MySceneView.Scene.BaseSurface.GetElevationAsync(lookAtPoint);
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
        public KmlNode Node { get; set; }

        public List<LayerDisplayVM> Children { get; set; }

        private LayerDisplayVM Parent { get; set; }

        public LayerDisplayVM(KmlNode info, LayerDisplayVM parent)
        {
            Node = info;
            Parent = parent;
        }

        // Override ToString to enhance display formatting.
        public override string ToString()
        {
            return Node.GetType().Name + " - " + Node.Name;
        }

        public static void BuildLayerInfoList(LayerDisplayVM root, IList<LayerDisplayVM> result)
        {
            // Make the node visible.
            root.Node.IsVisible = true;

            // Initialize the child collection for the root.
            root.Children = new List<LayerDisplayVM>();

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

                // Add the sublayer to the root's sublayer collection.
                root.Children.Add(layerVm);

                // Recursively add children.
                BuildLayerInfoList(layerVm, result);
            }
        }
    }
}