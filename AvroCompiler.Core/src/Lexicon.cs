using System.Text.Json;

namespace AvroCompiler.Core;

public class Lexicon
{
    public static bool HasValue(object? obj)
    {
        return obj != null;
    }
    public static T MustNeverBeNull<T>(T? obj)
    {
        if (obj != null)
        {
            return obj;
        }
        throw new ArgumentNullException();
    }
    public static T MustNeverBeNull<T>(Nullable<T> obj) where T : struct
    {
        if (obj != null)
        {
            return obj.Value;
        }
        throw new ArgumentNullException();
    }
    public static T MaybeSafelyNull<T>(Nullable<T> obj) where T : struct
    {
        if (obj != null)
        {
            return obj.Value;
        }
        return default!;
    }
    public static T MaybeSafelyNull<T>(T? obj)
    {
        if (obj != null)
        {
            return obj;
        }
        return default!;
    }
    public static bool IsUnionType(JsonElement? jsonElement)
    {
        return jsonElement?.ValueKind == JsonValueKind.Array;
    }
    public static bool IsFieldType(JsonElement? jsonElement)
    {
        return jsonElement?.ValueKind == JsonValueKind.String;
    }
    public static bool IsArrayType(JsonElement? jsonElement, out string? arrayItem)
    {
        if (jsonElement?.TryGetProperty("items", out JsonElement item) == true)
        {
            string? itemType = item.GetString();
            if (itemType != null)
            {
                arrayItem = itemType;
                return true;
            }
        }
        arrayItem = null;
        return false;
    }
    public static void ThrowIfOtherwise(params bool[] args)
    {
        foreach(var i in args) {
            if(!i) {
                throw new ArgumentException();
            }
        }
    }
    public static Exception ThrowIfUnpredictable()
    {
        throw new ArgumentException();
    }
}