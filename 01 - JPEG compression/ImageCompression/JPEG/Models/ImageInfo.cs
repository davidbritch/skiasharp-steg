using SkiaSharp;

namespace ImageCompression.JPEG.Models;

public class ImageInfo
{
    public SKBitmap? Bitmap { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public int HorizontalPixelDensity { get; set; }
    public int VerticalPixelDensity { get; set; }
    public int Precision { get; set; }
    public ColourData? ColourData { get; set; }
    public DCTData? DCTData { get; set; }
    public DCTData? QuantizedDCTData { get; set; }
    public DCTData? EmbeddedData { get; set; }
    public Component[] Components { get; set; }
    public QuantisationTable[] QuantizationTables { get; set; }
    public HuffmanTableData HuffmanTableData { get; set; }
    public List<HuffmanTable> HuffmanTables { get; set; }

    public ImageInfo()
    {
        QuantizationTables = new QuantisationTable[2];
        HuffmanTableData = new HuffmanTableData();
        HuffmanTables = new List<HuffmanTable>();
        Components = new Component[]
        {
            new Component(1),
            new Component(2),
            new Component(3)
        };
    }

}