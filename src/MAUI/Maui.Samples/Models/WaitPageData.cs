using ArcGIS.Samples.Shared.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArcGIS.Models
{
    public class WaitPageData
    {
        public WaitPageData() { }

        private CancellationTokenSource _cancellationTokenSource;
        public CancellationTokenSource CancellationTokenSource
        {
            get => _cancellationTokenSource;
            set => _cancellationTokenSource = value;
        }

        private SampleInfo _sample;
        public SampleInfo Sample
        {
            get => _sample;
            set => _sample = value;
        }
    }
}
