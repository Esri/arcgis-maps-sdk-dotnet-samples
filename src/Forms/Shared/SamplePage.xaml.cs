using ArcGISRuntime.Samples.Shared.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace ArcGISRuntime 
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class SamplePage : TabbedPage
    {
        public SamplePage ()
        {
            InitializeComponent();

        }

        public SamplePage(ContentPage sample, SampleInfo sampleInfo) : this()
        {
            this.BindingContext = sampleInfo;
            this.cpSample.Content = sample.Content;
            // This ensures navigation works correctly within samples
            sample.Parent = this;
            if (!string.IsNullOrWhiteSpace(sample.Title))
            {
                this.Title = sample.Title;
            }
            else
            {
                this.Title = sampleInfo.SampleName;
            }
            
            if (!String.IsNullOrWhiteSpace(sampleInfo.Instructions))
            {
                lblInstr.IsVisible = true;
            }
        }
    }
}