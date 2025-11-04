using ImageCompression.JPEG.Extensions;
using ImageCompression.JPEG.Models;
using ImageCompression.F5.Exceptions;
using System.Text;

namespace ImageCompression.F5.Services;

public class F5ExtractingService : IF5ExtractingService
{        
    readonly IPermutationService _permutationService;
    readonly IMCUConverterService _mcuConverterService;
    readonly IF5ParameterCalculatorService _f5ParameterCalulatorService;

    public F5ExtractingService(IPermutationService permutationService,
        IMCUConverterService mcuConverterSservice,
        IF5ParameterCalculatorService f5ParameterCalulatorService)
    {
        _permutationService = permutationService;
        _mcuConverterService = mcuConverterSservice;
        _f5ParameterCalulatorService = f5ParameterCalulatorService;
    }

    public string Extract(DCTData dctData, string password)
    {
        if (dctData == null)
            throw new ArgumentNullException(nameof(dctData), nameof(dctData).ToArgumentNullExceptionMessage());

        if (string.IsNullOrEmpty(password))
            throw new ArgumentNullException(nameof(password), nameof(password).ToArgumentNullExceptionMessage());

        // Convert dctData object to MCU array
        var mcuArray = _mcuConverterService.DCTDataToMCUArray(dctData);

        // Permutate MCU array
        var permutatedMCUArray = _permutationService.PermutateArray(password, mcuArray, false);

        // Convert permutated MCU array to coeff array
        var coeffs = _mcuConverterService.MCUArrayToCoeffArray(permutatedMCUArray);

        // Read decoding info (k and msgLen) and calculate n
        int k, msgLen;
        int currentIndex = ReadDecodingInfo(coeffs, out k, out msgLen);

        if (k > 9)
            throw new MatrixEncodingException("Error while reading parameter k from the image.");

        int n = _f5ParameterCalulatorService.CalculateN(k);

        // Read embedded message 
        string result = ReadEmbeddedMessage(coeffs, k, n, msgLen, currentIndex);

        return result;
    }

    int ReadDecodingInfo(float[] coeffs, out int k, out int msgLen)
    {
        int index = 0;
        int bitsRead = 0;
        int decodeDataBitLength = 32;
        int readValue = 0;

        while (bitsRead < decodeDataBitLength)
        {
            int coeff = (int)coeffs[index];

            if (coeff != 0 && ((index % 64) != 0))
            {
                var bit = ReadDecodingInfoBitFromCoeff(coeff);
                readValue = readValue << 1;
                readValue = readValue | bit;
                bitsRead++;
            }
            index++;
        }

        ExtractDecodedData(readValue, out k, out msgLen);

        return index;
    }

    int ReadDecodingInfoBitFromCoeff(int coeff)
    {
        if ((coeff < 0 && (coeff % 2 == 0)) || (coeff > 0 && (coeff % 2 == 1)))
            return 1;
        else
            return 0;
    }

    void ExtractDecodedData(int input, out int upperByte, out int lowerInt)
    {
        uint value = (UInt32)(input);
        upperByte = (byte)(value >> 24);
        lowerInt = (int)(value & 0xFFFFFF);
    }

    string ReadEmbeddedMessage(float[] coeffs, int k, int n, int msgLen, int lastReadIndex)
    {
        int counter = 0;
        int coeffCount = 0;
        int index = lastReadIndex + 1;

        byte[]? messageBytes = new byte[msgLen / 8];
        int messageByteIndex = 0;
        int messageBitIndex = 0;

        while (counter < msgLen)
        {
            int[]? coeffsToRead = GetCoefficients(coeffs, n, ref coeffCount, ref index);
            int hash = CalculateHash(n, coeffsToRead);
            ExtractMessageBits(k, messageBytes, ref messageByteIndex, ref messageBitIndex, hash);

            counter = counter + k;
            coeffCount = 0;
        }

        string result = Encoding.UTF8.GetString(messageBytes);
        return result;
    }

    void ExtractMessageBits(int k, byte[] messageBytes, ref int messageByteIndex, ref int messageBitIndex, int hash)
    {
        if (messageBitIndex == 8)
        {
            messageBitIndex = 0;
            messageByteIndex++;
        }

        int start = GetHashStartIndex(k, messageBytes, messageByteIndex, messageBitIndex);

        for (int i = start; i >= 0; i--)
        {
            int bitToExtract = (hash >> i) & 1;
            if (messageBitIndex == 8)
            {
                messageBitIndex = 0;
                messageByteIndex++;
            }
            messageBytes[messageByteIndex] = (byte)(messageBytes[messageByteIndex] << 1);
            messageBytes[messageByteIndex] = (byte)(messageBytes[messageByteIndex] | (byte)bitToExtract);
            messageBitIndex++;
        }
    }

    int GetHashStartIndex(int k, byte[] messageBytes, int messageByteIndex, int messageBitIndex)
    {
        bool isLastByte = ((messageBitIndex + k) > 7) && (messageByteIndex == (messageBytes.Count() - 1));
        int limit = k - 1;

        if (isLastByte)
            limit = 8 - (messageBitIndex + 1);

        return limit;
    }

    int CalculateHash(int n, int[] coeffsToRead)
    {
        int hash = 0;
        for (int i = 0; i < n; i++)
        {
            int coeffToRead = coeffsToRead[i];
            int coeffLsb = coeffToRead > 0 ? coeffToRead & 1 : (1 - (coeffToRead & 1));

            if (coeffLsb == 1)
                hash ^= i + 1;
        }

        return hash;
    }

    int[] GetCoefficients(float[] coeffs, int n, ref int coeffCount, ref int index)
    {
        int[] coeffsToRead = new int[n];

        while (coeffCount < n)
        {
            int coeff = (int)coeffs[index];
            if (coeff != 0 && ((index % 64) != 0))
            {
                coeffsToRead[coeffCount] = coeff;
                coeffCount++;
            }
            index++;
        }

        return coeffsToRead;
    }
}