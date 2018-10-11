using System;
using Esri.ArcGISRuntime.Data;
using UIKit;
using System.Linq;
using Esri.ArcGISRuntime.UI;

namespace ArcGISRuntime.Samples.ReadShapefileMetadata
{
    public partial class MetadataDisplayViewController : UIViewController
    {
        private UIImageView _imageView;
        private UIStackView _stackLayout;

        // Hold a reference to the shapefile metadata.
        private ShapefileInfo _metadata;

        public MetadataDisplayViewController(ShapefileInfo metadata) : base("MetadataDisplayViewController", null)
        {
            _metadata = metadata;
        }

        public override void LoadView()
        {
            View = new UIView
            {
                BackgroundColor = UIColor.White
            };

            _imageView = new UIImageView();
            _imageView.TranslatesAutoresizingMaskIntoConstraints = false;
            _imageView.ContentMode = UIViewContentMode.ScaleAspectFit;

            _stackLayout = new UIStackView(new UIView[] {
                _imageView,
                getContentLabel(_metadata.Summary),
                getHeaderLabel("Description"),
                getContentLabel(_metadata.Description),
                getHeaderLabel("Credits"),
                getContentLabel(_metadata.Credits),
                getHeaderLabel("Tags"),
                getContentLabel(string.Join(", ", _metadata.Tags)),
                new UIView()});
            _stackLayout.TranslatesAutoresizingMaskIntoConstraints = false;
            _stackLayout.Axis = UILayoutConstraintAxis.Vertical;
            _stackLayout.Spacing = 8;

            View.AddSubview(_stackLayout);
            NSLayoutConstraint.ActivateConstraints(new NSLayoutConstraint[]
            {
                _stackLayout.TopAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.TopAnchor),
                _stackLayout.BottomAnchor.ConstraintEqualTo(View.BottomAnchor),
                _stackLayout.LeftAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.LeftAnchor, 8),
                _stackLayout.RightAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.RightAnchor, -8)
            });

            loadImage();
        }

        private async void loadImage()
        {
            _imageView.Image = await _metadata.Thumbnail.ToImageSourceAsync();
        }

        UILabel getHeaderLabel(string text)
        {
            var label = new UILabel();
            label.Text = text;
            label.TextAlignment = UITextAlignment.Center;
            label.Font = UIFont.BoldSystemFontOfSize(14);
            label.TranslatesAutoresizingMaskIntoConstraints = false;
            return label;
        }

        UILabel getContentLabel(string content)
        {
            var label = new UILabel();
            label.Text = content;
            label.LineBreakMode = UILineBreakMode.WordWrap;
            label.TranslatesAutoresizingMaskIntoConstraints = false;
            label.Lines = 0;
            return label;
        }

        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            // Perform any additional setup after loading the view, typically from a nib.
        }
    }
}

