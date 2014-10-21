using Esri.ArcGISRuntime;
using Esri.ArcGISRuntime.Symbology.Specialized;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Layers;
using Esri.ArcGISRuntime.Controls;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Xml.Linq;
using System.Threading.Tasks;


namespace ArcGISRuntimeSDKDotNet_DesktopSamples.Samples.Symbology.Specialized
{
	/// <summary>
	/// Sample shows how to read and process Mil2525C message data from XML file. 
	/// </summary>
	/// <title>Message Processor</title>
	/// <category>Symbology</category>
	/// <subcategory>Specialized</subcategory>
	public partial class MessageProcessingSample : UserControl
	{
		private const string DATA_PATH = @"..\..\..\..\..\samples-data\symbology\Mil2525CMessages.xml";

		private MessageLayer _messageLayer;

		public MessageProcessingSample()
		{
			InitializeComponent();

			MyMapView.ExtentChanged += MyMapView_ExtentChanged;
		}

		// Load data - enable functionality after layers are loaded.
		private async void MyMapView_ExtentChanged(object sender, EventArgs e)
		{
			try
			{
				MyMapView.ExtentChanged -= MyMapView_ExtentChanged;

				// Wait until all layers are loaded
				await MyMapView.LayersLoadedAsync();
				
				_messageLayer = MyMapView.Map.Layers.OfType<MessageLayer>().First();
				processMessagesBtn.IsEnabled = true;
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message, "Message Processing Sample");
			}
		}

		private async void ProcessMessagesButton_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				await MyMapView.LayersLoadedAsync();
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

				var file = new FileInfo(DATA_PATH);

				// Load the XML document
				XDocument xmlDocument = XDocument.Load(file.FullName, LoadOptions.None);

				// Create a collection of messages
				IEnumerable<XElement> messagesXml = from n in xmlDocument.Root.Elements() 
													where n.Name == "message" 
													select n;

				// Iterate through the messages passing each to the ProcessMessage method on the MessageProcessor.
				// The MessageGroupLayer associated with this MessageProcessor will handle the creation of any 
				// GraphicsLayers and Graphic objects necessary to display the message.
				foreach (XElement messageXml in messagesXml)
				{
					Message message = new Message(from n in messageXml.Elements() select new KeyValuePair<string, string>(n.Name.ToString(), n.Value));
					_messageLayer.ProcessMessage(message);
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
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message, "Message Processing Sample");
			}
		}

		private List<MilitaryMessage> selectedMessages = new List<MilitaryMessage>();
		
		private async void AddSelectButton_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			try
			{
				await FindIntersectingGraphicsAsync(DrawShape.Point);
			}
			catch (Exception ex)
			{
				MessageBox.Show("Selection Error: " + ex.Message, "Message Processing Sample");
			}
		}

		private async void MultipleSelectButton_Click(object sender, RoutedEventArgs e)
		{
			try
			{
				await FindIntersectingGraphicsAsync(DrawShape.Envelope);
			}
			catch (Exception ex)
			{
				MessageBox.Show("Selection Error: " + ex.Message, "Message Processing Sample");
			}
		}

		private async Task FindIntersectingGraphicsAsync(DrawShape drawMode)
		{
			var messageSubLayers = _messageLayer.ChildLayers.Cast<MessageSubLayer>();

			IEnumerable<Graphic> results = Enumerable.Empty<Graphic>();
			
			int maxHits = 1;

			if (drawMode == DrawShape.Point)
			{
				var mapPoint = await MyMapView.Editor.RequestPointAsync();
				var screenPoint = MyMapView.LocationToScreen(mapPoint);
				foreach (var l in messageSubLayers)
					results = results.Concat(await l.HitTestAsync(MyMapView, screenPoint, maxHits)); 
			}
			else
			{
				maxHits = 100;
				var geometry = await MyMapView.Editor.RequestShapeAsync(drawMode);
				var mapEnvelope = (geometry as Envelope).Extent;
				var upperLeft = MyMapView.LocationToScreen
					(new MapPoint(mapEnvelope.XMin, mapEnvelope.YMax, geometry.SpatialReference));
				var lowerRight = MyMapView.LocationToScreen
					(new MapPoint(mapEnvelope.XMax, mapEnvelope.YMin, geometry.SpatialReference));
				var rect = new Rect(upperLeft, lowerRight);

				foreach (var l in messageSubLayers)
					results = results.Concat(await l.HitTestAsync(MyMapView, rect, maxHits));
			}

			if (results.Count() == 0)
				return;

			foreach (var graphic in results)
			{
				MilitaryMessage message = _messageLayer.GetMessage(graphic.Attributes["_id"].ToString()) as MilitaryMessage;
				message.MessageAction = MilitaryMessageAction.Select;
				if (_messageLayer.ProcessMessage(message))
				{
					selectedMessages.Add(message);
				}
				else
				{
					MessageBox.Show("Failed");
				}
			}
		}

		private void ClearSelectButton_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			try
			{
				foreach (MilitaryMessage message in selectedMessages)
				{
					message.MessageAction = MilitaryMessageAction.UnSelect;
					_messageLayer.ProcessMessage(message);
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show("Selection Error: " + ex.Message, "Message Processing Sample");
			}
		}
	}
}
