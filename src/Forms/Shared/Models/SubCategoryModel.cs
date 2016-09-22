// Copyright 2016 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific 
// language governing permissions and limitations under the License.

using System.Collections.Generic;
using System.Runtime.Serialization;

namespace ArcGISRuntimeXamarin.Models
{
    /// <summary>
    /// SubCategoryModel defines how samples are grouped under the categories. 
    /// </summary>
    /// <remarks>
    /// SubCategories are defined in a groups.json file that is used to construct the <see cref="SubCategoryModel"/>s.
    /// </remarks>
    [DataContract(Name = "SubCategories")]
    public class SubCategoryModel
    {
        /// <summary>
        /// Gets or sets the human readable name of the sub-category.
        /// </summary>
        [DataMember]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the name of the sub-category.
        /// </summary>
        [DataMember]
        public string SubCategoryName { get; set; }

        /// <summary>
        /// Gets or sets if the category should be shown as a sub-category.
        /// </summary>
        [DataMember]
        public bool ShowGroup { get; set; }

        // <summary>
        // Gets all sample names that are part of this group.
        // </summary>
        //[DataMember(Name = "SampleName")]
        //public List<string> SampleNames { get; set; }

        /// <summary>
        /// Gets all sample infos that are part of this group.
        /// </summary>
        [DataMember]
        public List<SampleInfo> SampleInfos { get; set; }

        /// <summary>
        /// Gets all the samples.
        /// </summary>
        [IgnoreDataMember]
        public List<SampleModel> Samples { get; set; }
    }


    [DataContract(Name = "SampleInfo")]
    public class SampleInfo
    {
        [DataMember()]
        public string Path { get; set; }

        [DataMember()]
        public string SampleName { get; set; }
    }
}
