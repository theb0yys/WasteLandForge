using System.IO.Compression;

if (args.Length == 0)
{
    Console.WriteLine("BSArch v0.7 synthetic WastelandForge fixture");
    return 0;
}

try
{
    if (args.Length == 4 && args[0] == "pack" && args[3] == "-fnv")
    {
        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(args[2]))!);
        using var archive = ZipFile.Open(args[2], ZipArchiveMode.Create);
        foreach (var file in Directory.EnumerateFiles(args[1], "*", SearchOption.AllDirectories).Order(StringComparer.Ordinal))
        {
            var entry = archive.CreateEntry(Path.GetRelativePath(args[1], file).Replace('\\', '/'), CompressionLevel.NoCompression);
            entry.LastWriteTime = new DateTimeOffset(1980, 1, 1, 0, 0, 0, TimeSpan.Zero);
            using var source = File.OpenRead(file);
            using var target = entry.Open();
            source.CopyTo(target);
        }
        Console.WriteLine("Packed synthetic archive.");
        return 0;
    }
    if (args.Length == 2 && args[1] == "-list")
    {
        using var archive = ZipFile.OpenRead(args[0]);
        Console.WriteLine("Format: Fallout 3/New Vegas");
        Console.WriteLine($"Files: {archive.Entries.Count}");
        foreach (var entry in archive.Entries) Console.WriteLine(entry.FullName.Replace('/', '\\'));
        return 0;
    }
    if (args.Length == 4 && args[0] == "unpack" && args[3] == "-q")
    {
        using var archive = ZipFile.OpenRead(args[1]);
        foreach (var entry in archive.Entries)
        {
            var path = Path.GetFullPath(Path.Combine(args[2], entry.FullName.Replace('/', Path.DirectorySeparatorChar)));
            var root = Path.GetFullPath(args[2]).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
            if (!path.StartsWith(root, StringComparison.OrdinalIgnoreCase)) throw new InvalidOperationException("Unsafe synthetic archive path.");
            Directory.CreateDirectory(Path.GetDirectoryName(path)!);
            entry.ExtractToFile(path);
        }
        Console.WriteLine("Unpacked synthetic archive.");
        return 0;
    }
    Console.Error.WriteLine("Unsupported synthetic BSArch arguments.");
    return 2;
}
catch (Exception exception)
{
    Console.Error.WriteLine(exception.Message);
    return 1;
}
