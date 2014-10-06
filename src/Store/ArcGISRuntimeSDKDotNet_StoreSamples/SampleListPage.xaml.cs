using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using Windows.UI.Xaml.Controls;

namespace ArcGISRuntimeSDKDotNet_StoreSamples
{
	/// <summary>
	/// Display an overview of all the samples and allow user to pick a sample to run
	/// </summary>
	public sealed partial class SampleListPage : Page
	{
		public SampleListPage()
		{
			this.InitializeComponent();
			DataContext = SampleDatasource.Current;
		}

		private void GridView_ItemClick(object sender, ItemClickEventArgs e)
		{
			var item = (Sample)e.ClickedItem;
			Frame.Navigate(item.Page);
			AppState.Current.CurrentSampleTitle = item.Name;
		}

		public class SampleDatasource
		{
			private SampleDatasource()
			{
				var pages = from t in App.Current.GetType().GetTypeInfo().Assembly.ExportedTypes
                            where t.GetTypeInfo().IsSubclassOf(typeof(Page)) && t.FullName.Contains(".Samples.")
							select t;

				Samples = (from p in pages
						   select new Sample()
						   {
							   Page = p,
							   Name = SplitCamelCasedWords(p.Name),
							   SampleFile = p.Name + ".xaml",
							   Category = "Misc"
						   }).ToArray();

				//Update descriptions and category based on included XML Doc
				XDocument xdoc = null;
				try
				{
					xdoc = XDocument.Load(new StreamReader(
						this.GetType().GetTypeInfo().Assembly.GetManifestResourceStream("ArcGISRuntimeSDKDotNet_StoreSamples.Assets.SampleDescriptions.xml")));
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
                    List<string> groupOrder = new List<string>(new[] { "Mapping", "Tiled Layers", "Dynamic Service Layers", "Feature Layers", 
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
			public Type Page { get; set; }
			public string Name { get; set; }
			public string Category { get; set; }
			public string Subcategory { get; set; }
			public string Description { get; set; }
			public string SampleFile { get; set; }

			public override string ToString()
			{
				return Name;
			}
		}
	}
}
