﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Shell;
using Xunit;
using Xunit.Abstractions;

namespace Clide.Interop
{
	[Collection ("OpenSolution11")]
	[Trait ("Feature", "Interop")]
	[Trait ("Class", "VsSolutionSelectionSpec")]
	public class VsSolutionSelectionSpec
	{
		ISolutionFixture fixture;
		ISolutionExplorer explorer;
		IVsSolutionSelection selection;

		public VsSolutionSelectionSpec (OpenSolution11Fixture fixture)
		{
			this.fixture = fixture;
			selection = GlobalServices.Instance.GetServiceLocator ().GetExport<IVsSolutionSelection> ();
			explorer = GlobalServices.Instance.GetServiceLocator ().GetExport<ISolutionExplorer> ();
		}

		[VsixFact]
		public void when_retrieving_active_hierarchy_then_succeeds ()
		{
			var library = fixture.Solution.FindProject(x => x.Text == "CsLibrary");
			Assert.NotNull (library);

			library.Select ();

			Assert.Equal (library, explorer.Solution.ActiveProject);
		}

		[VsixFact]
		public void when_selection_is_item_then_active_hierarchy_is_owning_project ()
		{
			var library = fixture.Solution.FindProject(x => x.Text == "CsLibrary");
			Assert.NotNull (library);

			var item = library.Nodes.OfType<IItemNode> ().FirstOrDefault ();
			Assert.NotNull (item);

			item.Select ();

			var active = explorer.Solution.ActiveProject;
			Assert.Equal (library, active);

			var selected = selection.GetSelection ().FirstOrDefault ();

			Assert.Equal (selected.HierarchyIdentity, item.As<IVsHierarchyItem> ().HierarchyIdentity);
		}

		[VsixFact]
		public void when_selection_is_multiple_items_of_same_hierarchy_then_active_hierarchy_is_owning_project ()
		{
			var library = fixture.Solution.FindProject(x => x.Text == "CsLibrary");
			Assert.NotNull (library);
			library.Select (false);

			library.Nodes.OfType<IItemNode> ().First ().Select (false);
			library.Nodes.OfType<IFolderNode> ().First ().Nodes.OfType<IItemNode> ().First ().Select (true);

			var selected = selection.GetSelection ().ToList();
			Assert.Equal (2, selected.Count);

			var active = explorer.Solution.ActiveProject;

			Assert.Equal (library, active);
		}

		[VsixFact]
		public void when_selection_is_multiple_projects_then_active_hierarchy_is_null ()
		{
			fixture.Solution.FindProject (x => x.Text == "CsLibrary").Select (false);
			fixture.Solution.FindProject (x => x.Text == "VbLibrary").Select (true);

			var selected = selection.GetSelection ().ToList();
			Assert.Equal (2, selected.Count);

			var active = explorer.Solution.ActiveProject;

			Assert.Null (active);
		}

		[VsixFact]
		public void when_selection_contains_multiple_items_from_different_hierarchies_then_active_hierarchy_is_null ()
		{
			fixture.Solution.FindProject (x => x.Text == "CsLibrary").Nodes.OfType<IItemNode> ().First ().Select (false);
			fixture.Solution.FindProject (x => x.Text == "VbLibrary").Nodes.OfType<IItemNode> ().First ().Select (true);

			var selected = selection.GetSelection ().ToList();
			Assert.Equal (2, selected.Count);

			var active = explorer.Solution.ActiveProject;

			Assert.Null (active);
		}

		[VsixFact]
		public void when_selecting_items_then_returns_items ()
		{
			fixture.Solution.FindProject (x => x.Text == "CsLibrary").Nodes.OfType<IItemNode> ().First ().Select (false);
			fixture.Solution.FindProject (x => x.Text == "VbLibrary").Nodes.OfType<IItemNode> ().First ().Select (true);

			var selected = selection.GetSelection ().ToList();
			Assert.Equal (2, selected.Count);
		}
	}
}
