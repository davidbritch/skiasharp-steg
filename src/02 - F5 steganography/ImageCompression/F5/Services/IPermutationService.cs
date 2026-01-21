using ImageCompression.JPEG.Models;

namespace ImageCompression.F5.Services;

public interface IPermutationService
{
    Block8x8[] PermutateArray(string password, Block8x8[] inputArray, bool reverse);
}