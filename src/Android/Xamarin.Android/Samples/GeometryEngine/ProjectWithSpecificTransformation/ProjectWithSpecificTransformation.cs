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

namespace ArcGISRuntimeXamarin.Samples.ProjectWithSpecificTransformation
{
    [Activity]
    public class ProjectWithSpecificTransformation : Activity
    {
        // Label for showing the coordinates before projection
        private TextView _lblBefore;

        // Label for showing the coordinates after projection
        private TextView _lblAfter;

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
            MapPoint myPoint = new MapPoint(-73.984513, 40.748469, SpatialReferences.Wgs84);

            // Update the UI with the initial coordinates
            _lblBefore.Text = $"x: {myPoint.X}, y: {myPoint.Y}";

            // Create a geographic transformation step for transfrom WKID 108055, WGS_1984_To_MSK_1942
            GeographicTransformationStep geoStep = new GeographicTransformationStep(108055);

            // Create the transformation
            GeographicTransformation geoTransform = new GeographicTransformation(geoStep);

            // Project to a coordinate system used in New York, NAD_1983_HARN_StatePlane_New_York_Central_FIPS_3102
            MapPoint myAfterPoint = (MapPoint)GeometryEngine.Project(myPoint, SpatialReference.Create(2829), geoTransform);

            // Update the UI with the projected coordinates
            _lblAfter.Text = $"x: {myAfterPoint.X}, y: {myAfterPoint.Y}";
        }

        private void CreateLayout()
        {
            // Create a new vertical layout for the app
            var layout = new LinearLayout(this) { Orientation = Orientation.Vertical };

            // Create the before label
            _lblBefore = new TextView(this);

            // Create the after label
            _lblAfter = new TextView(this);

            // Create two more labels to label the output
            TextView lblExplainBefore = new TextView(this) { Text = "Before: " };
            TextView lblExplainAfter = new TextView(this) { Text = "After: " };

            // Add all labels to the layout
            layout.AddView(lblExplainBefore);
            layout.AddView(_lblBefore);
            layout.AddView(lblExplainAfter);
            layout.AddView(_lblAfter);

            // Show the layout in the app
            SetContentView(layout);
        }
    }
}