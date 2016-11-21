// Copyright 2016 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific 
// language governing permissions and limitations under the License.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;

namespace ArcGISRuntimeXamarin.Models
{
	/// <summary>
	/// <see cref="SampleStructureMap "/> is a main level model for samples structure.
	/// </summary>
	/// <remarks>
	/// This class is constructed using <see cref="Create(Stream)"/> factory from the json.
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

		#region Factory methods
		/// <summary>
		/// Creates new instance of <see cref="SampleStructureMap"/> by deserializing it from the json file provided.
		/// Returned instance will be fully loaded including other information that is not provided
		/// in the json file like samples.
		/// </summary>
		/// <param name="groupsJSON">Full path to the groups JSON file</param>
		/// <returns>Deserialized <see cref="SampleStructureMap"/></returns>
		internal static SampleStructureMap Create(Stream groupsJSON)
		{
			var serializer = new DataContractJsonSerializer(typeof(SampleStructureMap));

			SampleStructureMap structureMap = null;

			try
			{
				// KD - Need two MemoryStreams? Need to investigate. Has to do with needing to open the json from the Android
				// Activity which gives you a stream. Then you need to get back to bytes. 
				using (groupsJSON)
				{
					using (MemoryStream ms = new MemoryStream())
					{
						groupsJSON.CopyTo(ms);
						var jsonInBytes = ms.ToArray();

						using (MemoryStream ms2 = new MemoryStream(jsonInBytes))
						{
							structureMap = serializer.ReadObject(ms2) as SampleStructureMap;
							structureMap.Samples = new List<SampleModel>();
						}
					}
				}
			}
			catch (System.Exception ex)
			{
				Logger.WriteLine(ex.Message);
			}

			#region CreateSamples

			List<string> pathList = new List<string>();
			foreach (var category in structureMap.Categories)
			{
				foreach (var subCategory in category.SubCategories)
				{
					if (subCategory.SampleInfos != null)
					{
						foreach (var sample in subCategory.SampleInfos)
						{
							pathList.Add(sample.Path.Replace("/", "."));
						}
					}
				}
			}

			var sampleModel = new SampleModel();
			foreach (var samplePath in pathList)
			{
				sampleModel = SampleModel.Create(samplePath);
				if (sampleModel != null)
					structureMap.Samples.Add(sampleModel);
			}

			foreach (var category in structureMap.Categories)
			{
				foreach (var subCategory in category.SubCategories)
				{
					if (subCategory.Samples == null)
						subCategory.Samples = new List<SampleModel>();

					foreach (var sampleName in subCategory.SampleInfos)
					{
						var sample = structureMap.Samples.FirstOrDefault(x => x.SampleName == sampleName.SampleName);

						if (sample == null) continue;

						subCategory.Samples.Add(sample);
					}
				}
			}

			#endregion
			
			return structureMap;
		}
		#endregion
	}
}
