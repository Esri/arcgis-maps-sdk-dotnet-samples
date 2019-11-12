using Esri.ArcGISRuntime;
using System;
using System.Diagnostics;
using System.Reflection;
using UIKit;
using WebKit;

namespace ArcGISRuntime
{
    public class SettingsViewController : UIViewController
    {
        private WKWebView _aboutView;
        private WKWebView _licensesView;
        private UIView _downloadView;

        private UISegmentedControl _switcher;

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            Initialize();
        }

        private void Initialize()
        {
            NavigationItem.TitleView = _switcher;
            _switcher.ValueChanged += TabChanged;
        }

        private void TabChanged(object sender, EventArgs e)
        {
            switch (_switcher.SelectedSegment)
            {
                case 0:
                    _aboutView.Hidden = false;
                    _licensesView.Hidden = _downloadView.Hidden = true;
                    break;

                case 1:
                    _licensesView.Hidden = false;
                    _aboutView.Hidden = _downloadView.Hidden = true;
                    break;

                case 2:
                    _downloadView.Hidden = false;
                    _aboutView.Hidden = _licensesView.Hidden = true;
                    break;
            }
        }

        public override void LoadView()
        {
            // Create and configure the views.
            View = new UIView { BackgroundColor = UIColor.White };

            _switcher = new UISegmentedControl(new string[] { "About", "Licenses", "Offline Data" }) { SelectedSegment = 0 };

            _aboutView = new WKWebView(new CoreGraphics.CGRect(), new WKWebViewConfiguration());
            _aboutView.TranslatesAutoresizingMaskIntoConstraints = false;
            _aboutView.NavigationDelegate = new BrowserLinksNavigationDelegate();

            UILabel versionLabel = new UILabel();
            var runtimeTypeInfo = typeof(ArcGISRuntimeEnvironment).GetTypeInfo();
            var rtVersionString = FileVersionInfo.GetVersionInfo(runtimeTypeInfo.Assembly.Location).FileVersion;

            _aboutView.AddSubview(versionLabel);

            _licensesView = new WKWebView(new CoreGraphics.CGRect(), new WKWebViewConfiguration()) { Hidden = true };
            _licensesView.TranslatesAutoresizingMaskIntoConstraints = false;
            _licensesView.NavigationDelegate = new BrowserLinksNavigationDelegate();


            _downloadView = new UIView { BackgroundColor = UIColor.White, Hidden = true };
            _downloadView.TranslatesAutoresizingMaskIntoConstraints = false;

            // Add sub views to main view.
            View.AddSubviews(_switcher, _aboutView, _licensesView, _downloadView);

            NSLayoutConstraint.ActivateConstraints(new[]
            {
                 _aboutView.TopAnchor.ConstraintEqualTo(_switcher.TopAnchor),
                 _aboutView.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor),
                 _aboutView.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor),
                 _aboutView.BottomAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.BottomAnchor),

                 _licensesView.TopAnchor.ConstraintEqualTo(_switcher.TopAnchor),
                 _licensesView.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor),
                 _licensesView.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor),
                 _licensesView.BottomAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.BottomAnchor),

                 _downloadView.TopAnchor.ConstraintEqualTo(_switcher.TopAnchor),
                 _downloadView.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor),
                 _downloadView.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor),
                 _downloadView.BottomAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.BottomAnchor),
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
    public class BrowserLinksNavigationDelegate : WKNavigationDelegate
    {
        public override void DecidePolicy(WKWebView webView, WKNavigationAction navigationAction, Action<WKNavigationActionPolicy> decisionHandler)
        {
            if (navigationAction.NavigationType == WKNavigationType.LinkActivated)
            {
                // Open links in browser.
                if (UIApplication.SharedApplication.CanOpenUrl(navigationAction.Request.Url))
                {
                    UIApplication.SharedApplication.OpenUrl(navigationAction.Request.Url);
                }
                decisionHandler(WKNavigationActionPolicy.Cancel);
            }
            else
            {
                decisionHandler(WKNavigationActionPolicy.Allow);
            }
        }
    }
}