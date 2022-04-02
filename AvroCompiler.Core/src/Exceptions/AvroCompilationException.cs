namespace AvroCompiler.Core.Exceptions;

public class AvroCompilationException : Exception
{
    public AvroCompilationException(string error) : base(error)
    {
    }
}