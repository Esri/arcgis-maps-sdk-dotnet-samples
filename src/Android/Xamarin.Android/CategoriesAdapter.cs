// Copyright 2016 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an 
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific 
// language governing permissions and limitations under the License.

using Android.Widget;
using Android.App;
using System.Collections.Generic;
using Android.Views;
using ArcGISRuntime.Samples.Managers;
using ArcGISRuntime.Samples.Shared.Models;

namespace ArcGISRuntime
{
    /// <summary>
    /// Custom ArrayAdapter to display the list of Categories
    /// </summary>
    class CategoriesAdapter : BaseAdapter<SearchableTreeNode>
    {

        List<SearchableTreeNode> items;
        Activity context;

        public CategoriesAdapter(Activity context, List<SearchableTreeNode> items) : base()
        {
            this.items = items;
            this.context = context;
        }

        public override long GetItemId(int position)
        {
            return position;
        }

        public override SearchableTreeNode this[int position]
        {
            get { return items[position]; }
        }

        public override int Count
        {
            get
            {
                return items.Count;
            }
        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            var view = context.LayoutInflater.Inflate(Resource.Layout.CategoriesLayout, parent, false);

            var name = view.FindViewById<TextView>(Resource.Id.nameTextView);

            name.Text = items[position].Name;

            return view;
        }
    }
}