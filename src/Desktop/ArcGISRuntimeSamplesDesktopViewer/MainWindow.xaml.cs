using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Xml.Linq;

namespace ArcGISRuntime.Samples.DesktopViewer
{
	public partial class MainWindow : Window
	{
		private bool _isSdkInstalled;
		private Sample _currentSample;

		public MainWindow()
		{
			InitializeComponent();
			LoadSamples();
			CheckForLocalData();
			CheckIfSdkIsInstalled();
		}

		private string GetRuntimeVersionNumber()
		{
			var runtimeVersion = string.Empty;
			var assembly = System.Reflection.Assembly.Load(new AssemblyName("Esri.ArcGISRuntime"));
			var version = assembly.GetName().Version;
			// Extract version number, note build happens to be the 3rd place number, but we use it for the minor-minor version e.g. "10.1.1"
			runtimeVersion = version.Major + "." + version.Minor;
			if (version.Build != 0)
				runtimeVersion += "." + version.Build;

			return runtimeVersion;
		}

		// Checks if the SDK is installed to this machine.
		private void CheckIfSdkIsInstalled()
		{
			try
			{
				// Check if the SDK is installed using registry key
				using (RegistryKey Key =
					Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Wow6432Node\Microsoft\.NETFramework\v4.5.50709\AssemblyFoldersEx\ArcGIS Runtime SDK " + GetRuntimeVersionNumber()))
				{
					if (Key == null)
						_isSdkInstalled = false;
					else
						_isSdkInstalled = true;
				}

				if (!_isSdkInstalled) // Check 32bit registry
				{
					using (RegistryKey Key =
						Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\.NETFramework\v4.5.50709\AssemblyFoldersEx\ArcGIS Runtime SDK " + GetRuntimeVersionNumber()))
					{
						if (Key != null)
						{
							_isSdkInstalled = true;
						}
					}
				}
			}
			catch (Exception)
			{
				_isSdkInstalled = false;
			}
		}

		// Checks if deployment folder is found from the same folder where application is and
		// optionally if it has symbols folder in it. If it has it, we assume that all symbology files 
		// are deployed.
		private bool CheckIfHasDeploymentFolder(bool checkSymbolsFolder = true)
		{
			// Check if there is a deployment folder
			if (Directory.Exists("arcgisruntime" + GetRuntimeVersionNumber()))
			{
				if (checkSymbolsFolder)
				{
					// deployment folder is found, check that symbols are deployed
					if (Directory.Exists("arcgisruntime" + GetRuntimeVersionNumber() + "\\resources\\symbols"))
						return true; // found
					else
						return false; // not found
				}
				else
					return true; // found
			}

			return false; // not found
		}

		/// <summary>
		/// Checks if either 32 or 64 bit local server is found under deployment folder.
		/// </summary>
		private bool CheckIfLocalServerIsDeployed()
		{
			// deployment folder is found, check that symbols are deployed
			if (Directory.Exists("arcgisruntime" + GetRuntimeVersionNumber() + "\\LocalServer32") ||
				Directory.Exists("arcgisruntime" + GetRuntimeVersionNumber() + "\\LocalServer64"))
				return true;
			return false; // not found
		}

		/// <summary>
		/// Checks if deployment folder is located with application and if it contains local server.
		/// Return true both conditions are true, false other vise. If this returns true, deployment is ok for local server samples.
		/// </summary>
		private bool CheckIfHasDeploymentAndLocalServer()
		{
			if (CheckIfHasDeploymentFolder(false) && CheckIfLocalServerIsDeployed())
				return true;

			return false;
		}

		/// <summary>
		/// Checks if SDK is installed and no deployment is done. This means that centralized developer deployment is used.
		/// </summary>
		private bool CheckIfSdkIsInstalledAndNoDeploymentIsFound()
		{
			if (_isSdkInstalled && !CheckIfHasDeploymentFolder(false))
				return true;

			return false;
		}

		private void CheckForLocalData()
		{
			if (!Directory.Exists(@"..\..\..\samples-data\"))
			{
				sampleDataNotFound.Visibility = System.Windows.Visibility.Visible;
			}
		}

		private void LoadSamples()
		{
			var samples = SampleDatasource.Current.SamplesByCategory;
			foreach (var group in samples)
			{
				MenuItem samplesItem = new MenuItem() { Header = group.Key };
				foreach (var sample in group.Items.Where(g => g.Subcategory == null))
				{
					CreateSampleMenuItem(samplesItem, sample);
				}
                var subGroups = group.Items.GroupBy(g => g.Subcategory);
                foreach (var subGroup in subGroups.Where(sg => sg.Key != null))
                {
                    MenuItem subGroupItem = new MenuItem() { Header = subGroup.Key };
                    foreach (var sample in subGroup)
                    {
                        CreateSampleMenuItem(subGroupItem, sample);
                    }
                    samplesItem.Items.Add(subGroupItem);
                }	
                
				menu.Items.Add(samplesItem);
			}
		}

		private void CreateSampleMenuItem(MenuItem parentMenu, Sample sample)
		{
			MenuItem sampleitem = new MenuItem()
			{
				Header = sample.Name,
				ToolTip = new TextBlock() { Text = sample.Description, MaxWidth = 300, TextWrapping = TextWrapping.Wrap }
			};
			parentMenu.Items.Add(sampleitem);
			sampleitem.Click += (s, e) => { sampleitem_Click(sample, s as MenuItem); };
		}

		MenuItem currentSampleMenuItem;
		private void sampleitem_Click(Sample sample, MenuItem menu)
		{
			var isSampleAvailable = true;

			// Check if sample needs SDK installation and if it's available
			// If build with using Nuget reference, deployment folder is copied under the bin folder
			// without symbols or other deployable extensions. 
			if (sample.RequiresSymbols && !(CheckIfHasDeploymentFolder() || CheckIfSdkIsInstalledAndNoDeploymentIsFound()))
				isSampleAvailable = false;

			// Check if local server is needed and if it's available
			if (sample.RequiresLocalServer && !(CheckIfHasDeploymentAndLocalServer() || CheckIfSdkIsInstalledAndNoDeploymentIsFound()))
				isSampleAvailable = false;

			if (!isSampleAvailable)
			{
				// Todo deploy local server.
				SampleContainer.Child = new SdkInstallNeeded();

				if (currentSampleMenuItem != null)
					currentSampleMenuItem.IsChecked = false;

				StatusBar.DataContext = new Sample() { Description = "Sample isn't available.",	UserControl = typeof(SdkInstallNeeded)};

				return;
			}

			var c = sample.UserControl.GetConstructor(new Type[] { });
			var ctrl = c.Invoke(new object[] { }) as UIElement;
			SampleContainer.Child = ctrl;
			if (currentSampleMenuItem != null)
				currentSampleMenuItem.IsChecked = false;
			menu.IsChecked = true;
			currentSampleMenuItem = menu;
			StatusBar.DataContext = sample;

			_currentSample = sample;

			GC.Collect();
			GC.WaitForPendingFinalizers();
		}

		private void CommandBinding_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
		{
			TakeThumbnail();
		}

		private async void TakeThumbnail()
		{
			if (_currentSample == null)
			{
				MessageBox.Show("Please select Live Sample to before creating a thumbnail.");
				return;
			}

			var rtb = new RenderTargetBitmap((int)SampleContainer.ActualWidth, (int)SampleContainer.ActualHeight, 96, 96, PixelFormats.Pbgra32);
			rtb.Render(SampleContainer);

			// Encoding the RenderBitmapTarget as a PNG file.
			PngBitmapEncoder png = new PngBitmapEncoder();
			png.Frames.Add(BitmapFrame.Create(rtb));

			if (!Directory.Exists("samples"))
			{
				Directory.CreateDirectory("samples");
			}

			var file = new System.IO.FileInfo(System.IO.Path.Combine("samples", _currentSample.Name + ".png"));
			if (file.Exists)
			{
				await Task.Delay(1000);

				file.Delete();
				using (System.IO.Stream stm = System.IO.File.Create(file.FullName))
				{
					png.Save(stm);
				}
			}
			else
			{
				using (System.IO.Stream stm = System.IO.File.Create(file.FullName))
				{
					png.Save(stm);
				}
			}
		}
	}

	public class SampleDatasource
	{
		private SampleDatasource()
		{
			var samplesAssembly = Assembly.Load("ArcGISRuntimeSamplesDesktop");

			var pages = from t in samplesAssembly.ExportedTypes
						where t.GetTypeInfo().IsSubclassOf(typeof(UserControl)) && t.FullName.Contains(".Samples.")
						select t;

			Samples = (from p in pages
					   select new Sample()
					   {
						   UserControl = p,
						   Name = SplitCamelCasedWords(p.Name),
						   SampleFile = p.Name + ".xaml",
						   Category = "Misc"
					   }).ToArray();

			//Update descriptions and category based on included XML Doc
			XDocument xdoc = null;
			try
			{
				xdoc = XDocument.Load(new StreamReader(
					samplesAssembly.GetManifestResourceStream("ArcGISRuntime.Samples.Desktop.Assets.SampleDescriptions.xml")));
				foreach (XElement member in xdoc.Descendants("member"))
				{
					try
					{
						string name = (string)member.Attribute("name");
						if (name == null)
							continue;
						bool isType = name.StartsWith("T:", StringComparison.OrdinalIgnoreCase);
						if (isType)
						{
							var match = (from s in Samples where name == "T:" + s.UserControl.FullName select s).FirstOrDefault();
							if (match != null)
							{
								var title = member.Descendants("title").FirstOrDefault();
								if (title != null && !string.IsNullOrWhiteSpace(title.Value))
									match.Name = title.Value.Trim();
								var summary = member.Descendants("summary").FirstOrDefault();
								if (summary != null)
									match.Description = summary.Value.Trim().Replace("<br/>","\n");
								var category = member.Descendants("category").FirstOrDefault();
								if (category != null)
									match.Category = category.Value.Trim();
								var subcategory = member.Descendants("subcategory").FirstOrDefault();
								if (subcategory != null)
									match.Subcategory = subcategory.Value.Trim();

								// Get information if the sample needs symbol installation
								var requiresSymbols = member.Descendants("requiresSymbols").FirstOrDefault();
								if (requiresSymbols != null && requiresSymbols.Value is string)
								{
									var result = false;
									bool.TryParse(requiresSymbols.Value.Trim(), out result);
									match.RequiresSymbols = result;
								}

								// Get information if the sample needs LocalServer
								var requiresLocalServer = member.Descendants("requiresLocalServer").FirstOrDefault();
								if (requiresLocalServer != null && requiresLocalServer.Value is string)
								{
									var result = false;
									bool.TryParse(requiresLocalServer.Value.Trim(), out result);
									match.RequiresLocalServer = result;
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
			get
			{
				List<SampleGroup> groups = new List<SampleGroup>();
				List<string> groupOrder = new List<string>( new [] { "Mapping", "Scene", "Layers", "Geometry", "Symbology", "Tasks", "Offline", "Printing", "Portal", "Security", "Extras" } );
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

		public Type UserControl { get; set; }
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
		/// Defines if the sample needs symbols to work. Defaults to false.
		/// </summary>
		/// <remarks>This is used for sample that needs something to being deployed like military symbology or S57 symbology.</remarks>
		public bool RequiresSymbols { get; set; }

		/// <summary>
		/// Defines if the sample needs local data to work.
		/// </summary>
		public bool RequiresLocalData { get; set; }

		/// <summary>
		/// Defines if the sample needs Local server to work. Defaults to false.
		/// </summary>
		/// <remarks>Only used in desktop.</remarks>
		public bool RequiresLocalServer { get; set; }
	}
}
