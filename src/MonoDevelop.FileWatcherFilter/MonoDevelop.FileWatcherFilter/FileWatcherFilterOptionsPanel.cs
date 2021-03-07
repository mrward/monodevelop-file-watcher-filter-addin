//
// FileWatcherFilterOptionsPanel.cs
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

using System;
using System.Collections.Generic;
using MonoDevelop.Components;
using MonoDevelop.Core;
using MonoDevelop.Ide.Gui.Dialogs;
using Xwt;

namespace MonoDevelop.FileWatcherFilter
{
	class FileWatcherFilterOptionsPanel : OptionsPanel
	{
		ListView globPatternsListView;
		ListStore globPatternsListStore;
		DataField<string> globPatternDataField;
		TextCellView textCell;
		Button addButton;
		Button removeButton;

		public override Control CreatePanelWidget ()
		{
			var vbox = new VBox ();
			vbox.Spacing = 6;

			var label = new Label ();
			label.Text = GettextCatalog.GetString ("Configure glob patterns of file paths to exclude from file watching. Patterns must match on absolute paths (i.e. prefix with ** or the full path to match properly).");
			label.Wrap = WrapMode.Word;
			vbox.PackStart (label);

			globPatternDataField = new DataField<string> ();
			globPatternsListStore = new ListStore (globPatternDataField);

			globPatternsListView = new ListView ();
			globPatternsListView.HeadersVisible = false;
			globPatternsListView.DataSource = globPatternsListStore;

			var column = new ListViewColumn ();
			textCell = new TextCellView (globPatternDataField);
			textCell.Editable = true;
			column.Views.Add (textCell);
			globPatternsListView.Columns.Add (column);

			vbox.PackStart (globPatternsListView, true, true);

			var buttonsHBox = new HBox ();
			vbox.PackStart (buttonsHBox);

			addButton = new Button ();
			addButton.Label = GettextCatalog.GetString ("Add Pattern");
			buttonsHBox.PackStart (addButton);

			removeButton = new Button ();
			removeButton.Label = GettextCatalog.GetString ("Remove Pattern");
			removeButton.Sensitive = false;
			buttonsHBox.PackStart (removeButton);

			foreach (string pattern in FileWatcherFilterService.Options.GlobPatterns) {
				int row = globPatternsListStore.AddRow ();
				globPatternsListStore.SetValue (row, globPatternDataField, pattern);
			}

			globPatternsListView.SelectionChanged += GlobPatternsListViewSelectionChanged;
			addButton.Clicked += AddButtonClicked;
			removeButton.Clicked += RemoveButtonClicked;

			return new XwtControl (vbox);
		}

		void RemoveButtonClicked (object sender, EventArgs e)
		{
			int row = globPatternsListView.SelectedRow;
			if (row >= 0) {
				globPatternsListStore.RemoveRow (row);
			}
		}

		void AddButtonClicked (object sender, EventArgs e)
		{
			int row = globPatternsListStore.AddRow ();
			globPatternsListStore.SetValue (row, globPatternDataField, "**/Folder/**");
			globPatternsListView.StartEditingCell (row, textCell);
		}

		void GlobPatternsListViewSelectionChanged (object sender, EventArgs e)
		{
			removeButton.Sensitive = globPatternsListView.SelectedRow >= 0;
		}

		public override void Dispose ()
		{
			globPatternsListView.SelectionChanged -= GlobPatternsListViewSelectionChanged;
			addButton.Clicked -= AddButtonClicked;
			removeButton.Clicked -= RemoveButtonClicked;

			base.Dispose ();
		}

		public override void ApplyChanges ()
		{
			List<string> updatedPatterns = GetGlobPatterns ();
			FileWatcherFilterService.UpdateGlobPatterns (updatedPatterns);
		}

		List<string> GetGlobPatterns ()
		{
			var updatedGlobPatterns = new List<string> ();

			for (int row = 0; row < globPatternsListStore.RowCount; row++) {
				string pattern = globPatternsListStore.GetValue (row, globPatternDataField);
				if (!string.IsNullOrWhiteSpace (pattern)) {
					updatedGlobPatterns.Add (pattern);
				}
			}

			return updatedGlobPatterns;
		}
	}
}
