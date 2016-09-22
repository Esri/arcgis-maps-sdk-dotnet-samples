// Copyright 2016 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific 
// language governing permissions and limitations under the License.


using ArcGISRuntimeXamarin.Managers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;

namespace ArcGISRuntimeXamarin.Models
{
    /// <summary>
    /// SampleModel defines each sample. 
    /// </summary>
    /// <remarks>
    /// Samples are defined in metadata.json files that are located in the application folder
    /// structure. Samples can be either API, Tutorial or Workflow sample.
    /// </remarks>
    [DataContract]
    public class SampleModel
    {
        /// <summary>
        /// Defines sample types 
        /// </summary>
        [DataContract]
        public enum SampleType
        {
            [EnumMember(Value = "API")]
            API,

            [EnumMember(Value = "Workflow")]
            Workflow,

            [EnumMember(Value = "Tutorial")]
            Tutorial,
        }

        /// <summary>
        /// Gets or sets the human readable name of the sample.
        /// </summary>
        [DataMember]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the name of the sample.
        /// </summary>
        [DataMember]
        public string SampleName { get; set; }

        /// <summary>
        /// Gets or sets the description. This defines what the sample is about.
        /// </summary>
        [DataMember]
        public string Description { get; set; }

        /// <summary>
        /// Gets or sets the instructions. This defines how to use the sample.
        /// </summary>
        [DataMember]
        public string Instructions { get; set; }

        /// <summary>
        /// Gets or sets the type of the sample.
        /// </summary>
        [DataMember]
        public SampleType Type { get; set; }

        /// <summary>
        /// Gets or sets the name of the category where sample belongs.
        /// </summary>
        [DataMember]
        public string Category { get; set; }

        /// <summary>
        /// Gets or sets the name of the sub-category.
        /// </summary>
        [DataMember]
        public string SubCategory { get; set; }

        /// <summary>
        /// Get or sets the value indicating whether the sample requires online connection.
        /// </summary>
        [DataMember]
        public bool RequiresOnlineConnection { get; set; }

        /// <summary>
        /// Get or sets the value indicating whether the sample requires offline data.
        /// </summary>
        [DataMember]
        public bool RequiresOfflineData { get; set; }

        /// <summary>
        /// Get or sets the value indicating whether the sample requires local server.
        /// </summary>
        [DataMember]
        public bool RequiresLocalServer { get; set; }

        /// <summary>
        /// Gets or sets the relative path to the thumbnail image.
        /// </summary>
        [DataMember]
        public string Image { get; set; }

        /// <summary>
        /// Gets or sets the URL to the developers site tutorial documentation.
        /// </summary>
        /// <remarks>This is only for samples that are type of Tutorial.</remarks>
        [DataMember]
        public string Link { get; set; }

        /// <summary>
        /// Gets or sets list of types that the sample is related to.
        /// </summary>
        [DataMember]
        public List<string> TypeLink { get; set; }

        /// <summary>
        /// Gets or sets the type of the sample. This is used to create sample visuals to the
        /// viewer.
        /// </summary>
        [IgnoreDataMember]
        public Type SampleFileType { get; set; }

        /// <summary>
        /// Gets or sets the folder where sample is located.
        /// </summary>
        [DataMember()]
        public string SampleFolder { get; set; } // Changed this from Directory to string. Can't use Directory in Android. This required adding a new item to metadata.json file. 

        /// <summary>
        /// Gets the namespace of the sample.
        /// </summary>
        [IgnoreDataMember]
        public string SampleNamespace
        {
            get
            {
                var fullNamespace = string.Format("ArcGISRuntimeXamarin.Samples.{0}", SampleFolder);
                return fullNamespace;
            }
        }

        /// <summary>
        /// Gets the full path to the image file.
        /// </summary>
        [IgnoreDataMember]
        public string SampleImageName
        {
            get
            {
                // TODO: Not sure if this is going to work without Directory. Might need the Assets.Open again. 
                var fullPath = Path.Combine(SampleFolder, Image);
                return fullPath;
            }
        }

        #region Factory methods
        /// <summary>
        /// Creates new instance of SampleModel by deserializing it from the json file provided.
        /// </summary>
        /// <param name="samplePath">Full path to the metadata JSON file of the sample</param>
        /// <returns>Deserialized SampleModel</returns>
        internal static SampleModel Create(string samplePath)
        {
            var metadataStream = SampleManager.Current.GetMetadataManifest(samplePath);

            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(SampleModel));

            SampleModel sampleModel = null;
            
            // The samplePath is the path specified in the groups.json file for each metadata.json file
           // var assetPath = Path.Combine(samplePath, "metadata.json");
            try
            {
                // TODO: Wondering if we can rework this to not have to open two different MemoryStreams.
                using (Stream stream = metadataStream)
                {
                    using (MemoryStream ms = new MemoryStream())
                    {
                        stream.CopyTo(ms);
                        var jsonInBytes = ms.ToArray();

                        using (MemoryStream ms2 = new MemoryStream(jsonInBytes))
                        {
                            sampleModel = serializer.ReadObject(ms2) as SampleModel;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.WriteLine(ex.Message);
            }
            return sampleModel;
        }
        #endregion // Factory methods
    }
}
