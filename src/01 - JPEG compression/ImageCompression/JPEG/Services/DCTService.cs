using ImageCompression.JPEG.Models;
using ImageCompression.JPEG.Extensions;
using System.Runtime.CompilerServices;

namespace ImageCompression.JPEG.Services;

public class DCTService : IDCTService
{
    readonly IPaddingService _paddingService;

    public DCTService(IPaddingService paddingService)
    {
        _paddingService = paddingService;
    }

    public DCTData CalculateDCT(ColourData input, int width, int height)
    {
        if (input == null)
            throw new ArgumentNullException(nameof(input), nameof(input).ToArgumentNullExceptionMessage());

        if (width <= 0)
            throw new ArgumentNullException(nameof(width), nameof(width).ToArgumentEqualsZeroExceptionMessage());

        if (height <= 0)
            throw new ArgumentNullException(nameof(height), nameof(height).ToArgumentEqualsZeroExceptionMessage());

        // Pad input to fit 8x8 block
        var paddedInput = _paddingService.ApplyPadding(input, width, height);
        var paddedWidth = _paddingService.CalculatePaddedDimension(width);
        var paddedHeight = _paddingService.CalculatePaddedDimension(height);

        // Split YCbCr data into blocks
        var result = CreateMCUs(paddedInput, paddedWidth, paddedHeight);

        // Perform DCT
        result = ApplyDCT(result, paddedWidth, paddedHeight);

        return result;
    }

    public DCTData QuantizeDCT(DCTData input, byte[]? chrominanceTable, byte[]? luminanceTable)
    {
        if (input == null)
            throw new ArgumentNullException(nameof(input), nameof(input).ToArgumentNullExceptionMessage());

        if (chrominanceTable == null)
            chrominanceTable = QuantisationTables.Chrominance;

        if (luminanceTable == null)
            luminanceTable = QuantisationTables.Luminance;

        var result = new DCTData();

        result.Y = QuantizeDCTComponent(input.Y, luminanceTable);
        result.Cr = QuantizeDCTComponent(input.Cr, chrominanceTable);
        result.Cb = QuantizeDCTComponent(input.Cb, chrominanceTable);

        return result;
    }

    DCTData ApplyDCT(DCTData dctData, int width, int height)
    {
        dctData.Y = ApplyDCTToColourComponent(dctData.Y, width, height);
        dctData.Cr = ApplyDCTToColourComponent(dctData.Cr, width, height);
        dctData.Cb = ApplyDCTToColourComponent(dctData.Cb, width, height);

        return dctData;
    }

    Block8x8[] ApplyDCTToColourComponent(Block8x8[] input, int width, int height)
    {
        Block8x8[] result = new Block8x8[width * height / 64];

        Unsafe.SkipInit(out Block8x8 inputFBuffer);
        Unsafe.SkipInit(out Block8x8 outputFBuffer);
        Unsafe.SkipInit(out Block8x8 tempFBuffer);

        for (int i = 0; i < input.Length; i++)
        {
            inputFBuffer = input[i];
            FastFloatingPointDCT.TransformFDCT(ref inputFBuffer, ref outputFBuffer, ref tempFBuffer);
            result[i] = outputFBuffer;
        }

        return result;
    }

    DCTData CreateMCUs(ColourData padddedInput, int width, int height)
    {
        var result = new DCTData();

        result.Y = CreateMCUsForColourComponent(padddedInput.Y, width, height);
        result.Cr = CreateMCUsForColourComponent(padddedInput.Cr, width, height);
        result.Cb = CreateMCUsForColourComponent(padddedInput.Cb, width, height);

        return result;
    }

    Block8x8[] CreateMCUsForColourComponent(float[,] input, int width, int height)
    {
        Block8x8[] res = new Block8x8[width * height / 64];

        int counter = 0;
        for (int i = 0; i < height; i = i + 8)
        {
            for (int j = 0; j < width; j = j + 8)
            {
                var mcu = CreateMinimumCodedUnit(input, j, i);
                res[counter++] = mcu;
            }
        }

        return res;
    }

    Block8x8 CreateMinimumCodedUnit(float[,] input, int sWidth, int sHeight)
    {
        var result = new Block8x8();

        for (int i = 0; i < 8; i++)
        {
            for (int j = 0; j < 8; j++)
                result[j, i] = input[sHeight + i, sWidth + j];
        }

        return result;
    }

    Block8x8[] QuantizeDCTComponent(Block8x8[] input, byte[] quantizationTable)
    {
        var result = new Block8x8[input.Length];

        for (int iBlock = 0; iBlock < input.Length; iBlock++)
            QuantizeDCTBlock(input, quantizationTable, result, iBlock);

        return result;
    }

    static void QuantizeDCTBlock(Block8x8[] input, byte[] quantizationTable, Block8x8[] result, int iBlock)
    {
        Block8x8 tmp = input[iBlock];

        for (int iElement = 0; iElement < 64; iElement++)
            tmp[iElement] = (float)Math.Round(tmp[iElement] / quantizationTable[iElement]);

        result[iBlock] = tmp;
    }
}