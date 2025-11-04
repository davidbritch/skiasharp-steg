namespace ImageCompression;

public static class RGB
{
    public static float ToY(byte r, byte g, byte b)
    {
        return (float)(0.299 * r + 0.587 * g + 0.114 * b);
    }

    public static float ToCb(byte r, byte g, byte b)
    {
        return (float)(128 + (-0.16874 * r - 0.33126 * g + 0.5 * b));
    }

    public static float ToCr(byte r, byte g, byte b)
    {
        return (float)(128 + (0.5 * r - 0.41869 * g - 0.08131 * b));
    }
}