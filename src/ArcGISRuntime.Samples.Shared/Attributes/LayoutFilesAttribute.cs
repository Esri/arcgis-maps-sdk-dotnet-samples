using System;
using System.Collections.Generic;

namespace ArcGISRuntime.Samples.Shared.Attributes
{
    public abstract class AdditionalFilesAttribute : Attribute
    {
        private string[] files;

        public AdditionalFilesAttribute(params string[] files)
        {
            this.files = files;
        }

        public IReadOnlyList<string> Files { get { return files; } }
    }

    public class XamlFilesAttribute : AdditionalFilesAttribute {
        public XamlFilesAttribute(params string[] files) : base(files)
        {
        }
    }

    public class AndroidLayoutAttribute : AdditionalFilesAttribute {
        public AndroidLayoutAttribute(params string[] files) : base(files)
        {
        }
    }

    public class ClassFileAttribute : AdditionalFilesAttribute
    {
        public ClassFileAttribute(params string[] files) : base(files)
        {
        }
    }
}