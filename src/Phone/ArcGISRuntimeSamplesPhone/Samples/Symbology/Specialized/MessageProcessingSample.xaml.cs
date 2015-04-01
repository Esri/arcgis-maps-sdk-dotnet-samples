using Esri.ArcGISRuntime.Controls;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Layers;
using Esri.ArcGISRuntime.Symbology.Specialized;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using Windows.Foundation;
using Windows.Storage;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace ArcGISRuntime.Samples.Phone.Samples.Symbology.Specialized
{
	/// <summary>
	/// Sample shows how to read and process Mil2525C message data from XML file. 
	/// </summary>
	/// <title>Message Processor</title>
	/// <category>Symbology</category>
	/// <subcategory>Specialized</subcategory>
	public sealed partial class MessageProcessingSample : Page
	{
		private const string DATA_PATH = @"symbology\Mil2525CMessages.xml";

		private MessageLayer _messageLayer;

		// The list of selected Messages to clear.
		private List<MilitaryMessage> selectedMessages = new List<MilitaryMessage>();

		public MessageProcessingSample()
		{
			InitializeComponent();

			MyMapView.ExtentChanged += mapView_ExtentChanged;
		}

		// Load data - enable functionality after layers are loaded.
		private async void mapView_ExtentChanged(object sender, EventArgs e)
		{
			try
			{
				MyMapView.ExtentChanged -= mapView_ExtentChanged;

				// Wait until all layers are loaded
				var layers = await MyMapView.LayersLoadedAsync();

				_messageLayer = MyMapView.Map.Layers.OfType<MessageLayer>().First();
				processMessagesBtn.IsEnabled = true;
			}
			catch (Exception ex)
			{
				var _ = new MessageDialog(ex.Message, "Message Processing Sample").ShowAsync();
			}
		}

		private async void ProcessMessagesButton_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				// This function simulates real time message processing by processing a static set of messages from an XML document.
				/* 
				* |== Example Message ==|
				* 
				* <message>
				*      <_type>position_report</_type>
				*      <_action>update</_action>
				*      <_id>16986029-8295-48d1-aa6a-478f400a53c0</_id>
				*      <_wkid>3857</_wkid>
				*      <sic>GFGPOLKGS-----X</sic>
				*      <_control_points>-226906.99878,6679149.88998;-228500.51759,6677576.8009;-232194.67644,6675625.78198</_control_points>
				*      <uniquedesignation>DIRECTION OF ATTACK</uniquedesignation>
				* </message>
				*/

				var file = await ApplicationData.Current.LocalFolder.GetFileAsync(DATA_PATH);
				using (var stream = await file.OpenStreamForReadAsync())
				{
					XDocument xmlDocument = XDocument.Load(stream);

					// Create a collection of messages
					IEnumerable<XElement> messagesXml = from n in xmlDocument.Root.Elements() where n.Name == "message" select n;

					// Iterate through the messages passing each to the ProcessMessage method on the MessageProcessor.
					foreach (XElement messageXml in messagesXml)
					{
						Message message = new Message(from n in messageXml.Elements() select new KeyValuePair<string, string>(n.Name.ToString(), n.Value));
						var messageProcessingSuccesful = _messageLayer.ProcessMessage(message);
						
						if (messageProcessingSuccesful == false)
						{
							var _ = new MessageDialog("Could not process the message.", "Message Processing Sample").ShowAsync();
						}
					}
				}

				/*
				* Alternatively you can programmatically construct the message and set the attributes.
				* e.g.
				* 
				* // Create a new message
				* Message msg = new Message();           
				* 
				* // Set the ID and other parts of the message
				* msg.Id = messageID;
				* msg.Add("_type", "position_report");
				* msg.Add("_action", "update");
				* msg.Add("_control_points", X.ToString(CultureInfo.InvariantCulture) + "," + Y.ToString(CultureInfo.InvariantCulture));
				* msg.Add("_wkid", "3857");
				* msg.Add("sic", symbolID);
				* msg.Add("uniquedesignation", "1");
				* 
				* // Process the message using the MessageProcessor within the MessageGroupLayer
				* _messageLayer.ProcessMessage(msg);
				*/

				// Hide info box
				infoBox.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
			}
			catch(FileNotFoundException)
			{
				var _ = new MessageDialog("Local message data not found. Please download sample data before using this sample.", "Message Processing Sample").ShowAsync();
			}
			catch (Exception ex)
			{
				var _ = new MessageDialog(ex.Message, "Message Processing Sample").ShowAsync();
			}
		}

		// Select Messages individually
		private async void AddSelectButton_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				await FindIntersectingGraphicsAsync(DrawShape.Point);
			}
			catch (Exception ex)
			{
				var _x = new MessageDialog("Selection Error: " + ex.Message, "Message Processing Sample").ShowAsync();
			}
		}

		// Select Messages by envelope
		private async void MultipleSelectButton_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				await FindIntersectingGraphicsAsync(DrawShape.Envelope);
			}
			catch (Exception ex)
			{
				var _x = new MessageDialog("Selection Error: " + ex.Message, "Message Processing Sample").ShowAsync();
			}
		}

		// Performs a HitTest on the rendered Messages and selects the results
		private async Task FindIntersectingGraphicsAsync(DrawShape drawMode)
		{
			// Get sub layers of the MessageLayer
			var messageSubLayers = _messageLayer.ChildLayers.Cast<MessageSubLayer>();

			// Create an empty result set
			IEnumerable<Graphic> results = Enumerable.Empty<Graphic>();

			// Set the max hits to 1
			int maxHits = 1;

			// Handle the individual Message selection mode
			if (drawMode == DrawShape.Point)
			{
				// Ask the user for the point of interest
				MapPoint mapPoint = null;
				try
				{
					mapPoint = await MyMapView.Editor.RequestPointAsync();
				}
				catch (TaskCanceledException) { }

				// Check the geometry
				if (Geometry.IsNullOrEmpty(mapPoint))
					return;

				// Get the location in screen coordinates
				var screenPoint = MyMapView.LocationToScreen(mapPoint);
						
				// Iterate the Message sub layers and await the HitTestAsync method on each layer
				foreach (var l in messageSubLayers)
					results = results.Concat(await l.HitTestAsync(MyMapView, screenPoint, maxHits));
			}	
			// Handle the multiple Message selection mode
			else
			{
				// Increase the max hits value
				maxHits = 100;
	
				// Ask the user for the area of interest
				Envelope envelope = null;
				try
				{
					envelope = await MyMapView.Editor.RequestShapeAsync(drawMode) as Envelope;
				}
				catch (TaskCanceledException) { }

				// Check the geometry
				if (Geometry.IsNullOrEmpty(envelope))
					return;
								
				// Get the screen location of the upper left
				var upperLeft = MyMapView.LocationToScreen
					(new MapPoint(envelope.XMin, envelope.YMax, envelope.SpatialReference));
								
				// Get the screen location of the lower right
				var lowerRight = MyMapView.LocationToScreen
					(new MapPoint(envelope.XMax, envelope.YMin, envelope.SpatialReference));

				// Create a Rect from the two corners
				var rect = new Rect(upperLeft, lowerRight);

				// Iterate the Message sub layers and await the HitTestAsync method on each layer
				foreach (var l in messageSubLayers)
					results = results.Concat(await l.HitTestAsync(MyMapView, rect, maxHits));
			}

			if (results.Count() == 0)
				return;

			// Iterate the results and modify the Action value to Select then reprocess each Message
			foreach (var graphic in results)
			{
				// Retrieve the Message representation from the MessageLayer for each Graphic returned
				MilitaryMessage message = _messageLayer.GetMessage(graphic.Attributes["_id"].ToString()) as MilitaryMessage;

				// Modify the Action to Select
				message.MessageAction = MilitaryMessageAction.Select;

				// Reprocess the Message and add to the list
				if (_messageLayer.ProcessMessage(message))
				{
					selectedMessages.Add(message);
				}
			}
		}

		// Clear all selected Messages
		private void ClearSelectButton_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				// Iterate the collection of Messages
				foreach (MilitaryMessage message in selectedMessages)
				{
					// Modify the Action to UnSelect
					message.MessageAction = MilitaryMessageAction.UnSelect;

					// Reprocess the Message
					_messageLayer.ProcessMessage(message);
				}
				// Clear the collection
				selectedMessages.Clear();
			}
			catch (Exception ex)
			{
				var _x = new MessageDialog("Selection Error: " + ex.Message, "Message Processing Sample").ShowAsync();
			}
		}
	}
}