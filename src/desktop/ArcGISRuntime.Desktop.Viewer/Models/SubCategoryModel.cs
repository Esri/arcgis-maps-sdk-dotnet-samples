using System.Collections.Generic;
using System.Runtime.Serialization;

namespace ArcGISRuntime.Samples.Models
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

        /// <summary>
        /// Gets all sample names that are part of this group.
        /// </summary>
        [DataMember(Name = "Samples")]
        public List<string> SampleNames { get; set; }

        /// <summary>
        /// Gets all the samples.
        /// </summary>
        [IgnoreDataMember]
        public List<SampleModel> Samples { get; set; }
    }
}
