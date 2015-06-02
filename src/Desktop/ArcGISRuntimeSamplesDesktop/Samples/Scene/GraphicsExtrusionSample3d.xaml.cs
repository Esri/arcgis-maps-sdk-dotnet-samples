using Esri.ArcGISRuntime.Controls;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Layers;
using Esri.ArcGISRuntime.Symbology;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace ArcGISRuntime.Samples.Desktop
{
	/// <summary>
	/// Shows how to use extrusion which means stretching a flat 2D shape vertically to create a 3D object.
	/// </summary>
	/// <title>3D Graphics extrusion</title>
	/// <category>Scene</category>
	/// <subcategory>Graphics</subcategory>
	public partial class GraphicsExtrusionSample3d : UserControl
	{
		public GraphicsExtrusionSample3d()
		{
			InitializeComponent();
			MySceneView.SpatialReferenceChanged += MySceneView_SpatialReferenceChanged;
		}

		private async void MySceneView_SpatialReferenceChanged(object sender, System.EventArgs e)
		{
			MySceneView.SpatialReferenceChanged -= MySceneView_SpatialReferenceChanged;

			try
			{
				// Wait until all layers are initialized
				await MySceneView.LayersLoadedAsync();

				// Set viewpoint and navigate to it
				var viewpoint = new Camera(
					new MapPoint(
						-122.406025330049,
						37.7890934457207,
						209.54040953517,
						SpatialReferences.Wgs84),
					338.125939203603,
					72.7452621261101);

				await MySceneView.SetViewAsync(viewpoint, new TimeSpan(0, 0, 3), false);
			}
			catch (Exception ex)
			{
				MessageBox.Show("Error occured while navigating to the target viewpoint",
					"An error occured");
				Debug.WriteLine(ex.ToString());
			}
		}
	}

	public class ExtrusionGraphicsMainViewModel : INotifyPropertyChanged
	{
		public ExtrusionGraphicsMainViewModel()
		{
			SelectedExtrusionRenderer = _extrusionRenderers.Where(e => e.Title == "None").First();
		}
		private ICommand _clearAllCommand;
		public ICommand ClearAllCommand
		{
			get
			{
				if (_clearAllCommand == null)
					_clearAllCommand = new DelegateCommand(
						(parameter) => ClearAll());

				return _clearAllCommand;
			}
		}

		private ICommand _addShapesCommand;
		public ICommand AddShapesCommand
		{
			get
			{
				if (_addShapesCommand == null)
					_addShapesCommand = new DelegateCommand(
						(parameter) => addShapes(parameter.ToString())
						);


				return _addShapesCommand;
			}
		}

		private ICommand _switchExtrusionAttributeCommand;
		public ICommand SwitchExtrusionAttributeCommand
		{
			get
			{
				if (_switchExtrusionAttributeCommand == null)
					_switchExtrusionAttributeCommand = new DelegateCommand(
						(parameter) => switchExtrusionAttribute(parameter.ToString())
						);

				return _switchExtrusionAttributeCommand;
			}
		}

		private Scene _scene;
		public Scene Scene
		{
			get { return _scene; }
			set
			{
				_scene = value;
				NotifyPropertyChanged("Scene");
			}
		}

		private List<SimpleExtrusionRenderer> _extrusionRenderers = new List<SimpleExtrusionRenderer>()
		{
			new SimpleExtrusionRenderer(){Renderer=new SimpleRenderer()
			{
				SceneProperties=new RendererSceneProperties()
				{
					ExtrusionExpression="100000 * [A]",
					ExtrusionMode=ExtrusionMode.AbsoluteHeight
}
},Title="Absolute Height"},
new SimpleExtrusionRenderer(){Renderer=new SimpleRenderer()
			{
				SceneProperties=new RendererSceneProperties()
				{
					ExtrusionExpression="[A]",
					ExtrusionMode=ExtrusionMode.Baseheight
}
},Title="Base Height"},
new SimpleExtrusionRenderer(){Renderer=new SimpleRenderer()
			{
				SceneProperties=new RendererSceneProperties()
				{
					ExtrusionExpression="[A]",
					ExtrusionMode=ExtrusionMode.Maximum
}
},Title="Maximum"},
new SimpleExtrusionRenderer(){Renderer=new SimpleRenderer()
			{
				SceneProperties=new RendererSceneProperties()
				{
					ExtrusionExpression="[A]",
					ExtrusionMode=ExtrusionMode.Minimum
}
},Title="Minimum"},
new SimpleExtrusionRenderer(){Renderer=new SimpleRenderer()
			{
				SceneProperties=new RendererSceneProperties()
				{
					ExtrusionExpression="[A]",
					ExtrusionMode=ExtrusionMode.None
}
},Title="None"}


		};

		public List<SimpleExtrusionRenderer> ExtrusionRenderers
		{
			get { return _extrusionRenderers; }
			set
			{
				NotifyPropertyChanged("ExtrusionRenderers");
			}
		}

		private SimpleExtrusionRenderer _selectedExtrusionRenderer = null;
		public SimpleExtrusionRenderer SelectedExtrusionRenderer
		{
			get { return _selectedExtrusionRenderer; }
			set
			{
				_selectedExtrusionRenderer = value;
				if (_selectedExtrusionRenderer.Renderer != null && MyGraphicsLayer != null)
					MyGraphicsLayer.Renderer = _selectedExtrusionRenderer.Renderer;
				NotifyPropertyChanged("SelectedExtrusionRenderer");

			}
		}

		public GraphicsLayer MyGraphicsLayer
		{
			get { return this.Scene != null ? this.Scene.Layers["DynamicGraphicsLayer"] as GraphicsLayer : null; }
			set
			{
				NotifyPropertyChanged("MyGraphicsLayer");
			}
		}

		public bool IsGraphicAvailable
		{
			get { return (this.Scene.Layers["DynamicGraphicsLayer"] as GraphicsLayer).Graphics.Count > 0; }
			set
			{
				NotifyPropertyChanged("IsGraphicAvailable");
			}
		}

		private void ClearAll()
		{
			MyGraphicsLayer.Graphics.Clear();
			NotifyPropertyChanged("IsGraphicAvailable");
		}

		private void addPolylines()
		{
			ObservableCollection<MapPoint> cc = new ObservableCollection<MapPoint>();

			cc.Add(new MapPoint(-122.410521484809, 37.7918774561425, SpatialReferences.Wgs84));
			cc.Add(new MapPoint(-122.410324448543, 37.7919488885661, SpatialReferences.Wgs84));
			cc.Add(new MapPoint(-122.410203882271, 37.791913618768, SpatialReferences.Wgs84));
			cc.Add(new MapPoint(-122.409893155125, 37.791929107188, SpatialReferences.Wgs84));
			cc.Add(new MapPoint(-122.409596281126, 37.7919658630922, SpatialReferences.Wgs84));
			cc.Add(new MapPoint(-122.409108421115, 37.7920624120246, SpatialReferences.Wgs84));
			cc.Add(new MapPoint(-122.408817075003, 37.7921308679055, SpatialReferences.Wgs84));
			cc.Add(new MapPoint(-122.408487157309, 37.7921404237124, SpatialReferences.Wgs84));
			cc.Add(new MapPoint(-122.408318594361, 37.7921634551306, SpatialReferences.Wgs84));
			cc.Add(new MapPoint(-122.407960718313, 37.7922423743177, SpatialReferences.Wgs84));
			cc.Add(new MapPoint(-122.407649210245, 37.7922609783687, SpatialReferences.Wgs84));
			cc.Add(new MapPoint(-122.407319379371, 37.7923365607979, SpatialReferences.Wgs84));
			cc.Add(new MapPoint(-122.407087140057, 37.7923507171833, SpatialReferences.Wgs84));
			cc.Add(new MapPoint(-122.406860565968, 37.7923645287614, SpatialReferences.Wgs84));
			cc.Add(new MapPoint(-122.406655156229, 37.7923727726631, SpatialReferences.Wgs84));
			cc.Add(new MapPoint(-122.406300167333, 37.7923798808881, SpatialReferences.Wgs84));
			cc.Add(new MapPoint(-122.406155210349, 37.7924008897704, SpatialReferences.Wgs84));
			cc.Add(new MapPoint(-122.405946326338, 37.7924247794125, SpatialReferences.Wgs84));
			cc.Add(new MapPoint(-122.405699271002, 37.7925230337143, SpatialReferences.Wgs84));
			cc.Add(new MapPoint(-122.405454358112, 37.7926090310406, SpatialReferences.Wgs84));
			cc.Add(new MapPoint(-122.405077277732, 37.7926305633495, SpatialReferences.Wgs84));
			cc.Add(new MapPoint(-122.404748570807, 37.792656153651, SpatialReferences.Wgs84));
			cc.Add(new MapPoint(-122.404624092484, 37.7927069877562, SpatialReferences.Wgs84));
			cc.Add(new MapPoint(-122.404443495634, 37.7927364457713, SpatialReferences.Wgs84));


			SimpleLineSymbol sls = new SimpleLineSymbol();
			sls.Style = SimpleLineStyle.Solid;
			sls.Color = Color.FromArgb(170, 255, 0, 0);
			sls.Width = 5;

			Esri.ArcGISRuntime.Geometry.Polyline polyline = new Esri.ArcGISRuntime.Geometry.Polyline(cc, SpatialReferences.Wgs84);

			Esri.ArcGISRuntime.Geometry.Geometry geometry = (Esri.ArcGISRuntime.Geometry.Geometry)polyline;
			Graphic graphic = new Graphic(geometry, sls);
			graphic.Attributes.Add("A", 74);
			graphic.Attributes.Add("B", 100);
			MyGraphicsLayer.Graphics.Add(graphic);

		}


		private void addPolygon()
		{
			ObservableCollection<MapPoint> cc = new ObservableCollection<MapPoint>();

			cc.Add(new MapPoint(-122.411033517241, 37.7928248779988, SpatialReferences.Wgs84));
			cc.Add(new MapPoint(-122.409439828211, 37.7929574531202, SpatialReferences.Wgs84));
			cc.Add(new MapPoint(-122.407879728918, 37.7928979616957, SpatialReferences.Wgs84));
			cc.Add(new MapPoint(-122.407366343129, 37.7929400050513, SpatialReferences.Wgs84));
			cc.Add(new MapPoint(-122.40620355885, 37.7928932150506, SpatialReferences.Wgs84));
			cc.Add(new MapPoint(-122.405970239004, 37.7919861537598, SpatialReferences.Wgs84));
			cc.Add(new MapPoint(-122.405851348679, 37.7915014272396, SpatialReferences.Wgs84));
			cc.Add(new MapPoint(-122.406282194305, 37.7914529231787, SpatialReferences.Wgs84));
			cc.Add(new MapPoint(-122.406947985248, 37.7913177148322, SpatialReferences.Wgs84));
			cc.Add(new MapPoint(-122.407388139234, 37.7912593889064, SpatialReferences.Wgs84));
			cc.Add(new MapPoint(-122.408391496303, 37.7911085191116, SpatialReferences.Wgs84));
			cc.Add(new MapPoint(-122.409274453043, 37.791057174337, SpatialReferences.Wgs84));
			cc.Add(new MapPoint(-122.410233608604, 37.7910235416472, SpatialReferences.Wgs84));


			SimpleFillSymbol sfs = new SimpleFillSymbol();

			Esri.ArcGISRuntime.Geometry.Polygon polygon = new Esri.ArcGISRuntime.Geometry.Polygon(cc, SpatialReferences.Wgs84);

			sfs.Style = SimpleFillStyle.Solid;
			sfs.Color = Color.FromArgb(125, 255, 0, 0);
			sfs.Outline = new SimpleLineSymbol() { Color = Colors.Red, Width = 2 };

			Graphic graphic = new Graphic(polygon, sfs);
			graphic.Attributes.Add("A", 76);
			graphic.Attributes.Add("B", 100);
			MyGraphicsLayer.Graphics.Add(graphic);

		}

		private void addShapes(string shapeType)
		{
			ClearAll();
			//SelectedExtrusionMode = ExtrusionMode.None;
			switch (shapeType)
			{
				case "Point":
					addRandomPoints();
					break;
				case "Polyline":
					addPolylines();
					break;
				case "Polygon":
					addPolygon();
					break;
			}

			NotifyPropertyChanged("IsGraphicAvailable");
		}

		private void addRandomPoints()
		{
			foreach (SimpleExtrusionRenderer simpleRenderer in ExtrusionRenderers)
			{
				simpleRenderer.Renderer.Symbol = new SimpleMarkerSymbol()
				{
					Style = SimpleMarkerStyle.Circle,
					Color = Color.FromArgb(155, 255, 0, 0),
					Size = 30
				};

			}



			ObservableCollection<MapPoint> cc = new ObservableCollection<MapPoint>();
			cc.Add(new MapPoint(-122.410521484809, 37.7918774561425, SpatialReferences.Wgs84));
			cc.Add(new MapPoint(-122.410324448543, 37.7919488885661, SpatialReferences.Wgs84));
			cc.Add(new MapPoint(-122.410203882271, 37.791913618768, SpatialReferences.Wgs84));
			cc.Add(new MapPoint(-122.409893155125, 37.791929107188, SpatialReferences.Wgs84));
			cc.Add(new MapPoint(-122.409596281126, 37.7919658630922, SpatialReferences.Wgs84));
			cc.Add(new MapPoint(-122.409108421115, 37.7920624120246, SpatialReferences.Wgs84));
			cc.Add(new MapPoint(-122.408817075003, 37.7921308679055, SpatialReferences.Wgs84));
			cc.Add(new MapPoint(-122.408487157309, 37.7921404237124, SpatialReferences.Wgs84));
			cc.Add(new MapPoint(-122.408318594361, 37.7921634551306, SpatialReferences.Wgs84));
			cc.Add(new MapPoint(-122.407960718313, 37.7922423743177, SpatialReferences.Wgs84));
			cc.Add(new MapPoint(-122.407649210245, 37.7922609783687, SpatialReferences.Wgs84));
			cc.Add(new MapPoint(-122.407319379371, 37.7923365607979, SpatialReferences.Wgs84));
			cc.Add(new MapPoint(-122.407087140057, 37.7923507171833, SpatialReferences.Wgs84));
			cc.Add(new MapPoint(-122.406860565968, 37.7923645287614, SpatialReferences.Wgs84));
			cc.Add(new MapPoint(-122.406655156229, 37.7923727726631, SpatialReferences.Wgs84));
			cc.Add(new MapPoint(-122.406300167333, 37.7923798808881, SpatialReferences.Wgs84));
			cc.Add(new MapPoint(-122.406155210349, 37.7924008897704, SpatialReferences.Wgs84));
			cc.Add(new MapPoint(-122.405946326338, 37.7924247794125, SpatialReferences.Wgs84));
			cc.Add(new MapPoint(-122.405699271002, 37.7925230337143, SpatialReferences.Wgs84));
			cc.Add(new MapPoint(-122.405454358112, 37.7926090310406, SpatialReferences.Wgs84));
			cc.Add(new MapPoint(-122.405077277732, 37.7926305633495, SpatialReferences.Wgs84));
			cc.Add(new MapPoint(-122.404748570807, 37.792656153651, SpatialReferences.Wgs84));
			cc.Add(new MapPoint(-122.404624092484, 37.7927069877562, SpatialReferences.Wgs84));
			cc.Add(new MapPoint(-122.404443495634, 37.7927364457713, SpatialReferences.Wgs84));

			foreach (MapPoint mp in cc)
			{
				Graphic graphic = new Graphic(mp);
				graphic.Attributes.Add("A", 10000);
				graphic.Attributes.Add("B", 100);
				MyGraphicsLayer.Graphics.Add(graphic);
			}
			MyGraphicsLayer.Renderer = SelectedExtrusionRenderer.Renderer;
			NotifyPropertyChanged("SelectedExtrusionRenderer");
		}

		private void switchExtrusionAttribute(string attribute)
		{
			(MyGraphicsLayer.Renderer as SimpleRenderer).SceneProperties.ExtrusionExpression = attribute;
		}

		#region INotifyPropertyChanged Members
		public event PropertyChangedEventHandler PropertyChanged;
		protected void NotifyPropertyChanged(string propertyName)
		{
			if (PropertyChanged != null)
			{
				PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
			}
		}
		#endregion


	}

	public class SimpleExtrusionRenderer
	{
		public SimpleRenderer Renderer { get; set; }
		public string Title { get; set; }
	}

	internal class DelegateCommand : ICommand
	{
		private Action<object> m_execute;

		public DelegateCommand(Action<object> execute)
		{
			m_execute = execute;

		}

		public event EventHandler CanExecuteChanged;

		public void Execute(object parameter)
		{
			m_execute(parameter);
		}

		public bool CanExecute(object parameter)
		{
			return true;
		}



	}


}