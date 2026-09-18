namespace DbRouter.SampleApi.Models;

public sealed record DatabaseProbeResult(
    string Database,
    string Provider,
    string ConnectionType,
    bool Available,
    int? ProbeValue);
