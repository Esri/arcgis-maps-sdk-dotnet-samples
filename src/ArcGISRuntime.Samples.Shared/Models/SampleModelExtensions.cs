// Copyright 2016 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific 
// language governing permissions and limitations under the License.

using System;

namespace ArcGISRuntime.Samples.Models
{
    /// <summary>
    /// Extension methods for SampleModel
    /// </summary>
    public static class SampleModelExtensions
    {
        public static string GetSampleName(this SampleModel model, Language language = Language.CSharp)
        {
            if (language == Language.VBNet)
                return string.Format("{0}VB", model.SampleName);
            return model.SampleName;
        }

        /// <summary>
        /// Gets the name of C# the xaml file.
        /// </summary>
        public static string GetSamplesXamlFileName(this SampleModel model, Language language = Language.CSharp)
        {
            return string.Format("{0}.xaml", model.GetSampleName(language));
        }

        /// <summary>
        /// Gets the name of the C# code behind file.
        /// </summary>
        public static string GetSamplesCodeBehindFileName(this SampleModel model, Language language = Language.CSharp)
        {
            if (language == Language.CSharp)
                return string.Format("{0}.xaml.cs", model.GetSampleName(language));

            return string.Format("{0}.xaml.vb", model.GetSampleName(language));
        }

        /// <summary>
        /// Gets the relative path to the solution folder where sample is located.
        /// </summary>
        /// <remarks>
        /// This assumes that output folder is 3 levels from the repository root folder ie. repositoryRoot\output\desktop\debug
        /// </remarks>
        public static string GetSampleFolderInRelativeSolution(this SampleModel model)
        {
            switch (model.Type)
            {
                case SampleModel.SampleType.API:
                    return string.Format(
                        "..\\..\\..\\src\\WPF\\ArcGISRuntime.WPF.Samples\\{0}\\{1}\\{2}",
                            model.SampleFolder.Parent.Parent.Name,
                            model.SampleFolder.Parent.Name,
                            model.SampleFolder.Name);
                case SampleModel.SampleType.Workflow:
                case SampleModel.SampleType.Tutorial:
                    return string.Format(
                        "..\\..\\..\\src\\WPF\\ArcGISRuntime.WPF.Samples\\{0}\\{1}",
                            model.SampleFolder.Parent.Name,
                            model.SampleFolder.Name);
                default:
                    throw new NotSupportedException("Sample type isn't supported.");
            }
        }
    }
}
