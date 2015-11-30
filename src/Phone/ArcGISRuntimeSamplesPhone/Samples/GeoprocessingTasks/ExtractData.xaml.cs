using Esri.ArcGISRuntime.Controls;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Layers;
using Esri.ArcGISRuntime.Tasks.Geoprocessing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Windows.ApplicationModel.Activation;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Provider;
using Windows.Storage.Streams;
using Windows.UI.Popups;
using Windows.UI.Xaml;

namespace ArcGISRuntime.Samples.Phone.Samples
{
	/// <summary>
	/// This sample demonstrates how to work with an asynchronous Extract Data geoprocessing service. The Extract Data Task lets users specify an area of interest then download a zip file that contains data for that area from one or more layers.
	/// </summary>
	/// <title>Extract Data</title>
	/// <category>Geoprocessing Tasks</category>
	public partial class ExtractData : Windows.UI.Xaml.Controls.Page
	{
		private const string ExtractDataServiceUrl =
			"http://sampleserver4.arcgisonline.com/ArcGIS/rest/services/HomelandSecurity/Incident_Data_Extraction/GPServer/Extract%20Data%20Task";

		private GraphicsOverlay _graphicsOverlay;
		private Geoprocessor _gpTask;

		/// <summary>Construct Extract Data sample control</summary>
		public ExtractData()
		{
			InitializeComponent();

			_graphicsOverlay = MyMapView.GraphicsOverlays["graphicsOverlay"];
			_gpTask = new Geoprocessor(new Uri(ExtractDataServiceUrl));

			SetupUI();
		}

		// Sets up UI choices from the extract data service information
		private async void SetupUI()
		{
			try
			{
				var info = await _gpTask.GetTaskInfoAsync();

				listLayers.ItemsSource = info.Parameters.First(p => p.Name == "Layers_to_Clip").DefaultValue;

				comboFormat.ItemsSource = info.Parameters.First(p => p.Name == "Feature_Format").ChoiceList;
				if (comboFormat.ItemsSource != null && comboFormat.Items.Count > 0)
					comboFormat.SelectedIndex = 0;
			}
			catch (Exception ex)
			{
				var _x = new MessageDialog(ex.Message, "Sample Error").ShowAsync();
			}
		}

		// Gets the users digitized area of interest polygon
		private async void AreaOfInterestButton_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				_graphicsOverlay.Graphics.Clear();

				Polygon aoi = null;
				if (chkFreehand.IsChecked == true)
				{
					var boundary = await MyMapView.Editor.RequestShapeAsync(DrawShape.Freehand) as Polyline;
					if (boundary.Parts.First().Count <= 1)
						return;

					aoi = new Polygon(boundary.Parts, MyMapView.SpatialReference);
					aoi = GeometryEngine.Simplify(aoi) as Polygon;
				}
				else
				{
					aoi = await MyMapView.Editor.RequestShapeAsync(DrawShape.Polygon) as Polygon;
				}

				_graphicsOverlay.Graphics.Add(new Graphic(aoi));
			}
			catch (Exception ex)
			{
				var _x = new MessageDialog(ex.Message, "Sample Error").ShowAsync();
			}
		}

		// Calls the ExtractData service and prompts the user for saving the results
		private async void ExtractDataButton_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				progress.Visibility = txtStatus.Visibility = Visibility.Visible;

				var layersToClip = listLayers.SelectedItems.OfType<string>().Select(s => new GPString(s, s)).ToList();
				if (layersToClip == null || layersToClip.Count == 0)
					throw new Exception("Please select layers to extract data from.");

				if (_graphicsOverlay.Graphics.Count == 0)
					throw new Exception("Please digitize an area of interest polygon on the map.");

				var parameter = new GPInputParameter() { OutSpatialReference = SpatialReferences.WebMercator };
				parameter.GPParameters.Add(new GPMultiValue<GPString>("Layers_to_Clip", layersToClip));
				parameter.GPParameters.Add(new GPFeatureRecordSetLayer("Area_of_Interest", _graphicsOverlay.Graphics[0].Geometry));
				parameter.GPParameters.Add(new GPString("Feature_Format", (string)comboFormat.SelectedItem));

				var result = await SubmitAndPollStatusAsync(parameter);

				if (result.JobStatus == GPJobStatus.Succeeded)
				{
					txtStatus.Text = "Finished processing. Retrieving results...";

					var outParam = await _gpTask.GetResultDataAsync(result.JobID, "Output_Zip_File") as GPDataFile;
					if (outParam != null && outParam.Uri != null)
					{
						await SaveResultsToFile(outParam);
					}
				}
			}
			catch (Exception ex)
			{
				var _x = new MessageDialog(ex.Message, "Sample Error").ShowAsync();
			}
			finally
			{
				progress.Visibility = txtStatus.Visibility = Visibility.Collapsed;
			}
		}

		// Submit GP Job and Poll the server for results every 2 seconds.
		private async Task<GPJobInfo> SubmitAndPollStatusAsync(GPInputParameter parameter)
		{
			// Submit gp service job
			var result = await _gpTask.SubmitJobAsync(parameter);

			// Poll for the results async
			while (result.JobStatus != GPJobStatus.Cancelled && result.JobStatus != GPJobStatus.Deleted
				&& result.JobStatus != GPJobStatus.Succeeded && result.JobStatus != GPJobStatus.TimedOut)
			{
				result = await _gpTask.CheckJobStatusAsync(result.JobID);

				txtStatus.Text = string.Join(Environment.NewLine, result.Messages.Select(x => x.Description));

				await Task.Delay(2000);
			}

			return result;
		}

		// Saves results to a local file - with user prompt for file name
		private async Task SaveResultsToFile(GPDataFile gpDataFile)
		{
			var dlg = new MessageDialog("Data file created. Would you like to download the file?", "Success");
			dlg.Commands.Add(new UICommand("Yes", async cmd => { await DownloadDataAndDisplayAsync(gpDataFile.Uri); }));
			dlg.Commands.Add(new UICommand("No"));
			await dlg.ShowAsync();
		}

		private async Task DownloadDataAndDisplayAsync(Uri dataUri)
		{
			try
			{
				HttpClient client = new HttpClient();
				var response = await client.GetAsync(dataUri);
				var zipStream = await response.EnsureSuccessStatusCode().Content.ReadAsStreamAsync();
				var file = await SaveFileAsync(zipStream.AsRandomAccessStream());

				CachedFileManager.DeferUpdates(file);

				using (var stream = await file.OpenAsync(FileAccessMode.ReadWrite))
				{
					await zipStream.CopyToAsync(stream.AsStreamForWrite());
				}

				FileUpdateStatus status = await CachedFileManager.CompleteUpdatesAsync(file);
			}
			catch (Exception ex)
			{
				var _x = new MessageDialog(ex.Message, "Sample Error").ShowAsync();
			}
		}


		private Task<StorageFile> SaveFileAsync(IRandomAccessStream stream)
		{
			// See http://msdn.microsoft.com/en-us/library/windows/apps/xaml/dn614994.aspx
			// For ContinuationManager complete implementation.
			// A smaller version of this is also available in the sample and in the App.xaml.cs.
			var tcs = new TaskCompletionSource<StorageFile>();
			EventHandler<FileSavePickerContinuationEventArgs> fileSavedHandler = null;
			fileSavedHandler = (s, e) =>
			{
				ContinuationManager.Current.FilePickerSaved -= fileSavedHandler;
				var file = e.File;
				if (file == null)
					tcs.TrySetCanceled();
				else
					tcs.TrySetResult(file);
			};

			ContinuationManager.Current.FilePickerSaved += fileSavedHandler;

			FileSavePicker filePicker = new FileSavePicker();
			filePicker.SuggestedStartLocation = PickerLocationId.DocumentsLibrary;
			filePicker.FileTypeChoices.Add("ZIP", new List<string>() { ".zip" });
			filePicker.DefaultFileExtension = ".zip";
			filePicker.SuggestedFileName = "Output";
			filePicker.PickSaveFileAndContinue();
			return tcs.Task;
		}
	}
}
