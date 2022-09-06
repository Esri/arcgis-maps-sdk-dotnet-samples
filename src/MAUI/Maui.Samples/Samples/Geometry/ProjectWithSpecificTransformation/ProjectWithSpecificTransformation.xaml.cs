// Copyright 2018 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using Esri.ArcGISRuntime.Geometry;

namespace ArcGISRuntime.Samples.ProjectWithSpecificTransformation
{
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        name: "Project with specific transformation",
        category: "Geometry",
        description: "Project a point from one coordinate system to another using a specific transformation step.",
        instructions: "View the values for: unprojected point, projected with the GeometryEngine default, and projected with a specific transformation step.",
        tags: new[] { "coordinate system", "geographic", "project", "projection", "transform", "transformation", "transformation step" })]
    public partial class ProjectWithSpecificTransformation : ContentPage
    {
        public ProjectWithSpecificTransformation()
        {
            InitializeComponent();

            // Create the UI, setup the control references and execute initialization
            Initialize();
        }

        private void Initialize()
        {
            // Create a point geometry in NYC in WGS84
            MapPoint startingPoint = new MapPoint(-73.984513, 40.748469, SpatialReferences.Wgs84);

            // Update the UI with the initial coordinates
            BeforeLabel.Text = $"x: {startingPoint.X}, y: {startingPoint.Y}";

            // Create a geographic transformation step for transform WKID 108055, WGS_1984_To_MSK_1942
            GeographicTransformationStep geoStep = new GeographicTransformationStep(108055);

            // Create the transformation
            GeographicTransformation geoTransform = new GeographicTransformation(geoStep);

            // Project to a coordinate system used in New York, NAD_1983_HARN_StatePlane_New_York_Central_FIPS_3102
            MapPoint afterPoint = (MapPoint)GeometryEngine.Project(startingPoint, SpatialReference.Create(2829), geoTransform);

            // Update the UI with the projected coordinates
            AfterLabel.Text = $"x: {afterPoint.X}, y: {afterPoint.Y}";

            // Perform the same projection without specified transformation
            MapPoint unspecifiedTransformPoint = (MapPoint)GeometryEngine.Project(startingPoint, SpatialReference.Create(2829));

            // Update the UI with the projection done without specific transform for comparison purposes
            NonSpecificLabel.Text = $"x: {unspecifiedTransformPoint.X}, y: {unspecifiedTransformPoint.Y}";
        }
    }
}