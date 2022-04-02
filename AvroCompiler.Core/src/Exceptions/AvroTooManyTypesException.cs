namespace AvroCompiler.Core.Exceptions;

public class AvroTooManyTypesException : Exception
{
    public AvroTooManyTypesException(string fieldName) : base($"Too many types for field {fieldName}")
    {

    }
}