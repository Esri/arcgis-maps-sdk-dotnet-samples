// Copyright 2018 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using Android.App;
using Android.OS;
using Android.Views;
using Android.Widget;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.UI.Controls;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ArcGISRuntime.Samples.ChangeViewpoint
{
    [Activity (ConfigurationChanges=Android.Content.PM.ConfigChanges.Orientation | Android.Content.PM.ConfigChanges.ScreenSize)]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        name: "Change viewpoint",
        category: "MapView",
        description: "Set the map view to a new viewpoint.",
        instructions: "The map view has several methods for setting its current viewpoint. Select a viewpoint from the UI to see the viewpoint changed using that method.",
        tags: new[] { "animate", "extent", "pan", "rotate", "scale", "view", "zoom" })]
    public class ChangeViewpoint : Activity
    {
        // Create and hold reference to the used MapView
        private MapView _myMapView;

        // Coordinates for London
        private readonly MapPoint _londonCoords = new MapPoint(-13881.7678417696, 6710726.57374296, SpatialReferences.WebMercator);

        private const double LondonScale = 8762.7156655228955;

        // Coordinates for Redlands
        private readonly Polygon _redlandsPolygon = new Polygon(
            new List<MapPoint>
                {
                    new MapPoint(-13049785.1566222, 4032064.6003424),
                    new MapPoint(-13049785.1566222, 4040202.42595729),
                    new MapPoint(-13037033.5780234, 4032064.6003424),
                    new MapPoint(-13037033.5780234, 4040202.42595729)
                },
            SpatialReferences.WebMercator);

        // Coordinates for Edinburgh
        private readonly Polygon _edinburghPolygon = new Polygon(
            new List<MapPoint>
            {
                new MapPoint(-354262.156621384, 7548092.94093301),
                new MapPoint(-354262.156621384, 7548901.50684376),
                new MapPoint(-353039.164455303, 7548092.94093301),
                new MapPoint(-353039.164455303, 7548901.50684376)},
            SpatialReferences.WebMercator);

        // String array to store titles for the viewpoints specified above.
        private readonly string[] _titles = {
            "Geometry",
            "Center & Scale",
            "Animate"
        };

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            Title = "Change viewpoint";

            // Create the UI, setup the control references and execute initialization
            CreateLayout();
            Initialize();
        }

        private void Initialize()
        {
            // Create new Map with basemap and initial location
            Map myMap = new Map(BasemapStyle.ArcGISTopographic);

            // Assign the map to the MapView
            _myMapView.Map = myMap;
        }

        private void OnMapsClicked(object sender, EventArgs e)
        {
            Button viewpointsButton = (Button)sender;

            // Create menu to show viewpoint options
            PopupMenu mapsMenu = new PopupMenu(this, viewpointsButton);
            mapsMenu.MenuItemClick += OnViewpointMenuItemClicked;

            // Create menu options
            foreach (string title in _titles)
            {
                mapsMenu.Menu.Add(title);
            }

            // Show menu in the view
            mapsMenu.Show();
        }

        private async void OnViewpointMenuItemClicked(object sender, PopupMenu.MenuItemClickEventArgs e)
        {
            try
            {
                // Get title from the selected item
                string selectedMapTitle = e.Item.TitleCondensedFormatted.ToString();

                switch (selectedMapTitle)
                {
                    case "Geometry":

                        // Set Viewpoint using Redlands envelope defined above and a padding of 20
                        await _myMapView.SetViewpointGeometryAsync(_redlandsPolygon, 20);
                        break;

                    case "Center & Scale":

                        // Set Viewpoint so that it is centered on the London coordinates defined above
                        await _myMapView.SetViewpointCenterAsync(_londonCoords);

                        // Set the Viewpoint scale to match the specified scale
                        await _myMapView.SetViewpointScaleAsync(LondonScale);
                        break;

                    case "Animate":

                        // Navigate to full extent of the first baselayer before animating to specified geometry
                        await _myMapView.SetViewpointAsync(
                            new Viewpoint(_myMapView.Map.Basemap.BaseLayers.First().FullExtent));

                        // Create a new Viewpoint using the specified geometry
                        Viewpoint viewpoint = new Viewpoint(_edinburghPolygon);

                        // Set Viewpoint of MapView to the Viewpoint created above and animate to it using a timespan of 5 seconds
                        await _myMapView.SetViewpointAsync(viewpoint, TimeSpan.FromSeconds(5));
                        break;

                    default:
                        break;
                }
            }
            catch (Exception ex)
            {
                new AlertDialog.Builder(this).SetMessage(ex.ToString()).SetTitle("Error").Show();
            }
        }

        private void CreateLayout()
        {
            // Create a new vertical layout for the app
            LinearLayout layout = new LinearLayout(this) { Orientation = Orientation.Vertical };

            // Create button to show possible map options
            Button mapsButton = new Button(this)
            {
                Text = "Change viewpoint"
            };

            mapsButton.Click += OnMapsClicked;

            // Add maps button to the layout
            layout.AddView(mapsButton);

            // Add the map view to the layout
            _myMapView = new MapView(this);
            layout.AddView(_myMapView);

            // Show the layout in the app
            SetContentView(layout);
        }
    }
}