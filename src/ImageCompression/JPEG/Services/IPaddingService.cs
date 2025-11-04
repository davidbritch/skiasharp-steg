using ImageCompression.JPEG.Models;

namespace ImageCompression.JPEG.Services;

public interface IPaddingService
{
    ColourData ApplyPadding(ColourData input, int width, int height);
    int CalculatePaddedDimension(int input);
}