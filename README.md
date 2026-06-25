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
linerate -f migration.log -filter 'Batching (\d+) items' -sum-matches
some-long-running-process | linerate
some-long-running-process | linerate -tail
```

By default, `linerate` only repaints a single stats line:

```text
[     100.5 lines/sec ]
[     100.5 lines/sec |      85.1 matched-lines/sec ]
[     100.5 lines/sec |      85.1 matched-lines/sec |     297.9 matches/sec ]
```

Use `-tail` to show tailed log output too:

```text
[     100.5 lines/sec ] original line
[     100.5 lines/sec |      85.1 matched-lines/sec ] original line
```

`-f` follows a log file from the end by default, like `tail -f`. Use `-from-start` to read existing content first. `lines/sec` is total lines per second. `matched-lines/sec` is matching lines per second when `-filter` is provided. Use `-multiplier [number]` with `-filter` to show `multiplier * matched-lines/sec` as `matches/sec`.

Use `-sum-matches` with a filter containing exactly one `(\d+)` capture group to sum captured numeric values into the rolling `matches/sec` rate. `-sum-matches` and `-multiplier` cannot be used together. Rates are rolling averages over the last 10 seconds by default; use `-window [seconds]` to change that.

Press Ctrl+C to quit while following a file.
