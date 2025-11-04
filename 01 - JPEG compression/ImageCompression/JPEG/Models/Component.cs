namespace ImageCompression.JPEG.Models;

public class Component
{
    public int Id { get; set; }

    public int SamplingFactor { get; set; }

    public int QuantizationTableId { get; set; }

    public int DCHuffmanTableId { get; set; }
    
    public int ACHuffmanTableId { get; set; }

    public Component()
    {        
    }

    public Component(int id)
    {
        Id = id;
    }
}