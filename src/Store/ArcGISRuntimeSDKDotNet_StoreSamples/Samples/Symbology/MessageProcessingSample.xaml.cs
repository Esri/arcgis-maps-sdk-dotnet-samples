using Esri.ArcGISRuntime;
using Esri.ArcGISRuntime.AdvancedSymbology;
using Esri.ArcGISRuntime.Geometry;
using Esri.ArcGISRuntime.Layers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using Windows.Storage;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace ArcGISRuntimeSDKDotNet_StoreSamples.Samples.Symbology
{
	/// <summary>
	/// Sample shows how to read and process Mil2525C message data from XML file. 
	/// </summary>
	/// <title>Message Processor</title>
	/// <category>Symbology</category>
	/// <subcategory>Advanced</subcategory>
	public sealed partial class MessageProcessingSample : Page
	{
		private const string DATA_PATH = @"symbology\Mil2525CMessages.xml";

		private MessageLayer _messageLayer;

		public MessageProcessingSample()
		{
			InitializeComponent();

			mapView.Map.InitialExtent = new EnvelopeBuilder(
					-245200, 6665900, -207000, 6687300, 
					SpatialReferences.WebMercator).ToGeometry();

			mapView.ExtentChanged += mapView_ExtentChanged;
		}

		// Load data - enable functionality after layers are loaded.
		private async void mapView_ExtentChanged(object sender, EventArgs e)
		{
			try
			{
				mapView.ExtentChanged -= mapView_ExtentChanged;

				// Wait until all layers are loaded
				var layers = await mapView.LayersLoadedAsync();

				_messageLayer = mapView.Map.Layers.OfType<MessageLayer>().First();
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
				var file = await ApplicationData.Current.LocalFolder.TryGetItemAsync(DATA_PATH);
				if (file == null)
					throw new Exception("Local message data not found. Please download sample data from 'Sample Data Settings'");


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

				// Load the XML document
				XDocument xmlDocument = XDocument.Load(file.Path, LoadOptions.None);

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
				var _ = new MessageDialog(ex.Message, "Message Processing Sample").ShowAsync();
			}
		}
	}
}