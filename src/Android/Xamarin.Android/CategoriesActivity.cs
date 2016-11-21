
using Android.App;
using Android.OS;
using Android.Widget;
using ArcGISRuntimeXamarin.Managers;
using ArcGISRuntimeXamarin.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Threading.Tasks;

namespace ArcGISRuntimeXamarin
{
    [Activity(Label = "_OLD_CategoriesActivity", MainLauncher = false)]
    public class CategoriesActivity : Activity
    {
        List<TreeItem> _sampleCategories;
        protected async override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            // Create your application here
            SetContentView(Resource.Layout.CategoriesList);

            // var androidResourceUrlPrefix = string.Format("android.resource://{0}/", Application.Context.PackageName);
            // var resources = Application.Context.Resources;
            try
            {
                await SampleManager.Current.InitializeAsync();

                var jsonInBytes = loadJSONFromAsset();
                var sampleStructureMap = CreateSampleStructureMap(jsonInBytes);
                var sampleModel = CreateSampleModel(jsonInBytes);

                _sampleCategories = SampleManager.Current.GetSamplesAsTree(sampleStructureMap);
                //List<string> categories = new List<string>();

                //foreach (var item in data)
                //{
                //    categories.Add(item.Name);
                //}

                //  var adapter = new ArrayAdapter<TreeItem>(this, Android.Resource.Layout.SimpleListItem1, data);
                //  var newAdapter = new ArrayAdapter(this, Android.Resource.Layout.SimpleListItem1, categories);

                var categoriesAdapter = new CategoriesAdapter(this, _sampleCategories);

                ListView categoriesListView = FindViewById<ListView>(Resource.Id.categoriesListView);

                categoriesListView.Adapter = categoriesAdapter;

                categoriesListView.ItemClick += CategoriesItemClick;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
            }
        }
        public SampleStructureMap CreateSampleStructureMap(byte[] jsonInBytes)
        {
            var serializer = new DataContractJsonSerializer(typeof(SampleStructureMap));
            SampleStructureMap structureMap = null;

            using (Stream stream = Assets.Open("groups.json"))
            {
                using (MemoryStream ms = new MemoryStream(jsonInBytes))
                {
                    structureMap = serializer.ReadObject(ms) as SampleStructureMap;
                    structureMap.Samples = new List<SampleModel>();
                }
            }
            return structureMap;
        }

        public SampleModel CreateSampleModel(byte[] jsonInBytes)
        {
            var serializer = new DataContractJsonSerializer(typeof(SampleModel));
            SampleModel sampleModel = null;

            using (Stream stream = Assets.Open("groups.json"))
            {
                using (MemoryStream ms = new MemoryStream(jsonInBytes))
                {
                    sampleModel = serializer.ReadObject(stream) as SampleModel;
                    //  sampleModel.SampleFolder = metadataFile.Directory;
                }
            }
            return sampleModel;
        }

        public byte[] loadJSONFromAsset()
        {
            try
            {
                using (Stream stream = Assets.Open("groups.json"))
                {
                    using (MemoryStream ms = new MemoryStream())
                    {
                        stream.CopyTo(ms);
                        return ms.ToArray();
                    }
                }
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        private void CategoriesItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            // Start a new Intent that starts the Samples List Activity
            // Use the position property to see which one was clicked?

            var category = _sampleCategories[e.Position];

            var dialog = new AlertDialog.Builder(this);
            dialog.SetMessage(category.Name);
            dialog.SetNeutralButton("OK", delegate { });
            dialog.Show();


        }
    }
}