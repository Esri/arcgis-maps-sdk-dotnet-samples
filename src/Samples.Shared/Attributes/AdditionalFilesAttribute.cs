// Copyright 2018 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using System;
using System.Collections.Generic;

namespace ArcGIS.Samples.Shared.Attributes
{
    public abstract class AdditionalFilesAttribute : Attribute
    {
        private readonly string[] _files;

        protected AdditionalFilesAttribute(params string[] files)
        {
            _files = files;
        }

        public IReadOnlyList<string> Files
        { get { return _files; } }
    }

    /// <summary>
    /// Attribute for annotating a sample with XAML layout files it uses.
    /// This should not be used for the primary layout on WPF, UWP, WinUI, MAUI.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class XamlFilesAttribute : AdditionalFilesAttribute
    {
        public XamlFilesAttribute(params string[] files) : base(files)
        {
        }
    }

    /// <summary>
    /// Attribute for annotating a sample with android layout files it uses.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class AndroidLayoutAttribute : AdditionalFilesAttribute
    {
        public AndroidLayoutAttribute(params string[] files) : base(files)
        {
        }
    }

    /// <summary>
    /// Attribute for annotating a sample with additional class files.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class ClassFileAttribute : AdditionalFilesAttribute
    {
        public ClassFileAttribute(params string[] files) : base(files)
        {
        }
    }

    /// <summary>
    /// Attribute for annotating a sample with additional embedded resources files.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class EmbeddedResourceAttribute : AdditionalFilesAttribute
    {
        public EmbeddedResourceAttribute(params string[] files) : base(files)
        {
        }
    }
}