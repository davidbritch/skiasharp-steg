namespace ImageCompression;

internal static class RGB
{
    internal static float ToY(byte r, byte g, byte b)
    {
        return (float)(0.299 * r + 0.587 * g + 0.114 * b);
    }

    internal static float ToCb(byte r, byte g, byte b)
    {
        return (float)(128 + (-0.16874 * r - 0.33126 * g + 0.5 * b));
    }

    internal static float ToCr(byte r, byte g, byte b)
    {
        return (float)(128 + (0.5 * r - 0.41869 * g - 0.08131 * b));
    }
}