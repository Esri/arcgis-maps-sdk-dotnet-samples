// Copyright 2018 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using ArcGISRuntime.Samples.Shared.Attributes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace ArcGISRuntime.Samples.Shared.Models
{
    public class SampleInfo
    {
        private string _pathStub = Directory.GetCurrentDirectory();

        /// <summary>
        /// Gets the path to the sample on disk.
        /// </summary>
        public string Path 
        {
            get
            {
                return System.IO.Path.Combine(_pathStub, "Samples", Category, SampleType.Name);
            }
        }

        /// <summary>
        /// The human-readable name of the sample.
        /// </summary>
        public string SampleName { get; set; }

        /// <summary>
        /// The name of the sample as it appears in code.
        /// </summary>
        public string FormalName { get { return SampleType.Name; } }

        /// <summary>
        /// The human-readable category of the sample.
        /// </summary>
        public string Category { get; set; }

        public string Description { get; set; }

        public string Instructions { get; set; }

        /// <summary>
        /// A list of offline data items that should be downloaded 
        /// from ArcGIS Online prior to loading the sample. These 
        /// should be expressed as portal item identifiers. 
        /// </summary>
        public IEnumerable<string> OfflineDataItems { get; set; }

        public IEnumerable<string> Tags { get; set; }

        public IEnumerable<string> AndroidLayouts { get; set; }

        public IEnumerable<string> XamlLayouts { get; set; }

        public IEnumerable<string> ClassFiles { get; set; }

        /// <summary>
        /// A list of files used by the sample as embedded resources (e.g. PictureMarkerSymbols\pin_star_blue.png)
        /// </summary>
        public IEnumerable<string> EmbeddedResources { get; set; }

        /// <summary>
        /// The expected filename of the sample's image, without path.
        /// This is intened for use on Windows.
        /// </summary>
        public string Image { get { return String.Format("{0}.jpg", SampleType.Name); } }

        /// <summary>
        /// The underlying .NET type for this sample.
        /// Note: this is used by the sample viewer to 
        /// construct samples at run time. 
        /// </summary>
        public Type SampleType { get; set; }

        /// <summary>
        /// The path to the sample image on disk; intended for use on Windows.
        /// </summary>
        public string SampleImageName { get { return System.IO.Path.Combine(Path, Image); } }

        /// <summary>
        /// Base directory for the samples; defaults to executable directory
        /// </summary>
        public string PathStub
        {
            get { return _pathStub; }
            set { _pathStub = value; }
        }

        /// <summary>
        /// This constructor is for use when the sample 
        /// type is in the executing assembly.
        /// </summary>
        /// <param name="sampleType">The type for the sample object.</param>
        public SampleInfo(Type sampleType)
        {
            SampleType = sampleType;
            TypeInfo typeInfo = sampleType.GetTypeInfo();

            var sampleAttr = GetAttribute<SampleAttribute>(typeInfo);
            if (sampleAttr == null) { throw new ArgumentException("Type must be decorated with 'Sample' attribute"); }

            var offlineDataAttr = GetAttribute<OfflineDataAttribute>(typeInfo);
            var xamlAttr = GetAttribute<XamlFilesAttribute>(typeInfo);
            var androidAttr = GetAttribute<AndroidLayoutAttribute>(typeInfo);
            var classAttr = GetAttribute<ClassFileAttribute>(typeInfo);
            var embeddedResourceAttr = GetAttribute<EmbeddedResourceAttribute>(typeInfo);

            Category = sampleAttr.Category;
            Description = sampleAttr.Description;
            Instructions = sampleAttr.Instructions;
            SampleName = sampleAttr.Name;
            Tags = sampleAttr.Tags;
            if (androidAttr != null) { AndroidLayouts = androidAttr.Files; }
            if (xamlAttr != null) { XamlLayouts = xamlAttr.Files; }
            if (classAttr != null) { ClassFiles = classAttr.Files; }
            if (offlineDataAttr != null) { OfflineDataItems = offlineDataAttr.Items; }
            if (embeddedResourceAttr != null) { EmbeddedResources = embeddedResourceAttr.Files; }
        }

        /// <summary>
        /// This constructor is for use when constructing from a type in another assembly
        /// Because of the way type comparison is done, types from different assembly (even if the source is the same) are different
        /// This breaks casts and comparisons relied on in the other constructor.
        /// </summary>
        /// <param name="sampleType">The type for the sample object.</param>
        /// <param name="assembly">The assembly from which the sample originated.</param>
        public SampleInfo(Type sampleType, Assembly assembly)
        {
            // The type from which to build the sample info.
            SampleType = sampleType;

            // Type info for the sample.
            TypeInfo typeInfo = sampleType.GetTypeInfo();

            // Get the types from the originating assembly. This is needed when working from another assembly (e.g. reflecting on a loaded DLL). 
            var attributeTypeSample = assembly.GetType("ArcGISRuntime.Samples.Shared.Attributes.SampleAttribute");
            var attributeTypeAndroid = assembly.GetType("ArcGISRuntime.Samples.Shared.Attributes.AndroidLayoutAttribute");
            var attributeTypeOffline = assembly.GetType("ArcGISRuntime.Samples.Shared.Attributes.OfflineDataAttribute");
            var attributeTypeXaml = assembly.GetType("ArcGISRuntime.Samples.Shared.Attributes.XamlFilesAttribute");
            var attributeTypeClass = assembly.GetType("ArcGISRuntime.Samples.Shared.Attributes.ClassFileAttribute");
            var attributeTypeResource = assembly.GetType("ArcGISRuntime.Samples.Shared.Attributes.EmbeddedResourceAttribute");

            // Get the attributes decorating the sample.
            var sampleAttr = typeInfo.GetCustomAttribute(attributeTypeSample, false);
            if (sampleAttr == null) { throw new ArgumentException("Type must be decorated with 'Sample' attribute"); }
            var offlineDataAttr = typeInfo.GetCustomAttribute(attributeTypeOffline, false);
            var xamlAttr = typeInfo.GetCustomAttribute(attributeTypeXaml, false);
            var androidAttr = typeInfo.GetCustomAttribute(attributeTypeAndroid, false);
            var classAttr = typeInfo.GetCustomAttribute(attributeTypeClass, false);
            var embeddedAttr = typeInfo.GetCustomAttribute(attributeTypeResource, false);

            // Get the values for each attribute property.
            Category = GetStringFromAttribute(sampleAttr, "Category");
            Description = GetStringFromAttribute(sampleAttr, "Description");
            Instructions = GetStringFromAttribute(sampleAttr, "Instructions");
            SampleName = GetStringFromAttribute(sampleAttr, "Name");
            Tags = GetListFromAttribute(sampleAttr, "Tags");
            AndroidLayouts = GetListFromAttribute(androidAttr, "Files");
            XamlLayouts = GetListFromAttribute(xamlAttr, "Files");
            ClassFiles = GetListFromAttribute(classAttr, "Files");
            OfflineDataItems = GetListFromAttribute(offlineDataAttr, "Items");
            EmbeddedResources = GetListFromAttribute(embeddedAttr, "Files");
        }

        /// <summary>
        /// Gets the attribute of type <typeparamref name="T"/> for a type described by <paramref name="typeInfo"/>.
        /// </summary>
        /// <typeparam name="T">The type of the attribute object to return.</typeparam>
        /// <param name="typeInfo">Describes the type that will be examined.</param>
        /// <returns>The matching attribute object.</returns>
        private static T GetAttribute<T>(MemberInfo typeInfo) where T : Attribute
        {
            return typeInfo.GetCustomAttributes(typeof(T)).SingleOrDefault() as T;
        }

        /// <summary>
        /// Takes an attribute object and returns the value from the property matching the supplies <paramref name="propertyName"/>.
        /// </summary>
        /// <param name="attr">The attribute object to pull values from.</param>
        /// <param name="propertyName">The specific property whose value will be returned.</param>
        /// <returns>The value held by the attribute's property.</returns>
        private static string GetStringFromAttribute(Attribute attr, string propertyName)
        {
            return attr.GetType().GetProperty(propertyName).GetValue(attr).ToString();
        }

        /// <summary>
        /// Takes an attribute object and returns the value from the property matching the supplied <paramref name="propertyName" />.
        /// </summary>
        /// <param name="attr">The attribute object to pull values from.</param>
        /// <param name="propertyName">The specific property whose value will be returned.</param>
        /// <returns>Null if <paramref name="attr"/> is null. Otherwise the value held by the attribute's property.</returns>
        private static IEnumerable<string> GetListFromAttribute(Attribute attr, string propertyName)
        {
            // Return null if attribute is null.
            if (attr == null)
            {
                return null;
            }
            return attr.GetType().GetProperty(propertyName).GetValue(attr) as string[];
        }
    }
}