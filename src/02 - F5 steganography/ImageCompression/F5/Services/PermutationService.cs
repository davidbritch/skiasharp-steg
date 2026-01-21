using ImageCompression.JPEG.Models;
using ImageCompression.JPEG.Extensions;
using System.Numerics;
using System.Text;

namespace ImageCompression.F5.Services;

internal class PermutationService : IPermutationService
{
    public Block8x8[] PermutateArray(string password, Block8x8[] inputArray, bool reverse)
    {
        if (inputArray == null || inputArray.Length <= 0)
            throw new ArgumentNullException(nameof(inputArray), nameof(inputArray).ToArgumentNullExceptionMessage());

        if (string.IsNullOrEmpty(password))
            throw new ArgumentNullException(nameof(password), nameof(password).ToArgumentNullExceptionMessage());

        int mcuCount = inputArray.Length - 1;
        Block8x8[] permutedArray = (Block8x8[])inputArray.Clone();

        var permutationSequence = GetPermutationSequence(password, mcuCount);

        if (reverse)
            for (int i = mcuCount - 1; i >= 0; i--)
                SwapElements(permutedArray, permutationSequence, i);
        else
            for (int i = 0; i < mcuCount; i++)
                SwapElements(permutedArray, permutationSequence, i);

        return permutedArray;
    }

    void SwapElements(Block8x8[] permutedArray, Dictionary<int, int> permutationSequence, int i)
    {
        int randomIndex = permutationSequence[i];
        Block8x8 temp = permutedArray[i];
        permutedArray[i] = permutedArray[randomIndex];
        permutedArray[randomIndex] = temp;
    }

    Dictionary<int, int> GetPermutationSequence(string password, int mcuCount)
    {
        var permutationSequence = new Dictionary<int, int>();
        int[]? rngIndexArray = GenerateDeterministicNumberSequence(password, mcuCount);
        for (int i = 0; i < mcuCount; i++)
        {
            var randomIndex = rngIndexArray[i] % (mcuCount - 1);
            permutationSequence.Add(i, randomIndex);
        }

        return permutationSequence;
    }

    static int[] GenerateDeterministicNumberSequence(string seed, int length)
    {
        BigInteger bigSeed = new BigInteger(Encoding.UTF8.GetBytes(seed));
        int[] sequence = new int[length];

        for (int i = 0; i < length; i++)
        {
            bigSeed = BigInteger.Multiply(bigSeed, 397); // Use any prime number for multiplication
            bigSeed = BigInteger.ModPow(bigSeed, 1, int.MaxValue);
            sequence[i] = (int)bigSeed;
        }

        return sequence;
    }
}