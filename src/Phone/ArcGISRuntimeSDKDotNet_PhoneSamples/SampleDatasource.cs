using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Reflection;
using System.Windows.Controls;

namespace ArcGISRuntimeSDKDotNet_PhoneSamples
{
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
                               Category = "Misc"
                           }).ToArray();

            //Update descriptions and category based on included XML Doc
            XDocument xdoc = null;
            try
            {
                xdoc = XDocument.Load(new StreamReader(
                    this.GetType().GetTypeInfo().Assembly.GetManifestResourceStream("ArcGISRuntimeSDKDotNet_PhoneSamples.Assets.SampleDescriptions.xml")));
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
                                var summary = member.Descendants("summary").FirstOrDefault();
                                if (summary != null && summary.Value is string)
                                    match.Description = summary.Value.Trim();
                                var category = member.Descendants("category").FirstOrDefault();
                                if (category != null && category.Value is string)
                                    match.Category = category.Value.Trim();
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
            return text.Replace("Arc GIS", "ArcGIS");
        }

        public class SampleGroup : List<Sample>
        {
            public SampleGroup(IEnumerable<Sample> samples)
                : base(samples)
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

                var query = from item in Samples
                            orderby item.Category
                            group item by item.Category into g
                            select new { GroupName = g.Key, Items = g };

                foreach (var g in query)
                {
                    groups.Add(new SampleGroup(g.Items.OrderBy(i => i.Name)) { Key = g.GroupName });
                }

                // Define order of Mapping samples
                SampleGroup mappingSamplesGroup = groups.Where(i => i.Key == "Mapping").First();
                List<Sample> mappingSamples = new List<Sample>();
                //mappingSamples.Add(mappingSamplesGroup.Items.Where(i => i.Name == "Switch Basemaps").First());
                mappingSamples.Add(mappingSamplesGroup.Items.Where(i => i.Name == "Map Properties").First());
                mappingSamples.Add(mappingSamplesGroup.Items.Where(i => i.Name == "Overview Map").First());
                mappingSamples.Add(mappingSamplesGroup.Items.Where(i => i.Name == "Location Display").First());
                mappingSamples.Add(mappingSamplesGroup.Items.Where(i => i.Name == "Handle Errors").First());
                SampleGroup newMappingSamplesGroup = new SampleGroup(mappingSamples) { Key = mappingSamplesGroup.Key };

                // Define order of categories
                List<SampleGroup> orderedSampleGroups = new List<SampleGroup>();
                orderedSampleGroups.Add(newMappingSamplesGroup);
                orderedSampleGroups.Add(groups.Where(i => i.Key == "Tiled Layers").First());
                orderedSampleGroups.Add(groups.Where(i => i.Key == "Dynamic Service Layers").First());
                orderedSampleGroups.Add(groups.Where(i => i.Key == "Feature Layers").First());
                orderedSampleGroups.Add(groups.Where(i => i.Key == "Graphics Layers").First());
                orderedSampleGroups.Add(groups.Where(i => i.Key == "Symbology").First());
                orderedSampleGroups.Add(groups.Where(i => i.Key == "Query Tasks").First());
                orderedSampleGroups.Add(groups.Where(i => i.Key == "Geoprocessing Tasks").First());

                return orderedSampleGroups;
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

        public string Description { get; set; }
    }
}
