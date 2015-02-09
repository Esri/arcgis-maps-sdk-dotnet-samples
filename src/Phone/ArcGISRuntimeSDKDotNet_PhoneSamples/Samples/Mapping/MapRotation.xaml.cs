using Esri.ArcGISRuntime.Controls;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace ArcGISRuntimeSDKDotNet_PhoneSamples.Samples
{
	/// <summary>
	/// This sample shows how to rotate a map using the MapView.Rotation property.
	/// </summary>
	/// <title>Map Rotation</title>
	/// <category>Mapping</category>
	public sealed partial class MapRotation : Page
	{
		public MapRotation()
		{
			this.InitializeComponent();

			// Since this is Phone, always have touch rotation enabled.
			MyMapView.InteractionOptions.RotationOptions.IsEnabled = true;	
		}

		private void rotationSlider_ValueChanged(object sender, Windows.UI.Xaml.Controls.Primitives.RangeBaseValueChangedEventArgs e)
		{
			MyMapView.SetRotation(e.NewValue);
		}
	}
}
