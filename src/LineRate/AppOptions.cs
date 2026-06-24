namespace LineRate;

using PowerCode;

internal sealed class AppOptions
{
  private const int COMMON = AppArgumentAttribute.DefaultGlobalOrder + 100;
  private const int FILE = AppArgumentAttribute.DefaultPropertyOrder + 100;
  private const int FILTER = AppArgumentAttribute.DefaultPropertyOrder + 200;
  private const int DISPLAY = AppArgumentAttribute.DefaultPropertyOrder + 300;

  // ── Common ───────────────────────────────────────────────────────────

  [AppArgument(
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

  [AppArgument(
    namedParameters: ["about"],
    description: "Show full app details.",
    order: COMMON + 3,
    valueIsOptional: true,
    defaultIfNoValue: true,
    allowEnvar: false
  )]
  public bool About { get; set; }

  [AppArgument(
    namedParameters: ["v"],
    description: "Show version (short).",
    order: COMMON + 10,
    valueIsOptional: true,
    defaultIfNoValue: true,
    allowEnvar: false
  )]
  public bool Version { get; set; }

  [AppArgument(
    namedParameters: ["version"],
    description: "Show full version details.",
    order: COMMON + 11,
    valueIsOptional: true,
    defaultIfNoValue: true,
    allowEnvar: false
  )]
  public bool VersionFull { get; set; }

  [AppArgument(
    namedParameters: ["pause"],
    description: "Pause before exiting always or when there's an error.",
    order: COMMON + 21,
    valueIsOptional: true,
    defaultIfNoValue: Pause.Always,
    defaultIfMissing: Pause.Never,
    allowEnvar: true
  )]
  public Pause Pause { get; set; } = Pause.Never;

  [AppArgument(
    namedParameters: ["show-examples", "examples"],
    showInHelp: false,
    allowEnvar: false
  )]
  public bool ShowExamples { get; set; }

  [AppArgument(
    namedParameters: ["show-envars", "envars"],
    showInHelp: false,
    allowEnvar: false
  )]
  public bool ShowEnvars { get; set; }

  // ── File ─────────────────────────────────────────────────────────────

  [AppArgument(
    namedParameters: ["f", "file"],
    description: "Log file to follow. Starts at end of file like tail -f.",
    order: FILE + 1,
    defaultIfMissing: null,
    allowEnvar: false
  )]
  public string? File { get; set; }
  public bool FileWasSet => !string.IsNullOrWhiteSpace(File);

  [AppArgument(
    namedParameters: ["from-start"],
    description: "Reads an existing file from the beginning before following new lines.",
    order: FILE + 2,
    valueIsOptional: true,
    defaultIfNoValue: true,
    defaultIfMissing: false,
    allowEnvar: false
  )]
  public bool FromStart { get; set; }

  [AppArgument(
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

  [AppArgument(
    namedParameters: ["filter", "match"],
    description: "Regex used to count matching lines separately.",
    order: FILTER + 1,
    defaultIfMissing: null,
    allowEnvar: true
  )]
  public string? Filter { get; set; }
  public bool FilterWasSet => !string.IsNullOrEmpty(Filter);

  [AppArgument(
    namedParameters: ["ignore-case", "i"],
    description: "Matches the filter regex without case sensitivity.",
    order: FILTER + 2,
    valueIsOptional: true,
    defaultIfNoValue: true,
    defaultIfMissing: false,
    allowEnvar: true
  )]
  public bool IgnoreCase { get; set; }

  [AppArgument(
    namedParameters: ["multiplier"],
    description: "Multiplies the matching-line rate and shows the result. Requires -filter.",
    order: FILTER + 3,
    allowEnvar: true
  )]
  public double Multiplier { get; set; } = double.NaN;
  public bool MultiplierWasSet => App.CommandLineArguments.Any(IsMultiplierArgument) || Environment.GetEnvironmentVariable("linerate_multiplier") is not null;

  private static bool IsMultiplierArgument( string argument )
  {
    var normalized = argument.TrimStart('-', '/', '!');
    return normalized.Equals("multiplier", StringComparison.InvariantCultureIgnoreCase);
  }

  // ── Display ──────────────────────────────────────────────────────────

  [AppArgument(
    namedParameters: ["window", "rolling-window"],
    description: "Seconds of recent output to average for lines/sec and matches/sec rates.",
    order: DISPLAY + 1,
    defaultIfMissing: 10,
    allowEnvar: true
  )]
  public int Window { get; set; } = 10;
}

