using System.Runtime.Serialization;

namespace ArcGISRuntime.Samples.Models
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
