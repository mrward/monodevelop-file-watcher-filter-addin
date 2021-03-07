//
// FileWatcherFilterOptions.cs
//
// Author:
//       Matt Ward <matt.ward@microsoft.com>
//
// Copyright (c) 2021 Microsoft Corporation
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

using System.Collections.Generic;
using System.Linq;
using MonoDevelop.Core;

namespace MonoDevelop.FileWatcherFilter
{
	public class FileWatcherFilterOptions
	{
		static readonly string FileWatcherExcludeGlobsPropertyName = "FileWatcherExcludeGlobs";

		List<string> globPatterns = new List<string> ();

		public FileWatcherFilterOptions ()
		{
			var defaultGlobPatterns = new List<string> ();
			defaultGlobPatterns.Add ("**/.git/objects/**");
			defaultGlobPatterns.Add ("**/.git/subtree-cache/**");
			defaultGlobPatterns.Add ("**/node_modules/**");

			globPatterns = PropertyService.Get (FileWatcherExcludeGlobsPropertyName, defaultGlobPatterns);
		}

		public IEnumerable<string> GlobPatterns {
			get { return globPatterns; }
		}

		public bool UpdateGlobPatterns (IEnumerable<string> updatedGlobPatterns)
		{
			if (!GlobPatternsHaveChanged (updatedGlobPatterns))
				return false;

			globPatterns = updatedGlobPatterns.ToList ();
			Save ();

			return true;
		}

		bool GlobPatternsHaveChanged (IEnumerable<string> updatedGlobPatterns)
		{
			return !updatedGlobPatterns.SequenceEqual (globPatterns);
		}

		void Save ()
		{
			PropertyService.Set (FileWatcherExcludeGlobsPropertyName, globPatterns);
			PropertyService.SaveProperties ();
		}
	}
}
