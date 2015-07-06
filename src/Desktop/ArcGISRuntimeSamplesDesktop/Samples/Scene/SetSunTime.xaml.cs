using Esri.ArcGISRuntime.Controls;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace ArcGISRuntime.Samples.Desktop
{
	/// <summary>
	/// Demonstrates how to use use the sun mode
	/// </summary>
	/// <title>3D Set Sun Time</title>
	/// <category>Scene</category>
	/// <subcategory>Mapping</subcategory>
	public partial class SetSunTime : UserControl
	{
		System.Windows.Threading.DispatcherTimer _myDispatcherTimer;
		private DateTime utcBerlin;

		public SetSunTime()
		{
			InitializeComponent();
			Initialize();
		}

		public async void Initialize()
		{
			await MySceneView.SetViewAsync(new Camera(52.4970586495449, 13.3387481843594, 739.703398887999, 320.56288091543763, 54.529512824712647));
		
			// Get the current time in Berlin
			utcBerlin = DateTime.UtcNow.AddHours(2);
			AnimateSunTimeLabel.Content = utcBerlin;

			// Set the AmbientLight and IsShadowsEnabled and then set the Sun Time
			MySceneView.AmbientLight = Colors.Gray;
			MySceneView.IsShadowsEnabled = true;
			MySceneView.SetSunTime(utcBerlin);
		}

		private void Animate_Click(object sender, RoutedEventArgs e)
		{
			string exceptionMessage = string.Empty;

			try
			{
				// Change the Content of the Animate button depending on whether it has been clicked. 
				AnimateSunTimeButton.Content = AnimateSunTimeButton.IsChecked.Value ? "Stop Animation" : "Animate Light Position";

				if (_myDispatcherTimer == null)
				{
					// Start a new Timer at 1 second intervals
					_myDispatcherTimer = new DispatcherTimer();
					_myDispatcherTimer.Interval = TimeSpan.FromSeconds(1);
					

					DateTime dt = utcBerlin;
					AnimateSunTimeLabel.Content = utcBerlin;
					int i = 0;
					MySceneView.SetSunTime(dt);
					_myDispatcherTimer.Tick += (s, p) =>
					{
						i++;
						if (i <= 24)
						{
							dt = dt.AddHours(1);
							MySceneView.SetSunTime(dt);
							AnimateSunTimeLabel.Content = dt;
						}
						else
							i = 1;
					};
					_myDispatcherTimer.Start();
				}
				else
				{
					_myDispatcherTimer.Stop();
					_myDispatcherTimer = null;
				}
			}
			catch (Exception ex)
			{
				exceptionMessage = ex.Message;
				if (_myDispatcherTimer != null)
					_myDispatcherTimer.Stop();
				_myDispatcherTimer = null;
			}

			if (!string.IsNullOrEmpty(exceptionMessage))
				MessageBox.Show(exceptionMessage, "Sample Error");
		}
	}
}
