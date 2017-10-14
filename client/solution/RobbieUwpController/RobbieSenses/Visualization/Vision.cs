using RobbieSenses.Evaluation;
using System;
using System.Collections.Generic;
using System.Linq;
using Windows.Foundation;
using Windows.Graphics.Imaging;
using Windows.UI;
using Windows.UI.Text;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Shapes;

namespace RobbieSenses.Visualization
{
    /// <summary>
    /// Helper class to visualize everything Robbie sees, thinks or discovers.
    /// </summary>
    public class Vision
    {
        /// <summary>
        /// Constant defining the text printed in the upper left corner of the visualization.
        /// </summary>
        private const string RobbieLogoText = "ROBBIE vision";

        /// <summary>
        /// Constant defining the required bitmap format for the visualization process, used for verification and conversion.
        /// </summary>
        private const BitmapPixelFormat RequiredBitmapFormat = BitmapPixelFormat.Bgra8;

        /// <summary>
        /// Brush defining the color of the crop marks.
        /// </summary>
        private readonly SolidColorBrush cropMarksBrush;

        /// <summary>
        /// Brush defining the color of the detected face boxes.
        /// </summary>
        private readonly SolidColorBrush faceBoxBrush;

        /// <summary>
        /// Brush defining the color of the on screen text.
        /// </summary>
        private readonly SolidColorBrush onScreenTextBrush;

        /// <summary>
        /// Brush defining the color of the meta data backdrop.
        /// </summary>
        private readonly SolidColorBrush metaDataFillBrush;

        /// <summary>
        /// Brush defining the color of the meta data text.
        /// </summary>
        private readonly SolidColorBrush metaDataTextBrush;

        /// <summary>
        /// Transparent brush used for multiple strokes and fills that should be transparent.
        /// </summary>
        private readonly SolidColorBrush transparentBrush;

        /// <summary>
        /// Default font family used for writing text onto the screen.
        /// </summary>
        private readonly FontFamily defaultFamily;

        /// <summary>
        /// Initializes the visualization class.
        /// </summary>
        public Vision()
        {
            cropMarksBrush = new SolidColorBrush(Colors.LightCyan);
            faceBoxBrush = new SolidColorBrush(Colors.LimeGreen);
            onScreenTextBrush = new SolidColorBrush(Colors.LightGray);
            metaDataFillBrush = new SolidColorBrush(Color.FromArgb(127, 0, 0, 0));
            metaDataTextBrush = new SolidColorBrush(Colors.White);
            transparentBrush = new SolidColorBrush(Colors.Transparent);

            defaultFamily = new FontFamily("Calibri");
        }

        /// <summary>
        /// Decorates the given bitmap image with meta data info, pointing out detected faces and their identity,
        /// the focal point coordinates to move to and other meta data like a time stamp and a Robbie 'logo'.
        /// </summary>
        /// <param name="canvas">The canvas to draw the result on.</param>
        /// <param name="bitmap">The bitmap containing the image of the screen capture.</param>
        /// <param name="faces">A list of detected faces.</param>
        /// <param name="focalPoint">The point for the camera to focus on, of which the coordinates will be shown on screen.</param>
        public async void DecorateScreenCapture(Canvas canvas, SoftwareBitmap bitmap, List<TrackedIdentity> faces, Point focalPoint)
        {
            // if not already so, convert the bitmap to the required format
            if (bitmap.BitmapPixelFormat != RequiredBitmapFormat)
            {
                try
                {
                    bitmap = SoftwareBitmap.Convert(bitmap, RequiredBitmapFormat);
                }
                catch (Exception)
                {
                    // if the supplied bitmap isn't convertable to the desired format, just skip this frame
                    return;
                }
            }

            // draw the contents of the software bitmap to the canvas
            var brush = new ImageBrush();
            var bitmapSource = new SoftwareBitmapSource();
            await bitmapSource.SetBitmapAsync(bitmap);
            brush.ImageSource = bitmapSource;
            brush.Stretch = Stretch.Fill;
            canvas.Background = brush;

            // clear previously added marks and texts on bitmap
            canvas.Children.Clear();

            // determine canvas scaling
            var widthScale = bitmap.PixelWidth / canvas.ActualWidth;
            var heightScale = bitmap.PixelHeight / canvas.ActualHeight;

            // add cropp marks
            DrawCropMarks(canvas, 15d, 50d, widthScale, heightScale);

            // loop through all available faces, adding them to the canvas child elements
            if (faces != null)
            {
                foreach (var face in faces)
                {
                    DrawFaceBox(canvas, face, widthScale, heightScale);
                    DrawFaceCaption(canvas, face, widthScale, heightScale);
                }
            }

            // add the meta data overlay
            DrawMetaData(canvas, 25d, focalPoint, widthScale, heightScale);
            DrawTimeStamp(canvas, 25d, widthScale, heightScale);

            // dispose the bitmap source after rendering the canvas
            bitmap.Dispose();
        }

        /// <summary>
        /// Draws crop marks on the given canvas.
        /// </summary>
        /// <param name="canvas">The canvas to draw the crop marks on.</param>
        /// <param name="offset">The relative offset of the crop marks from the canvas edge.</param>
        /// <param name="size">The relative size of the legs of the crop marks in pixels.</param>
        /// <param name="widthScale">The horizontal canvas scaling factor.</param>
        /// <param name="heightScale">The vertical canvas scaling factor.</param>
        private void DrawCropMarks(Canvas canvas, double offset, double size, double widthScale, double heightScale)
        {
            var offsetX = offset / widthScale;
            var offsetY = offset / heightScale;

            var sizeX = size / widthScale;
            var sizeY = size / heightScale;

            var topLeft = new PointCollection
            {
                new Point(offsetX, offsetY + sizeY),
                new Point(offsetX, offsetY),
                new Point(offsetX + sizeX, offsetY)
            };
            DrawCropMark(canvas, topLeft);

            var topRight = new PointCollection
            {
                new Point(canvas.ActualWidth - offsetX, offsetY + sizeY),
                new Point(canvas.ActualWidth - offsetX, offsetY),
                new Point(canvas.ActualWidth - (offsetX + sizeX), offsetY)
            };
            DrawCropMark(canvas, topRight);

            var bottomLeft = new PointCollection
            {
                new Point(offsetX, canvas.ActualHeight - (offsetY + sizeY)),
                new Point(offsetX, canvas.ActualHeight - offsetY),
                new Point(offsetX + sizeX, canvas.ActualHeight - offsetY)
            };
            DrawCropMark(canvas, bottomLeft);

            var bottomRight = new PointCollection
            {
                new Point(canvas.ActualWidth - offsetX, canvas.ActualHeight - (offsetY + sizeY)),
                new Point(canvas.ActualWidth - offsetX, canvas.ActualHeight - offsetY),
                new Point(canvas.ActualWidth - (offsetX + sizeX), canvas.ActualHeight - offsetY)
            };
            DrawCropMark(canvas, bottomRight);
        }

        /// <summary>
        /// Draws a crop mark using the given polygon coordinates.
        /// </summary>
        /// <param name="canvas">The canvas to draw the crop mark on.</param>
        /// <param name="points">The point collection describing the polygon points of the crop mark.</param>
        private void DrawCropMark(Canvas canvas, PointCollection points)
        {
            var cropMark = new Polyline()
            {
                Points = points,
                Stroke = cropMarksBrush,
                StrokeThickness = 2d
            };

            canvas.Children.Add(cropMark);
        }

        /// <summary>
        /// Draws a face box around the given face.
        /// </summary>
        /// <param name="canvas">The canvas to draw the face box on.</param>
        /// <param name="face">The (detected) face to draw the box around.</param>
        /// <param name="widthScale">The horizontal canvas scaling factor.</param>
        /// <param name="heightScale">The vertical canvas scaling factor.</param>
        private void DrawFaceBox(Canvas canvas, TrackedIdentity face, double widthScale, double heightScale)
        {
            var box = new Rectangle
            {
                Tag = face.FaceBox,
                Width = (uint)(face.FaceBox.Width / widthScale),
                Height = (uint)(face.FaceBox.Height / heightScale),
                Fill = transparentBrush,
                Stroke = faceBoxBrush,
                StrokeThickness = 3d,
                Margin = new Thickness((uint)(face.FaceBox.X / widthScale), (uint)(face.FaceBox.Y / heightScale), 0, 0)
            };

            canvas.Children.Add(box);
        }

        /// <summary>
        /// Draws a textual caption under the given face.
        /// </summary>
        /// <param name="canvas">The canvas to draw the face caption on.</param>
        /// <param name="face">The (detected) face to draw the caption under.</param>
        /// <param name="widthScale">The horizontal canvas scaling factor.</param>
        /// <param name="heightScale">The vertical canvas scaling factor.</param>
        private void DrawFaceCaption(Canvas canvas, TrackedIdentity face, double widthScale, double heightScale)
        {
            const double fontSize = 20d;

            var caption = BuildFaceCaption(face);
            if (string.IsNullOrEmpty(caption)) return;

            var textBlock = new TextBlock()
            {
                Tag = face.FaceBox,
                Text = caption,
                Foreground = onScreenTextBrush,
                FontFamily = defaultFamily,
                FontSize = fontSize / widthScale,
                FontWeight = FontWeights.Bold,
                Margin = new Thickness((uint)(face.FaceBox.X / widthScale), (uint)((face.FaceBox.Y + face.FaceBox.Height) / heightScale), 0, 0)
            };

            canvas.Children.Add(textBlock);
        }

        /// <summary>
        /// Builds up a string containing all available information of the given face.
        /// </summary>
        /// <param name="face">The (detected) face to build up the caption text for.</param>
        /// <returns></returns>
        private string BuildFaceCaption(TrackedIdentity face)
        {
            const int guidLength = 36;
            const string delimiter = "-";

            var caption = string.Empty;

            // add the name
            if (!string.IsNullOrEmpty(face.Name))
            {
                caption = face.Name;

                // if the face name contains a name and a Guid, split those values over two lines
                if (caption.Contains("-") && caption.Length > guidLength)
                {
                    var delimiterPosition = caption.IndexOf(delimiter, StringComparison.Ordinal);
                    caption = $"{caption.Substring(0, delimiterPosition)}{Environment.NewLine}{caption.Substring(++delimiterPosition)}";
                    caption = FirstCharacterToUpperCase(caption);
                }
            }

            // add (parts of) the detected appearance
            if (face.Appearance != null)
            {
                if (!string.IsNullOrEmpty(caption))
                {
                    caption += Environment.NewLine;
                }

                caption += $"{face.Appearance.Age} y/o {face.Appearance.Gender}";
            }

            // add the emotions
            if (face.EmotionScores != null)
            {
                if (!string.IsNullOrEmpty(caption))
                {
                    caption += Environment.NewLine;
                }

                var topScoringEmotion = face.EmotionScores.ToRankedList().First();
                caption += $"{topScoringEmotion.Key}: {(int)Math.Round(topScoringEmotion.Value * 100)}%";
            }

            return caption;
        }

        /// <summary>
        /// Draws a meta data bar on the given canvas, including the Robbie text logo and the indicated focal point.
        /// </summary>
        /// <param name="canvas">The canvas to draw the meta data on.</param>
        /// <param name="offset">The relative offset of the meta data from the canvas edge, excluding the character spacing required to clear the edge.</param>
        /// <param name="focalPoint">The focal point of which the coordinates should be displayed within the meta data bar.</param>
        /// <param name="widthScale">The horizontal canvas scaling factor.</param>
        /// <param name="heightScale">The vertical canvas scaling factor.</param>
        private void DrawMetaData(Canvas canvas, double offset, Point focalPoint, double widthScale, double heightScale)
        {
            const double fontSize = 19d;

            var background = new Rectangle
            {
                Width = canvas.ActualWidth,
                Height = fontSize / widthScale * 1.3,
                Fill = metaDataFillBrush,
                Stroke = transparentBrush,
                Margin = new Thickness(0, offset / heightScale, 0, 0)
            };

            canvas.Children.Add(background);

            var metaData = new TextBlock()
            {
                Text = $"{RobbieLogoText} - Focal point: {focalPoint.X}, {focalPoint.Y}",
                Foreground = metaDataTextBrush,
                FontFamily = defaultFamily,
                FontSize = fontSize / widthScale,
                Margin = new Thickness(offset * 1.5 / widthScale, offset / heightScale, 0, 0)
            };

            canvas.Children.Add(metaData);
        }

        /// <summary>
        /// Draws a time stamp on the given canvas.
        /// </summary>
        /// <param name="canvas">The canvas to draw the time stamp on.</param>
        /// <param name="offset">The relative offset of the time stamp from the canvas edge, excluding the character spacing required to clear the edge.</param>
        /// <param name="widthScale">The horizontal canvas scaling factor.</param>
        /// <param name="heightScale">The vertical canvas scaling factor.</param>
        private void DrawTimeStamp(Canvas canvas, double offset, double widthScale, double heightScale)
        {
            const double fontSize = 15d;
            const string timePrefix = @"TI\ME:";

            var timeStamp = new TextBlock()
            {
                Text = DateTime.Now.ToString($"MMM d, yyyy{Environment.NewLine}{timePrefix} h:mm tt").ToUpper(),
                Foreground = onScreenTextBrush,
                FontFamily = defaultFamily,
                FontSize = fontSize / widthScale,
                Margin = new Thickness(offset * 1.25 / widthScale, canvas.ActualHeight - offset * 1.25 / heightScale - fontSize / widthScale * 1.3 * 2, 0, 0)
            };

            canvas.Children.Add(timeStamp);
        }

        /// <summary>
        /// Turns the first character of a string into uppercase.
        /// </summary>
        /// <param name="value">The string value to transform into a string starting with an upper case character.</param>
        /// <returns>A string value of which the first character is upper case.</returns>
        private string FirstCharacterToUpperCase(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return string.Empty;
            }

            var charArray = value.Trim().ToCharArray();
            charArray[0] = char.ToUpper(charArray[0]);
            return new string(charArray);
        }
    }
}
