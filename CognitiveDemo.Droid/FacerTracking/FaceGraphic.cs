namespace CognitiveDemo.Droid
{
    using System;

    using Android.Gms.Vision.Faces;
    using Android.Graphics;

    using CognitiveDemo.Droid.Camera;
    using CognitiveDemo.Droid.Views;

    public class FaceGraphic : Graphic
    {
        private static readonly float FACE_POSITION_RADIUS = 10.0f;
        private static readonly float ID_TEXT_SIZE = 35.0f;
        private static readonly float ID_Y_OFFSET = 50.0f;
        private static readonly float ID_X_OFFSET = 20.0f;
        private static readonly float BOX_STROKE_WIDTH = 5.0f;

        private static Color[] COLOR_CHOICES = {
        Color.Blue,
        Color.Cyan,
        Color.Green,
        Color.Magenta,
        Color.Red,
        Color.White,
        Color.Yellow
    };
        private static int currentColorIndex = 0;

        private Paint textPaint;
        private Paint boxPaint;

        public Face Face { get; set; }

        public bool IsFaceRecognitionEnabled { get; set; }

        public bool IsRecognized { get; set; }

        public int RecognizeTries { get; set; }

        public FaceGraphic(GraphicOverlay overlay) : base(overlay)
        {
            this.IsRecognized = false;

            this.textPaint = new Paint()
            {
                TextSize = ID_TEXT_SIZE
            };

            this.boxPaint = new Paint();
            this.boxPaint.SetStyle(Paint.Style.Stroke);
            this.boxPaint.StrokeWidth = BOX_STROKE_WIDTH;
        }

        public FaceIdentificationResult IdentificationResult { get; set; }

        /**
         * Updates the face instance from the detection of the most recent frame.  Invalidates the
         * relevant portions of the overlay to trigger a redraw.
         */
        public void UpdateFace(Face face)
        {
            this.Face = face;
            this.PostInvalidate();
        }

        public override void Draw(Canvas canvas)
        {
            if (Face == null)
            {
                return;
            }

            Color color;

            if (this.IdentificationResult == null)
            {
                color = Color.WhiteSmoke;
            }
            else if (this.IdentificationResult.HasDetectionError)
            {
                color = Color.IndianRed;
            }
            else if (string.IsNullOrEmpty(this.IdentificationResult.PersonName))
            {
                color = Color.Orange;
            }
            else
            {
                color = Color.LightGreen;
            }

            this.textPaint.Color = this.boxPaint.Color = color;

            float faceMiddleX = this.TranslateX(Face.Position.X + Face.Width / 2);
            float faceMiddleY = this.TranslateY(Face.Position.Y + Face.Height / 2);

            float boxXOffset = this.ScaleX(Face.Width / 2.0f);
            float boxYOffset = this.ScaleY(Face.Height / 2.0f);
            float boxLeft = faceMiddleX - boxXOffset;
            float boxTop = faceMiddleY - boxYOffset;
            float boxRight = faceMiddleX + boxXOffset;
            float boxBottom = faceMiddleY + boxYOffset;
            //canvas.DrawCircle(x, y, FACE_POSITION_RADIUS, mFacePositionPaint);

            canvas.DrawText
                ($"{Math.Round(Math.Max(Face.IsSmilingProbability, 0), 2) * 100}% happy",
                boxLeft + ID_X_OFFSET, 
                boxTop + ID_Y_OFFSET,
                this.textPaint);

            //canvas.DrawText(
            //    $"Your right eye is {Math.Round(face.IsRightEyeOpenProbability, 2) * 100}% opened", 
            //    boxLeft + ID_X_OFFSET,
            //    boxTop + ID_Y_OFFSET * 2, 
            //    this.faceTextPaint);

            //canvas.DrawText(
            //    $"Your left eye is {Math.Round(face.IsLeftEyeOpenProbability, 2) * 100}% opened", 
            //    boxLeft + ID_X_OFFSET, 
            //    boxTop + ID_Y_OFFSET * 3, 
            //    this.faceTextPaint);

            string boxTitle = $"Face #{this.Face.Id}";
            if (this.IdentificationResult != null)
            {
                int iLevel = 2;
                canvas.DrawText(
                    $"{this.IdentificationResult.Gender}, {this.IdentificationResult.Age} years old",
                    boxLeft + ID_X_OFFSET,
                    boxTop + ID_Y_OFFSET * iLevel++,
                    this.textPaint);

                if (!string.IsNullOrEmpty(this.IdentificationResult.Hair))
                {
                    canvas.DrawText(
                        this.IdentificationResult.Hair,
                        boxLeft + ID_X_OFFSET,
                        boxTop + ID_Y_OFFSET * iLevel++,
                        this.textPaint);
                }

                if (!string.IsNullOrEmpty(this.IdentificationResult.Emotion))
                {
                    canvas.DrawText(
                        this.IdentificationResult.Emotion,
                        boxLeft + ID_X_OFFSET,
                        boxTop + ID_Y_OFFSET * iLevel,
                        this.textPaint);
                }

                if (!string.IsNullOrEmpty(this.IdentificationResult.PersonName))
                {
                    boxTitle = this.IdentificationResult.PersonName;
                }
            }
            else
            {
                canvas.DrawText(
                    "", //FaceIdentifyActivity.DefaultGreetings,
                    boxLeft + ID_X_OFFSET,
                    boxTop + ID_Y_OFFSET * 2,
                    this.textPaint);
            }

            canvas.DrawText(
                boxTitle,
                boxLeft + ID_X_OFFSET, 
                boxTop - (ID_Y_OFFSET / 2),
                this.textPaint);

            // Draws a bounding box around the face.           
            canvas.DrawRect(boxLeft, boxTop, boxRight, boxBottom, this.boxPaint);
        }
    }
}