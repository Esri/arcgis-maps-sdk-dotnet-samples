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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;

namespace ArcGISRuntime.Samples.Models
{
    /// <summary>
    /// <see cref="SampleStructureMap "/> is a main level model for samples structure.
    /// </summary>
    /// <remarks>
    /// This class is constructured using <see cref="Create(string)"/> factory from the json.
    /// </remarks>
    [DataContract]
    public class SampleStructureMap
    {
        /// <summary>
        /// Gets or sets the categories.
        /// </summary>
        [DataMember]
        public List<CategoryModel> Categories { get; set; }

        /// <summary>
        /// Gets or sets list of featured samples.
        /// </summary>
        [DataMember]
        public List<FeaturedModel> Featured { get; set; }

        /// <summary>
        /// Get all samples in a flat list.
        /// </summary>
        [IgnoreDataMember]
        public List<SampleModel> Samples { get; set; }

        /// <summary>
        /// Gets sample by it's name.
        /// </summary>
        /// <param name="sampleName">The name of the sample.</param>
        /// <returns>Return <see cref="SampleModel"/> for the sample if found. Null if sample not found.</returns>
        public SampleModel GetSampleByName(string sampleName)
        {
            foreach (var category in Categories)
            {
                foreach (var subCategory in category.SubCategories)
                {
                    var result = subCategory.Samples.FirstOrDefault(x => x.SampleName == sampleName);
                    if (result != null)
                        return result;
                }
            }

            return null;
        }

        #region Factory methods
        /// <summary>
        /// Creates new instance of <see cref="SampleStructureMap"/> by desirialing it from the json file provided.
        /// Returned instance will be fully loaded including other information that is not provided
        /// in the json file like samples.
        /// </summary>
        /// <param name="metadataFilePath">Full path to the metadata JSON file</param>
        /// <param name="language">Language that is used to create the samples</param>
        /// <returns>Deserialized <see cref="SampleStructureMap"/></returns>
        internal static SampleStructureMap Create(string metadataFilePath, Language language = Language.CSharp)
        {
            var serializer = new DataContractJsonSerializer(typeof(SampleStructureMap));

            SampleStructureMap structureMap = null;

            // Create new instance of SampleStuctureMap
            var metadataFile = new FileInfo(metadataFilePath);
            var json = File.ReadAllText(metadataFilePath);

            var jsonInBytes = Encoding.UTF8.GetBytes(json);
            using (MemoryStream stream = new MemoryStream(jsonInBytes))
            {
                // De-serialize sample model
                structureMap = serializer.ReadObject(stream) as SampleStructureMap;
                structureMap.Samples = new List<SampleModel>();
            }

            // Create all samples and add them to the groups since they are not part of
            // main configuration file
            var rootDirectory = metadataFile.Directory;

            var samplesDirectory = new DirectoryInfo(Path.Combine(rootDirectory.FullName, "Samples"));
            var tutorialsDirectory = new DirectoryInfo(Path.Combine(rootDirectory.FullName, "Tutorials"));
            var workflowDirectory = new DirectoryInfo(Path.Combine(rootDirectory.FullName, "Workflows"));

            var sampleGroupFolders = samplesDirectory.GetDirectories();

            // Create all samples
            foreach (var sampleGroupFolder in sampleGroupFolders) // ie. Samples\Layers
            {
                // This creates samples from all folders and adds them to the samples list
                // This means that sample is created even if it's not defined in the groups list
                var sampleFolders = sampleGroupFolder.GetDirectories();
                foreach (var sampleFolder in sampleFolders)  // ie. Samples\Layers\ArcgISTiledLayerFromUrl
                {
                    var sampleModel = SampleModel.Create(
                        Path.Combine(sampleFolder.FullName, "metadata.json"));

                    if (sampleModel != null)
                        structureMap.Samples.Add(sampleModel);
                }
            }

            // Create all tutorials
            if (tutorialsDirectory.Exists)
                foreach (var sampleFolder in tutorialsDirectory.GetDirectories()) // ie. Tutorials\AddMapToApp
                {
                    var sampleModel = SampleModel.Create(
                        Path.Combine(sampleFolder.FullName, "metadata.json"));

                    if (sampleModel != null)
                        structureMap.Samples.Add(sampleModel);
                }

            // Create all workflows
            if (workflowDirectory.Exists)
                foreach (var sampleFolder in workflowDirectory.GetDirectories()) // ie. Workflows\SearchFeatures
                {
                    var sampleModel = SampleModel.Create(
                        Path.Combine(sampleFolder.FullName, "metadata.json"));

                    if (sampleModel != null)
                        structureMap.Samples.Add(sampleModel);
                }

            // Set samples to the sub-categories
            var addedSamples = new List<SampleModel>();
            foreach (var cateory in structureMap.Categories)
            {
                foreach (var subCategory in cateory.SubCategories)
                {
                    if (subCategory.Samples == null)
                        subCategory.Samples = new List<SampleModel>();

                    if (subCategory.SampleNames == null)
                        subCategory.SampleNames = new List<string>();

                    foreach (var sampleName in subCategory.SampleNames)
                    {
                        var sample = structureMap.Samples.FirstOrDefault(x => x.SampleName == sampleName);

                        if (sample == null) continue;

                        subCategory.Samples.Add(sample);
                        addedSamples.Add(sample);
                    }
                }
            }

            // Add samples that are not defined to the end of the groups
            var notAddedSamples = structureMap.Samples.Where(x => !addedSamples.Contains(x)).ToList();
            foreach (var sampleModel in notAddedSamples)
            {
                var category = structureMap.Categories.FirstOrDefault(x => x.CategoryName == sampleModel.Category);
                if (category == null)
                    continue;

                var subCategory = category.SubCategories.FirstOrDefault(x => x.SubCategoryName == sampleModel.SubCategory);
                if (subCategory != null)
                {
                    subCategory.SampleNames.Add(sampleModel.SampleName);
                    subCategory.Samples.Add(sampleModel);
                }
            }

            if (structureMap.Featured == null)
                structureMap.Featured = new List<FeaturedModel>();

            // Set all sample models to the featured models
            foreach (var featured in structureMap.Featured)
            {
                var sample = structureMap.Samples.FirstOrDefault(x => x.SampleName == featured.SampleName);
                if (sample != null)
                    featured.Sample = sample;
            }

            return structureMap;
        }
        #endregion
    }
}
