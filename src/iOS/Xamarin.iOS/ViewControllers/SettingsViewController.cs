using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using Esri.ArcGISRuntime;
using Foundation;
using UIKit;

namespace ArcGISRuntime
{
    public class SettingsViewController : UIViewController
    {

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            Initialize();
        }

        private void Initialize()
        {
            NavigationItem.Title = "Settings";
        }

        public override void LoadView()
        {
            // Create and configure the views.
            View = new UIView { BackgroundColor = UIColor.White };

            UILabel label = new UILabel();
            var runtimeTypeInfo = typeof(ArcGISRuntimeEnvironment).GetTypeInfo();
            var rtVersion = FileVersionInfo.GetVersionInfo(runtimeTypeInfo.Assembly.Location);
            label.Text = rtVersion.FileVersion;

            // Add sub views to main view.
            View.AddSubviews(label);

            NSLayoutConstraint.ActivateConstraints(new[]
            {
                 label.TopAnchor.ConstraintEqualTo(View.TopAnchor),
                 label.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor),
                 label.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor),
                 label.BottomAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.BottomAnchor),
            });
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            // Subscribe to events.

        }

        public override void ViewDidDisappear(bool animated)
        {
            base.ViewDidDisappear(animated);

            // Unsubscribe from events, per best practice.

        }
    }
}