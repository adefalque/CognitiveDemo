using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;

using CognitiveDemo.Droid.Views;

[assembly: Xamarin.Forms.Dependency(typeof(NativePages))]

namespace CognitiveDemo.Droid.Views
{
    using CognitiveDemo.Views;

    using Xamarin.Forms;

    public class NativePages : INativePages
    {
        public void StartNativeFaceIdentifyPage()
        {
            //var intent = new Intent(Forms.Context, typeof(FaceIdentifyActivity));
            //Forms.Context.StartActivity(intent);
        }
    }
}