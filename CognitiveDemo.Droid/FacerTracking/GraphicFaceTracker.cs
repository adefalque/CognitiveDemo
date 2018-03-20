namespace CognitiveDemo.Droid
{
    using System;
    using System.IO;
    using System.Runtime.InteropServices;
    using System.Threading.Tasks;

    using Android.Gms.Vision;
    using Android.Gms.Vision.Faces;
    using Android.Graphics;
    using Android.Runtime;

    using CognitiveDemo;
    using CognitiveDemo.Droid.Camera;
    using CognitiveDemo.Droid.FacerTracking;

    using Java.IO;
    using Java.Nio;

    using ServiceHelpers;

    using Console = System.Console;
    using Object = Java.Lang.Object;

    public class GraphicFaceTracker : Tracker, CameraSource.IPictureCallback
    {
        private GraphicOverlay overlay;

        private FaceGraphic faceGraphic;

        private CameraSource cameraSource = null;

        private bool isProcessing = false;

        private int currentIdProcessed;

        private FaceService faceService = null;

        public GraphicFaceTracker(GraphicOverlay overlay, CameraSource cameraSource, FaceService faceService)
        {
            this.overlay = overlay;
            this.faceGraphic = new FaceGraphic(overlay);
            this.cameraSource = cameraSource;
            this.faceService = faceService;
        }

        /**
         * Start tracking the detected face instance within the face overlay.
         */
        public override async void OnNewItem(int id, Object item)
        {
            var face = item as Face;

            this.faceGraphic.Face = face;

            await Task.Delay(500);

            this.faceGraphic.IsFaceRecognitionEnabled = true;
            //await this.DetectFace(face);

            // this.faceGraphic.SetId(id);
            // this.faceGraphic.NewFace = item as Face;
            // if (this.cameraSource != null && !this.isProcessing)
            // {
            // this.currentIdProcessed = id;
            // MainActivity.Greetings = MainActivity.DefaultGreetings;
            // MainActivity.CognitiveFacesResult = null;
            // this.cameraSource.TakePicture(null, this);
            // Console.WriteLine("face detected: " + id);
            // }
        }

        private async Task DetectFace(Face face)
        {
            try
            {
                var frame = GraphicHolder.Frame;

                this.faceGraphic.RecognizeTries++;

                var faceImage = await Task.Run(() => GetProcessedImage(frame, face));

                //ExportBitmapAsPNG(faceImage);

                using (var imageStream = new MemoryStream())
                {
                    faceImage.Compress(Bitmap.CompressFormat.Jpeg, 100, imageStream);
                    imageStream.Position = 0;

                    var identityResult = await this.faceService.IdentityFace(imageStream);

                    if (identityResult != null)
                    {
                        this.faceGraphic.IdentificationResult = identityResult;
                        this.faceGraphic.IsRecognized = true;
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);                
            }
        }

        void ExportBitmapAsPNG(Bitmap bitmap)
        {
            var sdCardPath = System.Environment.GetFolderPath(System.Environment.SpecialFolder.ApplicationData).Replace("/.config", ""); //Android.OS.Environment.ExternalStorageDirectory.AbsolutePath;
            var filePath = System.IO.Path.Combine(sdCardPath, "CognitiveFrame.png");
            var stream = new FileStream(filePath, FileMode.Create);
            bitmap.Compress(Bitmap.CompressFormat.Png, 100, stream);
            stream.Close();
        }

        /**
         * Update the position/characteristics of the face within the overlay.
         */
        public override async void OnUpdate(Detector.Detections detections, Object item)
        {
            var face = item as Face;
            this.overlay.Add(this.faceGraphic);
            this.faceGraphic.UpdateFace(face);

            //if face was not detected on add (due tue bad image), retry to detect a face
            if(this.faceGraphic.IsFaceRecognitionEnabled &&
               this.faceGraphic.RecognizeTries < 2 && 
               !this.faceGraphic.IsRecognized)
            {
                await this.DetectFace(face);
            }
        }

        public override void OnMissing(Detector.Detections detections)
        {
            this.overlay.Remove(this.faceGraphic);
        }

        public override void OnDone()
        {
            this.overlay.Remove(this.faceGraphic);
        }

        public void OnPictureTaken(byte[] data)
        {
            Task.Run(
                async () =>
                    {
                        try
                        {
                            this.isProcessing = true;

                            Console.WriteLine("face detected: ");

                            var imageAnalyzer = new ImageAnalyzer(data);
                            await LiveCamHelper.ProcessCameraCapture(imageAnalyzer, this.currentIdProcessed);
                        }
                        finally
                        {
                            this.isProcessing = false;
                        }
                    });
        }

        private Bitmap GetProcessedImage(Frame frame, Face face)
        {
            int rotationAngle = 0;
            int width = frame.GetMetadata().Width;
            int height = frame.GetMetadata().Height;

            switch (frame.GetMetadata().Rotation)
            {
                case FrameRotation.Rotate0:
                    break;
                case FrameRotation.Rotate90:
                    rotationAngle = 90;
                    break;
                case FrameRotation.Rotate180:
                    rotationAngle = 180;
                    break;
                case FrameRotation.Rotate270:
                    rotationAngle = 270;
                    break;
            }

            var buffer = frame.GrayscaleImageData;
            //ByteBuffer buffer = ByteBuffer.Allocate(bitmap.ByteCount);
            //bitmap.CopyPixelsToBuffer(buffer);
            buffer.Rewind();

            IntPtr classHandle = JNIEnv.FindClass("java/nio/ByteBuffer");
            IntPtr methodId = JNIEnv.GetMethodID(classHandle, "array", "()[B");
            IntPtr resultHandle = JNIEnv.CallObjectMethod(buffer.Handle, methodId);
            byte[] bytes = JNIEnv.GetArray<byte>(resultHandle);
            JNIEnv.DeleteLocalRef(resultHandle);

            // var bufferSize = frame.GrayscaleImageData.Capacity();
            // var buffer = new byte[bufferSize];
            // Marshal.Copy(frame.GrayscaleImageData.GetDirectBufferAddress(), buffer, 0, bufferSize);

            // var bytes = frame.GrayscaleImageData.ToArray<byte>();

             YuvImage yuvImage = new YuvImage(bytes, ImageFormatType.Nv21, width, height, null);
            using (var byteArrayOutputStream = new MemoryStream())
            {
                //frame.Bitmap.Compress(Bitmap.CompressFormat.Jpeg, 100, byteArrayOutputStream);

                yuvImage.CompressToJpeg(new Rect(0, 0, width, height), 100, byteArrayOutputStream);
                byte[] jpegArray = byteArrayOutputStream.GetBuffer();
                Bitmap bitmap = BitmapFactory.DecodeByteArray(jpegArray, 0, jpegArray.Length);
                Matrix matrix = new Matrix();
                matrix.PostRotate(rotationAngle);
                Bitmap scaledBitmap = Bitmap.CreateScaledBitmap(bitmap, width, height, true);
                Bitmap rotatedBitmap = Bitmap.CreateBitmap(
                    scaledBitmap,
                    0,
                    0,
                    scaledBitmap.Width,
                    scaledBitmap.Height,
                    matrix,
                    true);

                return this.Crop(rotatedBitmap, face);
            }
        }

        /**
            * Draws the face annotations for position on the supplied canvas.
        */
        private Bitmap Crop(Bitmap bitmap, Face face)
        {
            try
            {
                float left = face.Position.X;
                float top = face.Position.Y + 30;
                float width = face.Width;
                float height = face.Height;

                if (left < 0)
                {
                    left = 0;
                }

                if (left + width > bitmap.Width)
                {
                    width = bitmap.Width - left - 1;
                }

                if (top < 0)
                {
                    top = 0;
                }

                if (top + height > bitmap.Height)
                {
                    height = bitmap.Height - top - 1;
                }

                return Bitmap.CreateBitmap(bitmap, (int)left, (int)top, (int)width, (int)height);
            }
            catch (Exception e)
            {
                return Bitmap.CreateBitmap(bitmap, 0, 0, 1, 1);
            }
        }
    }
}