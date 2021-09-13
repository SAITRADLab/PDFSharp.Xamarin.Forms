using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PdfSharp.Xamarin.Forms.Droid
{
    using Urchin.Infrastructure;
    class GetExternalStorage : IGetExternalStorage
    {
        string IGetExternalStorage.GetExternalStorage()
        {
            var path = Android.OS.Environment.ExternalStorageDirectory.ToString();
            return path;
        }
    }
}