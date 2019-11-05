using ArcGISRuntime.Samples.Shared.Models;
using Foundation;
using System;
using System.Collections.Generic;
using System.IO;
using UIKit;

namespace ArcGISRuntime
{
    public class SampleInfoViewController : UIViewController
    {
        private UISegmentedControl _switcherControl;
        private UIWebView _readmeView;

        private UIView _codeView;
        private UIWebView _codeWebView;
        private UIToolbar _codePickerButtonToolbar;
        private UIBarButtonItem _codeButton;

        private SampleInfo _info;

        private string _contentDirectoryPath;

        private string _readmeHTML = "readme";
        private string _sourceCodeHTML = "sourcecode";

        private Dictionary<string, string> _sourceCodeFiles = new Dictionary<string, string>();

        // Start of each html string.
        private const string htmlStart =
            "<html>" +
            "<head>" +
            "<link rel=\"stylesheet\" href=\"{csspath}\">" +
            "<script type=\"text/javascript\" src=\"{jspath}\"></script>" +
            "<script>hljs.initHighlightingOnLoad();</script>" +
            "</head>" +
            "<body>" +
            "<pre>";

        private const string htmlEnd =
            "</pre>" +
            "</body>" +
            "</html>";

        public SampleInfoViewController(SampleInfo info)
        {
            Title = info.SampleName;
            _info = info;
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            Initialize();
        }

        private void Initialize()
        {
            _contentDirectoryPath = Path.Combine(NSBundle.MainBundle.BundlePath, "Content/");

            // Build source code html.

            string syntaxHighlightCSS = Path.Combine(NSBundle.MainBundle.BundlePath, "SyntaxHighlighting/highlight.css");
            string jsPath = Path.Combine(NSBundle.MainBundle.BundlePath, "SyntaxHighlighting/highlight.pack.js");

            var sourceCodePath = Path.Combine(NSBundle.MainBundle.BundlePath, "Samples", _info.Category, _info.FormalName, _info.FormalName + ".cs");
            string baseContent = File.ReadAllText(sourceCodePath);

            // Build the html.
            _sourceCodeHTML =
                htmlStart.Replace("{csspath}", syntaxHighlightCSS).Replace("{jspath}", jsPath) +
                "<code class=\"csharp\">" +
                baseContent +
                "</code>" +
                htmlEnd;

            _sourceCodeFiles.Add(sourceCodePath, _sourceCodeHTML);

            // Build out readme html
            string readmeCSSPath = Path.Combine(NSBundle.MainBundle.BundlePath, "SyntaxHighlighting/github-markdown.css");
            string readmePath = Path.Combine(NSBundle.MainBundle.BundlePath, "Samples", _info.Category, _info.FormalName, "readme.md");
            string readmeContent = File.ReadAllText(readmePath);
            string overrideCssPath = Path.Combine(NSBundle.MainBundle.BundlePath, "SyntaxHighlighting/hide-header.css");
            readmeContent = new MarkedNet.Marked().Parse(readmeContent);

            _readmeHTML = "<!doctype html><head><base href=\"" + readmePath + "\"><link rel=\"stylesheet\" href=\"" + readmeCSSPath + "\" /><link rel=\"stylesheet\" href=\"" + overrideCssPath + "\" /></head><body class=\"markdown-body\">" + readmeContent + "</body>";

            _readmeView.LoadHtmlString(_readmeHTML, new NSUrl(_contentDirectoryPath, true));

            _codeWebView.LoadHtmlString(_sourceCodeHTML, new NSUrl(_contentDirectoryPath, true));
        }

        private void SegmentChanged(object sender, EventArgs e)
        {
            if (_switcherControl.SelectedSegment == 0)
            {
                _codeView.Hidden = true;
                _readmeView.Hidden = false;
            }
            else
            {
                _readmeView.Hidden = true;
                _codeView.Hidden = false;
            }
        }

        private void SourceCodeButtonPressed(object sender, EventArgs e)
        {
            // Build a UI alert controller for picking the color.
            UIAlertController prompt = UIAlertController.Create(null, null, UIAlertControllerStyle.ActionSheet);
            foreach (string fileTitle in _sourceCodeFiles.Keys)
            {
                UIAlertAction action = UIAlertAction.Create(fileTitle, UIAlertActionStyle.Default, FileSelect);
                prompt.AddAction(action);
            }
            prompt.AddAction(UIAlertAction.Create("Cancel", UIAlertActionStyle.Cancel, null));

            // Needed to prevent crash on iPad.
            UIPopoverPresentationController ppc = prompt.PopoverPresentationController;
            if (ppc != null)
            {
                ppc.BarButtonItem = _codeButton;
                ppc.PermittedArrowDirections = UIPopoverArrowDirection.Down;
            }

            PresentViewController(prompt, true, null);
        }

        private void FileSelect(UIAlertAction obj)
        {

        }

        public override void LoadView()
        {
            // Create and configure the views.
            View = new UIView { BackgroundColor = UIColor.SystemBackgroundColor };

            _switcherControl = new UISegmentedControl(new string[] { "About", "Source Code" }) { SelectedSegment = 0 };
            _switcherControl.TranslatesAutoresizingMaskIntoConstraints = false;
            _switcherControl.ValueChanged += SegmentChanged;

            _readmeView = new UIWebView();
            _readmeView.TranslatesAutoresizingMaskIntoConstraints = false;

            _codeView = new UIView { BackgroundColor = UIColor.SystemBackgroundColor, Hidden = true };
            _codeView.TranslatesAutoresizingMaskIntoConstraints = false;

            _codeWebView = new UIWebView();
            _codeWebView.TranslatesAutoresizingMaskIntoConstraints = false;

            _codePickerButtonToolbar = new UIToolbar();
            _codePickerButtonToolbar.TranslatesAutoresizingMaskIntoConstraints = false;

            _codeButton = new UIBarButtonItem("SourceCode.cs", UIBarButtonItemStyle.Plain, SourceCodeButtonPressed);
            _codePickerButtonToolbar.Items = new UIBarButtonItem[]{ _codeButton };

            _codeView.AddSubviews(_codeWebView, _codePickerButtonToolbar);

            // Add the views.
            View.AddSubviews(_switcherControl, _readmeView, _codeView);

            // Lay out the views.
            NSLayoutConstraint.ActivateConstraints(new[]
            {
                _switcherControl.TopAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.TopAnchor),
                _switcherControl.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor),
                _switcherControl.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor),
                _switcherControl.BottomAnchor.ConstraintEqualTo(_readmeView.TopAnchor),

                 _readmeView.TopAnchor.ConstraintEqualTo(_switcherControl.BottomAnchor),
                 _readmeView.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor),
                 _readmeView.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor),
                 _readmeView.BottomAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.BottomAnchor),

                 _codeView.TopAnchor.ConstraintEqualTo(_switcherControl.BottomAnchor),
                 _codeView.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor),
                 _codeView.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor),
                 _codeView.BottomAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.BottomAnchor),

                 _codeWebView.TopAnchor.ConstraintEqualTo(_codeView.TopAnchor),
                 _codeWebView.LeadingAnchor.ConstraintEqualTo(_codeView.LeadingAnchor),
                 _codeWebView.TrailingAnchor.ConstraintEqualTo(_codeView.TrailingAnchor),
                 _codeWebView.BottomAnchor.ConstraintEqualTo(_codePickerButtonToolbar.TopAnchor),

                 _codePickerButtonToolbar.TopAnchor.ConstraintEqualTo(_codeWebView.BottomAnchor),
                 _codePickerButtonToolbar.LeadingAnchor.ConstraintEqualTo(_codeView.LeadingAnchor),
                 _codePickerButtonToolbar.TrailingAnchor.ConstraintEqualTo(_codeView.TrailingAnchor),
                 _codePickerButtonToolbar.BottomAnchor.ConstraintEqualTo(_codeView.BottomAnchor),

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
            _switcherControl.ValueChanged -= SegmentChanged;
        }
    }
}