namespace CognitiveDemo.Droid.Camera
{
    using System;
    using System.Collections.Generic;

    using Android.Content;
    using Android.Gms.Vision;
    using Android.Graphics;
    using Android.Util;
    using Android.Views;

    /**
     * A view which renders a series of custom graphics to be overlayed on top of an associated preview
     * (i.e., the camera preview).  The creator can add graphics objects, update the objects, and remove
     * them, triggering the appropriate drawing and invalidation within the view.<p>
     *
     * Supports scaling and mirroring of the graphics relative the camera's preview properties.  The
     * idea is that detection items are expressed in terms of a preview size, but need to be scaled up
     * to the full view size, and also mirrored in the case of the front-facing camera.<p>
     *
     * Associated {@link Graphic} items should use the following methods to convert to view coordinates
     * for the graphics that are drawn:
     * <ol>
     * <li>{@link Graphic#scaleX(float)} and {@link Graphic#scaleY(float)} adjust the size of the
     * supplied value from the preview scale to the view scale.</li>
     * <li>{@link Graphic#translateX(float)} and {@link Graphic#translateY(float)} adjust the coordinate
     * from the preview's coordinate system to the view coordinate system.</li>
     * </ol>
     */
    public class GraphicOverlay : View
    {
        private Object mLock = new Object();
        private int mPreviewWidth;
        private float mWidthScaleFactor = 1.0f;
        private int mPreviewHeight;
        private float mHeightScaleFactor = 1.0f;
        private CameraFacing mFacing = CameraFacing.Front;
        private HashSet<Graphic> mGraphics = new HashSet<Graphic>();

        public int PreviewWidth { get => this.mPreviewWidth; set => this.mPreviewWidth = value; }
        public float WidthScaleFactor { get => this.mWidthScaleFactor; set => this.mWidthScaleFactor = value; }
        public int PreviewHeight { get => this.mPreviewHeight; set => this.mPreviewHeight = value; }
        public float HeightScaleFactor { get => this.mHeightScaleFactor; set => this.mHeightScaleFactor = value; }
        public CameraFacing CameraFacing { get => this.mFacing; set => this.mFacing = value; }

        public GraphicOverlay(Context context, IAttributeSet attrs) : base(context, attrs)
        {
            
        }

        /// <summary>
        /// Removes all graphics from the overlay.
        /// </summary>
        public void Clear()
        {
            lock(this.mLock) {
                this.mGraphics.Clear();
            }
            this.PostInvalidate();
        }

        /// <summary>
        /// Adds a graphic to the overlay.
        /// </summary>
        /// <param name="graphic"></param>
        public void Add(Graphic graphic)
        {
            lock(this.mLock) {
                this.mGraphics.Add(graphic);
            }
            this.PostInvalidate();
        }

        /// <summary>
        /// Removes a graphic from the overlay.
        /// </summary>
        /// <param name="graphic"></param>
        public void Remove(Graphic graphic)
        {
            lock(this.mLock) {
                this.mGraphics.Remove(graphic);
            }
            this.PostInvalidate();
        }
       
        /// <summary>
        ///  Sets the camera attributes for size and facing direction, which informs how to transform image coordinates later.
        /// </summary>
        /// <param name="previewWidth"></param>
        /// <param name="previewHeight"></param>
        /// <param name="facing"></param>
        public void SetCameraInfo(int previewWidth, int previewHeight, CameraFacing facing)
        {
            lock(this.mLock) {
                this.PreviewWidth = previewWidth;
                this.PreviewHeight = previewHeight;
                this.CameraFacing = facing;
            }
            this.PostInvalidate();
        }

        public override void Draw(Canvas canvas)
        {
            base.Draw(canvas);
            lock(this.mLock) {
                if ((this.PreviewWidth != 0) && (this.PreviewHeight != 0))
                {
                    this.WidthScaleFactor = (float)canvas.Width / (float)this.PreviewWidth;
                    this.HeightScaleFactor = (float)canvas.Height / (float)this.PreviewHeight;
                }

                foreach (Graphic graphic in this.mGraphics)
                {
                    graphic.Draw(canvas);
                }
            }
        }
    }
}