using Esri.ArcGISRuntime.Controls;
using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Http;
using Esri.ArcGISRuntime.Layers;
using Esri.ArcGISRuntime.Tasks.Offline;
using Esri.ArcGISRuntime.Tasks.Query;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace ArcGISRuntime.Samples.Desktop
{
	/// <summary>
	/// This sample demonstrates how to synchronize a local geodatabase with an online service.
	/// </summary>
	/// <title>Sync Geodatabase</title>
	/// <category>Offline</category>
	/// <sampleType>Workflow</sampleType>
	public partial class SyncGeodatabase : UserControl, INotifyPropertyChanged
	{
		private const string BASE_URL = "http://sampleserver6.arcgisonline.com/arcgis/rest/services/Sync/SaveTheBaySync/FeatureServer";
		private const string GDB_PREFIX = "DOTNET_Sample";
		private const string GDB_BASENAME = "sample";
		private const string GDB_FILE_EXT = ".geodatabase";

		#region VM Properties
		private bool _isBusy;
		public bool IsBusy
		{
			get { return _isBusy; }
			set { _isBusy = value; RaisePropertyChanged(); }
		}

		private bool _canGenerate;
		public bool CanGenerate
		{
			get { return _canGenerate; }
			set { _canGenerate = value; RaisePropertyChanged(); }
		}

		private bool _canSync;
		public bool CanSync
		{
			get { return _canSync; }
			set { _canSync = value; RaisePropertyChanged(); }
		}

		private bool _canUnregister;
		public bool CanUnregister
		{
			get { return _canUnregister; }
			set { _canUnregister = value; RaisePropertyChanged(); }
		}

		private string _primaryStatus;
		public string PrimaryStatus
		{
			get { return _primaryStatus; }
			set { _primaryStatus = value; RaisePropertyChanged(); }
		}

		private string _secondaryStatus;
		public string SecondaryStatus
		{
			get { return _secondaryStatus; }
			set { _secondaryStatus = value; RaisePropertyChanged(); }
		}

		private CodedValueDomain _birdTypeDomain;
		public CodedValueDomain BirdTypeDomain
		{
			get { return _birdTypeDomain; }
			set { _birdTypeDomain = value; RaisePropertyChanged(); }
		}

		private IEnumerable<Feature> _localBirdFeatures;
		public IEnumerable<Feature> LocalBirdFeatures
		{
			get { return _localBirdFeatures; }
			set { _localBirdFeatures = value; RaisePropertyChanged(); }
		}

		private FeatureLayer _localBirdsLayer;
		public FeatureLayer LocalBirdsLayer
		{
			get { return _localBirdsLayer; }
			set { _localBirdsLayer = value; RaisePropertyChanged(); }
		}

		private MapPoint _tapLocation;
		public MapPoint TapLocation
		{
			get { return _tapLocation; }
			set { _tapLocation = value; RaisePropertyChanged(); }
		}

		private bool _isEditing;
		public bool IsEditing
		{
			get { return _isEditing; }
			set { _isEditing = value; RaisePropertyChanged(); }
		}
		#endregion

		private GeodatabaseSyncTask _syncTask;
		private ArcGISDynamicMapServiceLayer _onlineBirdsLayer;
		private GraphicsOverlay _graphicsOverlay;
		private string _gdbPath;
		
		/// <summary>Construct Generate Geodatabase sample control</summary>
		public SyncGeodatabase()
		{
			InitializeComponent();

			_syncTask = new GeodatabaseSyncTask(new Uri(BASE_URL));
			_onlineBirdsLayer = MyMapView.Map.Layers.OfType<ArcGISDynamicMapServiceLayer>().First();
			_graphicsOverlay = MyMapView.GraphicsOverlays["graphicsOverlay"];
			_localBirdsLayer = null;
			CanGenerate = true;

			this.DataContext = this;
		}

		// Sample exception handler
		private void HandleException(Exception ex)
		{
			string message = "Sample Exception";
			if (ex != null)
				message += ": " + ex.Message;

			MessageBox.Show(message, "Sample Error");
		}

		// Generate local geodatabase from the online service
		private async void GenerateButton_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				IsBusy = true;
               
                // Get current viewpoints extent from the MapView
                var currentViewpoint = MyMapView.GetCurrentViewpoint(ViewpointType.BoundingGeometry);
                var viewpointExtent = currentViewpoint.TargetGeometry.Extent;

				// Generate local gdb
				await GenerateLocalGeodatabaseAsync(viewpointExtent);

				// Set editing combobox itemssource from bird type domain
				if (_localBirdsLayer != null)
				{
					var typeField = _localBirdsLayer.FeatureTable.Schema.Fields.FirstOrDefault(fld => fld.Name == "type");
					if (typeField != null && typeField.Domain is CodedValueDomain)
					{
						BirdTypeDomain = ((CodedValueDomain)typeField.Domain);
					}

					CanSync = CanUnregister = true;
					CanGenerate = false;
				}

				// Show local bird data in the UI grid
				await RefreshDataView();
			}
			catch (Exception ex)
			{
				HandleException(ex);
			}
			finally
			{
				IsBusy = false;
			}
		}

		// Generate / download and display layers from a generated geodatabase
		private async Task GenerateLocalGeodatabaseAsync(Envelope extent)
		{
			try
			{
				IsBusy = true;

				ReportStatus("Creating GeodatabaseSyncTask...");
				var syncTask = new GeodatabaseSyncTask(new Uri(BASE_URL));

				var options = new GenerateGeodatabaseParameters(new int[] { 1 }, extent)
				{
					GeodatabasePrefixName = GDB_PREFIX,
					ReturnAttachments = false,
					OutSpatialReference = extent.SpatialReference,
					SyncModel = SyncModel.PerLayer
				};

				var tcs = new TaskCompletionSource<GeodatabaseStatusInfo>();
				Action<GeodatabaseStatusInfo, Exception> completionAction = (info, ex) =>
				{
					if (ex != null)
						tcs.SetException(ex);
					tcs.SetResult(info);
				};

				var generationProgress = new Progress<GeodatabaseStatusInfo>();
				generationProgress.ProgressChanged += (sndr, sts) => { SecondaryStatus = sts.Status.ToString(); };

				ReportStatus("Starting GenerateGeodatabase...");
				var result = await syncTask.GenerateGeodatabaseAsync(options, completionAction,
					TimeSpan.FromSeconds(3), generationProgress, CancellationToken.None);

				ReportStatus("Waiting on geodatabase from server...");
				var statusResult = await tcs.Task;

				ReportStatus("Downloading Geodatabase...");
				await DownloadGeodatabaseAsync(statusResult);

				ReportStatus("Opening Geodatabase...");
				var gdb = await Geodatabase.OpenAsync(_gdbPath);

				ReportStatus("Create local feature layers...");
				await CreateFeatureLayers(gdb);

				_onlineBirdsLayer.IsVisible = false;
			}
			finally
			{
				IsBusy = false;
			}
		}

		// Sychronizes local data with the online service
		private async void SyncButton_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				if (LocalBirdsLayer == null)
					throw new ApplicationException("Could not find local geodatabase.");

				IsBusy = true;
				ReportStatus("Synchronizing Local and Online data...");

				var tcs = new TaskCompletionSource<GeodatabaseStatusInfo>();
				Action<GeodatabaseStatusInfo, Exception> completionAction = (info, ex) =>
				{
					if (ex != null)
						tcs.SetException(ex);
					tcs.SetResult(info);
				};

				var syncProgress = new Progress<GeodatabaseStatusInfo>();
				syncProgress.ProgressChanged += (sndr, sts) => { SecondaryStatus = sts.Status.ToString(); };

				var syncTask = new GeodatabaseSyncTask(new Uri(BASE_URL));
				var gdbTable = _localBirdsLayer.FeatureTable as GeodatabaseFeatureTable;
				await syncTask.SyncGeodatabaseAsync(gdbTable.Geodatabase,
					completionAction,
					null,
					TimeSpan.FromSeconds(3),
					syncProgress,
					CancellationToken.None);

				await tcs.Task;

				ReportStatus("Refreshing map view...");
				await RefreshDataView();
			}
			catch (Exception ex)
			{
				HandleException(ex);
			}
			finally
			{
				IsBusy = false;
			}
		}

		// Unregister local geodatabase from the online service
		private async void UnregisterButton_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				IsBusy = true;
				ReportStatus("Unregistering Geodatabase...");

				if (LocalBirdsLayer != null)
				{
					var gdbTable = LocalBirdsLayer.FeatureTable as GeodatabaseFeatureTable;
					await _syncTask.UnregisterGeodatabaseAsync(gdbTable.Geodatabase);

					MyMapView.Map.Layers.Remove(LocalBirdsLayer);
					LocalBirdFeatures = null;
					LocalBirdsLayer = null;
				}

				CanSync = CanUnregister = false;
				CanGenerate = true;
			}
			catch (Exception ex)
			{
				HandleException(ex);
			}
			finally
			{
				IsBusy = false;
			}
		}

		// Handles click on the map to add a new bird sighting
		private void MyMapView_MapViewTapped(object sender, MapViewInputEventArgs e)
		{
			try
			{
				if (LocalBirdsLayer == null)
					throw new ApplicationException("No local geodatabase to edit.");

				_graphicsOverlay.Graphics.Clear();
				_graphicsOverlay.Graphics.Add(new Graphic(e.Location));

				TapLocation = e.Location;
				IsEditing = true;
			}
			catch (Exception ex)
			{
				HandleException(ex);
			}
		}

		// Adds a new bird row to the local geodatabase
		private async void RegisterBirdSightingButton_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				var birdsTable = LocalBirdsLayer.FeatureTable as ArcGISFeatureTable;
				if (birdsTable == null)
					throw new ApplicationException("Birds table was not found in the local geodatabase.");

				var bird = new GeodatabaseFeature(birdsTable.Schema);
				bird.Geometry = TapLocation;

				if (bird.Schema.Fields.Any(fld => fld.Name == "type"))
					bird.Attributes["type"] = comboBirdType.SelectedValue;
				if (bird.Schema.Fields.Any(fld => fld.Name == "comments"))
					bird.Attributes["comments"] = txtComment.Text;
				if (bird.Schema.Fields.Any(fld => fld.Name == "creator"))
					bird.Attributes["creator"] = "DOTNET_SAMPLE";
				if (bird.Schema.Fields.Any(fld => fld.Name == "created_date"))
					bird.Attributes["created_date"] = DateTime.Now;

				var id = await birdsTable.AddAsync(bird);

				await RefreshDataView();

				_graphicsOverlay.Graphics.Clear();
				IsEditing = false;
			}
			catch (Exception ex)
			{
				HandleException(ex);
			}
		}

		// Cancel current edit
		private void CancelBirdSightingButton_Click(object sender, RoutedEventArgs e)
		{
			_graphicsOverlay.Graphics.Clear();
			IsEditing = false;
		}

		// Download a generated geodatabase file
		private async Task DownloadGeodatabaseAsync(GeodatabaseStatusInfo statusResult)
		{
			var client = new ArcGISHttpClient();
			var gdbStream = client.GetOrPostAsync(statusResult.ResultUri, null);

			SetGeodatabaseFileName();

			await Task.Run(async () =>
			{
				using (var stream = System.IO.File.Create(_gdbPath))
				{
					await gdbStream.Result.Content.CopyToAsync(stream);
				}
			});
		}

		// Get unused (or deletable) geodatabase file
		private void SetGeodatabaseFileName()
		{
			var fullPathWithoutExt = System.IO.Path.Combine(System.IO.Path.GetTempPath(), GDB_BASENAME);

			int count = 0;
			var tempGdbPath = fullPathWithoutExt + GDB_FILE_EXT;
			while (System.IO.File.Exists(tempGdbPath))
			{
				try
				{
					System.IO.File.Delete(tempGdbPath);
				}
				catch
				{
					++count;
					tempGdbPath = fullPathWithoutExt + "_" + count.ToString("000") + GDB_FILE_EXT;
				}
			}

			_gdbPath = tempGdbPath;
		}

		// Create feature layers from the given geodatabase file
		private async Task CreateFeatureLayers(Geodatabase gdb)
		{
			if (gdb.FeatureTables.Count() == 0)
				throw new ApplicationException("Downloaded geodatabase has no feature tables.");

			if (LocalBirdsLayer != null)
				MyMapView.Map.Layers.Remove(LocalBirdsLayer);

			var birdsTable = gdb.FeatureTables.First();
			LocalBirdsLayer = new FeatureLayer()
			{
				ID = "Birds",
				DisplayName = "Local Birds",
				FeatureTable = birdsTable
			};
			MyMapView.Map.Layers.Insert(2, LocalBirdsLayer);

			await MyMapView.SetViewAsync(birdsTable.Extent);
		}

		// Update the UI grid with bird data queried from local gdb
		private async Task RefreshDataView()
		{
			LocalBirdFeatures = await _localBirdsLayer.FeatureTable.QueryAsync(new QueryFilter() { WhereClause = "1=1" });

			QueryTask queryTask = new QueryTask(new Uri(_onlineBirdsLayer.ServiceUri + "/1"));

            // Get current viewpoints extent from the MapView
            var currentViewpoint = MyMapView.GetCurrentViewpoint(ViewpointType.BoundingGeometry);
            var viewpointExtent = currentViewpoint.TargetGeometry.Extent;

			Query query = new Query("1=1") { Geometry = viewpointExtent, OutFields = new OutFields(new string[] { "globalid" }) };
			var queryResult = await queryTask.ExecuteAsync(query);

			var onlineBirdIds = queryResult.FeatureSet.Features.Select(f => f.Attributes["globalid"]);
			var localBirdIds = LocalBirdFeatures.Select(b => b.Attributes["globalid"]);
			var newBirdsIds = localBirdIds.Except(onlineBirdIds);

			var newBirdOIDs = from newBird in LocalBirdFeatures
							  join newBirdId in newBirdsIds on newBird.Attributes["globalid"] equals newBirdId
							  select (long)newBird.Attributes["objectid"];

			_localBirdsLayer.ClearSelection();
			_localBirdsLayer.SelectFeatures(newBirdOIDs.ToArray());
		}

		// Simple status update for UI
		private void ReportStatus(string status, string secondaryStatus = "")
		{
			PrimaryStatus = status;
			SecondaryStatus = secondaryStatus;
		}

		#region INotifyPropertyChanged
		public event PropertyChangedEventHandler PropertyChanged;

		private void RaisePropertyChanged([CallerMemberName] string propertyName = "")
		{
			var handler = PropertyChanged;
			if (handler != null)
			{
				handler(this, new PropertyChangedEventArgs(propertyName));
			}
		}
		#endregion
	}

	// Value Converter to transform feature domain field values from domain code to domain value
	internal class CodedValueDomainConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			var feature = value as GeodatabaseFeature;
			if (feature == null)
				return null;

			var fieldName = parameter as string;
			if (fieldName == null)
				return null;

			string convertedValue = string.Empty;
			object fieldValue;
			if (feature.Attributes.TryGetValue(fieldName, out fieldValue))
			{
			  if (fieldValue == null)
				return convertedValue;
				var field = feature.Schema.Fields
					.FirstOrDefault(fld => fld.Name.Equals(fieldName, StringComparison.CurrentCultureIgnoreCase));
				if (field != null)
				{
					var domain = field.Domain as CodedValueDomain;
					if (domain != null)
					{                      
						domain.CodedValues.TryGetValue(fieldValue, out convertedValue);
					}
				}
			}
			else
				convertedValue = fieldValue.ToString();

			return convertedValue;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			return null;
		}
	}
}
