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

namespace ArcGISRuntime.Samples.Store.Samples
{
    /// <summary>
    /// Demonstrates use of the Geoprocessor to call an asynchronous Clip Features geoprocessing service.
    /// </summary>
    /// <title>Clip Features</title>
    /// <category>Geoprocessing Tasks</category>
    public partial class ClipFeatures : Windows.UI.Xaml.Controls.Page
    {
        private static string ClipCountiesServiceUrl = "http://serverapps10.esri.com/ArcGIS/rest/services/SamplesNET/USA_Data_ClipTools/GPServer/ClipCounties";

        private Geoprocessor _gpTask;
        private GraphicsOverlay _inputOverlay;
        private GraphicsOverlay _resultsOverlay;

        /// <summary>Construct Clip Features sample control</summary>
        public ClipFeatures()
        {
            InitializeComponent();

			_inputOverlay = MyMapView.GraphicsOverlays["inputOverlay"];
			_resultsOverlay = MyMapView.GraphicsOverlays["resultsOverlay"];

            _gpTask = new Geoprocessor(new Uri(ClipCountiesServiceUrl));

            //Uncomment the following line to show the service parameters at startup.
            //GetServiceInfo();
        }

        // Get the users input line on the map and fire off a GP Job to clip features
        private async void StartButton_Click(object sender, RoutedEventArgs e)
        {
			try
			{
				_inputOverlay.Graphics.Clear();
				_resultsOverlay.Graphics.Clear();

				foreach (var lyr in MyMapView.Map.Layers.OfType<GPResultImageLayer>())
					MyMapView.Map.Layers.Remove(lyr);

				//get the user's input line
				var inputLine = await MyMapView.Editor.RequestShapeAsync(DrawShape.Polyline) as Polyline;

				progress.Visibility = Visibility.Visible;
				_inputOverlay.Graphics.Add(new Graphic() { Geometry = inputLine });

				var parameter = new GPInputParameter();
				parameter.GPParameters.Add(new GPFeatureRecordSetLayer("Input_Features", inputLine));
				parameter.GPParameters.Add(new GPLinearUnit("Linear_unit", LinearUnits.Miles, Int32.Parse(txtMiles.Text)));

				var result = await SubmitAndPollStatusAsync(parameter);

				if (result.JobStatus == GPJobStatus.Succeeded)
				{
					txtStatus.Text = "Finished processing. Retrieving results...";

					var resultData = await _gpTask.GetResultDataAsync(result.JobID, "Clipped_Counties");
					if (resultData is GPFeatureRecordSetLayer)
					{
						GPFeatureRecordSetLayer gpLayer = resultData as GPFeatureRecordSetLayer;
						if (gpLayer.FeatureSet.Features.Count == 0)
						{
							var resultImageLayer = await _gpTask.GetResultImageLayerAsync(result.JobID, "Clipped_Counties");

							GPResultImageLayer gpImageLayer = resultImageLayer;
							gpImageLayer.Opacity = 0.5;
							MyMapView.Map.Layers.Add(gpImageLayer);
							txtStatus.Text = "Greater than 500 features returned.  Results drawn using map service.";
							return;
						}

						_resultsOverlay.Graphics.AddRange(gpLayer.FeatureSet.Features.OfType<Graphic>());
					}
				}
			}
			catch (TaskCanceledException) { }
			catch (Exception ex)
			{
				var _x = new MessageDialog(ex.Message, "Sample Error").ShowAsync();
			}
            finally
            {
                progress.Visibility = Visibility.Collapsed;
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

        // Display service info
        private async void GetServiceInfo()
        {
            string message = null;

            try
            {
                var result = await _gpTask.GetTaskInfoAsync();

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
            }
            catch (Exception ex)
            {
                message = ex.Message;
            }

            var _x = new MessageDialog(message, "Service Info").ShowAsync();
        }
    }
}
