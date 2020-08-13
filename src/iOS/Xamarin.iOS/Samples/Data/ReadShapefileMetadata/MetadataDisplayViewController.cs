using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.UI;
using UIKit;

namespace ArcGISRuntime.Samples.ReadShapefileMetadata
{
    public partial class MetadataDisplayViewController : UIViewController
    {
        // Hold a reference to the shapefile metadata.
        private readonly ShapefileInfo _metadata;

        public MetadataDisplayViewController(ShapefileInfo metadata) : base("MetadataDisplayViewController", null)
        {
            _metadata = metadata;
        }

        public override async void LoadView()
        {
            View = new UIView { BackgroundColor = ApplicationTheme.BackgroundColor };

            UIImageView imageView = new UIImageView();
            imageView.TranslatesAutoresizingMaskIntoConstraints = false;
            imageView.ContentMode = UIViewContentMode.ScaleAspectFit;

            UIStackView stackLayout = new UIStackView(new[]
            {
                imageView,
                getContentLabel(_metadata.Summary),
                getHeaderLabel("Description"),
                getContentLabel(_metadata.Description),
                getHeaderLabel("Credits"),
                getContentLabel(_metadata.Credits),
                getHeaderLabel("Tags"),
                getContentLabel(string.Join(", ", _metadata.Tags)),
                new UIView()
            });
            stackLayout.TranslatesAutoresizingMaskIntoConstraints = false;
            stackLayout.Axis = UILayoutConstraintAxis.Vertical;
            stackLayout.Spacing = 8;
            stackLayout.LayoutMarginsRelativeArrangement = true;
            stackLayout.LayoutMargins = new UIEdgeInsets(8, 8, 8, 8);

            UIScrollView scrollView = new UIScrollView();
            scrollView.TranslatesAutoresizingMaskIntoConstraints = false;

            // Add the views.
            View.AddSubview(scrollView);
            scrollView.AddSubview(stackLayout);

            // Lay out the views.
            NSLayoutConstraint.ActivateConstraints(new[]
            {
                scrollView.TopAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.TopAnchor),
                scrollView.BottomAnchor.ConstraintEqualTo(View.BottomAnchor),
                scrollView.LeftAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.LeftAnchor),
                scrollView.RightAnchor.ConstraintEqualTo(View.SafeAreaLayoutGuide.RightAnchor),
                stackLayout.TopAnchor.ConstraintEqualTo(scrollView.TopAnchor),
                stackLayout.BottomAnchor.ConstraintEqualTo(scrollView.BottomAnchor),
                stackLayout.LeftAnchor.ConstraintEqualTo(scrollView.LeftAnchor),
                stackLayout.RightAnchor.ConstraintEqualTo(scrollView.RightAnchor),
                // Prevent horizontal scrolling
                stackLayout.WidthAnchor.ConstraintEqualTo(scrollView.WidthAnchor)
            });

            // Load the image.
            imageView.Image = await _metadata.Thumbnail.ToImageSourceAsync();
        }

        private UILabel getHeaderLabel(string text)
        {
            var label = new UILabel();
            label.Text = text;
            label.TextAlignment = UITextAlignment.Center;
            label.Font = UIFont.BoldSystemFontOfSize(14);
            label.TranslatesAutoresizingMaskIntoConstraints = false;
            return label;
        }

        private UILabel getContentLabel(string content)
        {
            var label = new UILabel();
            label.Text = content;
            label.LineBreakMode = UILineBreakMode.WordWrap;
            label.TranslatesAutoresizingMaskIntoConstraints = false;
            label.Lines = 0;
            return label;
        }
    }
}