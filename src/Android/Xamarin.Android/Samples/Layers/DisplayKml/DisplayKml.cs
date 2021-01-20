// Copyright 2018 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific 
// language governing permissions and limitations under the License.

using System;
using Android.App;
using Android.OS;
using Android.Views;
using Android.Widget;
using ArcGISRuntime.Samples.Managers;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Portal;
using Esri.ArcGISRuntime.UI.Controls;

namespace ArcGISRuntimeXamarin.Samples.DisplayKml
{
    [Activity (ConfigurationChanges=Android.Content.PM.ConfigChanges.Orientation | Android.Content.PM.ConfigChanges.ScreenSize)]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        name: "Display KML",
        category: "Layers",
        description: "Display KML from a URL, portal item, or local KML file.",
        instructions: "Use the UI to select a source. A KML file from that source will be loaded and displayed in the scene.",
        tags: new[] { "KML", "KMZ", "OGC", "keyhole" })]
    [ArcGISRuntime.Samples.Shared.Attributes.OfflineData("324e4742820e46cfbe5029ff2c32cb1f")]
    public class DisplayKml : Activity
    {
        // Hold references to UI controls.
        private SceneView _mySceneView;
        private Button _dataChoiceButton;
        private readonly Envelope _usEnvelope = new Envelope(-144.619561355187, 18.0328662832097, -66.0903762761083, 67.6390975806745, SpatialReferences.Wgs84);
        private readonly string[] _sources = {"URL", "Local file", "Portal item"};

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            Title = "Display KML";

            CreateLayout();
            Initialize();
        }

        private void Initialize()
        {
            // Set up the basemap.
            _mySceneView.Scene = new Scene(Basemap.CreateImageryWithLabels());
        }

        private void CreateLayout()
        {
            // Create a new vertical layout for the app.
            LinearLayout layout = new LinearLayout(this) {Orientation = Orientation.Vertical};

            // Create button to show data choices
            _dataChoiceButton = new Button(this)
            {
                Text = "Choose a KML data source"
            };
            _dataChoiceButton.Click += DataChoiceButtonOnClick;

            // Add button
            layout.AddView(_dataChoiceButton);

            // Add the scene view to the layout
            _mySceneView = new SceneView(this);
            layout.AddView(_mySceneView);

            // Show the layout in the app
            SetContentView(layout);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            // Remove the sceneview
            (_mySceneView.Parent as ViewGroup).RemoveView(_mySceneView);
            _mySceneView.Dispose();
            _mySceneView = null;
        }

        private void DataChoiceButtonOnClick(object sender, EventArgs e)
        {
            // Create menu to show the rendering rule options
            PopupMenu sourceMenu = new PopupMenu(this, _dataChoiceButton);
            sourceMenu.MenuItemClick += DataChoiceButton_Clicked;

            // Create menu options
            foreach (string option in _sources)
            {
                sourceMenu.Menu.Add(option);
            }

            // Show menu in the view
            sourceMenu.Show();
        }

        private async void DataChoiceButton_Clicked(object sender, PopupMenu.MenuItemClickEventArgs e)
        {
            // Clear existing layers.
            _mySceneView.Scene.OperationalLayers.Clear();

            // Get the name of the selected layer.
            string name = e.Item.TitleCondensedFormatted.ToString();

            try
            {
                // Create the layer using the chosen constructor.
                KmlLayer layer;
                switch (name)
                {
                    case "URL":
                    default:
                        layer = new KmlLayer(new Uri("https://www.wpc.ncep.noaa.gov/kml/noaa_chart/WPC_Day1_SigWx_latest.kml"));
                        break;
                    case "Local file":
                        string filePath = DataManager.GetDataFolder("324e4742820e46cfbe5029ff2c32cb1f", "US_State_Capitals.kml");
                        layer = new KmlLayer(new Uri(filePath));
                        break;
                    case "Portal item":
                        ArcGISPortal portal = await ArcGISPortal.CreateAsync();
                        PortalItem item = await PortalItem.CreateAsync(portal, "9fe0b1bfdcd64c83bd77ea0452c76253");
                        layer = new KmlLayer(item);
                        break;
                }

                // Add the selected layer to the map.
                _mySceneView.Scene.OperationalLayers.Add(layer);

                // Zoom to the extent of the United States.
                await _mySceneView.SetViewpointAsync(new Viewpoint(_usEnvelope));
            }
            catch (Exception ex)
            {
                new AlertDialog.Builder(this).SetMessage(ex.ToString()).SetTitle("Error").Show();
            }
        }
    }
}