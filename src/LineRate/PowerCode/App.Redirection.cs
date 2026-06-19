/*!
	Copyright (C) 2008-2026 Kody Brown (kody@bricksoft.com).

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

namespace PowerCode;

using System;
using System.IO;

internal static partial class App
{
  /// <summary>
  /// Returns true when stdin is being piped or redirected (e.g. cat file | app).
  /// </summary>
  public static bool IsInputRedirected => Console.IsInputRedirected;

  /// <summary>
  /// Returns true when stdout is being piped or redirected (e.g. app > file).
  /// </summary>
  public static bool IsOutputRedirected => Console.IsOutputRedirected;

  /// <summary>
  /// Returns true when stderr is being piped or redirected.
  /// </summary>
  public static bool IsErrorRedirected => Console.IsErrorRedirected;

  /// <summary>
  /// Reads all text from stdin. Returns null if stdin is not redirected.
  /// </summary>
  public static string? ReadStdIn()
  {
    if (!Console.IsInputRedirected) {
      return null;
    }
    return Console.In.ReadToEnd();
  }

  /// <summary>
  /// Reads all text from stdin and writes it to a temporary file.
  /// The caller is responsible for deleting the file when done.
  /// Returns null if stdin is not redirected.
  /// </summary>
  public static string? ReadStdInToTempFile()
  {
    var content = ReadStdIn();
    if (content is null) {
      return null;
    }
    var tempFile = Path.GetTempFileName();
    File.WriteAllText(tempFile, content);
    return tempFile;
  }
}
