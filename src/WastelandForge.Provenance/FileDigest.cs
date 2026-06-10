namespace WastelandForge.Provenance;

public sealed record FileDigest(
    string Path,
    string Sha256,
    long Length);
