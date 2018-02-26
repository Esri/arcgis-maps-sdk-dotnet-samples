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
        private string _pathStub = System.IO.Directory.GetCurrentDirectory();

        public SampleInfo(Type sampleType)
        {
            this.SampleType = sampleType;
            TypeInfo typeInfo = sampleType.GetTypeInfo();

            var sampleAttr = GetAttribute<SampleAttribute>(typeInfo);
            if (sampleAttr == null) { throw new ArgumentException("Type must be decorated with 'Sample' attribute"); }

            var offlineDataAttr = GetAttribute<OfflineDataAttribute>(typeInfo);
            var xamlAttr = GetAttribute<XamlFilesAttribute>(typeInfo);
            var androidAttr = GetAttribute<AndroidLayoutAttribute>(typeInfo);
            var classAttr = GetAttribute<ClassFileAttribute>(typeInfo);

            this.Category = sampleAttr.Category;
            this.Description = sampleAttr.Description;
            this.Instructions = sampleAttr.Instructions;
            this.SampleName = sampleAttr.Name;
            this.Tags = sampleAttr.Tags;
            if (androidAttr != null) { this.AndroidLayouts = androidAttr.Files; }
            if (xamlAttr != null) { this.XamlLayouts = xamlAttr.Files; }
            if (classAttr != null) { this.ClassFiles = classAttr.Files; }
            if (offlineDataAttr != null) { this.OfflineDataItems = offlineDataAttr.Items; }
        }

        /// <summary>
        /// This constructor is for use when constructing from a type in another assembly
        /// Because of the way type comparison is done, types from different assembly (even if the source is the same) are different, which breaks casts and comparisons
        /// </summary>
        /// <param name="sampleType"></param>
        /// <param name="assembly"></param>
        public SampleInfo(Type sampleType, Assembly assembly)
        {
            // The type from which to build the sample info
            this.SampleType = sampleType;

            // Type info for the sample
            TypeInfo typeInfo = sampleType.GetTypeInfo();

            // Get the types from the originating assembly
            var attributeTypeSample = assembly.GetType("ArcGISRuntime.Samples.Shared.Attributes.SampleAttribute"); // needed when working from other assembly
            var attributeTypeAndroid = assembly.GetType("ArcGISRuntime.Samples.Shared.Attributes.AndroidLayoutAttribute");
            var attributeTypeOffline = assembly.GetType("ArcGISRuntime.Samples.Shared.Attributes.OfflineDataAttribute");
            var attributeTypeXaml = assembly.GetType("ArcGISRuntime.Samples.Shared.Attributes.XamlFilesAttribute");
            var attributeTypeClass = assembly.GetType("ArcGISRuntime.Samples.Shared.Attributes.ClassFileAttribute");

            // Get the attributes decorating the sample
            var sampleAttr = typeInfo.GetCustomAttribute(attributeTypeSample, false);
            if (sampleAttr == null) { throw new ArgumentException("Type must be decorated with 'Sample' attribute"); }
            var offlineDataAttr = typeInfo.GetCustomAttribute(attributeTypeOffline, false);
            var xamlAttr = typeInfo.GetCustomAttribute(attributeTypeXaml, false);
            var androidAttr = typeInfo.GetCustomAttribute(attributeTypeAndroid, false);
            var classAttr = typeInfo.GetCustomAttribute(attributeTypeClass, false);

            // Use reflection to get the properties from each attribute. Then get the value for each property on each attribute
            this.Category = sampleAttr.GetType().GetProperty("Category").GetValue(sampleAttr).ToString();
            this.Description = sampleAttr.GetType().GetProperty("Description").GetValue(sampleAttr).ToString();
            this.Instructions = sampleAttr.GetType().GetProperty("Instructions").GetValue(sampleAttr).ToString(); ;
            this.SampleName = sampleAttr.GetType().GetProperty("Name").GetValue(sampleAttr).ToString();
            this.Tags = sampleAttr.GetType().GetProperty("Tags").GetValue(sampleAttr) as List<String>;
            if (androidAttr != null) { this.AndroidLayouts = androidAttr.GetType().GetProperty("Files").GetValue(androidAttr) as List<String>; }
            if (xamlAttr != null) { this.XamlLayouts = xamlAttr.GetType().GetProperty("Files").GetValue(xamlAttr) as List<String>; }
            if (classAttr != null) { this.ClassFiles = classAttr.GetType().GetProperty("Files").GetValue(classAttr) as List<String>; }
            if (offlineDataAttr != null) { this.OfflineDataItems = offlineDataAttr.GetType().GetProperty("Items").GetValue(offlineDataAttr) as List<String>; }
        }

        /// <summary>
        /// This path function assumes that the sample is in the executing assembly
        /// </summary>
        public string Path
        {
            get
            {
                return System.IO.Path.Combine(_pathStub, "Samples", this.Category, SampleType.Name);
            }
        }

        private static T GetAttribute<T>(MemberInfo typeInfo) where T : Attribute
        {
            return typeInfo.GetCustomAttributes(typeof(T)).SingleOrDefault() as T;
        }

        public string SampleName { get; set; }

        public string FormalName { get { return SampleType.Name; } }

        public string Category { get; set; }

        public string ProgCategory { get { return Category.Replace("Samples", "").Replace(" ", ""); } }

        public string Description { get; set; }

        public string Instructions { get; set; }

        public IEnumerable<string> OfflineDataItems { get; set; }

        public IEnumerable<string> Tags { get; set; }

        public IEnumerable<string> AndroidLayouts { get; set; }

        public IEnumerable<string> XamlLayouts { get; set; }

        public IEnumerable<string> ClassFiles { get; set; }

        public string Image { get { return String.Format("{0}.jpg", SampleType.Name); } }

        public Type SampleType { get; set; }

        public string SampleImageName { get { return System.IO.Path.Combine(this.Path, this.Image); } }

        /// <summary>
        /// Base directory for the samples; defaults to executable directory
        /// </summary>
        public string PathStub
        {
            get { return _pathStub; }
            set { _pathStub = value; }
        }
        /*
        private List<String> codeFiles;

        public List<string> CodeFiles
        {
            get
            {
                {
                    if (codeFiles != null) { return codeFiles; }
                    codeFiles = new List<string>();
                    // Any code files in the sample directory
                    foreach (var file in Directory.EnumerateFiles(this.Path).Where(f => f.EndsWith(".cs") || f.EndsWith(".xaml")))
                    {
                        codeFiles.Add(file);
                    }
                    // Any android layouts
                    if (this.AndroidLayouts != null)
                    {
                        foreach (var file in this.AndroidLayouts)
                        {
                            codeFiles.Add(System.IO.Path.Combine(_pathStub, "resource", "layout", file));
                        }
                    }
                    // Any additional class files
                    if (this.ClassFiles != null)
                    {
                        foreach (var file in this.ClassFiles)
                        {
                            if (!String.IsNullOrWhiteSpace(System.IO.Path.GetDirectoryName(file)))
                            {
                                codeFiles.Add(System.IO.Path.Combine(_pathStub, file));
                            }
                            else
                            {
                                codeFiles.Add(System.IO.Path.Combine(this.Path, file));
                            }
                        }
                    }
                    return codeFiles;
                }
            }
        }

        public List<string> CodeFileNames
        {
            get { return CodeFiles.Select(f => System.IO.Path.GetFileName(f)).ToList(); }
        }
        */
    }
}