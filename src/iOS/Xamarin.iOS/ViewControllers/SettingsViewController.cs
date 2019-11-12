using Esri.ArcGISRuntime;
using Foundation;
using System;
using System.Diagnostics;
using System.IO;
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
        private UILabel _downloadLabel;
        private UIBarButtonItem _downloadAllButton;
        private UIBarButtonItem _deleteAllButton;
        private UITableView _downloadTable;

        private UISegmentedControl _switcher;

        // Directory for loading HTML locally.
        private string _contentDirectoryPath = Path.Combine(NSBundle.MainBundle.BundlePath, "Content/");

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            Initialize();
        }

        private void Initialize()
        {
            NavigationItem.TitleView = _switcher;
            _switcher.ValueChanged += TabChanged;

            var runtimeTypeInfo = typeof(ArcGISRuntimeEnvironment).GetTypeInfo();
            var rtVersionString = FileVersionInfo.GetVersionInfo(runtimeTypeInfo.Assembly.Location).FileVersion;
            string aboutPath = Path.Combine(NSBundle.MainBundle.BundlePath, "about.md");
            string aboutContent = File.ReadAllText(aboutPath) + rtVersionString;
            string aboutHTML = MarkdownToHTML(aboutContent);

            _aboutView.LoadHtmlString(aboutHTML, new NSUrl(_contentDirectoryPath, true));

            string licensePath = Path.Combine(NSBundle.MainBundle.BundlePath, "licenses.md");
            string licenseContent = File.ReadAllText(licensePath);
            string licenseHTML = MarkdownToHTML(licenseContent);

            _licensesView.LoadHtmlString(licenseHTML, new NSUrl(_contentDirectoryPath, true));

        }

        private string MarkdownToHTML(string rawMarkdown)
        {
            string markdownCSSPath = Path.Combine(NSBundle.MainBundle.BundlePath, "SyntaxHighlighting/github-markdown.css");
            string parsedMarkdown = new MarkedNet.Marked().Parse(rawMarkdown);

            string markdowntHTML = "<!doctype html>" +
                "<head>" +
                "<link rel=\"stylesheet\" href=\"" +
                markdownCSSPath +
                "\" />" +
                "<meta name=\"viewport\" content=\"width=" +
                UIScreen.MainScreen.Bounds.Width.ToString() +
                ", shrink-to-fit=YES\">" +
                "</head>" +
                "<body class=\"markdown-body\">" +
                parsedMarkdown +
                "</body>";

            return markdowntHTML;
        }

        private void DeleteAll(object sender, EventArgs e)
        {
            
        }

        private void DownloadAll(object sender, EventArgs e)
        {
            
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

            // Used to switch between the different views.
            _switcher = new UISegmentedControl(new string[] { "About", "Licenses", "Offline Data" }) { SelectedSegment = 0 };

            // Displays the about.md in a web view.
            _aboutView = new WKWebView(new CoreGraphics.CGRect(), new WKWebViewConfiguration());
            _aboutView.TranslatesAutoresizingMaskIntoConstraints = false;
            _aboutView.NavigationDelegate = new BrowserLinksNavigationDelegate();

            // Displays the licenses.md in a web view.
            _licensesView = new WKWebView(new CoreGraphics.CGRect(), new WKWebViewConfiguration()) { Hidden = true };
            _licensesView.TranslatesAutoresizingMaskIntoConstraints = false;
            _licensesView.NavigationDelegate = new BrowserLinksNavigationDelegate();

            // View for managing offline data for samples.
            _downloadView = new UIView { BackgroundColor = UIColor.White, Hidden = true };
            _downloadView.TranslatesAutoresizingMaskIntoConstraints = false;

            _downloadLabel = new UILabel() { Text = "Download" };
            _downloadLabel.TranslatesAutoresizingMaskIntoConstraints = false;

            _downloadAllButton = new UIBarButtonItem() { Title = "Download all"};
            _deleteAllButton = new UIBarButtonItem() { Title = "Delete all" };
            var toolbar = new UIToolbar();
            toolbar.TranslatesAutoresizingMaskIntoConstraints = false;
            toolbar.Items = new[]
            {
                _downloadAllButton,
                new UIBarButtonItem(UIBarButtonSystemItem.FlexibleSpace),
                _deleteAllButton,
            };

            _downloadTable = new UITableView();
            _downloadTable.TranslatesAutoresizingMaskIntoConstraints = false;

            _downloadView.AddSubviews(_downloadLabel, toolbar, _downloadTable);

            // Add sub views to main view.
            View.AddSubviews(_aboutView, _licensesView, _downloadView);

            NSLayoutConstraint.ActivateConstraints(new[]
            {
                 _aboutView.TopAnchor.ConstraintEqualTo(View.TopAnchor),
                 _aboutView.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor),
                 _aboutView.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor),
                 _aboutView.BottomAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.BottomAnchor),

                 _licensesView.TopAnchor.ConstraintEqualTo(View.TopAnchor),
                 _licensesView.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor),
                 _licensesView.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor),
                 _licensesView.BottomAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.BottomAnchor),

                 _downloadView.TopAnchor.ConstraintEqualTo(View.TopAnchor),
                 _downloadView.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor),
                 _downloadView.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor),
                 _downloadView.BottomAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.BottomAnchor),

                 _downloadLabel.TopAnchor.ConstraintEqualTo(_downloadView.TopAnchor),
                 _downloadLabel.LeadingAnchor.ConstraintEqualTo(_downloadView.LeadingAnchor),
                 _downloadLabel.TrailingAnchor.ConstraintEqualTo(_downloadView.TrailingAnchor),
                 _downloadLabel.BottomAnchor.ConstraintEqualTo(toolbar.TopAnchor),

                 toolbar.TopAnchor.ConstraintEqualTo(_downloadLabel.BottomAnchor),
                 toolbar.LeadingAnchor.ConstraintEqualTo(_downloadView.LeadingAnchor),
                 toolbar.TrailingAnchor.ConstraintEqualTo(_downloadView.TrailingAnchor),
                 toolbar.BottomAnchor.ConstraintEqualTo(_downloadTable.TopAnchor),

                 _downloadTable.TopAnchor.ConstraintEqualTo(toolbar.BottomAnchor),
                 _downloadTable.LeadingAnchor.ConstraintEqualTo(_downloadView.LeadingAnchor),
                 _downloadTable.TrailingAnchor.ConstraintEqualTo(_downloadView.TrailingAnchor),
                 _downloadTable.BottomAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.BottomAnchor),
            });
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            // Subscribe to events.
            _downloadAllButton.Clicked += DownloadAll;
            _deleteAllButton.Clicked += DeleteAll;
        }

        

        public override void ViewDidDisappear(bool animated)
        {
            base.ViewDidDisappear(animated);

            // Unsubscribe from events, per best practice.
            _downloadAllButton.Clicked -= DownloadAll;
            _deleteAllButton.Clicked -= DeleteAll;
        }
    }
}