namespace ImageCompression.JPEG.Models;

public class QuantisationTable
{
    public int Id { get; set; }
    public byte[] Values { get; set; }

    public QuantisationTable()
    {
        Values = new byte[64];
    }

    public QuantisationTable(int id)
    {
        Id = id;
        Values = new byte[64];
    }
}