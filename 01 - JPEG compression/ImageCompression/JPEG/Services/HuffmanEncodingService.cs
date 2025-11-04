using ImageCompression.JPEG.Models;

namespace ImageCompression.JPEG.Services;

public class HuffmanEncodingService : IHuffmanEncodingService
{
    readonly IRunLengthEncodingService _runLengthEncodingService;

    Tuple<int, int>[]? _dcCrominanceDiffTable;
    Tuple<int, int>[]? _dcLuminanceDiffTable;
    Tuple<int, int>[]? _acCrominanceCoeffTable;
    Tuple<int, int>[]? _acLuminanceCoeffTable;
    int _bufferPutBits, _bufferPutBuffer;

    public HuffmanEncodingService(IRunLengthEncodingService runLengthEncodingService)
    {
        InitHuffmanTables();
        _runLengthEncodingService = runLengthEncodingService;
    }

    public void EncodeChrominanceAC(Block8x8 block, BinaryWriter bw)
    {
        EncodeAC(block, bw, _acCrominanceCoeffTable!);
    }

    public void EncodeLuminanceAC(Block8x8 block, BinaryWriter bw)
    {
        EncodeAC(block, bw, _acLuminanceCoeffTable!);
    }

    public void EncodeChrominanceDC(int dc, int prevDC, BinaryWriter bw)
    {
        EncodeDC(dc, prevDC, bw, _dcCrominanceDiffTable!);
    }

    public void EncodeLuminanceDC(int dc, int prevDC, BinaryWriter bw)
    {
        EncodeDC(dc, prevDC, bw, _dcLuminanceDiffTable!);
    }

    void InitHuffmanTables()
    {
        _dcLuminanceDiffTable = new Tuple<int, int>[12];
        _dcCrominanceDiffTable = new Tuple<int, int>[12];
        _acLuminanceCoeffTable = new Tuple<int, int>[255];
        _acCrominanceCoeffTable = new Tuple<int, int>[255];

        ExtractTable(HuffmanEncoding.DCLuminanceBits, HuffmanEncoding.DCLuminanceValues, _dcLuminanceDiffTable);
        ExtractTable(HuffmanEncoding.DCChrominanceBits, HuffmanEncoding.DCChrominanceValues, _dcCrominanceDiffTable);
        ExtractTable(HuffmanEncoding.ACLuminanceBits, HuffmanEncoding.ACLuminanceValues, _acLuminanceCoeffTable);
        ExtractTable(HuffmanEncoding.ACChrominanceBits, HuffmanEncoding.ACChrominanceValues, _acCrominanceCoeffTable);
    }

    static int CalculateValueCategory(int currentValue)
    {
        if (currentValue == 0)
            return 0;

        int currentValueBitLength = 1;
        int temp = currentValue;

        if (currentValue < 0)
            temp = -temp;

        while ((temp >>= 1) != 0)
            currentValueBitLength++;

        return currentValueBitLength;
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

    void WriteBits(BinaryWriter bw, int code, int size)
    {
        int putBuffer = code;
        int putBits = _bufferPutBits;
        putBuffer &= (1 << size) - 1;
        putBits += size;
        putBuffer <<= 24 - putBits;
        putBuffer |= _bufferPutBuffer;

        while (putBits >= 8)
        {
            int c = putBuffer >> 16 & 0xFF;
            WriteByte(bw, c);
            if (c == 0xFF)
                WriteByte(bw, 0);
            putBuffer <<= 8;
            putBits -= 8;
        }
        _bufferPutBuffer = putBuffer;
        _bufferPutBits = putBits;
    }

    void WriteByte(BinaryWriter bw, int b)
    {
        bw.Write((byte)b);
    }

    public void FlushBuffer(BinaryWriter bw)
    {
        int PutBuffer = _bufferPutBuffer;
        int PutBits = _bufferPutBits;

        while (PutBits >= 8)
        {
            int c = PutBuffer >> 16 & 0xFF;
            WriteByte(bw, c);
            if (c == 0xFF)
                WriteByte(bw, 0);
            PutBuffer <<= 8;
            PutBits -= 8;
        }
        if (PutBits > 0)
        {
            int c = PutBuffer >> 16 & 0xFF;
            WriteByte(bw, c);
        }
    }

    void EncodeACCoeff(BinaryWriter bw, Tuple<int, int> item, Tuple<int, int>[] coeffTable)
    {
        int currentAbsoluteValue = item.Item2;
        int currentValue = currentAbsoluteValue;
        if (currentAbsoluteValue < 00)
        {
            currentAbsoluteValue = -currentAbsoluteValue;
            currentValue--;
        }

        int currentValueBitLength = CalculateValueCategory(currentAbsoluteValue);
        WriteACCoeffCategory(bw, item, coeffTable, currentValueBitLength);
        WriteACCoeffValue(bw, item, currentValue, currentValueBitLength);
    }

    void WriteACCoeffValue(BinaryWriter bw, Tuple<int, int> item, int currentValue, int currentValueBitLength)
    {
        if ((item.Item1 != 0 && item.Item2 != 0) || (item.Item1 != 15 && item.Item2 != 0))
            WriteBits(bw, currentValue, currentValueBitLength);
    }

    void WriteACCoeffCategory(BinaryWriter bw, Tuple<int, int> item, Tuple<int, int>[] coeffTable, int currentValueBitLength)
    {
        int acCoefCategoryCode = coeffTable[item.Item1 * 16 + currentValueBitLength].Item1;
        int acCoefCategorySize = coeffTable[item.Item1 * 16 + currentValueBitLength].Item2;

        WriteBits(bw, acCoefCategoryCode, acCoefCategorySize);
    }

    void EncodeDC(int dc, int prevDC, BinaryWriter bw, Tuple<int, int>[] diffTable)
    {
        int diffAbsoluteValue = dc - prevDC;
        int diffValue = diffAbsoluteValue;

        if (diffAbsoluteValue < 0)
        {
            diffAbsoluteValue = -diffAbsoluteValue;
            diffValue--;
        }

        int diffBitLength = CalculateValueCategory(diffAbsoluteValue);
        WriteDCDiffCoeffCategory(bw, diffBitLength, diffTable);
        WriteDCDiffCoeffValue(bw, diffValue, diffBitLength);
    }

    void WriteDCDiffCoeffValue(BinaryWriter bw, int diffValue, int diffBitLength)
    {
        if (diffBitLength != 0)
            WriteBits(bw, diffValue, diffBitLength);
    }

    void WriteDCDiffCoeffCategory(BinaryWriter bw, int diffBitLength, Tuple<int, int>[] diffTable)
    {
        WriteBits(bw, diffTable[diffBitLength].Item1, diffTable[diffBitLength].Item2);
    }

    void EncodeAC(Block8x8 block, BinaryWriter bw, Tuple<int, int>[] coeffTable)
    {
        var runLengthValuePairs = _runLengthEncodingService.Encode(block);
        int coeffCount = 1;

        foreach (var item in runLengthValuePairs)
        {
            if (coeffCount == 64)
                return;

            EncodeACCoeff(bw, item, coeffTable);

            // Increment coefficient count. If it reaches 64, no need for EndOfBlock marker
            coeffCount = coeffCount + 1 + item.Item1;
        }
    }
}