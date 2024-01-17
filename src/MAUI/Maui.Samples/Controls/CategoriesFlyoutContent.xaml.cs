using ArcGIS.ViewModels;
using System.Diagnostics;

namespace ArcGIS.Controls;

public partial class CategoriesFlyoutContent : ContentView
{
	private FlyoutMenuViewModel _viewModel;

	public CategoriesFlyoutContent()
	{
		InitializeComponent();

		_viewModel = new FlyoutMenuViewModel();

		BindingContext = _viewModel;
    }

    private void CategoriesCollectionView_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
		try
		{
            var selectedCategory = e.CurrentSelection.FirstOrDefault() as FlyoutCategoryViewModel;
			_viewModel.CategorySelected(selectedCategory.CategoryName);

        }
		catch (Exception ex)
		{
			Debug.Write(ex.ToString());
		}
    }
}