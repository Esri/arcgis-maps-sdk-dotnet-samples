using CoreGraphics;
using System;
using UIKit;

namespace ArcGISRuntime
{
    public class LoadingOverlay : UIView
    {
        private UIActivityIndicatorView activitySpinner;
        private UILabel loadingLabel;

        public LoadingOverlay(CGRect frame) : base(frame)
        {
            this.BackgroundColor = UIColor.Black;
            this.Alpha = 0.8f;
            this.AutoresizingMask = UIViewAutoresizing.All;

            nfloat centerX = Frame.Width / 2;
            nfloat centerY = Frame.Height / 2;

            activitySpinner = new UIActivityIndicatorView(UIActivityIndicatorViewStyle.WhiteLarge);
            activitySpinner.Frame = new CGRect(
                centerX - (activitySpinner.Frame.Width / 2),
                centerY - activitySpinner.Frame.Height - 20,
                activitySpinner.Frame.Width,
                activitySpinner.Frame.Height);
            activitySpinner.AutoresizingMask = UIViewAutoresizing.All;
            this.AddSubview(activitySpinner);
            activitySpinner.StartAnimating();

            loadingLabel = new UILabel(new CGRect(
                centerX - ((Frame.Width - 20)/ 2),
                centerY + 20,
                Frame.Width - 20,
                22
            ));
            loadingLabel.BackgroundColor = UIColor.Clear;
            loadingLabel.TextColor = UIColor.White;
            loadingLabel.Text = "Downloading Data";
            loadingLabel.TextAlignment = UITextAlignment.Center;
            loadingLabel.AutoresizingMask = UIViewAutoresizing.All;
            this.AddSubview(loadingLabel);
        }

        public void Hide()
        {
            UIView.Animate(
                0.5,
                () => { Alpha = 0; },
                () => { RemoveFromSuperview(); }
            );
        }
    }
}