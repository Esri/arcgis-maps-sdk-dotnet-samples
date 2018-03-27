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

namespace ArcGISRuntime.Samples.ProjectWithSpecificTransformation
{
    [Register("ProjectWithSpecificTransformation")]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Project with specific transformation",
        "GeometryEngine",
        "This sample demonstrates how to use the GeometryEngine with a specified geographic transformation to transform a geometry from one coordinate system to another. ",
        "See [Coordinate Systems and Transformations](https://developers.arcgis.com/net/latest/wpf/guide/coordinate-systems-and-transformations.htm) for more information about geographic coordinate systems, geographic transformations, and projected coordinate systems. ")]
    public class ProjectWithSpecificTransformation : UIViewController
    {
        // Label to show the coordinates before projection
        private readonly UITextView _beforeLabel = new UITextView()
        {
            TextColor = UIColor.Red
        };

        // Label to show the coordinates after projection with specific transformation
        private readonly UITextView _afterLabel = new UITextView()
        {
            TextColor = UIColor.Red
        };

        // Label to show the coordinates after projection without specific transformation
        private readonly UITextView _nonSpecificLabel = new UITextView()
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
            MapPoint startingPoint = new MapPoint(-73.984513, 40.748469, SpatialReferences.Wgs84);

            // Update the UI with the initial coordinates
            _beforeLabel.Text = $"Before - x: {startingPoint.X}, y: {startingPoint.Y}";

            // Create a geographic transformation step for transform WKID 108055, WGS_1984_To_MSK_1942
            GeographicTransformationStep geoStep = new GeographicTransformationStep(108055);

            // Create the transformation
            GeographicTransformation geoTransform = new GeographicTransformation(geoStep);

            // Project to a coordinate system used in New York, NAD_1983_HARN_StatePlane_New_York_Central_FIPS_3102
            MapPoint afterPoint = (MapPoint)GeometryEngine.Project(startingPoint, SpatialReference.Create(2829), geoTransform);

            // Update the UI with the projected coordinates
            _afterLabel.Text = $"After (specific) - x: {afterPoint.X}, y: {afterPoint.Y}";

            // Perform the same projection without specified transformation
            MapPoint unspecifiedTransformPoint = (MapPoint)GeometryEngine.Project(startingPoint, SpatialReference.Create(2829));

            // Update the UI with the projection done without specific transform for comparison purposes
            _nonSpecificLabel.Text = $"After (non-specific) - x: {unspecifiedTransformPoint.X}, y: {unspecifiedTransformPoint.Y}";
        }

        private void CreateLayout()
        {
            // Add the labels to the page
            View.AddSubviews(_beforeLabel, _afterLabel, _nonSpecificLabel);

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
            _beforeLabel.Frame = new CGRect(10, View.Bounds.Height / 2, View.Bounds.Width - 20, 50);
            _afterLabel.Frame = new CGRect(10, View.Bounds.Height - 50, View.Bounds.Width - 20, 50);
            _nonSpecificLabel.Frame = new CGRect(10, View.Bounds.Height * 3.0 / 4.0, View.Bounds.Width - 20, 50);
            base.ViewDidLayoutSubviews();
        }
    }
}