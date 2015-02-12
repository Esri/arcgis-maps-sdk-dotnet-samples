using Esri.ArcGISRuntime.Controls;
using Esri.ArcGISRuntime.Geometry;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ArcGISRuntime.Samples.Desktop
{
	/// <summary>
	/// This sample shows how to perform view operations from a view model
	/// by using a controller that's bound to the map.
	/// The ViewModel owns the controller and doesn't know about the MapView it is
	/// being bound to, but can perform operations on the controller.
	/// </summary>
	/// <title>Navigate from ViewModel</title>
	/// <category>Mapping</category>
	public partial class NavigateFromViewModel : UserControl
	{
		public NavigateFromViewModel()
		{
			InitializeComponent();
		}
	}

	/// <summary>
	/// ViewModel for the NavigateFromViewModel page
	/// </summary>
	public class NavigateFromViewModel_VM
	{
		IList<Tuple<string, Viewpoint>> bookmarks;
		public NavigateFromViewModel_VM()
		{
			//Initialize controller
			Controller = new MapViewController();
			Controller.PropertyChanged += Controller_PropertyChanged;
			//Create a list of bookmarks to navigate to
			bookmarks = new ObservableCollection<Tuple<string, Viewpoint>>();
			bookmarks.Add(new Tuple<string,Viewpoint>("World", new Viewpoint(new Envelope(-180,-85,180,85, SpatialReferences.Wgs84))));
			bookmarks.Add(new Tuple<string,Viewpoint>("ESRI", new Viewpoint(new MapPoint(-117.19569,34.056849, SpatialReferences.Wgs84), 5000)));
			bookmarks.Add(new Tuple<string, Viewpoint>("California", new Viewpoint(new Envelope(-124.63, 32.65, -113.909, 41.99, SpatialReferences.Wgs84))));
			//Command for adding more bookmarks to the list, by grabbing the Controller's Extent property
			AddBookmark = new MapViewController.DelegateCommand(
						(parameter) => bookmarks.Add(new Tuple<string,Viewpoint>( "Bookmark #" + (bookmarks.Count+1).ToString(),
												new Viewpoint((Geometry)parameter))),
						(parameter) => { return parameter is Geometry; });
		}

		private void Controller_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			//Update the AddBookmark command if the controller reports that the extent has changed
			if (e.PropertyName == "Extent")
				((MapViewController.DelegateCommand)AddBookmark).OnCanExecuteChanged();
		}

		// Gets the bookmarks stored
		public IEnumerable<Tuple<string,Viewpoint>> Bookmarks
		{
			get
			{
				return bookmarks;
			}
		}

		//This is the view controller we'll bind to the mapView.
		//This controller acts as a proxy that allows the local view model
		//to request a view extent to whichever MapView might be bound to it
		//We can use ICommands directly to execute view operations,
		//Or we can call Controller.SetViewAsync to request a view operation
		public MapViewController Controller { get; private set; }

		//Command for adding a bookmark. The command parameter must be a geometry
		public ICommand AddBookmark { get; private set; }
	}
	
	/// <summary>
	/// This is the controller object. We use this bind to the map with an attached property.
	/// It manages performing view operations without exposing the view back to the viewmodel.
	/// Instead it exposes a SetViewCommand for command binding and a SetViewAsync method
	/// for performing view operations directly from the owner of the MapViewController instance (ie the ViewModel).
	/// </summary>
	// Syntax for using this:
	// <esri:MapView local:MapViewController.Controller="{Binding Controller}">
	public class MapViewController : INotifyPropertyChanged
	{
		//Keep a weak reference so a long lived controller won't hold on to a shortlived MapView
		private WeakReference<MapView> m_map;


		#region Commands

		public Task<bool> SetViewAsync(Viewpoint viewPoint)
		{
			var mapView = MapView;
			if (mapView != null && viewPoint != null)
				return mapView.SetViewAsync(viewPoint);

			return Task.FromResult(false);
		}

		public bool CanSetView(Viewpoint viewPoint)
		{
			MapView map = MapView;
			return (map != null && map.SpatialReference != null 
				&& viewPoint != null && viewPoint.TargetGeometry != null);
		}

		#endregion Commands

		#region Properties

		public Envelope Extent
		{
			get
			{
				MapView mapView = MapView;
				if (mapView != null)
				{
					return mapView.Extent;
				}
				return null;
			}
		}
		#endregion Properties

		#region MapView handling

		private MapView MapView
		{
			get
			{
				MapView map = null;
				if (m_map != null && m_map.TryGetTarget(out map))
					return map;
				return null;
			}
		}

		public static MapViewController GetController(DependencyObject obj)
		{
			return (MapViewController)obj.GetValue(ControllerProperty);
		}

		public static void SetController(DependencyObject obj, MapViewController value)
		{
			obj.SetValue(ControllerProperty, value);
		}

		public static readonly DependencyProperty ControllerProperty =
			DependencyProperty.RegisterAttached("Controller", typeof(MapViewController), typeof(MapViewController),
				new PropertyMetadata(null, OnControllerPropertyChanged));

		private static void OnControllerPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
		{
			if (d is MapView && e.OldValue is MapViewController)
			{
				var controller = (e.OldValue as MapViewController);
				controller.m_map = null;
				var delegateCommand = controller.SetViewCommand as MapViewController.DelegateCommand;
				if (delegateCommand != null)
					delegateCommand.OnCanExecuteChanged();
			}
			if (d is MapView && e.NewValue is MapViewController)
			{
				var controller = (e.NewValue as MapViewController);
				controller.m_map = new WeakReference<MapView>(d as MapView);

				var inpcListener = new WeakEventListener<MapView, object, PropertyChangedEventArgs>(d as MapView);
				inpcListener.OnEventAction =
					(instance, source, eventArgs) => controller.MapViewController_PropertyChanged(source, eventArgs);

				// the instance passed to the action is referenced (i.e. instance.ProeprtyChanged) so the lambda expression is 
				// compiled as a static method.  Otherwise it targets the mapView instance and holds it in memory.
				// This allows the MapView to get disposed even though this controller is still alive.
				inpcListener.OnDetachAction = (instance, listener) =>
				{
					if (instance != null)
						instance.PropertyChanged -= listener.OnEvent;
				};
				(d as MapView).PropertyChanged += inpcListener.OnEvent;
				inpcListener = null;
				if (controller.m_SetViewCommand != null)
					controller.m_SetViewCommand.OnCanExecuteChanged();
			}
		}

		private void MapViewController_PropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == "SpatialReference")
			{
				if (m_SetViewCommand != null) m_SetViewCommand.OnCanExecuteChanged();
			}
			else if (e.PropertyName == "Extent")
				OnPropertyChanged("Extent");
		}
		private MapViewController.DelegateCommand m_SetViewCommand;

		/// <summary>
		/// Calls SetView on the Envelope provided in the <see cref="System.Windows.Input.ICommand.CommandParameter"/>
		/// </summary>
		public System.Windows.Input.ICommand SetViewCommand
		{
			get
			{
				if (m_SetViewCommand == null)
				{
					m_SetViewCommand = new MapViewController.DelegateCommand(
						(parameter) => SetViewAsync(parameter as Viewpoint),
						(parameter) => CanSetView(parameter as Viewpoint));
				}
				return m_SetViewCommand;
			}
		}

		#endregion

		private void OnPropertyChanged(string propertyName)
		{
			if (PropertyChanged != null)
				PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
		}

		public event PropertyChangedEventHandler PropertyChanged;

		/// <summary>
		/// Implements a weak event listener that allows the owner to be garbage
		/// collected if its only remaining link is an event handler.
		/// </summary>
		/// <typeparam name="TInstance">Type of instance listening for the event.</typeparam>
		/// <typeparam name="TSource">Type of source for the event.</typeparam>
		/// <typeparam name="TEventArgs">Type of event arguments for the event.</typeparam>
		private class WeakEventListener<TInstance, TSource, TEventArgs> where TInstance : class
		{
			/// <summary>
			/// WeakReference to the instance listening for the event.
			/// </summary>
			private WeakReference _weakInstance;

			/// <summary>
			/// Gets or sets the method to call when the event fires.
			/// </summary>
			public Action<TInstance, TSource, TEventArgs> OnEventAction { get; set; }

			/// <summary>
			/// Gets or sets the method to call when detaching from the event.
			/// </summary>
			public Action<TInstance, WeakEventListener<TInstance, TSource, TEventArgs>> OnDetachAction { get; set; }

			/// <summary>
			/// Initializes a new instances of the WeakEventListener class.
			/// </summary>
			/// <param name="instance">Instance subscribing to the event.</param>
			public WeakEventListener(TInstance instance)
			{
				if (null == instance)
				{
					throw new ArgumentNullException("instance");
				}
				_weakInstance = new WeakReference(instance);
			}

			/// <summary>
			/// Handler for the subscribed event calls OnEventAction to handle it.
			/// </summary>
			/// <param name="source">Event source.</param>
			/// <param name="eventArgs">Event arguments.</param>
			public void OnEvent(TSource source, TEventArgs eventArgs)
			{
				TInstance target = (TInstance)_weakInstance.Target;
				if (null != target)
				{
					// Call registered action
					if (null != OnEventAction)
					{
						OnEventAction(target, source, eventArgs);
					}
				}
				else
				{
					// Detach from event
					Detach();
				}
			}

			/// <summary>
			/// Detaches from the subscribed event.
			/// </summary>
			public void Detach()
			{
				TInstance target = (TInstance)_weakInstance.Target;
				if (null != OnDetachAction)
				{
					OnDetachAction(target, this);
					OnDetachAction = null;
				}
			}
		}

		internal class DelegateCommand : ICommand
		{
			private Action<object> m_execute;
			private Func<object, bool> m_canExecute;

			public DelegateCommand(Action<object> execute, Func<object, bool> canExecute)
			{
				m_execute = execute;
				m_canExecute = canExecute;
			}

			public bool CanExecute(object parameter)
			{
				return m_canExecute(parameter);
			}

			public event EventHandler CanExecuteChanged;

			public void Execute(object parameter)
			{
				m_execute(parameter);
			}

			public void OnCanExecuteChanged()
			{
				if (CanExecuteChanged != null)
					CanExecuteChanged(this, EventArgs.Empty);
			}
		}
	}

}
