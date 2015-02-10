using Esri.ArcGISRuntime.Controls;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Layers;
using Esri.ArcGISRuntime.Tasks.Geoprocessing;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace ArcGISRuntime.Samples.Phone.Samples
{
	/// <summary>
	/// This sample demonstrates use of the Geoprocessor to call an asynchronous Clip Features geoprocessing service.
	/// </summary>
	/// <title>Clip Features</title>
	/// <category>Geoprocessing Tasks</category>
	public sealed partial class ClipFeatures : Page
	{
		private static string ServiceUri = "http://serverapps10.esri.com/ArcGIS/rest/services/SamplesNET/USA_Data_ClipTools/GPServer/ClipCounties";

		public ClipFeatures()
		{
			InitializeComponent();
			MyMapView.Map.InitialViewpoint = new Viewpoint(new Envelope(-130, 10, -70, 60));

			//Uncomment the following line to show the service parameters at startup.
			//var _ GetServiceInfo();

		}


		private async void StartGP_Click(object sender, RoutedEventArgs e)
		{
			StartGP.IsEnabled = false;
			ClearGraphics();
			ClearButton.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
			ProcessingTextBlock.Visibility = Visibility.Collapsed;

			var inputPolyline = await MyMapView.Editor.RequestShapeAsync(DrawShape.Polyline);

			var inputGraphic = new Graphic { Geometry = inputPolyline };
			GraphicsLayer inputLayer = MyMapView.Map.Layers["InputLayer"] as GraphicsLayer;
			inputLayer.Graphics.Add(inputGraphic);

			MyProgressRing.Visibility = Visibility.Visible;
			MyProgressRing.IsActive = true;

			string message = null;

			Geoprocessor task = new Geoprocessor(new Uri(ServiceUri));
			var inputParameter = new GPInputParameter();
			inputParameter.GPParameters.Add(new GPFeatureRecordSetLayer("Input_Features", inputPolyline));
			inputParameter.GPParameters.Add(new GPLinearUnit("Linear_unit", LinearUnits.Miles, Int32.Parse(DistanceTextBox.Text)));
			try
			{
				//Submit the job and await the results
				var gpJobInfo = await task.SubmitJobAsync(inputParameter);

				//Poll the server every 5 seconds for the status of the job.
				//Cancelled, Cancelling, Deleted, Deleting, Executing, Failed, New, Submitted, Succeeded, TimedOut, Waiting
				while (gpJobInfo.JobStatus != GPJobStatus.Cancelled &&
					gpJobInfo.JobStatus != GPJobStatus.Deleted &&
					 gpJobInfo.JobStatus != GPJobStatus.Failed &&
					 gpJobInfo.JobStatus != GPJobStatus.Succeeded &&
					 gpJobInfo.JobStatus != GPJobStatus.TimedOut)
				{
					gpJobInfo = await task.CheckJobStatusAsync(gpJobInfo.JobID);
					await Task.Delay(5000);

				}

				//Now that the job is completed, check whether the service returned the results as Features or as a GPResultImageLayer.
				//This can happen if the number of features to return exceeds the limit set on the service
				if (gpJobInfo.JobStatus == GPJobStatus.Succeeded)
				{
					var resultData = await task.GetResultDataAsync(gpJobInfo.JobID, "Clipped_Counties");
					if (resultData is GPFeatureRecordSetLayer)
					{
						GPFeatureRecordSetLayer gpLayer = resultData as GPFeatureRecordSetLayer;
						if (gpLayer.FeatureSet.Features.Count == 0)
						{
							var resultImageLayer = await task.GetResultImageLayerAsync(gpJobInfo.JobID, "Clipped_Counties");
							GPResultImageLayer gpImageLayer = resultImageLayer;
							gpImageLayer.Opacity = 0.5;
							MyMapView.Map.Layers.Add(gpImageLayer);
							ProcessingTextBlock.Visibility = Visibility.Visible;
							ProcessingTextBlock.Text = "Greater than 500 features returned.  Results drawn using map service.";
							return;
						}
						GraphicsLayer resultLayer = MyMapView.Map.Layers["MyResultGraphicsLayer"] as GraphicsLayer;
						foreach (Graphic g in gpLayer.FeatureSet.Features)
						{
							resultLayer.Graphics.Add(g);
						}
					}
				}
				MyProgressRing.Visibility = Visibility.Collapsed;
				MyProgressRing.IsActive = false;

				ClearButton.Visibility = Visibility.Visible;
				StartGP.IsEnabled = true;

			}
			catch (Exception ex)
			{
				message = ex.Message;
			}

			if (message != null)
				await new MessageDialog(message, "GP Failed").ShowAsync();
		}


		private void ClearButton_Click(object sender, RoutedEventArgs e)
		{
			ClearGraphics();
		}

		private void ClearGraphics()
		{
			if (MyMapView.Map.Layers.Any(l => l is GPResultImageLayer))
			{
				foreach (var o in (from l in MyMapView.Map.Layers
								   where l is GPResultImageLayer
								   select l).ToList())
					MyMapView.Map.Layers.Remove(o);
			}

			foreach (var layer in MyMapView.Map.Layers.OfType<GraphicsLayer>())
				(layer as GraphicsLayer).Graphics.Clear();

		}

		private async Task GetServiceInfo()
		{
			var t = new Geoprocessor(new Uri(ServiceUri));
			string message = null;
			try
			{
				var result = await t.GetTaskInfoAsync();
				#region Display Service Info
				var sb = new StringBuilder();
				if (result != null)
				{
					sb.Append("{");
					sb.AppendLine(string.Format("\t\"name\" : \"{0}\",", result.Name));
					sb.AppendLine(string.Format("\t\"displayName\" : \"{0}\",", result.DisplayName));
					sb.AppendLine(string.Format("\t\"category\" : \"{0}\",", result.Category));
					sb.AppendLine(string.Format("\t\"helpUrl\" : \"{0}\",", result.HelpUrl));
					sb.AppendLine(string.Format("\t\"executionType\" : \"esriExecutionType{0}\",", result.ExecutionType));
					sb.AppendLine("\t\"parameters\" : [");
					foreach (var p in result.Parameters)
					{
						sb.AppendLine("\t{");
						sb.AppendLine(string.Format("\t\t\"name\" : \"{0}\",", p.Name));
						sb.AppendLine(string.Format("\t\t\"dataType\" : \"{0}\",", p.DataType));
						sb.AppendLine(string.Format("\t\t\"displayName\" : \"{0}\",", p.DisplayName));
						sb.AppendLine(string.Format("\t\t\"direction\" : \"esriGPParameterDirection{0}\",", p.Direction));
						sb.AppendLine(string.Format("\t\t\"defaultValue\" : \"{0}\",", p.DefaultValue));
						sb.AppendLine(string.Format("\t\t\"parameterType\" : \"esriGPParameterType{0}\",", p.ParameterType));
						sb.AppendLine(string.Format("\t\t\"category\" : \"{0}\"", p.Category));
						if (p.ChoiceList != null)
						{
							sb.AppendLine("\t\t\"choiceList\" : [");
							foreach (var c in p.ChoiceList)
								sb.AppendLine(string.Format("\t\t\t\"{0}\"", c));

							sb.AppendLine("\t\t\"]");
						}
						sb.AppendLine("\t},");
					}
					sb.AppendLine("\t]");
					sb.Append("}");
					message = sb.ToString();
				}
				#endregion
			}
			catch (Exception ex)
			{
				message = ex.Message;
			}
			if (message != null)
				await new MessageDialog(message, "Service Info").ShowAsync();
		}
	}
}
