using ArcGIS.ViewModels;
using CommunityToolkit.Mvvm.Messaging;

namespace ArcGIS.Views;

public partial class MobileCategoryView : ContentView
{
	public MobileCategoryView()
	{
		InitializeComponent();

        WeakReferenceMessenger.Default.Register<string>(this, (message, category) => ScrollToTop());
    }

    private void ScrollToTop()
    {
        if ((this.BindingContext as CategoryViewModel).SamplesItems.Count > 0)
        {
            SamplesCollection.ScrollTo(0);
        }
    }
}