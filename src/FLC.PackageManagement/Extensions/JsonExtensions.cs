// -------------------------------------------------------------------------------------------------
// Copyright (c) Service Well AB.
// Modifications licensed under the Apache License, Version 2.0. See LICENSE in the repo root.
// -------------------------------------------------------------------------------------------------
using System.Text.Json;

namespace FLC.PackageManagement.Extensions;

public static class JsonExtensions
{
    public static bool TryParseAsJsonDocument(this string content, out JsonDocument document)
    {
        document = null;
        try
        {
            document = JsonDocument.Parse(content);
            return true;
        }
        catch (Exception e) when (
            e is JsonException ||
            e is ArgumentException)
        {
            return false;
        }
    }

    public static string GetRequiredString(this JsonElement root, string prop)
    {
        if (!root.TryGetProperty(prop, out var propValue) || propValue.ValueKind != JsonValueKind.String)
        {
            throw new InvalidDataException($"The JSON is missing the \"{prop}\" string property.");
        }

        var value = propValue.GetString();
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new JsonException($"The property \"{prop}\" has invalid value: {value}");
        }

        return value;
    }

    public static string GetOptionalString(this JsonElement root, string prop)
        => root.TryGetProperty(prop, out var value) && value.ValueKind == JsonValueKind.String
            ? value.GetString() : null;

    public static IEnumerable<JsonElement> GetArrayOrEmpty(this JsonElement root, string prop)
        => root.TryGetProperty(prop, out var value) && value.ValueKind == JsonValueKind.Array
            ? value.EnumerateArray() : Array.Empty<JsonElement>();

    public static string GetExtensionValue(this JsonElement obj, string extensionUrl)
    {
        foreach (var ext in obj.GetArrayOrEmpty("extension"))
        {
            if (ext.TryGetProperty("url", out var u) && u.ValueKind == JsonValueKind.String &&
                string.Equals(u.GetString(), extensionUrl, StringComparison.Ordinal))
            {
                if (ext.TryGetProperty("valueString", out var vs) && vs.ValueKind == JsonValueKind.String)
                {
                    return vs.GetString();
                }

                if (ext.TryGetProperty("valueCanonical", out var vc) && vc.ValueKind == JsonValueKind.String)
                {
                    return vc.GetString();
                }

                if (ext.TryGetProperty("valueUri", out var vu) && vu.ValueKind == JsonValueKind.String)
                {
                    return vu.GetString();
                }
            }
        }

        return null;
    }
}
