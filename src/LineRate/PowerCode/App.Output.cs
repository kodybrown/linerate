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

internal static partial class App
{
	public static void PauseIfNeeded( Pause pause, int exitCode = 0, string? message = null )
	{
		if (pause == Pause.Always || (pause == Pause.IfError && exitCode != 0)) {
			Console.Write(message ?? "Press any key to continue: ");
			Console.ReadKey(intercept: true);
			Console.WriteLine();
		}
	}

	public static void WriteDebug( string text )
	{
		if (AppDebug != Verbosity.None) {
			Console.WriteLine(text);
		}
	}

	public static void WriteDebug( Verbosity debug, string text )
	{
		if ((int)AppDebug >= (int)debug) {
			Console.WriteLine(text);
		}
	}

	public static void WriteDebug( Verbosity debug, string format, params object[] p )
	{
		if ((int)AppDebug >= (int)debug) {
			Console.WriteLine(string.Format(format, p));
		}
	}
}
