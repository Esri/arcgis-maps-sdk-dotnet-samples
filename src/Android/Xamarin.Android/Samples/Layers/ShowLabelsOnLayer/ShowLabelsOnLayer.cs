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
using Android.Widget;
using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.UI.Controls;

namespace ArcGISRuntime.Samples.ShowLabelsOnLayer
{
    [Activity]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Show labels on layer",
        "Layers",
        "This sample demonstrates how to show labels on a feature layer",
        "The labeling of the names on the US Highways layer is accomplished by supplying a JSON string to the FeatureLayer's LabelDefinition. The JSON is based on the new ArcGIS web map specification.",
        "")]
    public class ShowLabelsOnLayer : Activity
    {
        // Create and hold reference to the used MapView.
        private MapView _myMapView = new MapView();

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            Title = "Show labels on layer";

            // Create the UI, setup the control references and execute initialization. 
            CreateLayout();
            Initialize();
        }

        private async void Initialize()
        {
            // Create a map with a light gray canvas basemap.
            Map sampleMap = new Map(Basemap.CreateLightGrayCanvas());

            // Assign the map to the MapView.
            _myMapView.Map = sampleMap;

            // Define the Url string for the US highways feature layer.
            string highwaysUrlString = "http://sampleserver6.arcgisonline.com/arcgis/rest/services/USA/MapServer/1";

            // Create a service feature table from the url to the US highways feature service.
            ServiceFeatureTable highwaysServiceFeatureTable = new ServiceFeatureTable(new System.Uri(highwaysUrlString));

            // Create a feature layer from the service feature table.
            FeatureLayer highwaysFeatureLayer = new FeatureLayer(highwaysServiceFeatureTable);

            // Add the US highways feature layer to the operations layers collection of the map.
            sampleMap.OperationalLayers.Add(highwaysFeatureLayer);

            // Load the US highways feature layer - this way we can obtain it's extent.
            await highwaysFeatureLayer.LoadAsync();

            // Zoom the map view to the extent of the US highways feature layer.
            await _myMapView.SetViewpointGeometryAsync(highwaysFeatureLayer.FullExtent);

            // Help regarding the Json syntax for defining the LabelDefinition.FromJson syntax can be found here:
            // https://developers.arcgis.com/web-map-specification/objects/labelingInfo/
            // This particular JSON string will have the following characteristics:
            // (1) The 'labelExpressionInfo' defines that the label text displayed comes from the field 'rte_num1' in the 
            //     feature service and will be prefaced with an "I -". Example: "I - 10", "I - 15", "I - 95", etc.
            // (2) The 'labelPlacement' will be placed above and along the highway polyline segment.
            // (3) The 'where' clause restricts the labels to be displayed that has valid (non-empty) data. Empty data
            //     for this service has a single blank space in the 'rte_num1' field.
            // (4) The 'symbol' for the labeled text will be blue with a yellow halo.
            string theJSON_String =
             @"{
                    ""labelExpressionInfo"":{""expression"":""'I - ' + $feature.rte_num1""},
                    ""labelPlacement"":""esriServerLinePlacementAboveAlong"",
                    ""where"":""rte_num1 <> ' '"",
                    ""symbol"":
                        { 
                            ""angle"":0,
                            ""backgroundColor"":[0,0,0,0],
                            ""borderLineColor"":[0,0,0,0],
                            ""borderLineSize"":0,
                            ""color"":[0,0,255,255],
                            ""font"":
                                {
                                    ""decoration"":""none"",
                                    ""size"":15,
                                    ""style"":""normal"",
                                    ""weight"":""normal""
                                },
                            ""haloColor"":[255,255,0,255],
                            ""haloSize"":1.5,
                            ""horizontalAlignment"":""center"",
                            ""kerning"":false,
                            ""type"":""esriTS"",
                            ""verticalAlignment"":""middle"",
                            ""xoffset"":0,
                            ""yoffset"":0
                        }
               }";

            // Create a label definition from the JSON string. 
            LabelDefinition highwaysLabelDefinition = LabelDefinition.FromJson(theJSON_String);

            // Add the label definition to the feature layer's label definition collection.
            highwaysFeatureLayer.LabelDefinitions.Add(highwaysLabelDefinition);

            // Enable the visibility of labels to be seen.
            highwaysFeatureLayer.LabelsEnabled = true;
        }

        private void CreateLayout()
        {
            // Create a new vertical layout for the app.
            LinearLayout layout = new LinearLayout(this) { Orientation = Orientation.Vertical };

            // Add the map view to the layout.
            layout.AddView(_myMapView);

            // Show the layout in the app.
            SetContentView(layout);
        }
    }
}