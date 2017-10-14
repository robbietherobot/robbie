using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Web;

namespace RobbieVision.IO
{
    /// <summary>
    /// Imaging class handling all image related operations, mainly focussing on disk IO.
    /// </summary>
    public class Imaging
    {
        /// <summary>
        /// Save the given image data in bytes to desk using the JPEG image format.
        /// </summary>
        /// <param name="sessionId">The ID of the session to store this image for.</param>
        /// <param name="bytes">The image data as byte array.</param>
        public static void SaveImage(string sessionId, byte[] bytes)
        {
            if (bytes == null || bytes.Length == 0) return;

            using (var memoryStream = new MemoryStream(bytes))
            {
                var image = Image.FromStream(memoryStream);
                var filePath = $@"{HttpRuntime.AppDomainAppPath}DataStorage\{sessionId}.jpg";
                image.Save(filePath, ImageFormat.Jpeg);
                image.Dispose();
            }
        }
    }
}