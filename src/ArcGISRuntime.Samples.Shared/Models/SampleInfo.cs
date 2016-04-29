using System.Runtime.Serialization;

namespace ArcGISRuntime.Samples.Shared.Models
{

    [DataContract(Name = "SampleInfo")]
    public class SampleInfo
    {
        [DataMember()]
        public string Path { get; set; }

        [DataMember()]
        public string SampleName { get; set; }
    }
}
