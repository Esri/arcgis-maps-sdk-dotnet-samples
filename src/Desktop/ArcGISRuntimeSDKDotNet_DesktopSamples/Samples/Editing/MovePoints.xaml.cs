using Esri.ArcGISRuntime.Controls;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Layers;
using System;
using System.Windows.Controls;

namespace ArcGISRuntimeSDKDotNet_DesktopSamples.Samples
{
    /// <summary>
    /// Demonstrates one method of moving graphic points on the map.
    /// </summary>
    /// <title>Move Points</title>
    /// <category>Editing</category>
    public partial class MovePoints : UserControl
    {
        public MovePoints()
        {
			InitializeComponent();
			LoadData();
        }

		private void LoadData()
		{
			//Add some random points for editing
			Random r = new Random();
			var graphicsLayer = mapView1.Map.Layers["MyGraphicsLayer"] as GraphicsLayer;
			for (int i = 0; i < 20; i++)
			{
				graphicsLayer.Graphics.Add(
					new Graphic(new MapPoint(r.NextDouble() * 360, r.NextDouble() * 180, SpatialReferences.Wgs84)));
			}
		}

		private Graphic graphicBeingEdited;

		private async void mapView1_MapViewTapped(object sender, Esri.ArcGISRuntime.Controls.MapViewInputEventArgs e)
		{
			var graphicsLayer = mapView1.Map.Layers["MyGraphicsLayer"] as GraphicsLayer;
			var editGraphicsLayer = mapView1.Map.Layers["EditGraphicsLayer"] as GraphicsLayer;
			if (graphicBeingEdited == null)
			{
				var hit = await graphicsLayer.HitTestAsync(sender as Esri.ArcGISRuntime.Controls.ViewBase, e.Position);
				if (hit != null)
				{
					graphicBeingEdited = hit;
					//highlight the active graphic
					graphicBeingEdited.IsSelected = true;
					//Create a temporary we can move around without 'disturbing' the original feature until commit
					Graphic g = new Graphic();
					g.Symbol = hit.Symbol ?? graphicsLayer.Renderer.GetSymbol(hit);
					g.Geometry = hit.Geometry;
					editGraphicsLayer.Graphics.Add(g);
				}
			}
			else //Commit and clean up
			{
				graphicBeingEdited.Geometry = e.Location;
				graphicBeingEdited.IsSelected = false;
				graphicBeingEdited = null;
				editGraphicsLayer.Graphics.Clear();
			}
		}

		private void mapView1_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
		{
			if (graphicBeingEdited != null)
			{
				var editGraphicsLayer = mapView1.Map.Layers["EditGraphicsLayer"] as GraphicsLayer;
				MapView mapview = (MapView)sender;
				var location = mapview.ScreenToLocation(e.GetPosition(mapview));
				editGraphicsLayer.Graphics[0].Geometry = location;
			}
		}
    }
}
