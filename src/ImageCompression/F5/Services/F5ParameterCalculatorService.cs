using ImageCompression.JPEG.Models;
using ImageCompression.JPEG.Extensions;
using ImageCompression.F5.Extensions;
using ImageCompression.F5.Exceptions;

namespace ImageCompression.F5.Services;

internal class F5ParameterCalculatorService : IF5ParameterCalculatorService
{
    readonly IMCUConverterService _mcuConverterService;

    public F5ParameterCalculatorService(IMCUConverterService mcuConverterService)
    {
        _mcuConverterService = mcuConverterService;
    }

    public int CalculateN(int k)
    {
        int n = (int)Math.Pow(2, k) - 1;
        return n;
    }

    public int CalculateK(Block8x8[] mcus, string message)
    {
        if (mcus == null || mcus.Length <= 0)
            throw new ArgumentNullException(nameof(mcus), nameof(mcus).ToArgumentNullExceptionMessage());

        double messageBitLength = message.GetBitLength();
        int reservedBitsForMsgLen = 32;
        var coefficients = _mcuConverterService.MCUArrayToCoeffArray(mcus);
        var dcCoeffCount = coefficients.Length / 64;
        double availableCoefficientCount = coefficients.Where(item => item != 0 && item != 1 && item != -1).Count() - dcCoeffCount - reservedBitsForMsgLen;

        if (availableCoefficientCount < messageBitLength)
            throw new CapacityException($"Not enough capacity for the message. ({availableCoefficientCount}/{messageBitLength}) ", availableCoefficientCount, messageBitLength);

        double calculatedEmbeddingRate = messageBitLength / availableCoefficientCount;
        double[]? embeddingRates = EmbeddingRateTable.Table.Select(item => item.EmbeddingRate).ToArray();
        double optimalEmbeddingRate = FindClosestValue(embeddingRates, calculatedEmbeddingRate);

        int k = EmbeddingRateTable.Table.Where(item => item.EmbeddingRate == optimalEmbeddingRate).Select(item => item.K).FirstOrDefault();
        return k;
    }

    double FindClosestValue(double[] inputArray, double inputValue)
    {
        if (inputArray == null || inputArray.Length == 0)
            throw new ArgumentException("Input array cannot be null or empty.");

        double[]? orderedInput = inputArray.OrderBy(item => item).ToArray();

        int i = 0;
        double result = 0;
        while (i < inputArray.Length)
        {
            if (orderedInput[i] > inputValue)
            {
                result = orderedInput[i];
                break;
            }
            i++;
        }

        return result;
    }
}