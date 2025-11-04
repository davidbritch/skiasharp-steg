using ImageCompression.JPEG.Models;

namespace ImageCompression.JPEG.Services;

public class RunLengthEncodingService : IRunLengthEncodingService
{
    public List<Tuple<int, int>> Encode(Block8x8 block)
    {
        var result = new List<Tuple<int, int>>();
        int zeroCount = 0;

        for (int i = 1; i < DCTOrder.Order.Length; i++)
        {
            int currentValue = (int)block[DCTOrder.Order[i]];

            if (currentValue == 0)
                zeroCount++;
            else
            {
                result.Add(new Tuple<int, int>(zeroCount, currentValue));
                zeroCount = 0;
            }

            while (zeroCount > 15)
            {
                result.Add(new Tuple<int, int>(15, 0));
                zeroCount -= 16;
            }
        }

        result.Add(new Tuple<int, int>(0, 0));

        return result;
    }

    public Block8x8 Decode(List<Tuple<int, int>> pairs)
    {
        var result = new Block8x8();
        int i = 1;

        foreach (var pair in pairs)
        {
            if (pair.Item1 == 0 && pair.Item2 == 0)
            {
#pragma warning disable 1717
                for (i = i; i < 64; i++)
                    result[DCTOrder.Order[i]] = 0;
#pragma warning restore 1717
            }
            else
            {
                for (int c = 0; c < pair.Item1; c++)
                {
                    result[DCTOrder.Order[i]] = 0;
                    i++;
                }

                result[DCTOrder.Order[i]] = pair.Item2;
                i++;
            }
        }

        return result;
    }

    static void AddEndOfBlockMarker(List<Tuple<int, int>> result)
    {
        int i = result.Count - 1;
        while (i >= 0 && result[i].Item2 == 0)
        {
            result.RemoveAt(i);
            i--;
        }

        result.Add(new Tuple<int, int>(0, 0));
    }
}