using Microsoft.Win32;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using Ookii.Dialogs.Wpf;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.IO;

namespace CompareDirs { 
	public partial class MainWindow : Window {
		//constants
		private const int MAX_REMEMBERED_PATHS = 10;
		//members
		private Properties.Settings settings;
		private VistaFolderBrowserDialog chooserDialog;
		//constructors
		public MainWindow() {
			InitializeComponent();
			settings = Properties.Settings.Default;
			chooserDialog = new VistaFolderBrowserDialog();
			chooserDialog.ShowNewFolderButton = false;
			Console.WriteLine(settings.visitedQueue1 == null);
			Console.WriteLine(settings.visitedQueue2 == null);
			if (settings.visitedQueue1 == null) {
				settings.visitedQueue1 = new StringCollection();
			}
			if(settings.visitedQueue2 == null) {
				settings.visitedQueue2 = new StringCollection();
			}

			UpdateComboBox(fileSelector1, settings.visitedQueue1, 
				Properties.Resources.SelectBaseMessage, 0);
			UpdateComboBox(fileSelector2, settings.visitedQueue2, 
				Properties.Resources.SelectOtherMessage, 0);
        }
		//methods
		//event handlers
		private void fileBrowseButton1_Click(object sender, RoutedEventArgs e) {
			string path = OpenDialog();
			if(path != null) {
				PushTo(settings.visitedQueue1, path);
				UpdateComboBox(fileSelector1, settings.visitedQueue1, 
					Properties.Resources.SelectBaseMessage, 1);
			}
		}

		private void fileBrowseButton2_Click(object sender, RoutedEventArgs e) {
			string path = OpenDialog();
			if (path != null) {
				PushTo(settings.visitedQueue2, path);
				UpdateComboBox(fileSelector2, settings.visitedQueue2, 
					Properties.Resources.SelectOtherMessage, 1);
			}
		}
		private void compareGoButton_Click(object sender, RoutedEventArgs e) {
			string path1 = fileSelector1.SelectedItem as string;
			string path2 = fileSelector2.SelectedItem as string;
            if (!path1.Equals(Properties.Resources.SelectBaseMessage) &&
				!path2.Equals(Properties.Resources.SelectOtherMessage)) {
				BranchNode tree1 = BuildTree(path1), tree2 = BuildTree(path2);
				BranchNode merged = tree1.MergeTrees(tree2) as BranchNode;
				DisplayTreeOn(tree1, baseTreeView);
				DisplayTreeOn(tree2, otherTreeView);
				DisplayTreeOn(merged, diffTreeView);
			}
		}
		//non-handlers
		private void PushTo(StringCollection col, string path) {
			while (col.Count >= MAX_REMEMBERED_PATHS)
				col.RemoveAt(col.Count - 1);
			col.Insert(0, path);
			settings.Save();
		}
		public void UpdateComboBox(ComboBox cb, StringCollection queue, string defValue, int selection) {
			cb.Items.Clear();
			cb.Items.Add(defValue);
			foreach(string s in queue)
				cb.Items.Add(s);
			cb.SelectedIndex = selection;
		}
		public string OpenDialog() {
			bool? res = chooserDialog.ShowDialog();
			return res.HasValue && res.Value ? chooserDialog.SelectedPath : null;
		}
		public BranchNode BuildTree(string path) {
			DirectoryInfo dirInfo = new DirectoryInfo(path);
			FileSystemInfo[] fileInfos = dirInfo.GetFileSystemInfos();
			Array.Sort(fileInfos, delegate(FileSystemInfo a, FileSystemInfo b) {
				bool aDir = (a.Attributes & FileAttributes.Directory) == FileAttributes.Directory;
				bool bDir = (b.Attributes & FileAttributes.Directory) == FileAttributes.Directory;
				int dirDiff = aDir && !bDir ? -1 : (!aDir && bDir) ? 1 : 0;
				if (dirDiff != 0) return dirDiff;
				else return a.Name.CompareTo(b.Name);
			});
			List<Node> children = new List<Node>();
			foreach(FileSystemInfo fsi in fileInfos) {
				if((fsi.Attributes & FileAttributes.Directory) == FileAttributes.Directory) {
					children.Add(BuildTree(fsi.FullName));
				}
				else {
					children.Add(new LeafNode(fsi.Name, ChangeState.SAME));
				}
			}
			return new BranchNode(path.Substring(path.LastIndexOf('\\') + 1), ChangeState.SAME, children);
		}
		public void DisplayTreeOn(BranchNode tree, TreeView visTree) {
			visTree.Items.Clear();
			visTree.Items.Add(NodeToVisual(tree));
		}
		public TreeViewItem NodeToVisual(Node n) {
			TreeViewItem baseNode = CreateVisualNode(n);
			if (n is BranchNode) {
				foreach (Node c in (n as BranchNode).Children) {
					baseNode.Items.Add(NodeToVisual(c));
				}
				
			}
			return baseNode;
		}
		public TreeViewItem CreateVisualNode(Node n) {
			StackPanel p = new StackPanel();
			p.Orientation = Orientation.Horizontal;
			p.Children.Add(CreateColoredRect(n.State));

			Label l = new Label();
			l.Content = n.Name;
			p.Children.Add(l);
			TreeViewItem tvi = new TreeViewItem();
			tvi.Header = p;
			return tvi;
		}
		public Rectangle CreateColoredRect(ChangeState state) {
			Rectangle r = new Rectangle();
			double pixSize = (double)new LengthConverter().ConvertFrom("10px");
			r.HorizontalAlignment = HorizontalAlignment.Left;
			r.Height = pixSize;
			r.Width = pixSize;
			switch (state) {
				case ChangeState.SAME: r.Fill = Brushes.Bisque; break;
				case ChangeState.ADDED: r.Fill = Brushes.Green; break;
				case ChangeState.DELETED: r.Fill = Brushes.Red; break;
				case ChangeState.MIXED: r.Fill = Brushes.DodgerBlue; break;
				default: r.Fill = Brushes.White; break;
			}
			return r;
		}
		/*
		private void ConvertToVisualTree(System.Windows.Controls.TreeView visTree, TreeRoot root) {
			visTree.Items.Clear();
			TreeViewItem visRoot = CreateTreeViewItem(root.Change, root.Name);
			visTree.Items.Add(visRoot);
			foreach(Traversable t in root.Root.Children) {
				AddChildrenToVisualTree(visRoot, t);
			}
		}
		private void AddChildrenToVisualTree(TreeViewItem parent, Traversable level) {
			TreeViewItem vis = CreateTreeViewItem(level.Change, level.Name);
			parent.Items.Add(vis);
			if(level is FileTree) {
				foreach (Traversable t in (level as FileTree).Children) {
					AddChildrenToVisualTree(vis, t);
				}
			}
		}
		private TreeViewItem CreateTreeViewItem(Difference diff, string text) {
			StackPanel p = new StackPanel();
			p.Orientation = System.Windows.Controls.Orientation.Horizontal;
			Rectangle r = new Rectangle();
			r.HorizontalAlignment = System.Windows.HorizontalAlignment.Left;
			double pixSize = (double)new LengthConverter().ConvertFrom("10px");
			r.Height = pixSize;
			r.Width = pixSize;
			switch (diff) {
				case Difference.SAME: r.Fill = Brushes.Bisque; break;
				case Difference.NEW: r.Fill = Brushes.Green; break;
				case Difference.MISSING: r.Fill = Brushes.Red; break;
				case Difference.BOTH: r.Fill = Brushes.DodgerBlue; break;
				default: r.Fill = Brushes.White; break;
			}
			p.Children.Add(r);

			System.Windows.Controls.Label l = new System.Windows.Controls.Label();
			l.Content = text;
			p.Children.Add(l);
			TreeViewItem tvi = new TreeViewItem();
			tvi.Header = p;
			return tvi;
		}
		*/
	}
}
