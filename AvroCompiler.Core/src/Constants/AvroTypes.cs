namespace AvroCompiler.Core.Contants;
[Flags]
public enum AvroTypes
{
    UNDEFINED = 0,
    RECORD = 1 << 1,
    OBJECT = 1 << 1,
    ARRAY = 1 << 2,
    Enum = 1 << 3,
    FIXED = 1 << 4,
    UNION = 1 << 5,
    STRING = 1 << 6,
    INT = 1 << 7,
    LONG = 1 << 8,
    FLOAT = 1 << 9,
    DOUBLE = 1 << 10,
    BOOLEAN = 1 << 11,
    BYTES = 1 << 12,
    TIMESTAMP = 1 << 13,
    REFERENCE = 1 << 14,
    MAP = 1 << 15,
    NULL = 1 << 16,
}