using ArcGISRuntime.Samples.Shared.Models;
using Foundation;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using UIKit;

namespace ArcGISRuntime
{
    public class SampleInfoViewController : UIViewController
    {
        // SampleInfo for the sample provided by the constructor.
        private SampleInfo _info;

        // Control for switching between readme and source code viewing.
        private UISegmentedControl _switcherControl;

        // Web view for viewing readme markdown.
        private UIWebView _readmeView;

        // Controls for the source code viewer.
        private UIView _codeView;
        private UIWebView _codeWebView;
        private UIBarButtonItem _codeButton;

        // Dictionary where keys are filenames and values are HTML of source code.
        private Dictionary<string, string> _sourceCodeFiles = new Dictionary<string, string>();

        // Directory for loading HTML locally.
        private string _contentDirectoryPath = Path.Combine(NSBundle.MainBundle.BundlePath, "Content/");

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
            // Build out readme html
            try
            {
                string readmePath = Path.Combine(NSBundle.MainBundle.BundlePath, "Samples", _info.Category, _info.FormalName, "readme.md");
                string readmeCSSPath = Path.Combine(NSBundle.MainBundle.BundlePath, "SyntaxHighlighting/github-markdown.css");
                string overrideCssPath = Path.Combine(NSBundle.MainBundle.BundlePath, "SyntaxHighlighting/hide-header.css");
                string readmeContent = new MarkedNet.Marked().Parse(File.ReadAllText(readmePath));

                string readmeHTML = "<!doctype html><head><base href=\"" +
                    readmePath +
                    "\"><link rel=\"stylesheet\" href=\"" +
                    readmeCSSPath +
                    "\" /><link rel=\"stylesheet\" href=\"" +
                    overrideCssPath +
                    "\" /></head><body class=\"markdown-body\">" +
                    readmeContent +
                    "</body>";

                // Load the readme into the webview.
                _readmeView.LoadHtmlString(readmeHTML, new NSUrl(_contentDirectoryPath, true));
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                _readmeView.LoadHtmlString("Could not load information.", new NSUrl(_contentDirectoryPath, true));
            }

            // Build out source code html.
            try
            {
                // Get the paths for the .css and .js files for syntax highlighting.
                string syntaxHighlightCSS = Path.Combine(NSBundle.MainBundle.BundlePath, "SyntaxHighlighting/highlight.css");
                string jsPath = Path.Combine(NSBundle.MainBundle.BundlePath, "SyntaxHighlighting/highlight.pack.js");

                // Start and end HTML tags.
                const string _sourceHTMLStart =
                            "<html>" +
                            "<head>" +
                            "<link rel=\"stylesheet\" href=\"{csspath}\">" +
                            "<script type=\"text/javascript\" src=\"{jspath}\"></script>" +
                            "<script>hljs.initHighlightingOnLoad();</script>" +
                            "</head>" +
                            "<body>" +
                            "<pre>";

                const string _sourceHTMLEnd =
                           "</pre>" +
                           "</body>" +
                           "</html>";

                string sourceFilesPath = Path.Combine(NSBundle.MainBundle.BundlePath, "Samples", _info.Category, _info.FormalName);

                // Loop over every source code file in the sample directory.
                foreach (string sourceCodePath in Directory.GetFiles(sourceFilesPath, "*.cs"))
                {
                    // Get the code as a string.
                    string baseContent = File.ReadAllText(sourceCodePath);

                    // Build the html.
                    string sourceCodeHTML =
                        _sourceHTMLStart.Replace("{csspath}", syntaxHighlightCSS).Replace("{jspath}", jsPath) +
                        "<code class=\"csharp\">" +
                        baseContent +
                        "</code>" +
                        _sourceHTMLEnd;

                    // Get the filename without the full path.
                    string shortPath = sourceCodePath.Split('/').Last();

                    // Add the html to the source code files dictionary.
                    _sourceCodeFiles.Add(shortPath, sourceCodeHTML);
                }

                // Load the first source code file into the web view.
                string firstKey = _sourceCodeFiles.Keys.First();
                _codeWebView.LoadHtmlString(_sourceCodeFiles[firstKey], new NSUrl(_contentDirectoryPath, true));
                _codeButton.Title = firstKey;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                _codeWebView.LoadHtmlString("Could not load source code.", new NSUrl(_contentDirectoryPath, true));
                _codeButton.Enabled = false;
            }
        }

        private void SegmentChanged(object sender, EventArgs e)
        {
            if (_switcherControl.SelectedSegment == 0)
            {
                // If about section.
                _codeView.Hidden = true;
                _readmeView.Hidden = false;
            }
            else
            {
                // If code section.
                _readmeView.Hidden = true;
                _codeView.Hidden = false;
            }
        }

        private void SourceCodeButtonPressed(object sender, EventArgs e)
        {
            // Build a UI alert controller for picking the source code file.
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
            _codeWebView.LoadHtmlString(_sourceCodeFiles[obj.Title], new NSUrl(_contentDirectoryPath, true));
            _codeButton.Title = obj.Title;
        }

        public override void LoadView()
        {
            // Create and configure the views.
            View = new UIView { BackgroundColor = UIColor.SystemBackgroundColor };

            // Segmented control at the top of the page.
            _switcherControl = new UISegmentedControl(new string[] { "About", "Source Code" }) { SelectedSegment = 0 };
            _switcherControl.TranslatesAutoresizingMaskIntoConstraints = false;

            // Web view for the readme.
            _readmeView = new UIWebView();
            _readmeView.TranslatesAutoresizingMaskIntoConstraints = false;

            // View for source code files.
            _codeView = new UIView { BackgroundColor = UIColor.SystemBackgroundColor, Hidden = true };
            _codeView.TranslatesAutoresizingMaskIntoConstraints = false;

            // Web view of the source code html.
            _codeWebView = new UIWebView();
            _codeWebView.TranslatesAutoresizingMaskIntoConstraints = false;

            // Button for bringing up alertcontroller to switch between source code files.
            var toolbar = new UIToolbar();
            toolbar.TranslatesAutoresizingMaskIntoConstraints = false;
            _codeButton = new UIBarButtonItem("SourceCode.cs", UIBarButtonItemStyle.Plain, SourceCodeButtonPressed);
            toolbar.Items = new UIBarButtonItem[] { _codeButton };

            // Add sub views to code view.
            _codeView.AddSubviews(_codeWebView, toolbar);

            // Add sub views to main view.
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
                 _codeWebView.BottomAnchor.ConstraintEqualTo(toolbar.TopAnchor),

                 toolbar.TopAnchor.ConstraintEqualTo(_codeWebView.BottomAnchor),
                 toolbar.LeadingAnchor.ConstraintEqualTo(_codeView.LeadingAnchor),
                 toolbar.TrailingAnchor.ConstraintEqualTo(_codeView.TrailingAnchor),
                 toolbar.BottomAnchor.ConstraintEqualTo(_codeView.BottomAnchor),
            });
        }

        public override void ViewWillAppear(bool animated)
        {
            base.ViewWillAppear(animated);

            // Subscribe to events.
            _switcherControl.ValueChanged += SegmentChanged;
        }

        public override void ViewDidDisappear(bool animated)
        {
            base.ViewDidDisappear(animated);

            // Unsubscribe from events, per best practice.
            _switcherControl.ValueChanged -= SegmentChanged;
        }
    }
}