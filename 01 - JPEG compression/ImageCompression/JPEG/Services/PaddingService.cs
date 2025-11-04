using ImageCompression.JPEG.Models;
using ImageCompression.JPEG.Extensions;

namespace ImageCompression.JPEG.Services;

public class PaddingService : IPaddingService
{
    public ColourData ApplyPadding(ColourData input, int width, int height)
    {
        if (input == null)
            throw new ArgumentNullException(nameof(input), nameof(input).ToArgumentNullExceptionMessage());

        if (width <= 0)
            throw new ArgumentNullException(nameof(width), nameof(width).ToArgumentEqualsZeroExceptionMessage());

        if (height <= 0)
            throw new ArgumentNullException(nameof(height), nameof(height).ToArgumentEqualsZeroExceptionMessage());

        if (width % 8 == 0 && height % 8 == 0)
            return input;

        var result = new ColourData(CalculatePaddedDimension(width), CalculatePaddedDimension(height));

        result.Y = ApplyEdgeExtensionPadding(input.Y, width, height);
        result.Cr = ApplyEdgeExtensionPadding(input.Cr, width, height);
        result.Cb = ApplyEdgeExtensionPadding(input.Cb, width, height);

        return result;
    }

    public int CalculatePaddedDimension(int input)
    {
        int paddedResult;

        if (input % 8 != 0)
            paddedResult = input + 8 - (input % 8);
        else
            paddedResult = input;

        return paddedResult;
    }

    static void PopulatePaddedInput(float[,] input, int width, int height, int paddedWidth, int paddedHeight, float[,] paddedInput)
    {
        for (int i = 0; i < paddedHeight; i++)
        {
            for (int j = 0; j < paddedWidth; j++)
            {
                int w = j;
                int h = i;

                if (i >= height)
                    h = height - 1;

                if (j >= width)
                    w = width - 1;

                paddedInput[i, j] = input[h, w];
            }
        }
    }

    float[,] ApplyEdgeExtensionPadding(float[,] input, int width, int height)
    {
        int paddedWidth = CalculatePaddedDimension(width);
        int paddedHeight = CalculatePaddedDimension(height);
        float[,]? paddedInput = new float[paddedHeight, paddedWidth];

        PopulatePaddedInput(input, width, height, paddedWidth, paddedHeight, paddedInput);

        return paddedInput;
    }
}
