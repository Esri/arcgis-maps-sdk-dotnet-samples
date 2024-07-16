namespace ArcGIS.Views;
using ArcGIS.Samples.Managers;
using ArcGIS.ViewModels;
using CommunityToolkit.Mvvm.Messaging;

public partial class DesktopCategoryView : ContentView
{
	public DesktopCategoryView()
	{
		InitializeComponent();

        WeakReferenceMessenger.Default.Register<string>(this, async (message, category) => await ScrollToTop());
    }

    private void PointerGestureRecognizer_PointerEntered(object sender, PointerEventArgs e)
    {
        var view = (Border)sender;

        var grid = (Grid)view.Content;

        var imageButton = (Button)grid.Children[1];

        imageButton.IsVisible = true;

        Console.WriteLine("PointerRecognized");
    }

    private void PointerGestureRecognizer_PointerExited(object sender, PointerEventArgs e)
    {
        var view = (Border)sender;

        var grid = (Grid)view.Content;

        var imageButton = (Button)grid.Children[1];

        string sampleName = (string)imageButton.CommandParameter;

        imageButton.IsVisible = false || SampleManager.Current.IsSampleFavorited(sampleName);
    }

    private async Task ScrollToTop()
    {
        await SamplesScrollView.ScrollToAsync(0, 0, false);
    }
}