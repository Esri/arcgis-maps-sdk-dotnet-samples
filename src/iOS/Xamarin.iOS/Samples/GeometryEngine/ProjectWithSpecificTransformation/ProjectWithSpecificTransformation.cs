// Copyright 2018 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using CoreGraphics;
using Esri.ArcGISRuntime.Geometry;
using Foundation;
using UIKit;

namespace ArcGISRuntimeXamarin.Samples.ProjectWithSpecificTransformation
{
    [Register("ProjectWithSpecificTransformation")]
    public class ProjectWithSpecificTransformation : UIViewController
    {
        // Label to show the coordinates before projection
        private readonly UITextView _lblBefore = new UITextView()
        {
            TextColor = UIColor.Red
        };

        // Label to show the coordinates after projection
        private readonly UITextView _lblAfter = new UITextView()
        {
            TextColor = UIColor.Red
        };

        public ProjectWithSpecificTransformation()
        {
            Title = "Project with specific transformation";
        }

        private void Initialize()
        {
            // Create a point geometry in NYC in WGS84
            MapPoint myPoint = new MapPoint(-73.984513, 40.748469, SpatialReferences.Wgs84);

            // Update the UI with the initial coordinates
            _lblBefore.Text = $"Before - x: {myPoint.X}, y: {myPoint.Y}";

            // Create a geographic transformation step for transfrom WKID 108055, WGS_1984_To_MSK_1942
            GeographicTransformationStep geoStep = new GeographicTransformationStep(108055);

            // Create the transformation
            GeographicTransformation geoTransform = new GeographicTransformation(geoStep);

            // Project to a coordinate system used in New York, NAD_1983_HARN_StatePlane_New_York_Central_FIPS_3102
            MapPoint myAfterPoint = (MapPoint)GeometryEngine.Project(myPoint, SpatialReference.Create(2829), geoTransform);

            // Update the UI with the projected coordinates
            _lblAfter.Text = $"After - x: {myAfterPoint.X}, y: {myAfterPoint.Y}";
        }

        private void CreateLayout()
        {
            // Add the labels to the page
            View.AddSubviews(_lblBefore, _lblAfter);

            // Set the background color so labels are readable
            View.BackgroundColor = UIColor.White;
        }

        public override void ViewDidLoad()
        {
            CreateLayout();
            Initialize();

            base.ViewDidLoad();
        }

        public override void ViewDidLayoutSubviews()
        {
            _lblBefore.Frame = new CGRect(10, View.Bounds.Height / 2, View.Bounds.Width - 20, 80);
            _lblAfter.Frame = new CGRect(10, View.Bounds.Height - 80, View.Bounds.Width - 20, 80);
            base.ViewDidLayoutSubviews();
        }
    }
}