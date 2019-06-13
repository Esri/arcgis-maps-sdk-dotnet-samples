// Copyright 2018 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using Esri.ArcGISRuntime.Geometry;
using Foundation;
using UIKit;

namespace ArcGISRuntime.Samples.ProjectWithSpecificTransformation
{
    [Register("ProjectWithSpecificTransformation")]
    [ArcGISRuntime.Samples.Shared.Attributes.Sample(
        "Project with specific transformation",
        "Geometry",
        "This sample demonstrates how to use the GeometryEngine with a specified geographic transformation to transform a geometry from one coordinate system to another. ",
        "See [Coordinate Systems and Transformations](https://developers.arcgis.com/net/latest/wpf/guide/coordinate-systems-and-transformations.htm) for more information about geographic coordinate systems, geographic transformations, and projected coordinate systems. ")]
    public class ProjectWithSpecificTransformation : UIViewController
    {
        // Hold references to UI controls.
        private UILabel _beforeLabel;
        private UILabel _afterLabel;
        private UILabel _nonSpecificLabel;

        public ProjectWithSpecificTransformation()
        {
            Title = "Project with specific transformation";
        }

        private void Initialize()
        {
            // Create a point geometry in NYC in WGS84.
            MapPoint startingPoint = new MapPoint(-73.984513, 40.748469, SpatialReferences.Wgs84);

            // Update the UI with the initial coordinates.
            _beforeLabel.Text = $"Before - x: {startingPoint.X}, y: {startingPoint.Y}";

            // Create a geographic transformation step for transform WKID 108055, WGS_1984_To_MSK_1942.
            GeographicTransformationStep geoStep = new GeographicTransformationStep(108055);

            // Create the transformation.
            GeographicTransformation geoTransform = new GeographicTransformation(geoStep);

            // Project to a coordinate system used in New York, NAD_1983_HARN_StatePlane_New_York_Central_FIPS_3102.
            MapPoint afterPoint = (MapPoint) GeometryEngine.Project(startingPoint, SpatialReference.Create(2829), geoTransform);

            // Update the UI with the projected coordinates.
            _afterLabel.Text = $"After (specific) - x: {afterPoint.X}, y: {afterPoint.Y}";

            // Perform the same projection without specified transformation.
            MapPoint unspecifiedTransformPoint = (MapPoint) GeometryEngine.Project(startingPoint, SpatialReference.Create(2829));

            // Update the UI with the projection done without specific transform for comparison purposes.
            _nonSpecificLabel.Text = $"After (non-specific) - x: {unspecifiedTransformPoint.X}, y: {unspecifiedTransformPoint.Y}";
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            Initialize();
        }

        public override void LoadView()
        {
            // Create the views.
            View = new UIView {BackgroundColor = UIColor.White};

            UIStackView container = new UIStackView();
            container.TranslatesAutoresizingMaskIntoConstraints = false;
            container.Axis = UILayoutConstraintAxis.Vertical;
            container.Distribution = UIStackViewDistribution.FillEqually;
            container.LayoutMarginsRelativeArrangement = true;
            container.LayoutMargins = new UIEdgeInsets(8, 8, 8, 8);

            _beforeLabel = new UILabel();
            _beforeLabel.Lines = 2;
            _beforeLabel.TranslatesAutoresizingMaskIntoConstraints = false;

            _afterLabel = new UILabel();
            _afterLabel.Lines = 2;
            _afterLabel.TranslatesAutoresizingMaskIntoConstraints = false;

            _nonSpecificLabel = new UILabel();
            _nonSpecificLabel.Lines = 2;
            _nonSpecificLabel.TranslatesAutoresizingMaskIntoConstraints = false;

            // Add the views.
            container.AddArrangedSubview(_beforeLabel);
            container.AddArrangedSubview(_afterLabel);
            container.AddArrangedSubview(_nonSpecificLabel);
            View.AddSubview(container);

            // Lay out the views.
            NSLayoutConstraint.ActivateConstraints(new[]
            {
                container.LeadingAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.LeadingAnchor),
                container.TrailingAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.TrailingAnchor),
                container.TopAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.TopAnchor),
                container.BottomAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.BottomAnchor)
            });
        }
    }
}