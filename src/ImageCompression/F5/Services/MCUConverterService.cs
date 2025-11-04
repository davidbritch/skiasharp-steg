using ImageCompression.JPEG.Models;
using ImageCompression.JPEG.Extensions;

namespace ImageCompression.F5.Services;

public class MCUConverterService : IMCUConverterService
{
    public Block8x8[] DCTDataToMCUArray(DCTData dct)
    {
        if (dct == null || dct.Y.Length <= 0)
            throw new ArgumentNullException(nameof(dct), nameof(dct).ToArgumentNullExceptionMessage());

        int numberOfComponents = 3;
        int mcuPerComponentCount = dct.Y.Length;
        Block8x8[] result = new Block8x8[mcuPerComponentCount * numberOfComponents];
        int index = 0;

        for (int i = 0; i < mcuPerComponentCount; i++)
        {
            result[index++] = dct.Y[i];
            result[index++] = dct.Cb[i];
            result[index++] = dct.Cr[i];
        }

        return result;
    }

    public float[] MCUArrayToCoeffArray(Block8x8[] dctArray)
    {
        if (dctArray == null || dctArray.Length <= 0)
            throw new ArgumentNullException(nameof(dctArray), nameof(dctArray).ToArgumentNullExceptionMessage());

        int coeffPerMcu = 64;
        int totalMcuCount = dctArray.Length;
        float[] result = new float[totalMcuCount * coeffPerMcu];
        int index = 0;

        for (int i = 0; i < totalMcuCount; i++)
        {
            var mcu = dctArray[i];
            for (int j = 0; j < coeffPerMcu; j++)
                result[index++] = mcu[j];
        }

        return result;
    }

    public DCTData MCUArrayToDCTData(Block8x8[] mcuArray)
    {
        if (mcuArray == null || mcuArray.Length <= 0)
            throw new ArgumentNullException(nameof(mcuArray), nameof(mcuArray).ToArgumentNullExceptionMessage());

        int numberOfComponents = 3;
        int mcuPerComponentCount = mcuArray.Length / numberOfComponents;
        DCTData result = new DCTData(mcuPerComponentCount);
        int index = 0;

        for (int i = 0; i < mcuPerComponentCount; i++)
        {
            result.Y[i] = mcuArray[index++];
            result.Cb[i] = mcuArray[index++];
            result.Cr[i] = mcuArray[index++];
        }

        return result;
    }

    public Block8x8[] CoeffArrayMCUArray(float[] coeffArray)
    {
        int coeffPerMcu = 64;
        int totalMcuCount = coeffArray.Length / coeffPerMcu;
        Block8x8[] result = new Block8x8[totalMcuCount];
        int index = 0;

        for (int i = 0; i < totalMcuCount; i++)
        {
            var mcu = new Block8x8();
            for (int j = 0; j < coeffPerMcu; j++)
                mcu[j] = coeffArray[index++];

            result[i] = mcu;
        }

        return result;
    }
}