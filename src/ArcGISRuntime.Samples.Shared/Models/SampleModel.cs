// Copyright 2016 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific 
// language governing permissions and limitations under the License.

using ArcGISRuntime.Samples.Managers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;

namespace ArcGISRuntime.Samples.Models
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
        /// Get or sets the value indicating whether the sample requires offline data.
        /// </summary>
        [DataMember]
        public List<string> DataItemIds { get; set; }

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
        /// Gets or sets the url to the developers site tutorial documentation.
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
        [IgnoreDataMember]
        public DirectoryInfo SampleFolder { get; set; }

        /// <summary>
        /// Gets the (expected) assembly-qualified name for the VB sample; useful when determining if VB implementation is present
        /// </summary>
        [IgnoreDataMember]
        public String ExpectedVbAssemblyQualifiedType
        {
            get
            {
#if NETFX_CORE
                return String.Format("{0}.{1}VB, ArcGISRuntime.UWP.Samples.VB, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null", SampleNamespace, SampleName);
#else
                return String.Format("{0}.{1}VB, ArcGISRuntime.WPF.Samples.VB, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null", SampleNamespace, SampleName);
#endif
            }
        }

        /// <summary>
        /// Gets the namespace of the sample.
        /// </summary>
        /// <remarks>
        /// At the moment only desktop is supported.
        /// </remarks>
        [IgnoreDataMember]
        public string SampleNamespace
        {
            get
            {
#if NETFX_CORE
                var fullNamespace = string.Format("ArcGISRuntime.UWP.Samples.{0}", SampleFolder.Name);
#else
                var fullNamespace = string.Format("ArcGISRuntime.WPF.Samples.{0}", SampleFolder.Name);
#endif
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
                var fullPath = Path.Combine(SampleFolder.FullName, Image);
                return fullPath;
            }
        }

        #region Factory methods
        
        /// <summary>
        /// Creates new instance of SampleModel by deserializing it from the json file provided.
        /// </summary>
        /// <param name="metadataFilePath">Full path to the metadata JSON file</param>
        /// <returns>Deserialized SampleModel</returns>
        internal static SampleModel Create(string metadataFilePath)
        {
            if (!File.Exists(metadataFilePath))
                return null;

            var serializer = new DataContractJsonSerializer(typeof(SampleModel));

            SampleModel sampleModel = null;

            // Create sample model from the metadata file and set needed properties to 
            // the created model
            var metadataFile = new FileInfo(metadataFilePath);

            var json = File.ReadAllText(metadataFilePath);

            var jsonInBytes = Encoding.UTF8.GetBytes(json);
            using (var stream = new MemoryStream(jsonInBytes))
            {
                // De-serialize sample model
                sampleModel = serializer.ReadObject(stream) as SampleModel;

                sampleModel.SampleFolder = metadataFile.Directory;
            }

            // Stop if the sample doesn't have a VB implementation (VB sample viewer only)
            if (ApplicationManager.Current.SelectedLanguage == Language.VBNet && System.Type.GetType(sampleModel.ExpectedVbAssemblyQualifiedType, false) == null)
            {
                return null;
            }

            return sampleModel;
        }

        #endregion // Factory methods
    }
}