using Esri.ArcGISRuntime.Data;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Http;
using Esri.ArcGISRuntime.Layers;
using Esri.ArcGISRuntime.Tasks.Offline;
using Esri.ArcGISRuntime.Controls;
using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Popups;
using Windows.UI.Xaml;

namespace ArcGISRuntime.Samples.Phone.Samples
{
	/// <summary>
	/// This sample demonstrates how to generate and download a local geodatabase from an online map service for later offline use.
	/// </summary>
	/// <title>Generate Geodatabase</title>
	/// <category>Offline</category>
	public partial class GenerateGeodatabase : Windows.UI.Xaml.Controls.Page
	{
		private const string BASE_URL = "http://sampleserver6.arcgisonline.com/arcgis/rest/services/Sync/WildfireSync/FeatureServer";
		private const string GDB_PREFIX = "DOTNET_Sample";
		// private const string GDB_NAME = "sample.geodatabase";

		/// <summary>Construct Generate Geodatabase sample control</summary>
		public GenerateGeodatabase()
		{
			InitializeComponent();
		}

		// Generate / download and display layers from a generated geodatabase
		private async void GenerateGeodatabaseButton_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				panelStatus.Visibility = Visibility.Visible;

				ReportStatus("Creating GeodatabaseSyncTask...");
				var syncTask = new GeodatabaseSyncTask(new Uri(BASE_URL));

                // Get current viewpoints extent from the MapView
                var currentViewpoint = MyMapView.GetCurrentViewpoint(ViewpointType.BoundingGeometry);
                var viewpointExtent = currentViewpoint.TargetGeometry.Extent;

				var options = new GenerateGeodatabaseParameters(new int[] { 0, 1, 2 }, viewpointExtent)
				{
					GeodatabasePrefixName = GDB_PREFIX,
					ReturnAttachments = false,
					OutSpatialReference = MyMapView.SpatialReference,
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
				generationProgress.ProgressChanged += (sndr, sts) => { ReportStatus(sts.Status.ToString()); };

				ReportStatus("Starting GenerateGeodatabase...");
				var result = await syncTask.GenerateGeodatabaseAsync(options, completionAction,
					TimeSpan.FromSeconds(3), generationProgress, CancellationToken.None);

				ReportStatus("Waiting on geodatabase from server...");
				var statusResult = await tcs.Task;

				ReportStatus("Downloading Geodatabase...");
				var gdbFile = await DownloadGeodatabase(statusResult);

				ReportStatus("Create local feature layers...");
				await CreateFeatureLayersAsync(gdbFile.Path);

				MyMapView.Map.Layers["onlineService"].IsVisible = false;
			}
			catch (Exception ex)
			{
				var _x = new MessageDialog(ex.Message, "Sample Error").ShowAsync();
			}
			finally
			{
				panelStatus.Visibility = Visibility.Collapsed;
			}
		}

		// Download a generated geodatabase file
		private async Task<StorageFile> DownloadGeodatabase(GeodatabaseStatusInfo statusResult)
		{
			var file = await ApplicationData.Current.TemporaryFolder.CreateFileAsync(statusResult.GeodatabaseName, CreationCollisionOption.ReplaceExisting);
			var client = new ArcGISHttpClient();
			var download = await client.GetOrPostAsync(statusResult.ResultUri, null);
			using (var fileStream = await file.OpenStreamForWriteAsync())
			{
				await download.EnsureSuccessStatusCode().Content.CopyToAsync(fileStream);
			}

			return file;
		}

		// Create feature layers from the given geodatabase file
		private async Task CreateFeatureLayersAsync(string gdbPath)
		{
			try
			{
				var gdb = await Geodatabase.OpenAsync(gdbPath);

				if (gdb.FeatureTables.Count() == 0)
					throw new Exception("Downloaded geodatabase has no feature tables.");

				var groupLayer = MyMapView.Map.Layers["Local_Geodatabase"] as GroupLayer;
				if (groupLayer != null)
					MyMapView.Map.Layers.Remove(groupLayer);

				groupLayer = new GroupLayer()
				{
					ID = "Local_Geodatabase",
					DisplayName = string.Format("Local ({0})", gdbPath)
				};

				Envelope extent = gdb.FeatureTables.First().Extent;
				foreach (var table in gdb.FeatureTables)
				{
					//if this call is made after FeatureTable is initialized, a call to FeatureLayer.ResetRender will be required.
					table.UseAdvancedSymbology = true;
					var flayer = new FeatureLayer()
					{
						ID = table.Name,
						DisplayName = string.Format("{0} ({1})", table.Name, table.RowCount),
						FeatureTable = table
					};

					if (table.Extent != null)
					{
						if (extent == null)
							extent = table.Extent;
						else
							extent = extent.Union(table.Extent);
					}

					groupLayer.ChildLayers.Add(flayer);
				}

				MyMapView.Map.Layers.Add(groupLayer);

				await MyMapView.SetViewAsync(extent.Expand(1.10));
			}
			catch (Exception ex)
			{
				var _x = new MessageDialog("Error creating feature layer: " + ex.Message, "Sample Error").ShowAsync();
			}
		}

		private void ReportStatus(string status)
		{
			txtStatus.Text = status;
		}
	}
}
