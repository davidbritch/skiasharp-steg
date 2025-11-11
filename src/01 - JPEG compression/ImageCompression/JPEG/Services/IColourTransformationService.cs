using ImageCompression.JPEG.Models;
using SkiaSharp;

namespace ImageCompression.JPEG.Services;

public interface IColourTransformationService
{
    public ColourData RGBToYCbCr(SKBitmap bitmap);
}