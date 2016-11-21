// Copyright 2016 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific 
// language governing permissions and limitations under the License.

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
    /// This class is constructed using <see cref="Create(string,Language)"/> factory from the json.
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
        /// Creates new instance of <see cref="SampleStructureMap"/> by deserializing it from the json file provided.
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

            var groupsMetadataFileInfo = new FileInfo(metadataFilePath);
            if (!groupsMetadataFileInfo.Exists || groupsMetadataFileInfo == null)
                throw new FileNotFoundException("Groups.json file not found from given location.");

            // Create new instance of SampleStuctureMap
            var json = File.ReadAllText(metadataFilePath);

            var jsonInBytes = Encoding.UTF8.GetBytes(json);
            using (var stream = new MemoryStream(jsonInBytes))
            {
                // De-serialize sample model
                structureMap = serializer.ReadObject(stream) as SampleStructureMap;
                if (structureMap == null)
                    throw new SerializationException("Couldn't create StructureMap from provided groups.json file stream.");
                structureMap.Samples = new List<SampleModel>();
            }

            var pathList = new List<string>();
            foreach (var category in structureMap.Categories)
            {
                foreach (var subCategory in category.SubCategories)
                {
                    if (subCategory.SampleInfos != null)
                    {
                        foreach (var sample in subCategory.SampleInfos)
                        {
                            pathList.Add(sample.Path.Replace("/", "\\"));
                        }
                    }
                }
            }

            foreach (var samplePath in pathList)
            {
                var sampleMetadataFilePath = Path.Combine(
                    groupsMetadataFileInfo.Directory.FullName, samplePath, "metadata.json");
                var sampleModel = SampleModel.Create(sampleMetadataFilePath);
                if (sampleModel != null)
                    structureMap.Samples.Add(sampleModel);
            }

            foreach (var category in structureMap.Categories)
            {
                foreach (var subCategory in category.SubCategories)
                {
                    if (subCategory.Samples == null)
                        subCategory.Samples = new List<SampleModel>();

                    foreach (var sampleInfo in subCategory.SampleInfos)
                    {
                        var sample = structureMap.Samples.FirstOrDefault(x => x.SampleName == sampleInfo.SampleName);

                        if (sample == null) continue;

                        subCategory.Samples.Add(sample);
                    }
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
