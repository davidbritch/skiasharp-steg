namespace ImageCompression.JPEG.Models;

public class HuffmanTableData
{
    public int[] DCLuminanceBits;

    public int[] DCLuminanceValues;

    public int[] DCChrominanceBits;

    public int[] DCChrominanceValues;

    public int[] ACLuminanceBits;

    public int[] ACChrominanceBits;

    public int[] ACLuminanceValues;

    public int[] ACChrominanceValues;

    public HuffmanTableData() 
    {
        DCLuminanceBits = new int[HuffmanEncoding.DCLuminanceBits.Length];        
        DCLuminanceValues = new int[HuffmanEncoding.DCLuminanceValues.Length];
        DCChrominanceBits = new int[HuffmanEncoding.DCChrominanceBits.Length];
        DCChrominanceValues = new int[HuffmanEncoding.DCChrominanceValues.Length];
        ACLuminanceBits = new int[HuffmanEncoding.ACLuminanceBits.Length];
        ACChrominanceBits = new int[HuffmanEncoding.ACChrominanceBits.Length];
        ACLuminanceValues = new int[HuffmanEncoding.ACLuminanceValues.Length];
        ACChrominanceValues = new int[HuffmanEncoding.ACChrominanceValues.Length];
    }    
}