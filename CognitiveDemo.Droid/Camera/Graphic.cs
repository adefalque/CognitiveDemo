namespace CognitiveDemo.Droid.Camera
{
    using Android.Gms.Vision;
    using Android.Graphics;

    /**
     * Base class for a custom graphics object to be rendered within the graphic overlay.  Subclass
     * this and implement the {@link Graphic#draw(Canvas)} method to define the
     * graphics element.  Add instances to the overlay using {@link GraphicOverlay#add(Graphic)}.
     */
    public abstract class Graphic
    {
        private GraphicOverlay mOverlay;

        public Graphic(GraphicOverlay overlay)
        {
            this.mOverlay = overlay;
        }

        /**
         * Draw the graphic on the supplied canvas.  Drawing should use the following methods to
         * convert to view coordinates for the graphics that are drawn:
         * <ol>
         * <li>{@link Graphic#scaleX(float)} and {@link Graphic#scaleY(float)} adjust the size of
         * the supplied value from the preview scale to the view scale.</li>
         * <li>{@link Graphic#translateX(float)} and {@link Graphic#translateY(float)} adjust the
         * coordinate from the preview's coordinate system to the view coordinate system.</li>
         * </ol>
         *
         * @param canvas drawing canvas
         */
        public abstract void Draw(Canvas canvas);

        /**
         * Adjusts a horizontal value of the supplied value from the preview scale to the view
         * scale.
         */
        public float ScaleX(float horizontal)
        {
            return horizontal * this.mOverlay.WidthScaleFactor;
        }

        /**
         * Adjusts a vertical value of the supplied value from the preview scale to the view scale.
         */
        public float ScaleY(float vertical)
        {
            return vertical * this.mOverlay.HeightScaleFactor;
        }

        /**
         * Adjusts the x coordinate from the preview's coordinate system to the view coordinate
         * system.
         */
        public float TranslateX(float x)
        {
            if (this.mOverlay.CameraFacing == CameraFacing.Front)
            {
                return this.mOverlay.Width - this.ScaleX(x);
            }
            else
            {
                return this.ScaleX(x);
            }
        }

        /**
         * Adjusts the y coordinate from the preview's coordinate system to the view coordinate
         * system.
         */
        public float TranslateY(float y)
        {
            return this.ScaleY(y);
        }

        public void PostInvalidate()
        {
            this.mOverlay.PostInvalidate();
        }
    }
}