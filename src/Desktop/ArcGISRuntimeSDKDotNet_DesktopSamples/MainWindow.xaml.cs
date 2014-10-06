using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Xml.Linq;

namespace ArcGISRuntimeSDKDotNet_DesktopSamples
{
   public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
			LoadSamples();
			CheckForLocalData();
        }

		private void CheckForLocalData()
		{
			if(!Directory.Exists(@"..\..\..\..\..\samples-data\"))
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
				var subGroups = group.Items.GroupBy(g => g.Subcategory);
				foreach(var subGroup in subGroups.Where(sg=>sg.Key != null))
				{
					MenuItem subGroupItem = new MenuItem() { Header = subGroup.Key };
					foreach (var sample in subGroup)
					{
						CreateSampleMenuItem(subGroupItem, sample);
					}
					samplesItem.Items.Add(subGroupItem);
				}
				foreach (var sample in group.Items.Where(g=>g.Subcategory == null))
				{
					CreateSampleMenuItem(samplesItem, sample);
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
			var c = sample.UserControl.GetConstructor(new Type[] { });
			var ctrl = c.Invoke(new object[] { }) as UIElement;
			SampleContainer.Child = ctrl;
			if (currentSampleMenuItem != null)
				currentSampleMenuItem.IsChecked = false;
			menu.IsChecked = true;
			currentSampleMenuItem = menu;
			StatusBar.DataContext = sample;

			GC.Collect();
			GC.WaitForPendingFinalizers();
		}
    }

	public class SampleDatasource
	{
		private SampleDatasource()
		{
			var pages = from t in App.Current.GetType().GetTypeInfo().Assembly.ExportedTypes
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
					this.GetType().GetTypeInfo().Assembly.GetManifestResourceStream("ArcGISRuntimeSDKDotNet_DesktopSamples.Assets.SampleDescriptions.xml")));
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
				List<string> groupOrder = new List<string>( new [] { "Mapping", "Layers", "Geometry", "Symbology", "Tasks", "Offline", "Printing", "Portal", "Security", "Extras" } );
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
		public Type UserControl { get; set; }
		public string Name { get; set; }
		public string Category { get; set; }
		public string Subcategory { get; set; }
		public string Description { get; set; }
		public string SampleFile { get; set; }
	}
}
