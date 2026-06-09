namespace WastelandForge.Core;

public sealed record SourceLocation
{
    public SourceLocation(string file, JsonPointer? pointer = null, int? line = null, int? column = null)
    {
        if (string.IsNullOrWhiteSpace(file))
        {
            throw new ArgumentException("Source locations require a file path.", nameof(file));
        }

        if (line <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(line), "Line numbers must be positive when provided.");
        }

        if (column <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(column), "Column numbers must be positive when provided.");
        }

        File = file;
        Pointer = pointer;
        Line = line;
        Column = column;
    }

    public string File { get; }

    public JsonPointer? Pointer { get; }

    public int? Line { get; }

    public int? Column { get; }
}
