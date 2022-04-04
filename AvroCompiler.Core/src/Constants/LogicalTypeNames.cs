namespace AvroCompiler.Core.Contants;

public class LogicalTypeNames {
    private static readonly Dictionary<string, string> logicalTypeNameMap;
    static LogicalTypeNames() {
        logicalTypeNameMap = new Dictionary<string, string> {
            ["time-millis"] = "TIMESTAMP",
            ["timestamp-millis"] = "TIMESTAMP",
            ["decimal"] = "DOUBLE"
        };
    }
    public static string? GetTypeNameFromLogicalType(string logicalTypeName) {
        if(logicalTypeNameMap.TryGetValue(logicalTypeName, out string? output)) {
            return output;
        }
        return null;
    }
}