//
// RegexGenerator.cs
//
// Author:
//       Lluis Sanchez Gual <lluis@xamarin.com>
//
// Copyright (c) 2015 Xamarin, Inc (http://www.xamarin.com)
//
// Based on DefaultMSBuildEngine's ExcludeToRegex
//
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
//
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;
using System.Collections.Immutable;
using System.Linq;
using System.Text.RegularExpressions;
using MonoDevelop.Core.PooledObjects;

namespace MonoDevelop.Core
{
	static class RegexGenerator
	{
		public static Regex CreateRegex (string exclude)
		{
			if (exclude.EndsWith ("/", StringComparison.Ordinal)) {
				// Treat directory path as excluding all subdirectories and files.
				exclude += "**";
			}
			string regexPattern = ExcludeToRegex (exclude);
			return new Regex (regexPattern, RegexOptions.Compiled);
		}

		static string ExcludeToRegex (string exclude, bool excludeDirectoriesOnly = false)
		{
			exclude = exclude.Replace ('\\', '/');
			using var sb = PooledStringBuilder.GetInstance ();
			foreach (var ep in exclude.Split (new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries)) {
				var ex = ep.AsSpan ().Trim ();
				if (excludeDirectoriesOnly) {
					if (ex.EndsWith (@"/**".AsSpan (), StringComparison.OrdinalIgnoreCase))
						ex = ex.Slice (0, ex.Length - 3);
					else
						continue;
				}
				if (sb.Length > 0)
					sb.Append ('|');
				sb.Append ('^');
				for (int n = 0; n < ex.Length; n++) {
					var c = ex[n];
					if (c == '*') {
						if (n < ex.Length - 1 && ex[n + 1] == '*') {
							if (n < ex.Length - 2 && ex[n + 2] == '/') {
								// zero or more subdirectories
								sb.Append ("(.*/)?");
								n += 2;
							} else {
								sb.Append (".*");
								n++;
							}
						} else
							sb.Append ("[^/]*");
					} else if (regexEscapeChars.Contains (c)) {
						sb.Append ('\\').Append (c);
					} else
						sb.Append (c);
				}
				sb.Append ('$');
			}
			return sb;
		}

		static char[] regexEscapeChars = { '\\', '^', '$', '{', '}', '[', ']', '(', ')', '.', '*', '+', '?', '|', '<', '>', '-', '&' };
	}
}
