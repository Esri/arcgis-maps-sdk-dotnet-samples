// Copyright 2018 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using Esri.ArcGISRuntime.Geometry;
using System;

namespace ArcGISRuntime.WPF.Samples.ProjectWithSpecificTransformation
{
    public partial class ProjectWithSpecificTransformation
    {
        public ProjectWithSpecificTransformation()
        {
            InitializeComponent();

            Initialize();
        }

        private void Initialize()
        {
            // Create a point geometry in NYC in WGS84
            MapPoint myPoint = new MapPoint(-73.984513, 40.748469, SpatialReferences.Wgs84);

            // Update the UI with the initial coordinates
            lblBefore.Content = String.Format("x: {0}, y: {1}", myPoint.X, myPoint.Y);

            // Create a geographic transformation step for transfrom WKID 108055, WGS_1984_To_MSK_1942
            GeographicTransformationStep geoStep = new GeographicTransformationStep(108055);

            // Create the transformation
            GeographicTransformation geoTransform = new GeographicTransformation(geoStep);

            // Project to a coordinate system used in New York, NAD_1983_HARN_StatePlane_New_York_Central_FIPS_3102
            MapPoint myAfterPoint = (MapPoint)GeometryEngine.Project(myPoint, SpatialReference.Create(2829), geoTransform);

            // Update the UI with the projected coordinates
            lblAfter.Content = String.Format("x: {0}, y: {1}", myAfterPoint.X, myAfterPoint.Y);
        }
    }
}