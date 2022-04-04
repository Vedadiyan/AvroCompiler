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
    public static T MayBeSafelyNull<T>(T? obj)
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
    public static bool IsArrayType(JsonElement? jsonElement, ref int dimenstions, ref string? arrayItemType, ref string? itemGenericType)
    {
        if (jsonElement.Value.ValueKind == JsonValueKind.Object)
        {
            if (jsonElement?.TryGetProperty("items", out JsonElement item) == true)
            {
                if (item.ValueKind == JsonValueKind.String)
                {
                    string? itemType = item.GetString();
                    if (itemType != null)
                    {
                        arrayItemType = itemType;
                        return true;
                    }
                }
                else if (item.ValueKind == JsonValueKind.Object)
                {
                    string tmp = item.GetRawText();
                    if (item.TryGetProperty("values", out JsonElement _item))
                    {
                        arrayItemType = "map";
                        itemGenericType = MustNeverBeNull(_item.GetString());
                        return true;
                    }
                    else
                    {
                        dimenstions++;
                        IsArrayType(item, ref dimenstions, ref arrayItemType, ref itemGenericType);
                        return true;
                    }
                }
                else
                {
                    ThrowIfUnpredictable();
                }
            }
        }
        return false;
    }
    public static bool IsEnum(JsonElement jsonElement)
    {
        if (jsonElement.ValueKind == JsonValueKind.String)
        {
            string? typeName = jsonElement.GetString();
            if (HasValue(typeName))
            {
                if (typeName == "enum")
                {
                    return true;
                }
            }
        }
        return false;
    }
    public static bool IsMap(JsonElement jsonElement)
    {
        if (jsonElement.ValueKind == JsonValueKind.String)
        {
            string? typeName = jsonElement.GetString();
            if (HasValue(typeName))
            {
                if (typeName == "map")
                {
                    return true;
                }
            }
        }
        return false;
    }
    public static bool IsTypeDefinition(JsonElement jsonElement, out JsonElement typeDefinitions)
    {
        if (jsonElement.ValueKind == JsonValueKind.Object)
        {
            return jsonElement.TryGetProperty("type", out typeDefinitions);
        }
        typeDefinitions = default;
        return false;
    }
    public static void ThrowIfOtherwise(params bool[] args)
    {
        foreach (var i in args)
        {
            if (!i)
            {
                throw new ArgumentException();
            }
        }
    }
    public static Exception ThrowIfUnpredictable()
    {
        throw new ArgumentException();
    }
    public static T ShouldOr<T>(T? value, Exception exception)
    {
        if (value != null)
        {
            return value;
        }
        throw exception;
    }
    public static T ShouldOr<T>(Nullable<T> value, Exception exception) where T : struct
    {
        if (value != null)
        {
            return value.Value;
        }
        throw exception;
    }
    public static T[] MustOr<T>(T[]? value, Exception exception)
    {
        if (value != null && value.Length > 0)
        {
            return value;
        }
        throw exception;
    }
    public static IEnumerable<T> MustOr<T>(IEnumerable<T>? value, Exception exception)
    {
        if (value != null && value.Count() > 0)
        {
            return value;
        }
        throw exception;
    }
}