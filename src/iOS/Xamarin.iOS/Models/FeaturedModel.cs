// Copyright 2016 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific 
// language governing permissions and limitations under the License.

using System.Runtime.Serialization;

namespace ArcGISRuntimeXamarin.Models
{
    /// <summary>
    /// Featured model that contains information what sample is featured and why.
    /// </summary>
    [DataContract]
    public class FeaturedModel
    {
        /// <summary>
        /// Defines sample types 
        /// </summary>
        [DataContract]
        public enum Reason
        {
            [EnumMember(Value = "New")]
            New,

            [EnumMember(Value = "Important")]
            Important,

            [EnumMember(Value = "Other")]
            Other,
        }

        /// <summary>
        /// Gets or sets the name of the featured sample.
        /// </summary>
        [DataMember]
        public string SampleName { get; set; }

        /// <summary>
        /// Gets or sets the reason why the sample is featured.
        /// </summary>
        [DataMember]
        public Reason FeaturedReason { get; set; }

        /// <summary>
        /// Gets or sets the featured <see cref="SampleModel"/>.
        /// </summary>
        [IgnoreDataMember]
        public SampleModel Sample { get; set; }
    }
}
