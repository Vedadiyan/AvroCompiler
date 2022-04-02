namespace AvroCompiler.Core.Exceptions;

public enum MapExceptionMessages
{
    UNDEFINED,
    INAVLID_GENRIC_TYPE
}
public class AvroMapException : Exception
{
    private static IReadOnlyDictionary<MapExceptionMessages, string> errors = new Dictionary<MapExceptionMessages, string>
    {
        [MapExceptionMessages.UNDEFINED] = "{0}",
        [MapExceptionMessages.INAVLID_GENRIC_TYPE] = "Invalid generic field type for field `{0}`"
    };
    public AvroMapException(MapExceptionMessages mapExceptionMessages, params string[] args) : base(string.Format(errors[mapExceptionMessages], args)) {
        
     }
}