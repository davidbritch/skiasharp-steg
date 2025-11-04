namespace ImageCompression.JPEG.Extensions;

internal static class ValidationMessageExtensions
{
    internal static string ToArgumentNullExceptionMessage(this string input)
    {
        return $"{input} can't be null.";
    }

    internal static string ToArgumentEqualsZeroExceptionMessage(this string input)
    {
        return $"{input} must be greater than 0.";
    }
}