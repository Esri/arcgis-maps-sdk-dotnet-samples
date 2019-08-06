﻿// Copyright 2019 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific 
// language governing permissions and limitations under the License.

using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.UI.Controls;
using Foundation;
using System;
using System.Diagnostics;
using UIKit;

namespace ArcGISRuntimeXamarin.Samples.WfsXmlQuery
{
    [Register("WfsXmlQuery")]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Load WFS with XML query",
        "Layers",
        "Load a WFS feature table using an XML query.",
        "")]
    public class WfsXmlQuery : UIViewController
    {
        // Hold references to UI controls.
        private MapView _myMapView;

        // To learn more about specifying filters in OGC technologies, see https://www.opengeospatial.org/standards/filter.
        private const string XmlQuery = @"
<wfs:GetFeature service=""WFS"" version=""2.0.0""
  xmlns:Seattle_Downtown_Features=""https://dservices2.arcgis.com/ZQgQTuoyBrtmoGdP/arcgis/services/Seattle_Downtown_Features/WFSServer""
  xmlns:wfs=""http://www.opengis.net/wfs/2.0""
  xmlns:fes=""http://www.opengis.net/fes/2.0""
  xmlns:gml=""http://www.opengis.net/gml/3.2"">
  <wfs:Query typeNames=""Seattle_Downtown_Features:Trees"">
    <fes:Filter>
      <fes:PropertyIsLike wildCard=""*"" escapeChar=""\"">
        <fes:ValueReference>Trees:SCIENTIFIC</fes:ValueReference>
        <fes:Literal>Tilia *</fes:Literal>
      </fes:PropertyIsLike>
    </fes:Filter>
  </wfs:Query>
</wfs:GetFeature>
";

        // Constants for the table name and URL.
        private const string TableUrl = "https://dservices2.arcgis.com/ZQgQTuoyBrtmoGdP/arcgis/services/Seattle_Downtown_Features/WFSServer?service=wfs&request=getcapabilities";

        // Note that the layer name is defined by the service. The layer name can be accessed via WfsLayerInfo.Name. 
        private const string LayerName = "Seattle_Downtown_Features:Trees";

        public WfsXmlQuery()
        {
            Title = "Load WFS with XML query";
        }

        private async void Initialize()
        {
            // Create the map with basemap.
            _myMapView.Map = new Map(Basemap.CreateNavigationVector());

            try
            {
                // Create the WFS feature table from URL and name.
                WfsFeatureTable wfsTable = new WfsFeatureTable(new Uri(TableUrl), LayerName);

                // Set the feature request mode to manual. Only calls to PopulateFromService will load features.
                // Features will not be populated automatically when the user pans and zooms the layer.
                wfsTable.FeatureRequestMode = FeatureRequestMode.ManualCache;

                // Load the WFS feature table.
                await wfsTable.LoadAsync();

                // Create a feature layer to visualize the WFS feature table.
                FeatureLayer statesLayer = new FeatureLayer(wfsTable);

                // Add the layer to the map.
                _myMapView.Map.OperationalLayers.Add(statesLayer);

                // Populate the WFS feature table with the XML query.
                await wfsTable.PopulateFromServiceAsync(XmlQuery, true);

                // Zoom to the extent of the query results.
                await _myMapView.SetViewpointGeometryAsync(wfsTable.Extent, 50);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e.ToString());
                new UIAlertView("Error", e.ToString(), (IUIAlertViewDelegate) null, "Couldn't populate table with XML query.", null).Show();
            }
        }

        public override void LoadView()
        {
            // Create the views.
            View = new UIView();

            _myMapView = new MapView();
            _myMapView.TranslatesAutoresizingMaskIntoConstraints = false;

            // Add the views.
            View.AddSubviews(_myMapView);

            // Lay out the views.
            _myMapView.TopAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.TopAnchor).Active = true;
            _myMapView.BottomAnchor.ConstraintEqualTo(View.BottomAnchor).Active = true;
            _myMapView.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor).Active = true;
            _myMapView.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor).Active = true;
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            Initialize();
        }
    }
}