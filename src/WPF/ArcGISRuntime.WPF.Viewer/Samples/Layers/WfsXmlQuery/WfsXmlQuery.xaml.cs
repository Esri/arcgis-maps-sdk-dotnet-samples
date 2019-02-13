// Copyright 2019 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.Ogc;
using System;
using System.Diagnostics;
using System.Windows;

namespace ArcGISRuntime.WPF.Samples.WfsXmlQuery
{
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Load WFS with XML query",
        "Layers",
        "Load a WFS feature table using an XML query.",
        "")]
    public partial class WfsXmlQuery
    {
        // Constants for the service URL and the query.
        private const string XmlQuery = @"
<wfs:GetFeature service=""WFS"" version=""2.0.0""
  xmlns:Seattle_Downtown_Features=""https://dservices2.arcgis.com/ZQgQTuoyBrtmoGdP/arcgis/services/Seattle_Downtown_Features/WFSServer""
  xmlns:wfs=""http://www.opengis.net/wfs/2.0""
  xmlns:fes=""http://www.opengis.net/fes/2.0""
  xmlns:gml=""http://www.opengis.net/gml/3.2"">
  <wfs:Query typeNames=""Seattle_Downtown_Features:Trees"">
    <fes:Filter>
      <fes:PropertyIsEqualTo>
        <fes:ValueReference>Trees:SCIENTIFIC</fes:ValueReference>
        <fes:Literal>Tilia cordata</fes:Literal>
      </fes:PropertyIsEqualTo>
    </fes:Filter>
  </wfs:Query>
</wfs:GetFeature>
";
        private const string TableUrl = "https://dservices2.arcgis.com/ZQgQTuoyBrtmoGdP/arcgis/services/Seattle_Downtown_Features/WFSServer?service=wfs&request=getcapabilities";
        private const string LayerName = "Seattle_Downtown_Features:Trees";

        public WfsXmlQuery()
        {
            InitializeComponent();
            Initialize();
        }

        private async void Initialize()
        {
            // Create the map with basemap.
            MyMapView.Map = new Map(Basemap.CreateNavigationVector());

            try
            {
                // Create the WFS feature table from URL and name.
                WfsFeatureTable statesTable = new WfsFeatureTable(new Uri(TableUrl), LayerName);

                // Set the feature request mode and axis order.
                statesTable.AxisOrder = OgcAxisOrder.NoSwap;
                statesTable.FeatureRequestMode = FeatureRequestMode.ManualCache;

                // Load the table.
                await statesTable.LoadAsync();

                // Create a feature layer to visualize the table.
                FeatureLayer statesLayer = new FeatureLayer(statesTable);

                // Add the layer to the map.
                MyMapView.Map.OperationalLayers.Add(statesLayer);

                // Populate the feature table with the XML query.
                await statesTable.PopulateFromServiceWithXmlAsync(XmlQuery, true);

                // Zoom to the extent of the query results.
                await MyMapView.SetViewpointGeometryAsync(statesTable.Extent, 50);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.ToString());
                MessageBox.Show(e.ToString(), "Couldn't populate table with XML query.");
            }
        }
    }
}
