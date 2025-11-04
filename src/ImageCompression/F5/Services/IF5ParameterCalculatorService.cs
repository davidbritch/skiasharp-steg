using ImageCompression.JPEG.Models;

namespace ImageCompression.F5.Services;

public interface IF5ParameterCalculatorService
{
    int CalculateK(Block8x8[] mcus, string message);
    int CalculateN(int k);
}