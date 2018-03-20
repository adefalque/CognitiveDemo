namespace CognitiveDemo.Droid
{
    using System.IO;

    using Android;
    using Android.App;
    using Android.Content.PM;
    using Android.Gms.Common;
    using Android.Gms.Vision;
    using Android.Gms.Vision.Faces;
    using Android.OS;
    using Android.Runtime;
    using Android.Support.Design.Widget;
    using Android.Support.V4.App;
    using Android.Support.V7.App;
    using Android.Util;
    using Android.Webkit;
    using Android.Widget;

    using CognitiveDemo;
    using CognitiveDemo.Droid.Camera;
    using CognitiveDemo.Droid.FacerTracking;

    using Plugin.Permissions;
    using Plugin.Permissions.Abstractions;

    using ServiceHelpers;

    using Permission = Android.Content.PM.Permission;

    [Activity(
        Label = "CognitiveDemo.Droid",
        MainLauncher = true,
        Icon = "@drawable/icon",
        //Theme = "@style/Theme.AppCompat.NoActionBar")
        Theme = "@style/MainTheme")
    ]
    public class MainActivity : global::Xamarin.Forms.Platform.Android.FormsAppCompatActivity
    {
        protected async override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);

            Xamarin.Forms.Forms.Init(this, bundle);
            
            this.LoadApplication(new App());
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
        {
            PermissionsImplementation.Current.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
    }
}