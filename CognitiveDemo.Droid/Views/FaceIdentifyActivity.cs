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

namespace CognitiveDemo.Droid.Views
{
    using Android;
    using Android.Content.PM;
    using Android.Gms.Common;
    using Android.Gms.Vision;
    using Android.Gms.Vision.Faces;
    using Android.Support.Design.Widget;
    using Android.Support.V4.App;
    using Android.Util;

    using CognitiveDemo.Droid.Camera;
    using CognitiveDemo.Droid.FacerTracking;

    using ServiceHelpers;

    using Xamarin.Forms.Platform.Android;

    using Resource = CognitiveDemo.Droid.Resource;

    [Activity(
        Label = "Identify", 
        Theme = "@style/Theme.AppCompat.NoActionBar",
        ScreenOrientation = ScreenOrientation.FullSensor)]
    public class FaceIdentifyActivity : FormsAppCompatActivity, MultiProcessor.IFactory
    {
        private static readonly string TAG = "FaceTracker";

        private CameraSource cameraSource = null;
        private CameraSourcePreview cameraPreview;
        private GraphicOverlay graphicOverlay;

        private bool isFrontFacing = true;

        public const string DefaultGreetings = "I'm thinking...";
        public static string Greetings = DefaultGreetings;

        public static CognitiveFaceResult CognitiveFacesResult = null;        


        private static readonly int RC_HANDLE_GMS = 9001;

        // permission request codes need to be < 256
        private static readonly int RC_HANDLE_CAMERA_PERM = 2;

        private FaceService faceService;

        protected override async void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            
            SetContentView(Resource.Layout.FaceIdentify);

            this.cameraPreview = this.FindViewById<CameraSourcePreview>(Resource.Id.preview);
            this.graphicOverlay = this.FindViewById<GraphicOverlay>(Resource.Id.faceOverlay);

            var button = (ImageButton) this.FindViewById(Resource.Id.flipButton);
            button.Click += (sender, args) =>
                {
                    this.isFrontFacing = !this.isFrontFacing;

                    if (this.cameraSource != null) {
                        this.cameraSource.Release();
                        this.cameraSource = null;
                    }

                    this.CreateCameraSource();
                    this.StartCameraSource();
                };

            if (ActivityCompat.CheckSelfPermission(this, Manifest.Permission.Camera) == Permission.Granted)
            {
                this.CreateCameraSource();
                LiveCamHelper.Init();
                LiveCamHelper.GreetingsCallback = (s) => { this.RunOnUiThread(() => Greetings = s); };
                LiveCamHelper.FacesDetectedCallback = result => { this.RunOnUiThread(() => CognitiveFacesResult = result); };
                
                this.faceService = new FaceService();
                await this.faceService.Initialize();

                //using (var stream = File.OpenRead("/data/user/0/CognitiveDemo.Droid/files/CognitivveFrame.png"))
                //{
                //    await this.faceService.IdentityFace(stream);
                //}
            }
            else
            {
                this.RequestCameraPermission();
            }
        }

        protected override void OnResume()
        {
            base.OnResume();

            this.StartCameraSource();
        }

        protected override void OnPause()
        {
            base.OnPause();
            this.cameraPreview?.Stop();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            if (this.cameraSource != null)
            {
                this.cameraSource.Release();
            }
        }

        private void RequestCameraPermission()
        {
            Log.Warn(TAG, "Camera permission is not granted. Requesting permission");

            var permissions = new string[] { Manifest.Permission.Camera };

            if (!ActivityCompat.ShouldShowRequestPermissionRationale(this, Manifest.Permission.Camera))
            {
                ActivityCompat.RequestPermissions(this, permissions, RC_HANDLE_CAMERA_PERM);
                return;
            }

            Snackbar.Make(this.graphicOverlay, Resource.String.permission_camera_rationale, Snackbar.LengthIndefinite)
                .SetAction(
                    Resource.String.ok,
                    (o) => { ActivityCompat.RequestPermissions(this, permissions, RC_HANDLE_CAMERA_PERM); }).Show();
        }

        /**
         * Creates and starts the camera.  Note that this uses a higher resolution in comparison
         * to other detection examples to enable the barcode detector to detect small barcodes
         * at long distances.
         */
        private void CreateCameraSource()
        {
            var context = Application.Context;
            FaceDetector detector = new FaceDetector.Builder(context)
                .SetClassificationType(ClassificationType.All)
                .SetTrackingEnabled(true)
                .Build();

            MyFaceDetector myFaceDetector = new MyFaceDetector(detector);

            if (!myFaceDetector.IsOperational)
            {
                // Note: The first time that an app using face API is installed on a device, GMS will
                // download a native library to the device in order to do detection.  Usually this
                // completes before the app is run for the first time.  But if that download has not yet
                // completed, then the above call will not detect any faces.
                //
                // isOperational() can be used to check if the required native library is currently
                // available.  The detector will automatically become operational once the library
                // download completes on device.
                Log.Warn(TAG, "Face detector dependencies are not yet available.");
            }

            this.cameraSource = new CameraSource.Builder(context, myFaceDetector)
                .SetRequestedPreviewSize(640, 480)
                .SetFacing(this.isFrontFacing ? CameraFacing.Front : CameraFacing.Back)
                .SetRequestedFps(30.0f)
                .Build();

            myFaceDetector.SetProcessor(new MultiProcessor.Builder(this).Build());
        }

        /**
         * Starts or restarts the camera source, if it exists.  If the camera source doesn't exist yet
         * (e.g., because onResume was called before the camera source was created), this will be called
         * again when the camera source is created.
         */
        private void StartCameraSource()
        {
            // check that the device has play services available.
            int code = GoogleApiAvailability.Instance.IsGooglePlayServicesAvailable(this.ApplicationContext);
            if (code != ConnectionResult.Success)
            {
                Dialog dlg = GoogleApiAvailability.Instance.GetErrorDialog(this, code, RC_HANDLE_GMS);
                dlg.Show();
            }

            if (this.cameraSource != null)
            {
                try
                {
                    this.cameraPreview.Start(this.cameraSource, this.graphicOverlay);
                }
                catch (System.Exception e)
                {
                    Log.Error(TAG, "Unable to start camera source.", e);
                    this.cameraSource.Release();
                    this.cameraSource = null;
                }
            }
        }

        public override void OnRequestPermissionsResult(
            int requestCode,
            string[] permissions,
            [GeneratedEnum] Permission[] grantResults)
        {
            if (requestCode != RC_HANDLE_CAMERA_PERM)
            {
                Log.Debug(TAG, "Got unexpected permission result: " + requestCode);
                base.OnRequestPermissionsResult(requestCode, permissions, grantResults);

                return;
            }

            if (grantResults.Length != 0 && grantResults[0] == Permission.Granted)
            {
                Log.Debug(TAG, "Camera permission granted - initialize the camera source");
                // we have permission, so create the camerasource
                this.CreateCameraSource();
                return;
            }

            Log.Error(
                TAG,
                "Permission not granted: results len = " + grantResults.Length + " Result code = "
                + (grantResults.Length > 0 ? grantResults[0].ToString() : "(empty)"));

            var builder = new Android.Support.V7.App.AlertDialog.Builder(this);
            builder.SetTitle("CognitiveDemo").SetMessage(Resource.String.no_camera_permission)
                .SetPositiveButton(Resource.String.ok, (o, e) => this.Finish()).Show();
        }

        // Google Gsm IFactory Implementation
        public Tracker Create(Java.Lang.Object item)
        {
            return new GraphicFaceTracker(this.graphicOverlay, this.cameraSource, this.faceService);
        }
    }
}