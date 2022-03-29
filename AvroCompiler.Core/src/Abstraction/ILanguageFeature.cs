using AvroCompiler.Core.Contants;

namespace AvroCompiler.Core.Abstraction;

public interface ILanguageFeature
{
    string GetType(AvroTypes avroType);
    string GetNamespace(string @namespace);
    string GetImports();
    string GetCodec(string name, string schema, object? options);
    string GetMessage(string name, IReadOnlyDictionary<string, string> request, string response, object? options);
    string GetField(AvroTypes avroType, string name, object? options);
    string GetField(string avroType, AvroTypes actualAvroType, string name, object? options);
    string GetEnum(string name, string[] symbols, object? options);
    string GetFixed(string name, object? options);
    string GetArray(int dimensions, AvroTypes elementType, string? elementGenericType, string name, object? options);
    string GetArray(int dimensions, string elementType, string name, object? options);
    string GetMap(AvroTypes elementType, string name, object? options);
    string GetMap(string elementType, string name, object? options);
    string GetRecord(string name, AvroElement[] fields, object? options);
    Task<string> Format(string input);
}