// Copyright 2018 Esri.
//
// Licensed under the Apache License, Version 2.0 (the "License"); you may not use this file except in compliance with the License.
// You may obtain a copy of the License at: http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software distributed under the License is distributed on an
// "AS IS" BASIS, WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. See the License for the specific
// language governing permissions and limitations under the License.

using Android.App;
using Android.Views;
using Android.Widget;
using ArcGISRuntime.Samples.Shared.Models;
using System.Collections.Generic;
using Java.Lang;

namespace ArcGISRuntime
{
    /// <summary>
    /// Custom ArrayAdapter to display the list of categories, with samples underneath.
    /// </summary>
    internal class CategoriesAdapter : BaseExpandableListAdapter
    {
        private readonly List<SearchableTreeNode> _items;
        private readonly Activity _context;

        public CategoriesAdapter(Activity context, List<SearchableTreeNode> items)
        {
            _items = items;
            _context = context;
        }

        public override Object GetChild(int groupPosition, int childPosition)
        {
            return (Object)_items[groupPosition].Items[childPosition];
        }

        public override long GetChildId(int groupPosition, int childPosition)
        {
            return _items[groupPosition].Items[childPosition].GetHashCode();
        }

        public override int GetChildrenCount(int groupPosition)
        {
            return _items[groupPosition].Items.Count;
        }

        public override Object GetGroup(int groupPosition)
        {
            return (Object)(object)_items[groupPosition];
        }

        public override long GetGroupId(int groupPosition)
        {
            return _items[groupPosition].GetHashCode();
        }

        public override View GetGroupView(int groupPosition, bool isExpanded, View convertView, ViewGroup parent)
        {
            var view = _context.LayoutInflater.Inflate(Resource.Layout.CategoriesLayout, parent, false);

            var name = view.FindViewById<TextView>(Resource.Id.groupNameTextView);

            name.Text = _items[groupPosition].Name;

            return view;
        }

        public override View GetChildView(int groupPosition, int childPosition, bool isLastChild, View convertView, ViewGroup parent)
        {
            var view = _context.LayoutInflater.Inflate(Resource.Layout.CategoriesLayout, parent, false);

            var name = view.FindViewById<TextView>(Resource.Id.sampleNameTextView);
            SampleInfo sample = (SampleInfo)_items[groupPosition].Items[childPosition];
            name.Text = sample.SampleName;

            return view;
        }

        public override bool IsChildSelectable(int groupPosition, int childPosition)
        {
            if (_items[groupPosition]?.Items[childPosition] != null)
            {
                return true;
            }

            return false;
        }

        public override int GroupCount => _items.Count;
        public override bool HasStableIds => true;
    }
}