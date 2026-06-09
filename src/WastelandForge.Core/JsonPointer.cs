namespace WastelandForge.Core;

public readonly record struct JsonPointer
{
    public static readonly JsonPointer Root = new(string.Empty);

    private JsonPointer(string value)
    {
        Value = value;
    }

    public string Value { get; }

    public static JsonPointer Parse(string value)
    {
        if (!TryParse(value, out var pointer))
        {
            throw new ArgumentException("JSON Pointer values must be empty or start with '/'; '~' escapes must be '~0' or '~1'.", nameof(value));
        }

        return pointer;
    }

    public static bool TryParse(string? value, out JsonPointer pointer)
    {
        if (value is null)
        {
            pointer = default;
            return false;
        }

        if (value.Length == 0)
        {
            pointer = Root;
            return true;
        }

        if (value[0] != '/')
        {
            pointer = default;
            return false;
        }

        for (var index = 0; index < value.Length; index++)
        {
            if (value[index] != '~')
            {
                continue;
            }

            if (index + 1 >= value.Length || value[index + 1] is not ('0' or '1'))
            {
                pointer = default;
                return false;
            }

            index++;
        }

        pointer = new JsonPointer(value);
        return true;
    }

    public static string EscapeSegment(string segment)
    {
        ArgumentNullException.ThrowIfNull(segment);

        return segment.Replace("~", "~0", StringComparison.Ordinal)
            .Replace("/", "~1", StringComparison.Ordinal);
    }

    public override string ToString() => Value;
}
