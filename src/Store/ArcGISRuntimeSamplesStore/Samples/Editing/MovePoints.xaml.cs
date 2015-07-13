using Esri.ArcGISRuntime.Controls;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Layers;
using System;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

namespace ArcGISRuntime.Samples.Store.Samples
{
	/// <summary>
    /// Demonstrates one method of moving graphic points on the map.
    /// </summary>
    /// <title>Move Points</title>
    /// <category>Editing</category>
	public sealed partial class MovePoints : Page
    {
		private Graphic graphicBeingEdited;

		public MovePoints()
        {
            this.InitializeComponent();
			LoadData();
        }

		private void LoadData()
		{
			//Add some random points for editing
			Random r = new Random();
			var graphicsOverlay = MyMapView.GraphicsOverlays["MyGraphicsOverlay"];
            for (int i = 0; i < 20; i++)
			{
				graphicsOverlay.Graphics.Add(
					new Graphic(new MapPoint(r.NextDouble()*360, r.NextDouble()*180, SpatialReferences.Wgs84)));
			}
		}

        private async void MyMapView_MapViewTapped(object sender, Esri.ArcGISRuntime.Controls.MapViewInputEventArgs e)
        {
			var graphicsOverlay = MyMapView.GraphicsOverlays["MyGraphicsOverlay"];
			var editGraphicsOverlay = MyMapView.GraphicsOverlays["EditGraphicsOverlay"];
			if (graphicBeingEdited == null)
			{
				var hit = await graphicsOverlay.HitTestAsync(sender as ViewBase, e.Position);
				if (hit != null)
				{
					graphicBeingEdited = hit;
					//highlight the active graphic
					graphicBeingEdited.IsSelected = true;
					//Create a temporary we can move around without 'disturbing' the original feature until commit
					Graphic g = new Graphic();
					g.Symbol = hit.Symbol ?? graphicsOverlay.Renderer.GetSymbol(hit);
					g.Geometry = hit.Geometry;
					editGraphicsOverlay.Graphics.Add(g);
				}
			}
			else //Commit and clean up
			{
				graphicBeingEdited.Geometry = e.Location;
				graphicBeingEdited.IsSelected = false;
				graphicBeingEdited = null;
				editGraphicsOverlay.Graphics.Clear();
			}
        }

		private void MyMapView_PointerMoved(object sender, PointerRoutedEventArgs e)
		{
			if (graphicBeingEdited != null)
			{
				var editGraphicsOverlay = MyMapView.GraphicsOverlays["EditGraphicsOverlay"];
				MapView mapview = (MapView)sender;
				var location = mapview.ScreenToLocation(e.GetCurrentPoint(mapview).Position);
				editGraphicsOverlay.Graphics[0].Geometry = location;
			}
		}
    }
}
