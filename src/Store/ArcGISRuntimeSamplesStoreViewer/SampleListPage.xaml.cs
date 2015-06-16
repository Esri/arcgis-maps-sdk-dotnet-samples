using Esri.ArcGISRuntime;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using Windows.Storage;
using Windows.UI.Xaml.Controls;

namespace ArcGISRuntime.Samples.StoreViewer
{
	/// <summary>
	/// Display an overview of all the samples and allow user to pick a sample to run
	/// </summary>
	public sealed partial class SampleListPage : Page
	{
		private const string SAMPLE_ASSEMBLY_NAME = "ArcGISRuntimeSamplesStore";
		private const string SAMPLE_DESCRIPTION_NAME = "ArcGISRuntime.Samples.Store.Assets.SampleDescriptions.xml";

		private bool _hasDeployment;

		public SampleListPage()
		{
			ArcGISRuntimeEnvironment.SymbolsPath = @"arcgisruntime" + GetRuntimeVersionNumber() + @"\resources\symbols";
			this.InitializeComponent();
			DataContext = SampleDatasource.Current;

			CheckDeployment();
		}

		private string GetRuntimeVersionNumber()
		{
			// Get version number that is used in the deployment folder
			Assembly runtimeAssembly = typeof(ArcGISRuntimeEnvironment).GetTypeInfo().Assembly;

			var sdkVersion = string.Empty;
			var attr = CustomAttributeExtensions.GetCustomAttribute<AssemblyFileVersionAttribute>(runtimeAssembly);
			if (attr != null)
			{
				var version = attr.Version;
				string[] versions = attr.Version.Split(new[] { '.' });

				// Ensure that we only look maximum of 3 part version number ie. 10.2.4
				int partCount = 3;
				if (versions.Count() < 3)
					partCount = versions.Count();

				for (var i = 0; i < partCount; i++)
				{
					if (string.IsNullOrEmpty(sdkVersion))
						sdkVersion = versions[i];
					else
						sdkVersion += "." + versions[i];
				}
			}
			else
				throw new Exception("Cannot read version number from ArcGIS Runtime");

			return sdkVersion;
		}

		private async void CheckDeployment()
		{
			try
			{
				// Check that all folders are deployed - assuming that symbols folder contains all 
				// deployable dictionaries
				var appFolder = Windows.ApplicationModel.Package.Current.InstalledLocation;
				var runtimeFolder = await appFolder.GetFolderAsync("arcgisruntime" + GetRuntimeVersionNumber());
				var resourcesFolder = await runtimeFolder.GetFolderAsync("resources");
				var symbolsFolders = await resourcesFolder.GetFolderAsync("symbols");

				_hasDeployment = true;
			}
			catch (FileNotFoundException)
			{
				_hasDeployment = false;
			}
		}

		private void GridView_ItemClick(object sender, ItemClickEventArgs e)
		{
			var item = (Sample)e.ClickedItem;

			// Check if sample needs symbols and if deployment is available with symbols
			if (item.RequiresSymbols && !_hasDeployment)
			{
				// Deployment folder is not found show sample not available page
				Frame.Navigate(typeof(SdkInstallNeededPage));
				AppState.Current.CurrentSampleTitle = "Sample not available";
				return;
			}

			// Sample is available
			Frame.Navigate(item.Page);
			AppState.Current.CurrentSampleTitle = item.Name;
		}

		public class SampleDatasource
		{
			private SampleDatasource()
			{
				// Load assembly
				var samplesAssembly = Assembly.Load(new AssemblyName(SAMPLE_ASSEMBLY_NAME));

				// Get all samples from the assembly
				var sampleTypes = from t in samplesAssembly.ExportedTypes
								  where t.GetTypeInfo().IsSubclassOf(typeof(Page)) && t.FullName.Contains(".Samples.")
								  select t;

				Samples = (from p in sampleTypes
						   select new Sample()
						   {
							   Page = p,
							   Name = SplitCamelCasedWords(p.Name),
							   SampleFile = p.Name + ".xaml",
							   Category = "Misc"
						   }).ToArray();

				//Update descriptions and category based on included XML Doc
				XDocument sampleDescriptions = null;
				try
				{
					sampleDescriptions = XDocument.Load(new StreamReader(
						samplesAssembly.GetManifestResourceStream(SAMPLE_DESCRIPTION_NAME)));

					foreach (XElement member in sampleDescriptions.Descendants("member"))
					{
						try
						{
							string name = (string)member.Attribute("name");
							if (name == null)
								continue;
							bool isType = name.StartsWith("T:", StringComparison.OrdinalIgnoreCase);
							if (isType)
							{
								var match = (from s in Samples where name == "T:" + s.Page.FullName select s).FirstOrDefault();
								if (match != null)
								{
									var title = member.Descendants("title").FirstOrDefault();
									if (title != null && !string.IsNullOrWhiteSpace(title.Value))
										match.Name = title.Value.Trim();
									var summary = member.Descendants("summary").FirstOrDefault();
									if (summary != null && summary.Value is string)
										match.Description = summary.Value.Trim();
									var category = member.Descendants("category").FirstOrDefault();
									if (category != null && category.Value is string)
										match.Category = category.Value.Trim();
									var subcategory = member.Descendants("subcategory").FirstOrDefault();
									if (subcategory != null && category.Value is string)
										match.Subcategory = subcategory.Value.Trim();
									
									// Get information if the sample needs symbols
									var requiresSymbols = member.Descendants("requiresSymbols").FirstOrDefault();
									if (requiresSymbols != null && requiresSymbols.Value is string)
									{
										var result = false;
										bool.TryParse(requiresSymbols.Value.Trim(), out result);
										match.RequiresSymbols = result;
									}

									// Get samples type
									var sampleType = member.Descendants("sampleType").FirstOrDefault();
									if (sampleType != null && sampleType.Value is string)
									{
										var value = (string)sampleType;
										if (value == "Workflow")
											match.Type = Sample.SampleType.Workflow;
										else
											match.Type = Sample.SampleType.API;

									}
								}
							}
						}
						catch { } //ignore
					}
				}
				catch { } //ignore
			}
			private static string SplitCamelCasedWords(string value)
			{
				var text = System.Text.RegularExpressions.Regex.Replace(value, "([a-z])([A-Z])", "$1 $2");
				return text.Replace("Arc GIS", "ArcGIS ");
			}
			
			public class SampleGroup 
			{
				public SampleGroup(IEnumerable<Sample> samples) 
				{
					Items = samples;
				}
				public string Key { get; set; }

				public IEnumerable<Sample> Items { get; private set; }
			}

			public List<SampleGroup> SamplesByCategory
			{
				get {
					List<SampleGroup> groups = new List<SampleGroup>();
					List<string> groupOrder = new List<string>(new[] { "Mapping", "Scene", "Tiled Layers", "Dynamic Service Layers", "Feature Layers", 
						"Graphics Layers", "Geometry", "Symbology", "Query Tasks", "Geocode Tasks", "Network Analyst Tasks", "Geoprocessing Tasks" });

					var query = (from item in Samples
								 orderby item.Category
								 group item by item.Category into g
								 select new { GroupName = g.Key, Items = g, GroupIndex = groupOrder.IndexOf(g.Key) })
									.OrderBy(g => g.GroupIndex < 0 ? int.MaxValue : g.GroupIndex);

					foreach (var g in query)
					{                        
						groups.Add(new SampleGroup(g.Items.OrderBy(i => i.Subcategory).ThenBy(i => i.Name)) { Key = g.GroupName });
					}

					// Define order of Mapping samples
					SampleGroup mappingSamplesGroup = groups.Where(i => i.Key == "Mapping").First();
					List<Sample> mappingSamples = new List<Sample>();
					mappingSamples.Add(mappingSamplesGroup.Items.Where(i => i.Name == "Switch Basemaps").First());
					mappingSamples.Add(mappingSamplesGroup.Items.Where(i => i.Name == "Set Initial Map Extent").First());
					mappingSamples.Add(mappingSamplesGroup.Items.Where(i => i.Name == "Set Initial Center and Scale").First());
					mappingSamples.Add(mappingSamplesGroup.Items.Where(i => i.Name == "Set Spatial Reference").First());
					mappingSamples.Add(mappingSamplesGroup.Items.Where(i => i.Name == "Show Map Extent").First());
					mappingSamples.Add(mappingSamplesGroup.Items.Where(i => i.Name == "Show Map Scale").First());
					mappingSamples.Add(mappingSamplesGroup.Items.Where(i => i.Name == "Show Mouse Coordinates").First());
					mappingSamples.Add(mappingSamplesGroup.Items.Where(i => i.Name == "Map Rotation").First());
					mappingSamples.Add(mappingSamplesGroup.Items.Where(i => i.Name == "Map Grid").First());
					mappingSamples.Add(mappingSamplesGroup.Items.Where(i => i.Name == "Swipe").First());
					mappingSamples.Add(mappingSamplesGroup.Items.Where(i => i.Name == "Map Overlays").First());
					mappingSamples.Add(mappingSamplesGroup.Items.Where(i => i.Name == "Group Layers").First());
					mappingSamples.Add(mappingSamplesGroup.Items.Where(i => i.Name == "Location Display").First());
					mappingSamples.Add(mappingSamplesGroup.Items.Where(i => i.Name == "Layer List").First());
					mappingSamples.Add(mappingSamplesGroup.Items.Where(i => i.Name == "Overview Map").First());
					//Add any missing samples
					foreach (var item in mappingSamplesGroup.Items)
						if (!mappingSamples.Contains(item))
							mappingSamples.Add(item);

					SampleGroup newMappingSamplesGroup = new SampleGroup(mappingSamples) { Key = mappingSamplesGroup.Key };
					groups[groups.FindIndex(g => g.Key == mappingSamplesGroup.Key)] = newMappingSamplesGroup;

					return groups;
				}
			}

			public IEnumerable<Sample> Samples { get; private set; }

			private static SampleDatasource m_Current;
			public static SampleDatasource Current
			{
				get
				{
					if (m_Current == null)
						m_Current = new SampleDatasource();
					return m_Current;
				}
			}
		}

		public class Sample
		{
			public enum SampleType
			{
				API,
				Workflow
			};

			public Type Page { get; set; }
			public string Name { get; set; }
			public string Category { get; set; }
			public string Subcategory { get; set; }
			public string Description { get; set; }
			public string SampleFile { get; set; }

			/// <summary>
			/// Defines the type of the sample. Current options are API and Workflow. 
			/// </summary>
			public SampleType Type { get; set; }

			/// <summary>
			/// Defines if the sample needs symbol to work. 
			/// </summary>
			/// <remarks>This is used for samples that need something to being deployed like military symbology or S57 symbology.</remarks>
			public bool RequiresSymbols { get; set; }

			/// <summary>
			/// Defines if the sample needs local data to work.
			/// </summary>
			public bool RequiresLocalData { get; set; }

			public override string ToString()
			{
				return Name;
			}
		}
	}
}
