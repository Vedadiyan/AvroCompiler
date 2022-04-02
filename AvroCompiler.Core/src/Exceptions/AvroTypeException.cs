namespace AvroCompiler.Core.Exceptions;

public class AvroTypeException : Exception
{
    public AvroTypeException(string fieldName) : base($"No type could be determined for field '{fieldName}'") { 

    }
}