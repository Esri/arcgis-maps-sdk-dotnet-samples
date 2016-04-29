//Copyright 2015 Esri.
//
//Licensed under the Apache License, Version 2.0 (the "License");
//you may not use this file except in compliance with the License.
//You may obtain a copy of the License at
//
//http://www.apache.org/licenses/LICENSE-2.0
//
//Unless required by applicable law or agreed to in writing, software
//distributed under the License is distributed on an "AS IS" BASIS,
//WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//See the License for the specific language governing permissions and
//limitations under the License.
using ArcGISRuntime.Samples.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

#if NETFX_CORE
using Windows.UI.Xaml.Controls;
using Windows.ApplicationModel;
#else
using System.Windows.Controls;
#endif

namespace ArcGISRuntime.Samples.Managers
{
    /// <summary>
    /// Single instance class to manage samples.
    /// </summary>
    public class SampleManager 
    {
        private Assembly _samplesAssembly;
        private SampleStructureMap _sampleStructureMap;
        private Language _selectedLanguage;

#region Constructor and unique instance management

        // Private constructor
        private SampleManager() { }

        // Static initialization of the unique instance 
        private static readonly SampleManager SingleInstance = new SampleManager();

        public static SampleManager Current
        {
            get { return SingleInstance; }
        }

        public async Task InitializeAsync(Language language)
        {
          
            _selectedLanguage = language;
            if (language == Language.CSharp)
#if NETFX_CORE
                _samplesAssembly = Assembly.Load(new AssemblyName("ArcGISRuntime.Windows.Samples"));
#else
                _samplesAssembly = Assembly.Load("ArcGISRuntime.Desktop.Samples");
#endif
            else
#if NETFX_CORE
                _samplesAssembly = Assembly.Load(new AssemblyName("ArcGISRuntime.Windows.Samples.VB"));
#else
                _samplesAssembly = Assembly.Load("ArcGISRuntime.Desktop.Samples.VB");
#endif

            await CreateAllAsync();
        }

        #endregion // Constructor and unique instance management

        /// <summary>
        /// Gets or sets selected sample.
        /// </summary>
        public SampleModel SelectedSample { get; set; }
 
        /// <summary>
        /// Gets featured samples.
        /// </summary>
        /// <returns></returns>
        public List<FeaturedModel> GetFeaturedSamples()
        {
            return _sampleStructureMap.Featured;
        }

        /// <summary>
        /// Gets all samples in a list grouped by main category. All sub-categories are flattened.
        /// </summary>
        /// <returns>Return all categories and samples</returns>
        public List<TreeItem> GetSamplesInCategories()
        {
            var categories = new List<TreeItem>();

            foreach (var category in _sampleStructureMap.Categories)
            {
                var categoryItem = new TreeItem();
                categoryItem.Name = category.Name;

                foreach (var subCategory in category.SubCategories)
                {
                    foreach (var sample in subCategory.Samples)
                        categoryItem.Items.Add(sample);
                }

                categories.Add(categoryItem);
            }
            return categories;
        }
   
        /// <summary>
        /// Gets all samples as a tree.
        /// </summary>
        /// <returns>Return all categories, subcategories and samples.</returns>
        public List<TreeItem> GetSamplesAsTree()
        {
            var categories = new List<TreeItem>();

            foreach (var category in _sampleStructureMap.Categories)
            {
                var categoryItem = new TreeItem { Name = category.Name };

                foreach (var subCategory in category.SubCategories)
                {
                    if (subCategory.ShowGroup)
                    {
                        var subCategoryItem = new TreeItem() { Name = subCategory.Name };
                        categoryItem.Items.Add(subCategoryItem);

                        if (subCategory.Samples != null)
                            foreach (var sample in subCategory.Samples)
                                subCategoryItem.Items.Add(sample);
                    }
                    else
                    {
                        foreach (var sample in subCategory.Samples)
                            categoryItem.Items.Add(sample);
                    }
                }

                categories.Add(categoryItem);
            }
            return categories;
        }

        /// <summary>
        /// Creates a new control from sample.
        /// </summary>
        /// <param name="sampleModel">Sample that is transformed into a control</param>
        /// <returns>Sample as a control.</returns>
        public Control SampleToControl(SampleModel sampleModel)
        {
            var fullTypeAsString = string.Format("{0}.{1}", sampleModel.SampleNamespace,
                sampleModel.GetSampleName(_selectedLanguage));
            var sampleType = _samplesAssembly.GetType(fullTypeAsString);

            // TODO See better way to create objects in UWP

            var item = sampleType.GetConstructor(new Type[] { }).Invoke(new object[] { });

 //           var item = Activator.CreateInstance(sampleType);

           return (Control)item;
        }

        /// <summary>
        /// Creates whole sample structure.
        /// </summary>
        private async Task CreateAllAsync()
        {
            try
            {
                await Task.Run(() =>
                {
#if NETFX_CORE
                    var filePath = string.Format("ms-appx:///{0}", "ArcGISRuntime.Windows.Samples/groups.json");
                    try
                    {
                        filePath = Path.Combine(Package.Current.InstalledLocation.Path, "ArcGISRuntime.Windows.Samples", "groups.json");
                    }
                    catch (Exception)
                    {
                        throw new NotImplementedException("groups.json file is missing");
                    }
#else
                   var filePath = "groups.json";
                    
                    if (!File.Exists(filePath))
                        throw new NotImplementedException("groups.json file is missing");
#endif

                    _sampleStructureMap = SampleStructureMap.Create(filePath, _selectedLanguage);
                });
            }
            // This is thrown if even one of the files requires permissions greater 
            // than the application provides. 
            catch (UnauthorizedAccessException e)
            {
                throw; //TODO
            }
            catch (DirectoryNotFoundException e)
            {
                throw; //TODO
            }
            catch (Exception e)
            {
                throw; //TODO
            }
        }
    }
}
