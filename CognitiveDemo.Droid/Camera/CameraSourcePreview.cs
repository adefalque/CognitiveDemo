namespace CognitiveDemo.Droid.Camera
{
    using System;

    using Android.Content;
    using Android.Gms.Vision;
    using Android.Graphics;
    using Android.Runtime;
    using Android.Util;
    using Android.Views;

    public sealed class CameraSourcePreview : ViewGroup, ISurfaceHolderCallback
    {
        private static readonly String TAG = "CameraSourcePreview";

        private Context mContext;
        private SurfaceView mSurfaceView;
        private bool mStartRequested;
        private bool mSurfaceAvailable;
        private CameraSource mCameraSource;

        private GraphicOverlay mOverlay;

        public CameraSourcePreview(Context context, IAttributeSet attrs) : base(context, attrs)
        {
            this.mContext = context;
            this.mStartRequested = false;
            this.mSurfaceAvailable = false;

            this.mSurfaceView = new SurfaceView(context);
            this.mSurfaceView.Holder.AddCallback(this);
            
            this.AddView(this.mSurfaceView);
        }

        public void Start(CameraSource cameraSource)
        {
            if (cameraSource == null)
            {
                this.Stop();
            }

            this.mCameraSource = cameraSource;

            if (this.mCameraSource != null)
            {
                this.mStartRequested = true;
                this.StartIfReady();
            }
        }

        public void Start(CameraSource cameraSource, GraphicOverlay overlay)
        {
            this.mOverlay = overlay;
            this.Start(cameraSource);
        }

        public void Stop()
        {
            if (this.mCameraSource != null)
            {
                this.mCameraSource.Stop();
            }
        }

        public void Release()
        {
            if (this.mCameraSource != null)
            {
                this.mCameraSource.Release();
                this.mCameraSource = null;
            }
        }
        private void StartIfReady()
        {
            if (this.mStartRequested && this.mSurfaceAvailable)
            {
                this.mCameraSource.Start(this.mSurfaceView.Holder);
                if (this.mOverlay != null)
                {
                    var size = this.mCameraSource.PreviewSize;
                    var min = Math.Min(size.Width, size.Height);
                    var max = Math.Max(size.Width, size.Height);
                    if (this.IsPortraitMode())
                    {
                        // Swap width and height sizes when in portrait, since it will be rotated by
                        // 90 degrees
                        this.mOverlay.SetCameraInfo(min, max, this.mCameraSource.CameraFacing);
                    }
                    else
                    {
                        this.mOverlay.SetCameraInfo(max, min, this.mCameraSource.CameraFacing);
                    }
                    this.mOverlay.Clear();
                }
                this.mStartRequested = false;
            }
        }

        private bool IsPortraitMode()
        {
            var orientation = this.mContext.Resources.Configuration.Orientation;
            if (orientation == Android.Content.Res.Orientation.Landscape)
            {
                return false;
            }
            if (orientation == Android.Content.Res.Orientation.Portrait)
            {
                return true;
            }

            Log.Debug(TAG, "isPortraitMode returning false by default");
            return false;
        }

        public void SurfaceChanged(ISurfaceHolder holder, [GeneratedEnum] Format format, int width, int height)
        {
            
        }

        public void SurfaceCreated(ISurfaceHolder holder)
        {
            this.mSurfaceAvailable = true;

            try
            {
                this.StartIfReady();
            }
            catch (Exception e)
            {
                Log.Error(TAG, "Could not start camera source.", e);
            }
        }

        public void SurfaceDestroyed(ISurfaceHolder holder)
        {
            this.mSurfaceAvailable = false;
        }

        protected override void OnLayout(bool changed, int l, int t, int r, int b)
        {
            int width = 320;
            int height = 240;
            if (this.mCameraSource != null)
            {
                var size = this.mCameraSource.PreviewSize;
                if (size != null)
                {
                    width = size.Width;
                    height = size.Height;
                }
            }

            // Swap width and height sizes when in portrait, since it will be rotated 90 degrees
            if (this.IsPortraitMode())
            {
                int tmp = width;
                width = height;
                height = tmp;
            }

            int layoutWidth = r - l;
            int layoutHeight = b - t;

            // Computes height and width for potentially doing fit width.
            int childWidth = layoutWidth;
            int childHeight = (int)(((float)layoutWidth / (float)width) * height);

           // If height is too tall using fit width, does fit height instead.
            if (childHeight > layoutHeight)
            {
                childHeight = layoutHeight;
                childWidth = (int)(((float)layoutHeight / (float)height) * width);
            }

            for (int i = 0; i < this.ChildCount; ++i)
            {
                this.GetChildAt(i).Layout(0, 0, childWidth, childHeight);
            }

            try
            {
                this.StartIfReady();
            }
            catch (Exception e)
            {
                Log.Error(TAG, "Could not start camera source.", e);
            }
        }
    }

}