using ImageCompression.JPEG.Models;
using ImageCompression.JPEG.Extensions;
using ImageCompression.F5.Extensions;
using System.Collections;

namespace ImageCompression.F5.Services;

public class F5EmbeddingService : IF5EmbeddingService
{
    readonly IMCUConverterService _mcuConverterService;
    readonly IPermutationService _permutationService;
    readonly IF5ParameterCalculatorService _f5ParameterCalculatorService;

    public F5EmbeddingService(IMCUConverterService mcuConverterService,
        IPermutationService permutationService,
        IF5ParameterCalculatorService f5ParameterCalculatorService)
    {
        _mcuConverterService = mcuConverterService;
        _permutationService = permutationService;
        _f5ParameterCalculatorService = f5ParameterCalculatorService;
    }

    public DCTData Embed(DCTData quantizedData, string password, string message)
    {
        if (quantizedData == null || quantizedData.Y.Length <= 0)
            throw new ArgumentNullException(nameof(quantizedData), nameof(quantizedData).ToArgumentNullExceptionMessage());

        if (string.IsNullOrEmpty(password))
            throw new ArgumentNullException(nameof(password), nameof(password).ToArgumentNullExceptionMessage());

        if (string.IsNullOrEmpty(message))
            return quantizedData;

        // Convert DCT data object to array of MCUs
        var mcuArray = _mcuConverterService.DCTDataToMCUArray(quantizedData);

        // Permute the order of MCUs in the mcuArray
        var permutatedMcuArray = _permutationService.PermutateArray(password, mcuArray, false);

        // Calculate n and k
        int k = _f5ParameterCalculatorService.CalculateK(permutatedMcuArray, message);
        int n = _f5ParameterCalculatorService.CalculateN(k);

        // Convert permutated MCU array into coefficient array
        var coeffs = _mcuConverterService.MCUArrayToCoeffArray(permutatedMcuArray);

        // Save k and msgLen in first 4 bytes. (k = 1 byte, msgLen = 3 bytes)
        int lastModifiedIndex = EmbedDecodingInfo(coeffs, k, message);

        // Embed data
        var embededCoeffData = EmbedMessage(coeffs, message, k, n, lastModifiedIndex);

        // Convert back coefficient array to MCU array
        var permutatedEmbededMcuData = _mcuConverterService.CoeffArrayMCUArray(embededCoeffData);

        // Reverse permutation to get the original order of MCUs
        var embededMCUData = _permutationService.PermutateArray(password, permutatedEmbededMcuData, true);

        var result = _mcuConverterService.MCUArrayToDCTData(embededMCUData);

        return result;
    }

    float[] EmbedMessage(float[] coeffs, string message, int k, int n, int lastModifiedIndex)
    {
        byte[] messageBytes = System.Text.Encoding.UTF8.GetBytes(message);
        int messageBitLength = message.GetBitLength();
        int index = lastModifiedIndex + 1;
        int counter = 0;
        Dictionary<int, Tuple<int, int>> coeffsToEmbed = new Dictionary<int, Tuple<int, int>>();
        int byteToEmbed = 0;
        int bitsToEmbed = 0;
        int availableBitsForEmbedding = 0;
        int messageByteIndex = 0;
        int coeffCount = 0;
        bool shrinkageOccured = false;

        while (counter < messageBitLength)
        {
            coeffsToEmbed = GetCoefficients(coeffs, coeffsToEmbed, n, ref index, ref coeffCount);
            int hash = CalculateHash(n, coeffsToEmbed);
            bitsToEmbed = GetBitsToEmbed(k, shrinkageOccured, messageBytes, bitsToEmbed, ref byteToEmbed, ref availableBitsForEmbedding, ref messageByteIndex);

            int coeffToChange = hash ^ bitsToEmbed;
            if (coeffToChange == 0)
                // No need to change coefficients
                counter = ResetCounterAndCoefficients(k, counter, out coeffsToEmbed, out coeffCount, out shrinkageOccured);
            else
            {
                // Cange coefficient value
                coeffToChange -= 1;
                int coeffIndexToChange = coeffsToEmbed[coeffToChange].Item1;
                ModifyCoefficient(coeffs, coeffsToEmbed, coeffToChange, coeffIndexToChange);

                // Check for shrinkage
                if (coeffs[coeffIndexToChange] == 0)
                {
                    coeffCount--;
                    shrinkageOccured = true;
                    ReorderCoeffsToEmbed(n, coeffsToEmbed, coeffToChange);
                }
                else
                    counter = ResetCounterAndCoefficients(k, counter, out coeffsToEmbed, out coeffCount, out shrinkageOccured);
            }
        }

        return coeffs;
    }

    int ResetCounterAndCoefficients(int k, int counter, out Dictionary<int, Tuple<int, int>> coeffsToEmbed, out int coeffCount, out bool shrinkageOccured)
    {
        counter = counter + k;
        coeffsToEmbed = new Dictionary<int, Tuple<int, int>>();
        coeffCount = 0;
        shrinkageOccured = false;
        return counter;
    }

    void ReorderCoeffsToEmbed(int n, Dictionary<int, Tuple<int, int>> coeffsToEmbed, int coeffToChange)
    {
        int c = coeffToChange;
        while (c < n - 1)
        {
            coeffsToEmbed.Remove(c);
            coeffsToEmbed.Add(c, coeffsToEmbed[c + 1]);
            c++;
        }
        coeffsToEmbed.Remove(n - 1);
    }

    void ModifyCoefficient(float[] coeffs, Dictionary<int, Tuple<int, int>> coeffsToEmbed, int coeffToChange, int coeffIndexToChange)
    {
        int coeffValueToChange = coeffsToEmbed[coeffToChange].Item2;
        if (coeffValueToChange < 0)
            coeffs[coeffIndexToChange]++;

        if (coeffValueToChange > 0)
            coeffs[coeffIndexToChange]--;
    }

    int GetBitsToEmbed(int k, bool shrinkageOccured, byte[] messageBytes, int bitsToEmbed, ref int byteToEmbed, ref int availableBitsForEmbedding, ref int messageByteIndex)
    {
        if (!shrinkageOccured)
        {
            // If no shrinkage get new bits to embed
            bitsToEmbed = 0;
            for (int i = 0; i < k; i++)
            {
                if (availableBitsForEmbedding == 0)
                {
                    if (messageByteIndex == messageBytes.Length)
                        break;

                    byteToEmbed = messageBytes[messageByteIndex];
                    availableBitsForEmbedding = 8;
                    messageByteIndex++;
                }

                int nextBitToEmbed = (byteToEmbed >> (availableBitsForEmbedding - 1)) & 1;
                availableBitsForEmbedding--;
                bitsToEmbed = bitsToEmbed << 1;
                bitsToEmbed |= nextBitToEmbed;
            }
        }

        return bitsToEmbed;
    }

    int CalculateHash(int n, Dictionary<int, Tuple<int, int>> coeffsToEmbed)
    {
        int hash = 0;
        for (int i = 0; i < n; i++)
        {
            int coeffToEmbed = coeffsToEmbed[i].Item2;
            int coeffLsb = coeffToEmbed > 0 ? coeffToEmbed & 1 : (1 - (coeffToEmbed & 1));

            if (coeffLsb == 1)
                hash ^= i + 1;
        }

        return hash;
    }

    Dictionary<int, Tuple<int, int>> GetCoefficients(float[] coeffs, Dictionary<int, Tuple<int, int>> coeffsToEmbed, int n, ref int index, ref int coeffCount)
    {
        int currentCoeff;
        while (coeffCount < n)
        {
            currentCoeff = (int)coeffs[index];
            if (currentCoeff != 0 && ((index % 64) != 0))
            {
                AddCoefficientToArray(index, coeffsToEmbed, coeffCount, currentCoeff);
                coeffCount++;
            }
            index++;
        }

        return coeffsToEmbed;
    }

    void AddCoefficientToArray(int index, Dictionary<int, Tuple<int, int>> coeffsToEmbed, int coeffCount, int currentCoeff)
    {
        var coeffToEmbed = new Tuple<int, int>(index, currentCoeff);
        coeffsToEmbed.Add(coeffCount, coeffToEmbed);
    }

    int EmbedDecodingInfo(float[] coeffs, int k, string message)
    {
        int messageBitLength = message.GetBitLength();
        int index = 0;
        int counter = 31;

        int decodingInfo = PrepareDecodingInfo(k, messageBitLength);

        byte[]? bytesToEmbed = BitConverter.GetBytes(decodingInfo);
        var bitsToEmbed = new BitArray(bytesToEmbed);

        while (counter >= 0)
        {
            int coeff = (int)coeffs[index];
            int bitToEmbed = Convert.ToInt32(bitsToEmbed[counter]);

            if (index % 64 != 0 && coeff != 0)
                counter = EmbedDecodingInfoBit(coeffs, index, counter, coeff, bitToEmbed);

            index++;
        }

        return index;
    }

    int EmbedDecodingInfoBit(float[] coeffs, int index, int counter, int coeff, int bitToEmbed)
    {
        if (coeff > 0 && (coeff & 1) != bitToEmbed)
            coeffs[index]--;
        else if (coeff < 0 && (coeff & 1) == bitToEmbed)
            coeffs[index]++;

        if (coeffs[index] != 0)
            counter--;

        return counter;
    }

    int PrepareDecodingInfo(int paramK, int msgLen)
    {
        byte k = (byte)paramK;
        int kShifted = ((int)k) << 24;
        int msgLenMasked = msgLen & 0x00FFFFFF;
        int result = kShifted | msgLenMasked;

        return result;
    }
}