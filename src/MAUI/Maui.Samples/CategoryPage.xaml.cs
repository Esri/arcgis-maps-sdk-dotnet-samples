using ArcGIS.Helpers;
using ArcGIS.Samples.Shared.Models;

namespace ArcGIS;

public partial class CategoryPage : ContentPage
{
	private SearchableTreeNode _category;

	public CategoryPage(SearchableTreeNode category)
	{
		_category = category;

        InitializeComponent();

        Initialize();
	}

    private void Initialize()
    {
        PageTitle.Text = _category.Name;

        // Get the samples from the category.
        var listSampleItems = _category?.Items.OfType<SampleInfo>().ToList();

        // Update the binding to show the samples.
        BindingContext = listSampleItems;
    }
    private void TapGestureRecognizer_SampleTapped(object sender, TappedEventArgs e)
    {
        var sampleInfo = e.Parameter as SampleInfo;
        _ = SampleLoader.LoadSample(sampleInfo, this);
    }
}