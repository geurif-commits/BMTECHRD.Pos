using System;
using System.Collections.Generic;
using System.Linq;

public sealed class ScenarioResult
{
    public string Name { get; set; } = string.Empty;
    public bool Passed { get; set; }
    public string? Error { get; set; }
    public TimeSpan Duration { get; set; }
    public int RefreshCalls { get; set; }
    public int SessionExpiredEvents { get; set; }
}

public sealed class HarnessReport
{
    public List<ScenarioResult> Results { get; } = new();

    public bool AllPassed => Results.All(r => r.Passed);
    public int FailedCount => Results.Count(r => !r.Passed);

    // aggregated metrics
    public int TotalRefreshCalls { get; set; }
    public int SessionExpiredEvents { get; set; }

    public void AddResult(ScenarioResult r) => Results.Add(r);
}
