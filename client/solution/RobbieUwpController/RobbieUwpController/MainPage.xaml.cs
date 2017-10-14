using System;
using System.IO;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using RobbieSenses;
using Windows.ApplicationModel.Core;
using Windows.Graphics.Display;
using Windows.Graphics.Imaging;
using Windows.Storage.Streams;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media.Imaging;
using RobbieSenses.Evaluation;

namespace RobbieUwpController
{
    public sealed partial class MainPage : IDisposable
    {
        /// <summary>
        /// The brain object containing all business logic controlling Robbie.
        /// </summary>
        private Brain brain;

        /// <summary>
        /// Initializing the Page component.
        /// </summary>
        public MainPage()
        {
            InitializeComponent();
        }
        
        /// <summary>
        /// Handle the load event of the UWP app, starting all continuous tasks and firing up the brains.
        /// </summary>
        /// <param name="sender">Sender object.</param>
        /// <param name="e">Routed event arguments object.</param>
        private async void Page_Loaded(object sender, RoutedEventArgs e)
        {
            // start the brain
            brain = new Brain(VisionPreview, PreviewCanvas, AudioPlaybackElement);
            brain.SenseEvent += Brain_SenseEvent;
            brain.WakeUp();
            
            // we're ready!
            await brain.Say("Hello, I'm ready initializing");
        }

        /// <summary>
        /// Handle the on click event of the store face for {user} button.
        /// </summary>
        /// <param name="sender">Sender object.</param>
        /// <param name="e">Routed event arguments object.</param>
        private void StoreFaceFor_Click(object sender, RoutedEventArgs e)
        {
            brain.StoreFaceFor(Username.Text);
        }

        /// <summary>
        /// Handle the on click event of the train faces button.
        /// </summary>
        /// <param name="sender">Sender object.</param>
        /// <param name="e">Routed event arguments object.</param>
        private async void TrainFaces_Click(object sender, RoutedEventArgs e)
        {
            await brain.TrainFaces();
        }

        /// <summary>
        /// Handle the on click event of the Quit button to neatly close the (test) application.
        /// </summary>
        /// <param name="sender">Sender object.</param>
        /// <param name="e">Routed event arguments object.</param>
        private void Quit_Click(object sender, RoutedEventArgs e)
        {
            Dispose();
        }

        /// <summary>
        /// Handle the SenseEvent, fired whenever Robbie's brains do something, used to give feedback to the CLI.
        /// </summary>
        /// <param name="sense">The sense that fired the event.</param>
        /// <param name="message">A message describing the event.</param>
        private async void Brain_SenseEvent(string sense, string message)
        {
            await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                // print the event to the screen of the UWP app
                CLI.Text = string.Format("{0}: {1}{2}", sense, message, Environment.NewLine) + CLI.Text;
            });

            // if the quit command is detected by the brain, start the disposal of the application
            if (message.Equals(UtterancePrediction.QuitCommand, StringComparison.CurrentCultureIgnoreCase))
            {
                Dispose();
            }
        }

        /// <summary>
        /// Gets the rendered image of the given canvas as a JPEG converted byte array.
        /// </summary>
        /// <param name="canvas">The canvas to get the bytes from.</param>
        /// <returns>A byte array containing the image data of the given canvas</returns>
        public async Task<byte[]> GetBytesFromCanvas(UIElement canvas)
        {
            // if the preview canvas is not yet set, abort
            if (PreviewCanvas == null) return null;

            // render the contents of the canvas on the target bitmap
            var previewCanvasBitmap = new RenderTargetBitmap();
            await previewCanvasBitmap.RenderAsync(canvas);

            // try to get the pixel buffer from the rendered target bitmap, but return if not succeeded
            var pixelBuffer = await previewCanvasBitmap.GetPixelsAsync();
            if (pixelBuffer.Capacity == 0) return null;

            byte[] pixels;

            // encode the pixel buffer data to a JPEG format and read the pixel data from the memory stream
            using (var randomAccessStream = new InMemoryRandomAccessStream())
            {
                var encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.JpegEncoderId, randomAccessStream);
                encoder.SetPixelData(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Ignore,
                    (uint) previewCanvasBitmap.PixelWidth, (uint) previewCanvasBitmap.PixelHeight,
                    DisplayInformation.GetForCurrentView().LogicalDpi,
                    DisplayInformation.GetForCurrentView().LogicalDpi,
                    pixelBuffer.ToArray());

                await encoder.FlushAsync();

                var pixelCount = Convert.ToInt32(pixelBuffer.Length);
                pixels = new byte[pixelCount];

                await randomAccessStream.AsStream().ReadAsync(pixels, 0, pixelCount);
            }

            return pixels;
        }

        /// <summary>
        /// Handle the unload event of the UWP app, disposing all the used devices and objects.
        /// </summary>
        /// <param name="sender">Sender object.</param>
        /// <param name="e">Routed event arguments object.</param>
        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            Dispose();
        }

        /// <summary>
        /// Disposes the page, shuts down the brain and exits the application. 
        /// </summary>
        public void Dispose()
        {
            brain.Dispose();
            CoreApplication.Exit();
        }
    }
}