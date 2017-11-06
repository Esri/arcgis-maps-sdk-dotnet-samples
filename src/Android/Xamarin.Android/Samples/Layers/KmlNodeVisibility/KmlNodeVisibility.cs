// Copyright 2017 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using Android.App;
using Android.OS;
using Android.Widget;
using ArcGISRuntimeXamarin.Managers;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Ogc;
using Esri.ArcGISRuntime.UI.Controls;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace ArcGISRuntimeXamarin.Samples.KmlNodeVisibility
{
    [Activity]
    public class KmlNodeVisibility : Activity
    {
        // Create and hold reference to the used SceneView
        private SceneView _mySceneView = new SceneView();

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            Title = "KmlNode visibility";

            // Create the UI, setup the control references
            CreateLayout();

            // Initialize the sample
            Initialize();
        }

        private async void Initialize()
        {
            // Create dialog to display alert information
            var alert = new AlertDialog.Builder(this);

            try
            {
                // Create a new scene
                Scene myScene = new Scene();

                // Set the base map of the scene to the oceans web service
                myScene.Basemap = Basemap.CreateOceans();

                // Get the path on disk to where the KML data file is located
                string kmlFilePath = await GetKMLPath();

                // Create a URI for the location of the KML data file
                Uri myKmlUri = new Uri(kmlFilePath);

                // Create a KML dataset from the URI
                KmlDataset myKmlDataset = new KmlDataset(myKmlUri);

                // Create a new instance of a KmlLayer object 
                KmlLayer myKmlLayer = new KmlLayer(myKmlDataset);

                // Add the KmLayer to the scene's operational layers
                myScene.OperationalLayers.Add(myKmlLayer);

                // Assign the scene to the SceneView
                _mySceneView.Scene = myScene;

                // Define a map point to zoom to (in the case: North East North America)
                MapPoint myMapPoint = new MapPoint(-78.78, 40.30, 397774, SpatialReferences.Wgs84);

                // Define a camera centered on the map point with the appropriate heading, pitch and roll
                Camera myCamera = new Camera(myMapPoint, 356.98, 51.48, 0);

                // Zoom to the extent
                await _mySceneView.SetViewpointCameraAsync(myCamera);
            }
            catch (Exception ex)
            {
                // Something went wrong, display a message
                alert.SetTitle("Error");
                alert.SetMessage(ex.ToString());
                alert.Show();
            }
        }

        private IList<KmlNode> FindAllNodes(KmlDataset dataset)
        {
            // This function gathers a list of all KML nodes in the given dataset, recursively

            // Create an empty list of KML nodes
            var list = new List<KmlNode>();

            // Loop through all the KML nodes in the root node
            foreach (KmlNode rootNode in dataset.RootNodes)

                // Call the recursive function get the nested KML nodes from all of the child nodes
                CollectNodesAndChildren(rootNode, list);

            // Returns a flat list of all KML nodes in the KML dataset
            return list;
        }

        private void CollectNodesAndChildren(KmlNode myKmlNode, ICollection<KmlNode> collection)
        {
            // Add the KML node to the collection
            collection.Add(myKmlNode);

            if (myKmlNode is KmlContainer)
            {
                KmlContainer myKmlContainer = (KmlContainer)myKmlNode;

                // Recursively loop through each child node in the KML dataset  
                foreach (KmlNode child in myKmlContainer.ChildNodes)
                    CollectNodesAndChildren(child, collection);
            };
        }

        private void ToggleKmlNodeVisibility(KmlLayer myKmlLayer, string overlayType)
        {
            // Get the KML dataset from the KML layer
            KmlDataset myKmlDataSet = myKmlLayer.Dataset;

            // Get the root nodes from the KML dataset
            IReadOnlyList<KmlNode> myRootFeatures = myKmlDataSet.RootNodes;

            // Get all of the KML nodes in the KML dataset
            var myKmlNodes = FindAllNodes(myKmlDataSet);

            // Loop through each KML node
            foreach (KmlNode oneKmlNode in myKmlNodes)
            {
                // Toggle the visibility of KML ground overlay types
                if (overlayType == "aKmlGroundOverlay")
                {
                    if (oneKmlNode is KmlGroundOverlay)
                    {
                        oneKmlNode.IsVisible = !oneKmlNode.IsVisible;
                    }
                }

                // Toggle the visibility of KML screen overlay types 
                if (overlayType == "aKmlScreenOverlay")
                {
                    if (oneKmlNode is KmlScreenOverlay)
                    {
                        oneKmlNode.IsVisible = !oneKmlNode.IsVisible;
                    }
                }

                // Toggle the visibility of KML placemark types 
                if (overlayType == "aKmlPlacemark")
                {
                    if (oneKmlNode is KmlPlacemark)
                    {
                        oneKmlNode.IsVisible = !oneKmlNode.IsVisible;
                    }
                }
            }
        }

        private void ScreenOverlaysOnOff_Clicked(object sender, EventArgs e)
        {
            // Get the layer collection from the scene view
            LayerCollection myOperationLayersCollectionSV = _mySceneView.Scene.OperationalLayers;

            // Make sure there is at least one operational layer present 
            if (myOperationLayersCollectionSV.Count > 0)
            {
                // Get the first operational layer 
                Layer myLayerSV = myOperationLayersCollectionSV[0];

                // Make sure the operational layer is a KmlLayer
                if (myLayerSV is KmlLayer)
                {
                    // Cast the operational layer to a KmLayer
                    KmlLayer myKmlLayerSV = (KmlLayer)myLayerSV;

                    // Call the function to toggle on/off KML screen overlays
                    ToggleKmlNodeVisibility(myKmlLayerSV, "aKmlScreenOverlay");
                }
            }
        }

        private void GroundOverlaysOnOff_Clicked(object sender, EventArgs e)
        {
            // Get the layer collection from the scene view
            LayerCollection myOperationLayersCollectionSV = _mySceneView.Scene.OperationalLayers;

            // Make sure there is at least one operational layer present 
            if (myOperationLayersCollectionSV.Count > 0)
            {
                // Get the first operational layer 
                Layer myLayerSV = myOperationLayersCollectionSV[0];

                // Make sure the operational layer is a KmlLayer
                if (myLayerSV is KmlLayer)
                {
                    // Cast the operational layer to a KmLayer
                    KmlLayer myKmlLayerSV = (KmlLayer)myLayerSV;

                    // Call the function to toggle on/off KML ground overlays
                    ToggleKmlNodeVisibility(myKmlLayerSV, "aKmlGroundOverlay");
                }
            }
        }

        private void PLacemarksOnOff_Clicked(object sender, EventArgs e)
        {
            // Get the layer collection from the scene view
            LayerCollection myOperationLayersCollectionSV = _mySceneView.Scene.OperationalLayers;

            // Make sure there is at least one operational layer present 
            if (myOperationLayersCollectionSV.Count > 0)
            {
                // Get the first operational layer 
                Layer myLayerSV = myOperationLayersCollectionSV[0];

                // Make sure the operational layer is a KmlLayer
                if (myLayerSV is KmlLayer)
                {
                    // Cast the operational layer to a KmLayer
                    KmlLayer myKmlLayerSV = (KmlLayer)myLayerSV;

                    // Call the function to toggle on/off KML placemarks
                    ToggleKmlNodeVisibility(myKmlLayerSV, "aKmlPlacemark");
                }
            }
        }

        // Get the file path for the KML file
        private async Task<string> GetKMLPath()
        {
            #region offlinedata
            // The data manager provides a method to get the folder
            string folder = DataManager.GetDataFolder();

            // Get the full path
            string filepath = Path.Combine(folder, "SampleData", "KmlNodeVisibility", "LakesTest.kml");

            // Check if the file exists
            if (!File.Exists(filepath))
            {
                // Download the file
                await DataManager.GetData("b32481af00c94d638d51c58332d2c742", "KmlNodeVisibility");
            }

            return filepath;
            #endregion offlinedata
        }
        private void CreateLayout()
        {
            // Create a new vertical layout for the app
            var layout = new LinearLayout(this) { Orientation = Orientation.Vertical };

            // Create Button
            var button1 = new Button(this);
            button1.Text = "Toggle ScreenOverlays On/Off";
            button1.Click += ScreenOverlaysOnOff_Clicked;

            // Add Button to the layout  
            layout.AddView(button1);

            // Create Button
            var button2 = new Button(this);
            button2.Text = "Toggle GroundOverlays On/Off";
            button2.Click += GroundOverlaysOnOff_Clicked;

            // Add Button to the layout  
            layout.AddView(button2);

            // Create Button
            var button3 = new Button(this);
            button3.Text = "Toggle Placemarks On/Off";
            button3.Click += PLacemarksOnOff_Clicked;

            // Add Button to the layout  
            layout.AddView(button3);

            // Add the scene view to the layout
            layout.AddView(_mySceneView);

            // Show the layout in the app
            SetContentView(layout);
        }

    }
}