namespace ImageCompression.JPEG.Models;

public class HuffmanTable
{
    public int Id { get; set; }
    public int[]? Bits { get; set; }
    public int[]? HuffValue { get; set; }
}