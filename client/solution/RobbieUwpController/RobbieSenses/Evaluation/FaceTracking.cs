using System;
using System.Collections.Generic;
using Windows.Graphics.Imaging;
using Windows.Media;
using Windows.Media.FaceAnalysis;

namespace RobbieSenses.Evaluation
{
    /// <summary>
    /// Class implementing the face tracking functionality based on the Windows FaceAnalysis namespace.
    /// </summary>
    public class FaceTracking
    {
        /// <summary>
        /// The actual face detector object of the Windows FaceAnalysis namespace.
        /// </summary>
        private FaceDetector detector;

        /// <summary>
        /// The actual face tracker object of the Windows FaceAnalysis namespace.
        /// </summary>
        private FaceTracker tracker;

        /// <summary>
        /// The list of the most recent detected faces and their properties.
        /// </summary>
        private IList<DetectedFace> detectedFaces;

        /// <summary>
        /// Constructor initializing a face tracking object.
        /// </summary>
        public FaceTracking()
        {
            Initialize();
        }

        /// <summary>
        /// Initializes the face detector and tracker used for this face tracking class.
        /// </summary>
        private async void Initialize()
        {
            detector = await FaceDetector.CreateAsync();
            tracker = await FaceTracker.CreateAsync();
        }

        /// <summary>
        /// Detects faces in a single frame, using a software bitmap object as a source.
        /// The detected faces will be stored in the corresponding local class member.
        /// </summary>
        /// <param name="bitmap">The software bitmap object to detect the faces in.</param>
        public async void Detect(SoftwareBitmap bitmap)
        {
            var convertedBitmap = SoftwareBitmap.Convert(bitmap, BitmapPixelFormat.Gray8);
            detectedFaces = await detector.DetectFacesAsync(convertedBitmap);
            
            convertedBitmap.Dispose();
        }

        /// <summary>
        /// Detects faces in a frame of a running video or stream, using a video frame object as a source.
        /// The detected faces will be stored in the corresponding local class member.
        /// 
        /// Use this method if you want to continuously track faces in a stream or video.
        /// </summary>
        /// <param name="currentFrame">The video frame object representing the latest snapshot frame to detect the faces in.</param>
        public async void Track(VideoFrame currentFrame)
        {
            if (currentFrame != null && currentFrame.SoftwareBitmap.BitmapPixelFormat == BitmapPixelFormat.Nv12)
            {
                detectedFaces = await tracker.ProcessNextFrameAsync(currentFrame);
            }
        }

        /// <summary>
        /// The list of the most recent detected faces and their properties.
        /// </summary>
        public IList<DetectedFace> DetectedFaces => detectedFaces;
    }
}