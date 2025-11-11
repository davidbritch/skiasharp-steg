using ImageCompression.JPEG.Models;

namespace ImageCompression.JPEG.Services;

public class HuffmanDecodingService : IHuffmanDecodingService
{
    readonly IRunLengthEncodingService _runLengthEncodingService;
    readonly IBitReaderService _bitReaderService;

    Dictionary<Tuple<int, int>, int>? _dcCrominanceDiffDict;
    Dictionary<Tuple<int, int>, int>? _dcLuminanceDiffDict;
    Dictionary<Tuple<int, int>, Tuple<int, int>>? _acCrominanceCoeffDict;
    Dictionary<Tuple<int, int>, Tuple<int, int>>? _acLuminanceCoeffDict;

    public HuffmanDecodingService(IRunLengthEncodingService runLengthEncodingService, IBitReaderService bitReaderService)
    {
        _runLengthEncodingService = runLengthEncodingService;
        _bitReaderService = bitReaderService;
        InitHuffmanTables();
    }

    public int DecodeChrominanceDC(int prevDC, BinaryReader br)
    {
        return DecodeDC(prevDC, br, _dcCrominanceDiffDict!);
    }

    public int DecodeLuminanceDC(int prevDC, BinaryReader br)
    {
        return DecodeDC(prevDC, br, _dcLuminanceDiffDict!);
    }

    public Block8x8 DecodeChrominanceAC(BinaryReader br)
    {
        return DecodeAC(br, _acCrominanceCoeffDict!);
    }

    public Block8x8 DecodeLuminanceAC(BinaryReader br)
    {
        return DecodeAC(br, _acLuminanceCoeffDict!);
    }

    void InitHuffmanTables()
    {
        var dcLuminanceDiffTable = new Tuple<int, int>[12];
        var dcCrominanceDiffTable = new Tuple<int, int>[12];
        var acLuminanceCoeffTable = new Tuple<int, int>[255];
        var acCrominanceCoeffTable = new Tuple<int, int>[255];

        ExtractTable(HuffmanEncoding.DCLuminanceBits, HuffmanEncoding.DCLuminanceValues, dcLuminanceDiffTable);
        ExtractTable(HuffmanEncoding.DCChrominanceBits, HuffmanEncoding.DCChrominanceValues, dcCrominanceDiffTable);
        ExtractTable(HuffmanEncoding.ACLuminanceBits, HuffmanEncoding.ACLuminanceValues, acLuminanceCoeffTable);
        ExtractTable(HuffmanEncoding.ACChrominanceBits, HuffmanEncoding.ACChrominanceValues, acCrominanceCoeffTable);

        var acCrominanceCoeffList = acCrominanceCoeffTable.Where(item => item != null).ToList();
        var acLuminanceCoeffList = acLuminanceCoeffTable.Where(item => item != null).ToList();

        var dcCrominanceDiffList = dcCrominanceDiffTable.ToList();
        var dcLuminanceDiffList = dcLuminanceDiffTable.ToList();

        _dcCrominanceDiffDict = dcCrominanceDiffList.ToDictionary(key => key, item => dcCrominanceDiffList.IndexOf(item));
        _dcLuminanceDiffDict = dcLuminanceDiffList.ToDictionary(key => key, item => dcLuminanceDiffList.IndexOf(item));

        _acLuminanceCoeffDict = acLuminanceCoeffList.ToDictionary(key => key, item => CalculateCoefDictValue(item, acLuminanceCoeffList));
        _acCrominanceCoeffDict = acCrominanceCoeffList.ToDictionary(key => key, item => CalculateCoefDictValue(item, acCrominanceCoeffList));
    }

    static Tuple<int, int> CalculateCoefDictValue(Tuple<int, int> item, List<Tuple<int, int>> acLuminanceCoeffList)
    {
        int index = acLuminanceCoeffList.IndexOf(item);
        if (index == 0)
            return new Tuple<int, int>(0, 0);

        index = index - 1;

        int item1 = 0;
        int item2 = 0;
        item1 = index / 10;

        if (item1 == 15)
        {
            if (item2 == 10)
                return new Tuple<int, int>(item1, 0);
            else
                item2 = (index % 10);
        }
        else
            item2 = 1 + (index % 10);

        if (item1 == 16)
            return new Tuple<int, int>(15, 10);

        return new Tuple<int, int>(item1, item2);
    }

    void ExtractTable(int[] bits, int[] val, Tuple<int, int>[] table)
    {
        int i, j, v;
        int p = 0, code = 0;

        for (j = 1; j < bits.Length; j++)
        {
            for (i = 0; i < bits[j]; i++)
            {
                v = val[p];
                table[v] = new Tuple<int, int>(code++, j);
                p++;
            }
            code <<= 1;
        }
    }

    int CalculateDC(int prevDC, int diffValue)
    {
        int dc = diffValue + prevDC;
        return dc;
    }

    int ReadDiffValue(BinaryReader br, int diffValueCodeLength, ref bool isNegativeNumber)
    {
        int diffValue = _bitReaderService.Read(br, true);
        if (diffValue == 0)
            isNegativeNumber = true;

        for (int i = 0; i < diffValueCodeLength - 1; i++)
            diffValue = _bitReaderService.Read(br, false);

        return diffValue;
    }

    int DecodeDC(int prevDC, BinaryReader br, Dictionary<Tuple<int, int>, int> diffDict)
    {
        int dc = 0;
        int bitLength = 0;

        bitLength++;
        int diffCategoryCode = _bitReaderService.Read(br, true);
        var key = new Tuple<int, int>(diffCategoryCode, bitLength);

        while (true)
        {
            if (diffDict.TryGetValue(key, out int diffValueCodeLength))
            {
                int diffValue = 0;
                bool isNegativeNumber = false;

                if (diffValueCodeLength != 0)
                    diffValue = ReadDiffValue(br, diffValueCodeLength, ref isNegativeNumber);

                diffValue = ToTwosComplement(diffValue, diffValueCodeLength, isNegativeNumber ? 0 : 1);
                dc = CalculateDC(prevDC, diffValue);
                break;
            }
            else
            {
                diffCategoryCode = _bitReaderService.Read(br, false);
                bitLength++;
                key = new Tuple<int, int>(diffCategoryCode, bitLength);
            }
        }

        return dc;
    }

    int ToTwosComplement(int value, int numberOfBits, int firstBit)
    {
        if (firstBit == 1)
            return value;

        value = value | (1 << numberOfBits);

        int invertedValue = (~value) & ((1 << numberOfBits) - 1);
        return -(invertedValue);

    }

    Block8x8 DecodeAC(BinaryReader br, Dictionary<Tuple<int, int>, Tuple<int, int>> coeffDict)
    {
        var pairs = ReadRunLengthPairs(br, coeffDict);
        var result = _runLengthEncodingService.Decode(pairs);
        return result;
    }

    List<Tuple<int, int>> ReadRunLengthPairs(BinaryReader br, Dictionary<Tuple<int, int>, Tuple<int, int>> coefDict)
    {
        var runLengthValuePairs = new List<Tuple<int, int>>();
        int coeffCount = 1;

        int bitLength = 1;
        int coefCategoryCode = _bitReaderService.Read(br, true);
        var key = new Tuple<int, int>(coefCategoryCode, bitLength);

        while (coeffCount < 64)
        {
            if (coefDict.TryGetValue(key, out Tuple<int, int>? runLengthCategoryPair))
            {
                if ((runLengthCategoryPair.Item1 != 0 && runLengthCategoryPair.Item2 != 0) ||
                    (runLengthCategoryPair.Item1 != 15 && runLengthCategoryPair.Item2 != 0))
                {
                    // Read coeff value after reading zeroLengthCategory pair
                    var coeffValue = ReadCoeffValueFromCategory(br, runLengthCategoryPair);
                    runLengthValuePairs.Add(new Tuple<int, int>(runLengthCategoryPair.Item1, coeffValue));
                }
                else
                {
                    // No coeff value to read
                    runLengthValuePairs.Add(runLengthCategoryPair);
                    if (runLengthCategoryPair.Item1 == 0)
                        break;
                }

                coeffCount = coeffCount + 1 + runLengthCategoryPair.Item1;

                if (coeffCount > 63)
                    break;

                coefCategoryCode = _bitReaderService.Read(br, true);
                bitLength = 1;
                key = new Tuple<int, int>(coefCategoryCode, bitLength);
            }
            else
            {
                coefCategoryCode = _bitReaderService.Read(br, false);
                bitLength++;
                key = new Tuple<int, int>(coefCategoryCode, bitLength);
            }
        }

        return runLengthValuePairs;
    }

    int ReadCoeffValueFromCategory(BinaryReader br, Tuple<int, int> runLengthCategoryPair)
    {
        int value = _bitReaderService.Read(br, true);
        int firstBit = value;

        for (int i = 0; i < runLengthCategoryPair.Item2 - 1; i++)
            value = _bitReaderService.Read(br, false);

        value = ToTwosComplement(value, runLengthCategoryPair.Item2, firstBit);
        return value;
    }
}