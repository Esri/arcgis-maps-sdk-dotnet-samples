using System.Collections.Generic;
using System.Runtime.Serialization;

namespace ArcGISRuntime.Samples.Models
{
    /// <summary>
    /// CategoryModel defines a main level categories. These are mainly used to categorize
    /// groups of samples under specific high level topics like "Map" or "Routing & Navigation".
    /// Samples are always located under sub-categories
    /// </summary>
    [DataContract(Name ="Category")]
    public class CategoryModel
    {
        /// <summary>
        /// Gets or sets the human readable name of the category. 
        /// </summary>
        [DataMember]
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the name of the category.
        /// </summary>
        [DataMember]
        public string CategoryName { get; set; }

        /// <summary>
        /// Gets or sets the list of sub-categories that belongs into this category.
        /// </summary>
        [DataMember]
        public List<SubCategoryModel> SubCategories { get; set; }
    }
}
