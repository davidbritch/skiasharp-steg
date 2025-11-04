using ImageCompression.JPEG.Models;
using ImageCompression.JPEG.Extensions;

namespace ImageCompression.JPEG.Services;

public class HeaderService : IHeaderService
{
    public void WriteHeaders(BinaryWriter bw, ImageInfo jpeg)
    {
        if (jpeg == null)
            throw new ArgumentNullException(nameof(jpeg), nameof(jpeg).ToArgumentNullExceptionMessage());

        if (bw == null)
            throw new ArgumentNullException(nameof(bw), nameof(bw).ToArgumentNullExceptionMessage());

        WriteSOI(bw);
        WriteApp0(bw);
        WriteDQT(bw);
        WriteSOF0(bw, jpeg);
        WriteDHT(bw);
        WriteSOS(bw);
    }

    public void WriteEOI(BinaryWriter bw)
    {
        if (bw == null)
            throw new ArgumentNullException(nameof(bw), nameof(bw).ToArgumentNullExceptionMessage());

        bw.Write((byte)Marker.Padding);
        bw.Write((byte)Marker.EndOfImage);
    }

    public void ParseJpegMarkers(BinaryReader br, ImageInfo jpeg)
    {
        if (jpeg == null)
            throw new ArgumentNullException(nameof(jpeg), nameof(jpeg).ToArgumentNullExceptionMessage());

        if (br == null)
            throw new ArgumentNullException(nameof(br), nameof(br).ToArgumentNullExceptionMessage());

        byte currentByte = br.ReadByte();
        byte previousByte;
        bool loop = true;

        while (loop)
        {
            previousByte = currentByte;
            currentByte = br.ReadByte();

            if (previousByte == (byte)Marker.Padding && currentByte != (byte)Marker.Padding)
            {
                Marker? marker = ParseJpegMarker(currentByte);
                switch (marker)
                {
                    case Marker.StartOfImage:
                        break;
                    case Marker.App0:
                        currentByte = ParseApp0Segment(br, jpeg);
                        break;
                    case Marker.DefineQuantizationTable:
                        currentByte = ParseQuantizationTable(br, jpeg);
                        break;
                    case Marker.StartOfFrame0:
                        currentByte = ParseSOF0(br, jpeg);
                        break;
                    case Marker.DefineHuffmanTable:
                        currentByte = ParseHuffmanTables(br, jpeg);
                        break;
                    case Marker.StartOfScan:
                        currentByte = ParseStartOfScan(br, jpeg);
                        loop = false;
                        break;
                    default:
                        currentByte = ReadUnsupportedSegment(br);
                        break;
                }
            }
        }
    }

    void WriteSOI(BinaryWriter bw)
    {
        bw.Write((byte)Marker.Padding);
        bw.Write((byte)Marker.StartOfImage);
    }

    void WriteApp0(BinaryWriter bw)
    {
        bw.Write((byte)Marker.Padding);
        bw.Write((byte)Marker.App0);
        byte[] x = new byte[16] 
        {
            0x00, 0x10,                             // length
            0x4a, 0x46, 0x49, 0x46, 0x00,           // "JFIF\0"
            0x01, 0x01,                             // version
            0x01,                                   // units
            0x00, 0x60, 0x00, 0x60,                 // density
            0x00, 0x00                              // thumbnail
        };
        bw.Write(x);
    }

    void WriteDQT(BinaryWriter bw)
    {
        // Luminance
        WriteDQTMarkers(bw, 0x00);
        WriteDQTData(bw, QuantisationTables.Luminance);

        // Chrominance
        bw.Write((byte)0x01);
        WriteDQTData(bw, QuantisationTables.Chrominance);
    }

    static void WriteDQTMarkers(BinaryWriter bw, byte destination)
    {
        bw.Write((byte)Marker.Padding);
        bw.Write((byte)Marker.DefineQuantizationTable);
        bw.Write(new byte[2] { 0x00, 0x84 }); // Length
        bw.Write(destination);
    }

    void WriteDQTData(BinaryWriter bw, byte[] table)
    {
        for (var i = 0; i < table.Length; i++)
            bw.Write(table[DCTOrder.Order[i]]);
    }

    void WriteSOF0(BinaryWriter bw, ImageInfo jpeg)
    {
        bw.Write((byte)Marker.Padding);
        bw.Write((byte)Marker.StartOfFrame0);

        List<byte> SOF = new List<byte>(19)
        {
            0x00, 0x11,
            0x08,
            (byte)(jpeg.Height >> 8 & 0xFF),
            (byte)(jpeg.Height & 0xFF),
            (byte)(jpeg.Width >> 8 & 0xFF),
            (byte)(jpeg.Width & 0xFF),
            0x03 // ImageInfo.NumComponents
        };
        
        for (var i = 0; i < 3; i++)
        {
            SOF.Add((byte)(i + 1));
            SOF.Add(0x11);      // Samp factor 1x1

            if (i == 0)
                SOF.Add(0x00);
            else
                SOF.Add(0x01);
        }

        bw.Write(SOF.ToArray());
    }

    void WriteDHT(BinaryWriter bw)
    {
        bw.Write((byte)Marker.Padding);
        bw.Write((byte)Marker.DefineHuffmanTable);
        bw.Write(new byte[2] { 0x01, 0xA2 }); // Length

        bw.Write(ConvertToByteArray(HuffmanEncoding.DCLuminanceBits));
        bw.Write(ConvertToByteArray(HuffmanEncoding.DCLuminanceValues));
        bw.Write(ConvertToByteArray(HuffmanEncoding.ACLuminanceBits));
        bw.Write(ConvertToByteArray(HuffmanEncoding.ACLuminanceValues));

        bw.Write(ConvertToByteArray(HuffmanEncoding.DCChrominanceBits));
        bw.Write(ConvertToByteArray(HuffmanEncoding.DCChrominanceValues));
        bw.Write(ConvertToByteArray(HuffmanEncoding.ACChrominanceBits));
        bw.Write(ConvertToByteArray(HuffmanEncoding.ACChrominanceValues));
    }

    static byte[] ConvertToByteArray(int[] input)
    {
        return input.Select(item => (byte)item).ToArray();
    }

    void WriteSOS(BinaryWriter bw)
    {
        bw.Write((byte)Marker.Padding);
        bw.Write((byte)Marker.StartOfScan);

        bw.Write(new byte[2] { 0x00, 0x0C });

        bw.Write((byte)0x03); // Component count

        bw.Write((byte)0x01); // component id (Y)
        bw.Write((byte)0x00); // component table ids - dc table 0 ac table 0.

        bw.Write((byte)0x02); // component id (CR)
        bw.Write((byte)0x11); // component table ids - dc table 1 ac table 1.

        bw.Write((byte)0x03); // component id (CB)
        bw.Write((byte)0x11); // component table ids - dc table 1 ac table 1.

        bw.Write((byte)0x00);
        bw.Write((byte)0x3f);
        bw.Write((byte)0x00);
    }

    private byte ReadUnsupportedSegment(BinaryReader br)
    {
        int length = br.Read2Bytes();
        byte currentByte = br.ReadByte();

        for (int i = 0; i < length - 3; i++)
            currentByte = br.ReadByte();

        return currentByte;
    }

    Marker? ParseJpegMarker(byte marker)
    {
        if (Enum.IsDefined(typeof(Marker), marker))
            return (Marker)marker;
        else
            return null;
    }

    byte ParseApp0Segment(BinaryReader br, ImageInfo jpeg)
    {
        int length = br.Read2Bytes();
        byte[] jfifVersion = new byte[7] 
        {
            0x4a, 0x46, 0x49, 0x46, 0x00,           //"JFIF\0"
            0x01, 0x01                              //version
        };

        for (int i = 0; i < 7; i++)
        {
            if (jfifVersion[i] != br.ReadByte())
                throw new Exception("Error reading Jfif version.");

        }

        byte units = br.ReadByte();
        int horizontalPixelDensity = br.Read2Bytes();
        int verticalPixelDensity = br.Read2Bytes();

        jpeg.HorizontalPixelDensity = horizontalPixelDensity;
        jpeg.VerticalPixelDensity = verticalPixelDensity;

        int thumbnailData = br.Read2Bytes();
        byte currentByte = (byte)(thumbnailData & 0xFF);

        return currentByte;
    }

    byte ParseQuantizationTable(BinaryReader br, ImageInfo jpeg)
    {
        int length = br.Read2Bytes();

        if (length > 67)
        {
            // Read consecutive DQTs
            byte dest = br.ReadByte();
            byte current = ReadDQTForComponent(br, jpeg, dest);
            
            dest = br.ReadByte();
            current = ReadDQTForComponent(br, jpeg, dest);
            
            return current;
        }

        byte destination = br.ReadByte();
        byte currentByte = ReadDQTForComponent(br, jpeg, destination);

        return currentByte;
    }

    byte ReadDQTForComponent(BinaryReader br, ImageInfo jpeg, byte destination)
    {
        if (destination != 0 && destination != 1)
            throw new Exception("Unsupported DQT destination.");

        jpeg.QuantizationTables[destination] = new QuantisationTable(destination);
        jpeg.QuantizationTables[destination].Values = ReadDQTData(br);

        byte currentByte = jpeg.QuantizationTables[destination].Values.Last();

        return currentByte;
    }

    byte[] ReadDQTData(BinaryReader br)
    {
        var result = new byte[64];

        for (int i = 0; i < 64; i++)
            result[DCTOrder.Order[i]] = br.ReadByte();

        return result;
    }

    byte ParseSOF0(BinaryReader br, ImageInfo jpeg)
    {
        int length = br.Read2Bytes();
        int precision = br.ReadByte();
        int height = br.Read2Bytes();
        int width = br.Read2Bytes();
        byte numberOfComponents = br.ReadByte();
        byte currentByte = numberOfComponents;

        jpeg.Height = height;
        jpeg.Width = width;
        jpeg.Precision = precision;
        jpeg.Components = new Component[numberOfComponents];

        for (int i = 0; i < numberOfComponents; i++)
        {
            byte componentId = br.ReadByte();
            byte sampFactor = br.ReadByte();
            byte tableId = br.ReadByte();
            currentByte = tableId;

            jpeg.Components[i] = new Component
            {
                SamplingFactor = sampFactor,
                Id = componentId,
                QuantizationTableId = tableId
            };
        }

        return currentByte;
    }

    byte ParseHuffmanTables(BinaryReader br, ImageInfo jpeg)
    {
        int length = br.Read2Bytes();
        byte currentByte;

        if (length > 0xFF)
        {
            currentByte = ParseConsecutiveHuffmanTables(br, jpeg);
            return currentByte;
        }

        currentByte = br.ReadByte();
        byte classDestination = currentByte;
        byte dhtClass = (byte)(currentByte >> 4);
        byte dhtDestination = (byte)(currentByte & 0x0F);

        byte[] bitsArray = ReadBitsArray(br);

        int hufValCount = length - 3 - bitsArray.Length;
        byte[] hufValArray = ReadHufValArray(br, hufValCount);

        SaveHuffmanTableData(jpeg, dhtClass, dhtDestination, bitsArray, hufValArray);
        SaveHuffmanTable(jpeg, classDestination, bitsArray, hufValArray);

        currentByte = hufValArray.Last();
        return currentByte;
    }

    void SaveHuffmanTable(ImageInfo jpeg, byte classDestination, byte[] bitsArray, byte[] hufValArray)
    {
        jpeg.HuffmanTables.Add(new HuffmanTable
        {
            Id = classDestination,
            Bits = bitsArray.Select(item => (int)item).ToArray(),
            HuffValue = hufValArray.Select(item => (int)item).ToArray()
        });
    }

    byte[] ReadHufValArray(BinaryReader br, int hufValCount)
    {
        byte[]? hufValArray = new byte[hufValCount];

        for (int i = 0; i < hufValCount; i++)
            hufValArray[i] = br.ReadByte();

        return hufValArray;
    }

    byte[] ReadBitsArray(BinaryReader br)
    {
        byte[]? bitsArray = new byte[16];

        for (int i = 0; i < 16; i++)
            bitsArray[i] = br.ReadByte();

        return bitsArray;
    }

    void SaveHuffmanTableData(ImageInfo jpeg, byte dhtClass, byte dhtDestination, byte[] bitsArray, byte[] hufValArray)
    {
        if (dhtClass == 0 && dhtDestination == 0)
        {
            jpeg.HuffmanTableData.DCLuminanceBits = bitsArray.Select(item => (int)item).ToArray();
            jpeg.HuffmanTableData.DCLuminanceValues = hufValArray.Select(item => (int)item).ToArray();

        }
        if (dhtClass == 0 && dhtDestination == 1)
        {
            jpeg.HuffmanTableData.DCChrominanceBits = bitsArray.Select(item => (int)item).ToArray();
            jpeg.HuffmanTableData.DCChrominanceValues = hufValArray.Select(item => (int)item).ToArray();

        }
        if (dhtClass == 1 && dhtDestination == 0)
        {
            jpeg.HuffmanTableData.ACLuminanceBits = bitsArray.Select(item => (int)item).ToArray();
            jpeg.HuffmanTableData.ACLuminanceValues = hufValArray.Select(item => (int)item).ToArray();

        }
        if (dhtClass == 1 && dhtDestination == 1)
        {
            jpeg.HuffmanTableData.ACChrominanceBits = bitsArray.Select(item => (int)item).ToArray();
            jpeg.HuffmanTableData.ACChrominanceValues = hufValArray.Select(item => (int)item).ToArray();
        }
    }

    byte ParseConsecutiveHuffmanTables(BinaryReader br, ImageInfo jpeg)
    {
        for (int h = 0; h < 4; h++)
        {
            int hufValCount;
            byte[] hufValArray;
            byte currentByte = br.ReadByte();

            byte classDestination = currentByte;
            byte dhtClass = (byte)(classDestination >> 4);
            byte dhtDestination = (byte)(classDestination & 0x0F);

            byte[] bitsArray = ReadBitsArray(br);

            hufValCount = CalculateHufvalCount(dhtClass);

            hufValArray = ReadHufValArray(br, hufValCount);
            jpeg.HuffmanTables.Add(new HuffmanTable
            {
                Id = classDestination,
                Bits = bitsArray.Select(item => (int)item).ToArray(),
                HuffValue = hufValArray.Select(item => (int)item).ToArray()
            });
        }

        return (byte)jpeg.HuffmanTables.Last().HuffValue!.Last();
    }

    static int CalculateHufvalCount(byte dhtClass)
    {
        int hufValCount;
        if (dhtClass == 0x00)
            hufValCount = 12; // DC
        else
            hufValCount = HuffmanEncoding.ACLuminanceValues.Length; // AC
        return hufValCount;
    }

    byte ParseStartOfScan(BinaryReader br, ImageInfo jpeg)
    {
        int length = br.Read2Bytes();
        byte componentCount = br.ReadByte();
        byte currentByte = componentCount;

        for (int i = 0; i < componentCount; i++)
        {
            byte componentId = br.ReadByte();
            byte tableIds = br.ReadByte();

            byte dcTableId = (byte)(tableIds >> 4);
            byte acTableId = (byte)(tableIds & 0x0F);

            Component? component = jpeg.Components.Where(item => item.Id == componentId).FirstOrDefault();
            component!.DCHuffmanTableId = dcTableId;
            component!.ACHuffmanTableId = acTableId;
        }

        for (int i = 0; i < length - 3 - (2 * componentCount); i++)
            currentByte = br.ReadByte();

        return currentByte;
    }
}