namespace WastelandForge.Cli;

internal enum CliExitCode
{
    Success = 0,
    BlockingDiagnostics = 1,
    Usage = 2,
    ProjectDiscovery = 3,
    CapabilityResolution = 4,
    ExternalToolFailure = 5,
    UnsafeOperationRefused = 6,
    Interrupted = 7,
    InternalError = 8
}
