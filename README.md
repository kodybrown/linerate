# linerate

`linerate` follows a log file or reads stdin and reports rolling average throughput.

## Usage

```powershell
linerate -f migration.log
linerate -f migration.log -filter "ERROR|WARN"
linerate -f migration.log -tail
linerate -f migration.log -tail -filter "ERROR|WARN"
linerate -f migration.log -from-start
linerate -f migration.log -window 30
linerate -f migration.log -filter "ERROR|WARN" -multiplier 3.5
some-long-running-process | linerate
some-long-running-process | linerate -tail
```

By default, `linerate` only repaints a single stats line:

```text
[     100.5 lines/sec ]
[     100.5 lines/sec /      85.1 matches/sec ]
[     100.5 lines/sec /      85.1 matches/sec =     297.9 ]
```

Use `-tail` to show tailed log output too:

```text
[     100.5 lines/sec ] original line
[     100.5 lines/sec /      85.1 matches/sec ] original line
```

`-f` follows a log file from the end by default, like `tail -f`. Use `-from-start` to read existing content first. `lines/sec` is total lines per second. `matches/sec` is matching lines per second when `-filter` is provided. Use `-multiplier [number]` with `-filter` to show `multiplier * matches/sec`. Rates are rolling averages over the last 10 seconds by default; use `-window [seconds]` to change that.
