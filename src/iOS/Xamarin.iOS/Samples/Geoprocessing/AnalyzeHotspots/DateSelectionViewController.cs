// Copyright 2018 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific 
// language governing permissions and limitations under the License.

using System;
using Foundation;
using UIKit;

namespace ArcGISRuntime.Samples.AnalyzeHotspots
{
    public class DateSelectionViewController : UIViewController
    {
        // Hold references to UI controls.
        public readonly UIDatePicker StartPicker;
        public readonly UIDatePicker EndPicker;
        private UIStackView _outerStackView;

        public DateSelectionViewController()
        {
            Title = "Select a date range";

            // Configured here because these need to be initialized before the view is loaded/presented.
            StartPicker = new UIDatePicker();
            StartPicker.SetDate((NSDate) new DateTime(1998, 1, 1, 0, 0, 0, DateTimeKind.Local), false);
            EndPicker = new UIDatePicker();
            EndPicker.SetDate((NSDate) new DateTime(1998, 1, 31, 0, 0, 0, DateTimeKind.Local), false);
        }

        public override void LoadView()
        {
            // Create the views.
            View = new UIView {BackgroundColor = ApplicationTheme.BackgroundColor};

            _outerStackView = new UIStackView();
            _outerStackView.Axis = UILayoutConstraintAxis.Vertical;
            _outerStackView.TranslatesAutoresizingMaskIntoConstraints = false;
            _outerStackView.Spacing = 8;
            _outerStackView.Alignment = UIStackViewAlignment.Top;

            UILabel startLabel = new UILabel();
            startLabel.TranslatesAutoresizingMaskIntoConstraints = false;
            startLabel.Text = "Start date:";

            StartPicker.TranslatesAutoresizingMaskIntoConstraints = false;
            StartPicker.Mode = UIDatePickerMode.Date;

            UIStackView startStack = new UIStackView(new UIView[] {startLabel, StartPicker});
            startStack.TranslatesAutoresizingMaskIntoConstraints = false;
            startStack.Axis = UILayoutConstraintAxis.Vertical;
            _outerStackView.AddArrangedSubview(startStack);

            UILabel endLabel = new UILabel();
            endLabel.TranslatesAutoresizingMaskIntoConstraints = false;
            endLabel.Text = "End date:";

            EndPicker.TranslatesAutoresizingMaskIntoConstraints = false;
            EndPicker.Mode = UIDatePickerMode.Date;

            UIStackView endStack = new UIStackView(new UIView[] {endLabel, EndPicker});
            endStack.TranslatesAutoresizingMaskIntoConstraints = false;
            endStack.Axis = UILayoutConstraintAxis.Vertical;
            _outerStackView.AddArrangedSubview(endStack);

            UIView spacer = new UIView();
            spacer.TranslatesAutoresizingMaskIntoConstraints = false;
            spacer.SetContentHuggingPriority((float) UILayoutPriority.DefaultLow, UILayoutConstraintAxis.Vertical);
            spacer.SetContentHuggingPriority((float) UILayoutPriority.DefaultLow, UILayoutConstraintAxis.Horizontal);
            _outerStackView.AddArrangedSubview(spacer);

            // Add the views.
            View.AddSubview(_outerStackView);

            // Lay out the views.
            NSLayoutConstraint.ActivateConstraints(new[]
            {
                _outerStackView.LeadingAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.LeadingAnchor, 8),
                _outerStackView.TrailingAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.TrailingAnchor, -8),
                _outerStackView.TopAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.TopAnchor, 8),
                _outerStackView.BottomAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.BottomAnchor, -8)
            });
        }

        public override void TraitCollectionDidChange(UITraitCollection previousTraitCollection)
        {
            base.TraitCollectionDidChange(previousTraitCollection);

            if (View.TraitCollection.VerticalSizeClass == UIUserInterfaceSizeClass.Compact)
            {
                _outerStackView.Axis = UILayoutConstraintAxis.Horizontal;
            }
            else
            {
                _outerStackView.Axis = UILayoutConstraintAxis.Vertical;
            }
        }
    }
}