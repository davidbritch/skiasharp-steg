namespace ImageCompression.F5.Exceptions;

public class MatrixEncodingException : Exception
{
    public MatrixEncodingException() { }
    public MatrixEncodingException(string message) : base(message) { }
    public MatrixEncodingException(string message, Exception innerException) : base(message, innerException) { }
}