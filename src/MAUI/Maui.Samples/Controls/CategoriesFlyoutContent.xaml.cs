using ArcGIS.ViewModels;

namespace ArcGIS.Controls;

public partial class CategoriesFlyoutContent : ContentView
{
	public CategoriesFlyoutContent()
	{
		InitializeComponent();

		try
		{
            BindingContext = new FlyoutMenuViewModel();

        }
        catch (Exception ex)
		{

			throw;
		}
	}
}