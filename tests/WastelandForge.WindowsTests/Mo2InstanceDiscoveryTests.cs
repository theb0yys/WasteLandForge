using WastelandForge.Desktop;

namespace WastelandForge.WindowsTests;

public sealed class Mo2InstanceDiscoveryTests
{
    [Fact]
    public void DiscoversPortableAndGlobalCandidatesInDeterministicOrder()
    {
        var root = NewRoot();
        try
        {
            var portable = Path.Combine(root, "portable");
            var global = Path.Combine(root, "global");
            Directory.CreateDirectory(Path.Combine(portable, "mods"));
            Directory.CreateDirectory(Path.Combine(global, "Zulu", "mods"));
            Directory.CreateDirectory(Path.Combine(global, "Alpha", "mods"));
            File.WriteAllText(Path.Combine(portable, "ModOrganizer.exe"), "synthetic");
            File.Copy(Fixture("portable", "ModOrganizer.ini"), Path.Combine(portable, "ModOrganizer.ini"));
            WriteIni(Path.Combine(global, "Zulu", "ModOrganizer.ini"), "%BASE_DIR%", "%BASE_DIR%/mods");
            WriteIni(Path.Combine(global, "Alpha", "ModOrganizer.ini"), "%BASE_DIR%", "%BASE_DIR%/mods");

            var result = Mo2InstanceDiscovery.Discover(Path.Combine(portable, "ModOrganizer.exe"), global);

            Assert.Equal(3, result.Candidates.Count);
            Assert.All(result.Candidates, candidate => Assert.Equal("candidate", candidate.Status));
            Assert.Equal(result.Candidates.OrderBy(candidate => candidate.ConfigurationPath, StringComparer.OrdinalIgnoreCase).ThenBy(candidate => candidate.ConfigurationPath, StringComparer.Ordinal), result.Candidates);
            Assert.Contains(result.Candidates, candidate => candidate.InstanceKind == "portable" && candidate.ModsRoot == Path.Combine(portable, "mods"));
        }
        finally { Directory.Delete(root, true); }
    }

    [Fact]
    public void ResolvesDefaultsAndRefusesUnsupportedOrUnsafeRoots()
    {
        var root = NewRoot();
        try
        {
            var defaultRoot = Path.Combine(root, "default");
            Directory.CreateDirectory(Path.Combine(defaultRoot, "mods"));
            File.Copy(Fixture("invalid", "missing-mod-directory", "ModOrganizer.ini"), Path.Combine(defaultRoot, "ModOrganizer.ini"));
            var defaultCandidate = Mo2InstanceDiscovery.Parse(Path.Combine(defaultRoot, "ModOrganizer.ini"), "global", "Default");
            Assert.Equal("invalid", defaultCandidate.Status); // Fixture base path deliberately does not exist.

            WriteIni(Path.Combine(defaultRoot, "ModOrganizer.ini"), "%BASE_DIR%", null);
            defaultCandidate = Mo2InstanceDiscovery.Parse(Path.Combine(defaultRoot, "ModOrganizer.ini"), "global", "Default");
            Assert.Equal("candidate", defaultCandidate.Status);
            Assert.Equal(Path.Combine(defaultRoot, "mods"), defaultCandidate.ModsRoot);

            var tokenRoot = Path.Combine(root, "token"); Directory.CreateDirectory(tokenRoot);
            File.Copy(Fixture("invalid", "unresolved-token", "ModOrganizer.ini"), Path.Combine(tokenRoot, "ModOrganizer.ini"));
            Assert.Equal("unsupported", Mo2InstanceDiscovery.Parse(Path.Combine(tokenRoot, "ModOrganizer.ini"), "global", "Token").Status);

            var overwrite = Path.Combine(root, "Overwrite"); Directory.CreateDirectory(overwrite);
            WriteIni(Path.Combine(root, "unsafe.ini"), root, overwrite);
            Assert.Equal("invalid", Mo2InstanceDiscovery.Parse(Path.Combine(root, "unsafe.ini"), "global", "Unsafe").Status);

            var data = Path.Combine(root, "game", "Data"); Directory.CreateDirectory(Path.Combine(data, "mods"));
            Assert.Contains("game Data", Mo2InstanceDiscovery.ValidateModsRoot(Path.Combine(data, "mods"), data)!);

            var duplicate = Path.Combine(root, "duplicate.ini");
            File.WriteAllText(duplicate, "[Settings]\nmod_directory=one\nmod_directory=two\n");
            Assert.Equal("invalid", Mo2InstanceDiscovery.Parse(duplicate, "global", "Duplicate").Status);

            var malformed = Path.Combine(root, "malformed.ini");
            File.WriteAllText(malformed, "[Settings]\nmod_directory\n");
            Assert.Equal("invalid", Mo2InstanceDiscovery.Parse(malformed, "global", "Malformed").Status);
        }
        finally { Directory.Delete(root, true); }
    }

    private static void WriteIni(string path, string baseDirectory, string? modsDirectory)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, "[Settings]\nbase_directory=" + baseDirectory + "\n" + (modsDirectory is null ? string.Empty : "mod_directory=" + modsDirectory + "\n"));
    }

    private static string Fixture(params string[] parts)
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            var candidate = Path.Combine([directory.FullName, "fixtures", "mo2-instances", .. parts]);
            if (File.Exists(candidate)) return candidate;
            directory = directory.Parent;
        }
        throw new FileNotFoundException("MO2 fixture not found.");
    }

    private static string NewRoot() { var path = Path.Combine(Path.GetTempPath(), "WastelandForge.Mo2DiscoveryTests", Guid.NewGuid().ToString("N")); Directory.CreateDirectory(path); return path; }
}
