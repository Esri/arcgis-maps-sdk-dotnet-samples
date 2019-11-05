using ArcGISRuntime.Samples.Shared.Models;
using Foundation;
using System;
using System.IO;
using UIKit;

namespace ArcGISRuntime
{
    public class SampleInfoViewController : UIViewController
    {
        private UISegmentedControl _switcherControl;
        private UIWebView _sourceCodeView;

        private SampleInfo _info;

        private string _contentDirectoryPath;

        private string _readmeHTML = "readme";
        private string _sourceCodeHTML = "sourcecode";

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

            //string baseContent = "private async Task<byte[]> BitmapConvert(View v)\n{\nv.DrawingCacheEnabled = true;\nBitmap bitmap = Bitmap.CreateBitmap(v.DrawingCache);\nv.DrawingCacheEnabled = false;\nSystem.IO.Stream jpegStream = new System.IO.MemoryStream();\nawait bitmap.CompressAsync(Bitmap.CompressFormat.Jpeg, 100, jpegStream);\n// Store the jpeg data in a byte array.\njpegStream.Position = 0;\nbyte[] jpegArray = new byte[jpegStream.Length];\njpegStream.Read(jpegArray, 0, jpegArray.Length);\nreturn jpegArray;\n} ";

            // Build the html.
            _sourceCodeHTML =
                htmlStart.Replace("{csspath}", syntaxHighlightCSS).Replace("{jspath}", jsPath) +
                "<code class=\"csharp\">" +
                baseContent +
                "</code>" +
                htmlEnd;

            // Build out readme html
            string readmeCSSPath = Path.Combine(NSBundle.MainBundle.BundlePath, "SyntaxHighlighting/github-markdown.css");
            string readmePath = Path.Combine(NSBundle.MainBundle.BundlePath, "Samples", _info.Category, _info.FormalName, "readme.md");
            string readmeContent = File.ReadAllText(readmePath);
            string overrideCssPath = Path.Combine(NSBundle.MainBundle.BundlePath, "SyntaxHighlighting/hide-header.css");
            readmeContent = new MarkedNet.Marked().Parse(readmeContent);

            _readmeHTML = "<!doctype html><head><base href=\"" + readmePath + "\"><link rel=\"stylesheet\" href=\"" + readmeCSSPath + "\" /><link rel=\"stylesheet\" href=\"" + overrideCssPath + "\" /></head><body class=\"markdown-body\">" + readmeContent + "</body>";

            _sourceCodeView.LoadHtmlString(_readmeHTML, new NSUrl(_contentDirectoryPath, true));
        }

        private void SegmentChanged(object sender, EventArgs e)
        {
            if (_switcherControl.SelectedSegment == 0)
            {
                _sourceCodeView.LoadHtmlString(_readmeHTML, new NSUrl(_contentDirectoryPath, true));
            }
            else
            {
                _sourceCodeView.LoadHtmlString(_sourceCodeHTML, new NSUrl(_contentDirectoryPath, true));
            }
        }

        public override void LoadView()
        {
            // Create and configure the views.
            View = new UIView { BackgroundColor = UIColor.White };

            _switcherControl = new UISegmentedControl(new string[] { "About", "Source Code" }) { SelectedSegment = 0 };
            _switcherControl.TranslatesAutoresizingMaskIntoConstraints = false;
            _switcherControl.ValueChanged += SegmentChanged;

            _sourceCodeView = new UIWebView();
            _sourceCodeView.TranslatesAutoresizingMaskIntoConstraints = false;

            // Add the views.
            View.AddSubviews(_switcherControl, _sourceCodeView);

            // Lay out the views.
            NSLayoutConstraint.ActivateConstraints(new[]
            {
                _switcherControl.TopAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.TopAnchor),
                _switcherControl.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor),
                _switcherControl.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor),
                _switcherControl.BottomAnchor.ConstraintEqualTo(_sourceCodeView.TopAnchor),

                 _sourceCodeView.TopAnchor.ConstraintEqualTo(_switcherControl.BottomAnchor),
                 _sourceCodeView.LeadingAnchor.ConstraintEqualTo(View.LeadingAnchor),
                 _sourceCodeView.TrailingAnchor.ConstraintEqualTo(View.TrailingAnchor),
                 _sourceCodeView.BottomAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.BottomAnchor),
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