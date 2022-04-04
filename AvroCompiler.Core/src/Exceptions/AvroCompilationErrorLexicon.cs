namespace AvroCompiler.Core.Exceptions;

public class AvroCompliationErrorLexicon {
    public static string MissingFieldName() {
        return "Missing field name.";
    }
    public static string MissingFieldType() {
        return "Missing field type.";
    }
    public static string ExpectingArrayType(string fieldName) {
        return $"Expecting array type for field `{fieldName}`";
    }
}