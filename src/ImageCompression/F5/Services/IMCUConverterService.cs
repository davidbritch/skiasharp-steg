using ImageCompression.JPEG.Models;

namespace ImageCompression.F5.Services;

public interface IMCUConverterService
{
    Block8x8[] DCTDataToMCUArray(DCTData dct);
    float[] MCUArrayToCoeffArray(Block8x8[] dctArray);
    Block8x8[] CoeffArrayMCUArray(float[] coeffArray);
    DCTData MCUArrayToDCTData(Block8x8[] mcuArray);
}