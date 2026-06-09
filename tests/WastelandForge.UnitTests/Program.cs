using System.Text.Json.Nodes;
using WastelandForge.Core;

Run("logical IDs enforce dotted lowercase identity", () =>
{
    var id = LogicalId.Parse("io.github.theboyyss.examplemod");

    Equal("io.github.theboyyss.examplemod", id.ToString());
    Throws<ArgumentException>(() => LogicalId.Parse("ExampleMod"));
    Throws<ArgumentException>(() => LogicalId.Parse("examplemod"));
});

Run("JSON Pointer validates and escapes canonical locations", () =>
{
    var pointer = JsonPointer.Parse("/requires/capabilities/2/id");

    Equal("/requires/capabilities/2/id", pointer.ToString());
    Equal("a~1b~0c", JsonPointer.EscapeSegment("a/b~c"));
    Throws<ArgumentException>(() => JsonPointer.Parse("requires/capabilities"));
    Throws<ArgumentException>(() => JsonPointer.Parse("/bad~2escape"));
});

Run("semantic version constraints compare stable versions", () =>
{
    var minimum = SemanticVersion.Parse("6.4.0");
    var compatible = SemanticVersion.Parse("6.4.1");
    var incompatible = SemanticVersion.Parse("6.3.9");
    var constraint = VersionConstraint.AtLeast(minimum);

    True(constraint.Allows(compatible));
    False(constraint.Allows(incompatible));
    Equal(">= 6.4.0", constraint.ToString());
    Throws<ArgumentOutOfRangeException>(() => new SemanticVersion(-1, 0, 0));
});

Run("rule IDs use reserved WastelandForge families", () =>
{
    var ruleId = RuleId.Parse("WF-SEM-014");

    Equal("WF-SEM-014", ruleId.ToString());
    Throws<ArgumentException>(() => RuleId.Parse("WF-DEPS-001"));
});

Run("diagnostic issue JSON follows the canonical shape", () =>
{
    var issue = new DiagnosticIssue(
        RuleId.Parse("WF-SEM-014"),
        DiagnosticSeverity.Error,
        "semantic",
        "Unknown capability reference",
        "Dependency registry references capability 'runtime.ui.fake_provider' which is not defined.",
        new SourceLocation(
            "registries/dependencies/main.yaml",
            JsonPointer.Parse("/requires/capabilities/2/id"),
            19,
            11),
        LogicalId.Parse("io.github.theboyyss.examplemod"),
        [
            new SourceLocation(
                "registries/capabilities/runtime.yaml",
                JsonPointer.Parse("/capabilities"))
        ],
        "Declare the capability in the capability registry or remove the dependency.",
        new Uri("https://docs.wastelandforge.dev/rules/WF-SEM-014"),
        "wf:sem:014:runtime.ui.fake_provider");

    var json = DiagnosticIssueJsonSerializer.Serialize(issue);
    var node = JsonNode.Parse(json) ?? throw new InvalidOperationException("Issue JSON did not parse.");

    Equal("WF-SEM-014", (string?)node["ruleId"]);
    Equal("error", (string?)node["severity"]);
    Equal("semantic", (string?)node["category"]);
    Equal("io.github.theboyyss.examplemod", (string?)node["projectId"]);
    Equal("/requires/capabilities/2/id", (string?)node["primaryLocation"]?["pointer"]);
    Equal("registries/capabilities/runtime.yaml", (string?)node["relatedLocations"]?[0]?["file"]);
    Equal("https://docs.wastelandforge.dev/rules/WF-SEM-014", (string?)node["docsUri"]);
    Equal("wf:sem:014:runtime.ui.fake_provider", (string?)node["fingerprint"]);
});

static void Run(string name, Action test)
{
    try
    {
        test();
        Console.WriteLine($"PASS {name}");
    }
    catch (Exception ex)
    {
        Console.Error.WriteLine($"FAIL {name}");
        Console.Error.WriteLine(ex);
        Environment.ExitCode = 1;
    }
}

static void Equal<T>(T expected, T actual)
{
    if (!EqualityComparer<T>.Default.Equals(expected, actual))
    {
        throw new InvalidOperationException($"Expected '{expected}', got '{actual}'.");
    }
}

static void True(bool value)
{
    if (!value)
    {
        throw new InvalidOperationException("Expected true.");
    }
}

static void False(bool value)
{
    if (value)
    {
        throw new InvalidOperationException("Expected false.");
    }
}

static void Throws<TException>(Action action)
    where TException : Exception
{
    try
    {
        action();
    }
    catch (TException)
    {
        return;
    }

    throw new InvalidOperationException($"Expected exception {typeof(TException).Name}.");
}
