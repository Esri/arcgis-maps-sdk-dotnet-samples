using Esri.ArcGISRuntime.Controls;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Layers;
using Esri.ArcGISRuntime.Symbology;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ArcGISRuntime.Samples.Desktop
{
	/// <summary>
	/// Shows how to use extrusion which means stretching a flat 2D shape vertically to create a 3D object.
	/// </summary>
	/// <title>3D Graphics extrusion</title>
	/// <category>3D</category>
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
				var viewpoint = new Viewpoint3D(
					new MapPoint(
						-122.406025330049,
						37.7890934457207,
						209.54040953517,
						SpatialReferences.Wgs84),
					338.125939203603,
					72.7452621261101);

				await MySceneView.SetViewAsync(viewpoint, new TimeSpan(0,0,3), false);
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
		private ICommand _addShapesCommand;
		private ICommand _switchExtrusionAttributeCommand;
		private Scene _scene;
		private ExtrusionMode _selectedExtrusionMode = ExtrusionMode.None;

		private List<ExtrusionMode> _extrusionModes = new List<ExtrusionMode>() 
		{ 
			ExtrusionMode.AbsoluteHeight, 
			ExtrusionMode.Baseheight, 
			ExtrusionMode.Maximum, 
			ExtrusionMode.Minimum, 
			ExtrusionMode.None 
		};

		public ICommand AddShapesCommand
		{
			get
			{
				if (_addShapesCommand == null)
					_addShapesCommand = new DelegateCommand(
						(parameter) => AddShapes(parameter.ToString()));

				return _addShapesCommand;
			}
		}
		
		public ICommand SwitchExtrusionAttributeCommand
		{
			get
			{
				if (_switchExtrusionAttributeCommand == null)
					_switchExtrusionAttributeCommand = new DelegateCommand(
						(parameter) => SwitchExtrusionAttribute(parameter.ToString()));

				return _switchExtrusionAttributeCommand;
			}
		}

		public Scene Scene
		{
			get { return _scene; }
			set
			{
				_scene = value;
				NotifyPropertyChanged("Scene");
			}
		}

		public List<ExtrusionMode> ExtrusionModes
		{
			get { return _extrusionModes; }
		}

		public GraphicsLayer GraphicsLayer
		{
			get { return this.Scene.Layers["DynamicGraphicsLayer"] as GraphicsLayer; }
		}

		public bool IsGraphicAvailable
		{
			get { return (this.Scene.Layers["DynamicGraphicsLayer"] as GraphicsLayer).Graphics.Count > 0; }
		}

		public ExtrusionMode SelectedExtrusionMode
		{
			get { return _selectedExtrusionMode; }
			set
			{
				_selectedExtrusionMode = value;
				GraphicsLayer.ExtrusionMode = _selectedExtrusionMode;
				NotifyPropertyChanged("SelectedExtrusionMode");
			}
		}

		private void ClearAll()
		{
			GraphicsLayer.Graphics.Clear();
			NotifyPropertyChanged("IsGraphicAvailable");
		}

		private void AddPolylines()
		{
			var cc = new PointCollection(SpatialReferences.Wgs84);
			
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

			var polyline = new Polyline(cc, SpatialReferences.Wgs84);

			var sls = new SimpleLineSymbol();
			sls.Style = SimpleLineStyle.Solid;
			sls.Color = System.Windows.Media.Color.FromArgb(170, 255, 0, 0);
			sls.Width = 5;

			var geometry = (Geometry)polyline;
			var graphic = new Graphic(geometry, sls);
			graphic.Attributes.Add("A", 74);
			graphic.Attributes.Add("B", 100);
			GraphicsLayer.Graphics.Add(graphic);
		}

		private void AddPolygon()
		{
			var cc = new PointCollection(SpatialReferences.Wgs84);

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

			var polygon = new Polygon(cc, SpatialReferences.Wgs84);

			var sfs = new SimpleFillSymbol();
			sfs.Style = SimpleFillStyle.Solid;
			sfs.Color = System.Windows.Media.Color.FromArgb(125, 255, 0, 0);
			sfs.Outline = new SimpleLineSymbol() 
			{ 
				Color = System.Windows.Media.Colors.Red, 
				Width = 2 
			};

			var graphic = new Graphic(polygon, sfs);
			graphic.Attributes.Add("A", 76);
			graphic.Attributes.Add("B", 100);
			GraphicsLayer.Graphics.Add(graphic);
		}

		private void AddShapes(string shapeType)
		{
			ClearAll();
			SelectedExtrusionMode = ExtrusionMode.None;
			switch (shapeType)
			{
				case "Point":
					AddRandomPoints();
					break;
				case "Polyline":
					AddPolylines();
					break;
				case "Polygon":
					AddPolygon();
					break;
			}

			NotifyPropertyChanged("IsGraphicAvailable");
		}

		private void AddRandomPoints()
		{
			var sms = new SimpleMarkerSymbol();
			sms.Style = SimpleMarkerStyle.Circle;
			sms.Color = System.Windows.Media.Color.FromArgb(155, 255, 0, 0);
			sms.Size = 30;

			var cc = new PointCollection(SpatialReferences.Wgs84);
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
				var graphic = new Graphic(mp, sms);
				graphic.Attributes.Add("A", 75);
				graphic.Attributes.Add("B", 100);
				GraphicsLayer.Graphics.Add(graphic);
			}
		}

		private void SwitchExtrusionAttribute(string attribute)
		{
			GraphicsLayer.ExtrusionExpression = attribute;
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

		#endregion //INPC
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
