using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace Imagegram.WebApi.Helper
{
    public class ImageHelper
    {
        public static MemoryStream ResizeImage(Stream imageStream, int width, int height)
        {
            var destRect = new Rectangle(0, 0, width, height);

            Image image = Image.Load(imageStream);
            image.Mutate(x => x.Resize(width, height));

            var outStream = new MemoryStream();
            image.SaveAsJpeg(outStream);

            return outStream;
        }
    }
}
