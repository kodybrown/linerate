namespace LineRate;

using PowerCode;

internal sealed class AppOptions
{
  private const int COMMON = CliArgumentAttribute.DefaultGlobalOrder + 100;
  private const int FILE = CliArgumentAttribute.DefaultPropertyOrder + 100;
  private const int FILTER = CliArgumentAttribute.DefaultPropertyOrder + 200;
  private const int DISPLAY = CliArgumentAttribute.DefaultPropertyOrder + 300;

  // ── Common ───────────────────────────────────────────────────────────

  [CliArgument(
    namedParameters: ["help", "?", "h"],
    description: "Show help and usage information.",
    order: COMMON + 1,
    valueIsOptional: true,
    defaultIfNoValue: "",
    defaultIfMissing: null,
    allowEnvar: false
  )]
  public string? Help { get; set; }
  public bool HelpWasSet => Help != null;

  [CliArgument(
    namedParameters: ["about"],
    description: "Show full app details.",
    order: COMMON + 3,
    valueIsOptional: true,
    defaultIfNoValue: true,
    allowEnvar: false
  )]
  public bool About { get; set; }

  [CliArgument(
    namedParameters: ["v"],
    description: "Show version (short).",
    order: COMMON + 10,
    valueIsOptional: true,
    defaultIfNoValue: true,
    allowEnvar: false
  )]
  public bool Version { get; set; }

  [CliArgument(
    namedParameters: ["version"],
    description: "Show full version details.",
    order: COMMON + 11,
    valueIsOptional: true,
    defaultIfNoValue: true,
    allowEnvar: false
  )]
  public bool VersionFull { get; set; }

  [CliArgument(
    namedParameters: ["pause"],
    description: "Pause before exiting always or when there's an error.",
    order: COMMON + 21,
    valueIsOptional: true,
    defaultIfNoValue: Pause.Always,
    defaultIfMissing: Pause.Never,
    allowEnvar: true
  )]
  public Pause Pause { get; set; } = Pause.Never;

  [CliArgument(
    namedParameters: ["show-examples", "examples"],
    showInHelp: false,
    allowEnvar: false
  )]
  public bool ShowExamples { get; set; }

  [CliArgument(
    namedParameters: ["show-envars", "envars"],
    showInHelp: false,
    allowEnvar: false
  )]
  public bool ShowEnvars { get; set; }

  // ── File ─────────────────────────────────────────────────────────────

  [CliArgument(
    namedParameters: ["f", "file"],
    description: "Log file to follow. Starts at end of file like tail -f.",
    order: FILE + 1,
    defaultIfMissing: null,
    allowEnvar: false
  )]
  public string? File { get; set; }
  public bool FileWasSet => !string.IsNullOrWhiteSpace(File);

  [CliArgument(
    namedParameters: ["from-start"],
    description: "Reads an existing file from the beginning before following new lines.",
    order: FILE + 2,
    valueIsOptional: true,
    defaultIfNoValue: true,
    defaultIfMissing: false,
    allowEnvar: false
  )]
  public bool FromStart { get; set; }

  [CliArgument(
    namedParameters: ["tail"],
    description: "Shows tailed log lines instead of only repainting the stats line.",
    order: FILE + 3,
    valueIsOptional: true,
    defaultIfNoValue: true,
    defaultIfMissing: false,
    allowEnvar: false
  )]
  public bool Tail { get; set; }

  // ── Filter ───────────────────────────────────────────────────────────

  [CliArgument(
    namedParameters: ["filter", "match"],
    description: "Regex used to count matching lines separately.",
    order: FILTER + 1,
    defaultIfMissing: null,
    allowEnvar: true
  )]
  public string? Filter { get; set; }
  public bool FilterWasSet => !string.IsNullOrEmpty(Filter);

  [CliArgument(
    namedParameters: ["ignore-case", "i"],
    description: "Matches the filter regex without case sensitivity.",
    order: FILTER + 2,
    valueIsOptional: true,
    defaultIfNoValue: true,
    defaultIfMissing: false,
    allowEnvar: true
  )]
  public bool IgnoreCase { get; set; }

  [CliArgument(
    namedParameters: ["multiplier"],
    description: "Multiplies the matched-line rate and shows the result. Requires -filter.",
    order: FILTER + 3,
    allowEnvar: true
  )]
  public double Multiplier { get; set; } = double.NaN;
  public bool MultiplierWasSet { get; private set; }

  [CliArgument(
    namedParameters: ["sum-matches"],
    description: "Sums the numeric filter capture values and shows their rolling rate. Requires -filter and one (\\d+) capture group.",
    order: FILTER + 4,
    valueIsOptional: true,
    defaultIfNoValue: true,
    defaultIfMissing: false,
    allowEnvar: true
  )]
  public bool SumMatches { get; set; }
  public bool SumMatchesWasSet => SumMatches;

  // ── Display ──────────────────────────────────────────────────────────

  [CliArgument(
    namedParameters: ["window", "rolling-window"],
    description: "Seconds of recent output to average for rolling rates.",
    order: DISPLAY + 1,
    defaultIfMissing: 10,
    allowEnvar: true
  )]
  public int Window { get; set; } = 10;
}

