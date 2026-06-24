/*!
  Copyright (C) 2026 Kody Brown (kody@bricksoft.com).

  MIT License:

  Permission is hereby granted, free of charge, to any person obtaining a copy
  of this software and associated documentation files (the "Software"), to
  deal in the Software without restriction, including without limitation the
  rights to use, copy, modify, merge, publish, distribute, sublicense, and/or
  sell copies of the Software, and to permit persons to whom the Software is
  furnished to do so, subject to the following conditions:

  The above copyright notice and this permission notice shall be included in
  all copies or substantial portions of the Software.

  THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
  IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
  FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
  AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
  LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
  FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
  DEALINGS IN THE SOFTWARE.
*/

namespace LineRate;

using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Text.RegularExpressions;
using PowerCode;

public class Program
{
  public static async Task<int> Main( string[] arguments )
    => await new Program().Run(arguments);

  private readonly AppOptions _opt = new();

  public Program()
  {
    App.AppName = "linerate";
    App.AppDescription = @"A pipe-friendly console utility that prefixes stdin or followed log lines with line throughput.
Features:
- follows a log file with -f, starting at the end by default
- also supports stdin pipe input
- prefixes each line with rolling average line rate
- optionally prefixes each line with matching-line rate for a regex filter";
    App.AppProduct = "PowerTools";
    App.AppAuthors = "Kody Brown (@kodybrown)";
    App.AppCopyright = "Copyright (c) 2026 Kody Brown";
    App.AppEnvarPrefix = "linerate_";
    App.AppRepositoryUrl = "https://github.com/kodybrown/linerate";
  }

  public async Task<int> Run( string[] arguments )
  {
    var parse = App.ParseCommandLineArguments(arguments, _opt, allowEnvars: true);
    if (parse.ShouldExit) {
      return parse.ExitCode;
    }

    if (_opt.HelpWasSet || _opt.ShowExamples || _opt.ShowEnvars) {
      if (_opt.Help is "examples" or "show-examples" || _opt.ShowExamples) {
        ShowExamples();
      } else if (_opt.Help is "envars" or "show-envars" || _opt.ShowEnvars) {
        App.ShowEnvars(_opt, showHeader: true);
      } else {
        App.ShowUsage(_opt, _opt.Help);
      }
      App.PauseIfNeeded(_opt.Pause);
      return 0;
    }

    if (_opt.Window < 1) {
      Console.Error.WriteLine("**** Window must be at least 1 second. ****");
      App.PauseIfNeeded(_opt.Pause, exitCode: 1);
      return 2;
    }

    Regex? filter = null;
    if (_opt.FilterWasSet) {
      try {
        var options = RegexOptions.CultureInvariant | RegexOptions.Compiled;
        if (_opt.IgnoreCase) {
          options |= RegexOptions.IgnoreCase;
        }
        filter = new Regex(_opt.Filter!, options);
      } catch (ArgumentException ex) {
        Console.Error.WriteLine($"**** Invalid filter regex: {ex.Message} ****");
        App.PauseIfNeeded(_opt.Pause, exitCode: 1);
        return 3;
      }
    }

    if (_opt.MultiplierWasSet) {
      if (filter is null) {
        Console.Error.WriteLine("**** Multiplier requires -filter. ****");
        App.PauseIfNeeded(_opt.Pause, exitCode: 1);
        return 4;
      }

      if (!double.IsFinite(_opt.Multiplier)) {
        Console.Error.WriteLine("**** Multiplier must be a finite number. ****");
        App.PauseIfNeeded(_opt.Pause, exitCode: 1);
        return 4;
      }
    }

    if (!_opt.FileWasSet && !App.IsInputRedirected) {
      Console.Error.WriteLine("**** Missing input. Pipe stdin or specify -f <logfile>. ****");
      App.ShowHelpSuggestion();
      App.PauseIfNeeded(_opt.Pause, exitCode: 1);
      return 1;
    }

    var stats = new LineStats(TimeSpan.FromSeconds(_opt.Window));
    var showMatching = filter is not null;
    var multiplier = _opt.MultiplierWasSet ? _opt.Multiplier : (double?)null;

    try {
      using var output = new StreamWriter(Console.OpenStandardOutput(), new UTF8Encoding(encoderShouldEmitUTF8Identifier: false), bufferSize: 1024 * 1024) {
        NewLine = Environment.NewLine
      };
      var context = new OutputContext(output, stats, filter, showMatching, multiplier, _opt.Tail);

      if (_opt.FileWasSet) {
        await FollowFile(_opt.File!, _opt.FromStart, context).ConfigureAwait(false);
      } else {
        ReadStandardInput(context);
      }

      output.Flush();
    } catch (IOException) {
      // Downstream closed the pipe. Exit quietly like standard pipe filters do.
      return 0;
    }

    App.PauseIfNeeded(_opt.Pause);
    return 0;
  }

  private static void ReadStandardInput( OutputContext context )
  {
    using var input = new StreamReader(Console.OpenStandardInput(), Encoding.UTF8, detectEncodingFromByteOrderMarks: true, bufferSize: 1024 * 1024);
    string? line;
    while ((line = input.ReadLine()) is not null) {
      context.WriteLine(line);
    }
    context.Flush(force: true);
  }

  private static async Task FollowFile( string file, bool fromStart, OutputContext context )
  {
    var path = Path.GetFullPath(file);
    var readFromStart = fromStart;

    while (true) {
      await WaitForFile(path).ConfigureAwait(false);
      var lastKnownLength = 0L;

      using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete, bufferSize: 1024 * 1024, FileOptions.SequentialScan);
      var fileState = GetFileState(stream);
      if (!readFromStart) {
        stream.Seek(0, SeekOrigin.End);
      }
      readFromStart = false;
      lastKnownLength = stream.Length;

      using var reader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true, bufferSize: 1024 * 1024, leaveOpen: true);

      while (true) {
        var line = reader.ReadLine();
        if (line is not null) {
          context.WriteLine(line);
          lastKnownLength = stream.Length;
          continue;
        }

        context.Flush(force: false);
        await Task.Delay(250).ConfigureAwait(false);

        var currentState = TryGetFileState(path);
        if (currentState is null || currentState.Value != fileState) {
          readFromStart = true;
          break;
        }

        var currentLength = stream.Length;
        if (currentLength < stream.Position || currentLength < lastKnownLength) {
          reader.DiscardBufferedData();
          stream.Seek(0, SeekOrigin.Begin);
        }
        lastKnownLength = currentLength;
      }
    }
  }

  private static async Task WaitForFile( string path )
  {
    while (true) {
      var state = TryGetFileState(path);
      if (state is not null) {
        return;
      }

      await Task.Delay(250).ConfigureAwait(false);
    }
  }

  private static FileState? TryGetFileState( string path )
  {
    try {
      using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete, bufferSize: 1, FileOptions.SequentialScan);
      return GetFileState(stream);
    } catch (FileNotFoundException) {
      return null;
    } catch (DirectoryNotFoundException) {
      return null;
    } catch (IOException) {
      return null;
    } catch (UnauthorizedAccessException) {
      return null;
    }
  }

  private static FileState GetFileState( FileStream stream )
  {
    if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) && GetFileInformationByHandle(stream.SafeFileHandle, out var info)) {
      return new FileState(info.VolumeSerialNumber, info.FileIndexHigh, info.FileIndexLow);
    }

    return new FileState(0, 0, (uint)File.GetCreationTimeUtc(stream.Name).Ticks);
  }

  [DllImport("kernel32.dll", SetLastError = true)]
  private static extern bool GetFileInformationByHandle( SafeHandle fileHandle, out ByHandleFileInformation fileInformation );


  private void ShowExamples()
  {
    App.ShowHeader(includeDescription: false, includeBuildInfo: false);

    Console.WriteLine(@"EXAMPLES:
---------

> linerate -f migration.log
  Follows migration.log from the end and repaints one live stats line.

> linerate -f migration.log -tail
  Shows tailed log lines with prefixed rolling line rates.

> linerate -f migration.log -from-start
  Reads migration.log from the beginning, then follows new lines.

> linerate -f migration.log -filter ""ERROR|WARN""
  Follows a log file and repaints total and matching line rates.

> some-process | linerate -tail
  Prefixes piped stdin lines with the rolling line rate.

> linerate -f migration.log -window 30
  Uses a 30-second rolling average instead of the default 10-second window.
");
  }
}

[StructLayout(LayoutKind.Sequential)]
internal struct FileTime
{
  public uint LowDateTime;
  public uint HighDateTime;
}

[StructLayout(LayoutKind.Sequential)]
internal struct ByHandleFileInformation
{
  public uint FileAttributes;
  public FileTime CreationTime;
  public FileTime LastAccessTime;
  public FileTime LastWriteTime;
  public uint VolumeSerialNumber;
  public uint FileSizeHigh;
  public uint FileSizeLow;
  public uint NumberOfLinks;
  public uint FileIndexHigh;
  public uint FileIndexLow;
}

internal readonly record struct FileState( uint VolumeSerialNumber, uint FileIndexHigh, uint FileIndexLow );

internal sealed class OutputContext
{
  private readonly Regex? _filter;
  private readonly bool _showMatching;
  private readonly LineStats _stats;
  private readonly double? _multiplier;
  private readonly bool _tail;
  private readonly StreamWriter _writer;
  private int _lastStatusLength;
  private int _linesSinceFlush;
  private long _lastFlush = Stopwatch.GetTimestamp();
  private long _lastStatus = 0;

  public OutputContext( StreamWriter writer, LineStats stats, Regex? filter, bool showMatching, double? multiplier, bool tail )
  {
    _writer = writer;
    _stats = stats;
    _filter = filter;
    _showMatching = showMatching;
    _multiplier = multiplier;
    _tail = tail;
  }

  public void Flush( bool force )
  {
    if (!_tail) {
      WriteStatus(force);
      return;
    }

    if (!force && _linesSinceFlush == 0) {
      return;
    }

    if (force || Stopwatch.GetElapsedTime(_lastFlush).TotalMilliseconds >= 250) {
      _writer.Flush();
      _linesSinceFlush = 0;
      _lastFlush = Stopwatch.GetTimestamp();
    }
  }

  public void WriteLine( string line )
  {
    var isMatch = _filter?.IsMatch(line) == true;
    _stats.Record(isMatch);

    if (!_tail) {
      WriteStatus(force: false);
      return;
    }

    _writer.Write(FormatPrefix(_stats, _showMatching, _multiplier));
    _writer.WriteLine(line);

    _linesSinceFlush++;
    if (_linesSinceFlush >= 256 || Stopwatch.GetElapsedTime(_lastFlush).TotalMilliseconds >= 250) {
      Flush(force: true);
    }
  }

  private void WriteStatus( bool force )
  {
    var now = Stopwatch.GetTimestamp();
    if (!force && _lastStatus != 0 && Stopwatch.GetElapsedTime(_lastStatus).TotalMilliseconds < 250) {
      return;
    }

    var status = FormatStatus(_stats, _showMatching, _multiplier);
    var padding = Math.Max(0, _lastStatusLength - status.Length);

    _writer.Write('\r');
    _writer.Write(status);
    if (padding > 0) {
      _writer.Write(new string(' ', padding));
    }
    _writer.Flush();

    _lastStatusLength = status.Length;
    _lastStatus = now;
  }

  private static string FormatPrefix( LineStats stats, bool showMatching, double? multiplier )
    => FormatStatus(stats, showMatching, multiplier) + " ";

  private static string FormatStatus( LineStats stats, bool showMatching, double? multiplier )
  {
    var lineRate = stats.LineRate;

    if (showMatching) {
      var matchingRate = stats.MatchingRate;
      if (multiplier is not null) {
        var multipliedRate = matchingRate * multiplier.Value;
        return $"[ {lineRate,7:0.0} lines/sec / {matchingRate,7:0.0} matches/sec = {multipliedRate,7:0.0} ]";
      }

      return $"[ {lineRate,7:0.0} lines/sec / {matchingRate,7:0.0} matches/sec ]";
    }

    return $"[ {lineRate,7:0.0} lines/sec ]";
  }

}

internal sealed class LineStats
{
  private readonly Stopwatch _elapsed = Stopwatch.StartNew();
  private readonly Queue<long> _matchingLineTicks = [];
  private readonly Queue<long> _totalLineTicks = [];
  private readonly long _windowTicks;

  public LineStats( TimeSpan window )
  {
    _windowTicks = Math.Max(Stopwatch.Frequency, (long)(window.TotalSeconds * Stopwatch.Frequency));
  }

  public double LineRate {
    get {
      var now = _elapsed.ElapsedTicks;
      Prune(now);
      return GetRate(_totalLineTicks.Count, now);
    }
  }

  public double MatchingRate {
    get {
      var now = _elapsed.ElapsedTicks;
      Prune(now);
      return GetRate(_matchingLineTicks.Count, now);
    }
  }

  public void Record( bool isMatch )
  {
    var now = _elapsed.ElapsedTicks;
    _totalLineTicks.Enqueue(now);
    if (isMatch) {
      _matchingLineTicks.Enqueue(now);
    }
    Prune(now);
  }

  private double GetRate( int count, long now )
  {
    var elapsedTicks = Math.Max(1, now);
    var denominatorTicks = Math.Min(_windowTicks, elapsedTicks);
    var denominatorSeconds = Math.Max(0.001, denominatorTicks / (double)Stopwatch.Frequency);
    return count / denominatorSeconds;
  }

  private void Prune( long now )
  {
    var cutoff = now - _windowTicks;
    while (_totalLineTicks.Count > 0 && _totalLineTicks.Peek() < cutoff) {
      _totalLineTicks.Dequeue();
    }
    while (_matchingLineTicks.Count > 0 && _matchingLineTicks.Peek() < cutoff) {
      _matchingLineTicks.Dequeue();
    }
  }
}


