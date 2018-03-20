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

namespace CognitiveDemo.Droid.FacerTracking
{
    using Android.Gms.Vision;
    using Android.Gms.Vision.Faces;
    using Android.Util;

    public class MyFaceDetector : Detector
    {
        private readonly Detector detector;

        public MyFaceDetector(Detector detector)
        {
            this.detector = detector;
        }

        public override bool IsOperational
        {
            get
            {
                return this.detector.IsOperational;
            }
        }

        //public override void ReceiveFrame(Frame frame)
        //{
        //    this.detector.ReceiveFrame(frame);
        //}

        public override SparseArray Detect(Frame frame)
        {
            GraphicHolder.Frame = frame;

            return this.detector.Detect(frame);
        }

        public override bool SetFocus(int id)
        {
            return detector.SetFocus(id);
        }

        //public override void SetProcessor(IProcessor processor)
        //{
        //    this.detector.SetProcessor(processor);

        //    base.SetProcessor(processor);
        //}
    }
}