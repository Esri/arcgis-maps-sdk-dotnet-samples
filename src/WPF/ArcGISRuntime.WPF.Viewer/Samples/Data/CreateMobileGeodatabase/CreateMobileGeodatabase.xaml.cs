// Copyright 2022 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Mapping;
using Esri.ArcGISRuntime.UI.Controls;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows;

namespace ArcGISRuntime.WPF.Samples.CreateMobileGeodatabase
{
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Create mobile geodatabase",
        "Data",
        "Create and share a mobile geodatabase.",
        "")]
    [ArcGISRuntime.Samples.Shared.Attributes.OfflineData()]
    public partial class CreateMobileGeodatabase
    {
        private FeatureTable _featureTable;
        private Geodatabase _geodatabase;
        private string _gdbPath;

        public CreateMobileGeodatabase()
        {
            InitializeComponent();
            _ = Initialize();
        }

        private async Task Initialize()
        {
            MyMapView.Map = new Map(BasemapStyle.ArcGISTopographic);
            MyMapView.SetViewpoint(new Viewpoint(34.056295, -117.195800, 10000.0));

            try
            {
                // Create the geodatabase file.
                _gdbPath = Path.Combine(Environment.ExpandEnvironmentVariables("%TEMP%"), "LocationHistory.geodatabase");

                if (File.Exists(_gdbPath))
                {
                    File.Delete(_gdbPath);
                }

                _geodatabase = await Geodatabase.CreateAsync(_gdbPath);

                // Construct a table description which stores features as points on map.
                var tableDescription = new TableDescription("LocationHistory", SpatialReferences.Wgs84, GeometryType.Point);
                tableDescription.HasAttachments = false;
                tableDescription.HasM = false;
                tableDescription.HasZ = false;

                // Set up the fields to the table,
                // Field.Type.OID is the primary key of the SQLite table
                // Field.Type.DATE is a date column used to store a Calendar date
                // FieldDescriptions can be a SHORT, INTEGER, GUID, FLOAT, DOUBLE, DATE, TEXT, OID, GLOBALID, BLOB, GEOMETRY, RASTER, or XML.
                tableDescription.FieldDescriptions.Add(new FieldDescription("oid", FieldType.OID));
                tableDescription.FieldDescriptions.Add(new FieldDescription("collection_timestamp", FieldType.Date));

                // Add a new table to the geodatabase by creating one from the table description.
                _featureTable = await _geodatabase.CreateTableAsync(tableDescription);

                // Create a feature layer for the map.
                FeatureLayer featureLayer = new FeatureLayer(_featureTable);
                MyMapView.Map.OperationalLayers.Add(featureLayer);

                MyMapView.GeoViewTapped += MyMapView_GeoViewTapped;
            }
            catch (Exception ex)
            {
            }
        }

        private void ViewTable(object sender, RoutedEventArgs e)
        {
        }

        private void CreateGeodatabase(object sender, RoutedEventArgs e)
        {
        }

        private void MyMapView_GeoViewTapped(object sender, GeoViewInputEventArgs e)
        {
            _ = AddFeature(e.Location);
        }

        private async Task AddFeature(MapPoint location)
        {
            var attributes = new Dictionary<string, object>();
            attributes["collection_timestamp"] = DateTime.Now;

            Feature feature = _featureTable.CreateFeature(attributes, location);
            try
            {
                await _featureTable.AddFeatureAsync(feature);

                FeaturesLabel.Content = $"Number of features added: {_featureTable.NumberOfFeatures}";
            }
            catch (Exception ex)
            {
            }
        }
    }
}