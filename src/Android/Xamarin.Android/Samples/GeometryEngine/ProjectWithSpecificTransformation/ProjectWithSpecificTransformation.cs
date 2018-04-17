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
using Esri.ArcGISRuntime.Geometry;

namespace ArcGISRuntime.Samples.ProjectWithSpecificTransformation
{
    [Activity]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Project with specific transformation",
        "GeometryEngine",
        "This sample demonstrates how to use the GeometryEngine with a specified geographic transformation to transform a geometry from one coordinate system to another. ",
        "See [Coordinate Systems and Transformations](https://developers.arcgis.com/net/latest/wpf/guide/coordinate-systems-and-transformations.htm) for more information about geographic coordinate systems, geographic transformations, and projected coordinate systems. ")]
    public class ProjectWithSpecificTransformation : Activity
    {
        // Label for showing the coordinates before projection
        private TextView _beforeLabel;

        // Label for showing the coordinates after projection with specific transformation
        private TextView _afterLabel;

        // Label for showing the coordinates after projection without specific transformation
        private TextView _nonSpecificLabel;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            Title = "Project with specific transformation";

            // Create the UI, setup the control references and execute initialization
            CreateLayout();
            Initialize();
        }

        private void Initialize()
        {
            // Create a point geometry in NYC in WGS84
            MapPoint startingPoint = new MapPoint(-73.984513, 40.748469, SpatialReferences.Wgs84);

            // Update the UI with the initial coordinates
            _beforeLabel.Text = $"x: {startingPoint.X}, y: {startingPoint.Y}";

            // Create a geographic transformation step for transform WKID 108055, WGS_1984_To_MSK_1942
            GeographicTransformationStep geoStep = new GeographicTransformationStep(108055);

            // Create the transformation
            GeographicTransformation geoTransform = new GeographicTransformation(geoStep);

            // Project to a coordinate system used in New York, NAD_1983_HARN_StatePlane_New_York_Central_FIPS_3102
            MapPoint afterPoint = (MapPoint)GeometryEngine.Project(startingPoint, SpatialReference.Create(2829), geoTransform);

            // Update the UI with the projected coordinates
            _afterLabel.Text = $"x: {afterPoint.X}, y: {afterPoint.Y}";

            // Perform the same projection without specified transformation
            MapPoint unspecifiedTransformPoint = (MapPoint)GeometryEngine.Project(startingPoint, SpatialReference.Create(2829));

            // Update the UI with the projection done without specific transform for comparison purposes
            _nonSpecificLabel.Text = $"x: {unspecifiedTransformPoint.X}, y: {unspecifiedTransformPoint.Y}";
        }

        private void CreateLayout()
        {
            // Create a new vertical layout for the app
            var layout = new LinearLayout(this) { Orientation = Orientation.Vertical };

            // Create the labels
            _beforeLabel = new TextView(this);
            _afterLabel = new TextView(this);
            _nonSpecificLabel = new TextView(this);

            // Create three more labels to label the output
            TextView beforeLabelTitle = new TextView(this) { Text = "Before: " };
            TextView afterLabelTitle = new TextView(this) { Text = "After: " };
            TextView nonSpecificLabelTitle = new TextView(this) {Text = "After (without specific transformation):"};

            // Add all labels to the layout
            layout.AddView(beforeLabelTitle);
            layout.AddView(_beforeLabel);
            layout.AddView(afterLabelTitle);
            layout.AddView(_afterLabel);
            layout.AddView(nonSpecificLabelTitle);
            layout.AddView(_nonSpecificLabel);

            // Show the layout in the app
            SetContentView(layout);
        }
    }
}