// Copyright 2016 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific 
// language governing permissions and limitations under the License.

using System.Collections.Generic;
using Android.App;
using Android.Views;
using Android.Widget;
using ArcGISRuntime.Samples.Shared.Models;

namespace ArcGISRuntime
{
    /// <summary>
    /// Custom ArrayAdapter to display the list of Samples
    /// </summary>
    class SamplesListAdapter : BaseAdapter<SampleInfo>
    {
        Activity context;
        List<SampleInfo> items;

        public SamplesListAdapter(Activity context, List<SampleInfo> sampleItems) : base()
        {
            this.context = context;
            this.items = sampleItems;
        }
        public override int Count
        {
            get { return items.Count; }
        }

        public override SampleInfo this[int position]
        {
            get { return items[position]; }
        }

        public override long GetItemId(int position)
        {
            return position;
        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            var view = context.LayoutInflater.Inflate(Resource.Layout.SamplesLayout, parent, false);
            var name = view.FindViewById<TextView>(Resource.Id.sampleNameTextView);

            name.Text = items[position].SampleName;
            return view;
        }
    }
}