using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompareDirs {
	class TreeRoot {
		//members
		private FileTree root;
		//properties
		public string Name { get { return root.Name; } }
		public FileTree Root { get { return root; } }
		//constructors
		internal TreeRoot(string rootFile) : this(rootFile, int.MaxValue) { }
		internal TreeRoot(string rootFile, int depth) {
			root = new FileTree(Directory.GetFileSystemEntries(rootFile), new FileInfo(rootFile).Name, 0, depth);
		}
		internal TreeRoot(TreeRoot copyFrom) {
			root = new FileTree(copyFrom.root);
		}
		//methods
		public void CompareWith(TreeRoot otherTree) {
			root.CompareLevels(otherTree.root);
		}
		public void Print() {
			root.Print("");
		}
	}
	class FileTree : Traversable{
		//members
		private List<Traversable> children;
		public List<Traversable> Children { get { return children; } }
		//constructors
		internal FileTree(string[] fileNames, string folderName, int currentDepth, int maxDepth) : base(folderName) {
			children = new List<Traversable>();
			for(int i = 0; i < fileNames.Length; i++) {
				FileInfo fi = new FileInfo(fileNames[i]);
				if(currentDepth < maxDepth && fi.Attributes.HasFlag(FileAttributes.Directory)) {
					children.Add(new FileTree(Directory.GetFileSystemEntries(fileNames[i]), fi.Name, currentDepth + 1, maxDepth));
				}
				else {
					children.Add(new TreeItem(fi.Name));
				}
			}
		}
		internal FileTree(FileTree copyFrom) : base(copyFrom.Name){
			this.children = new List<Traversable>();
			for(int i = 0; i < copyFrom.children.Count; i++) {
				FileTree fi = copyFrom.children[i] as FileTree;
				if(fi == null) {
					children.Add(new TreeItem(copyFrom.children[i] as TreeItem));
				}
				else {
					children.Add(new FileTree(fi));
				}
			}
		}
		//methods
		public void CompareLevels(FileTree otherRoot) {
			foreach(Traversable t in children) {
				Traversable match = FindMatch(t, otherRoot);
				t.Change = match != null ? Difference.SAME : Difference.NEW;
				if (match != null) {
					match.Change = Difference.SAME;
				}
				if (t is FileTree) {
					FileTree fi = t as FileTree;
					if (fi.Change.Equals(Difference.SAME)) {
						fi.CompareLevels(match as FileTree);
					}
					else {
						fi.SetChildrenAs(Difference.NEW);
					}
				}
			}
			foreach(Traversable t in otherRoot.children) {
				if (t.Change.Equals(Difference.UNKNOWN)) {
					if(t is TreeItem) {
						TreeItem ti = new TreeItem(t as TreeItem);
						ti.Change = Difference.MISSING;
						Change = Difference.MISSING;
						this.children.Add(ti);
					}
					else {
						FileTree ft = new FileTree(t as FileTree);
						ft.Change = Difference.MISSING;
						Change = Difference.MISSING;
						ft.SetChildrenAs(Difference.MISSING);
						this.children.Add(ft);
					}
				}
			}
			this.SortChildren();
		}
		private void SortChildren() {
			children.Sort(delegate (Traversable t1, Traversable t2) {
				bool isTree1 = t1 is FileTree;
				bool isTree2 = t2 is FileTree;
				if(isTree1 == isTree2) {
					int i = t1.Name.CompareTo(t2.Name);
					if (i != 0)
						return i;
				}
				if (isTree1) {
					return -1;
				}
				else {
					return 1;
				}
			});
		}
		private void SetChildrenAs(Difference d) {
			foreach (Traversable t in children) {
				t.Change = d;
				if(t is FileTree) {
					(t as FileTree).SetChildrenAs(d);
				}
			}
		}
		private Traversable FindMatch(Traversable target, FileTree other) {
			Traversable found = null;
			for(int i = 0; i < other.children.Count && found == null; i++) {
				Traversable t = other.children[i];
				if (t.Compare(target)) {
					found = t;
				}
			}
			return found;
		}
		public override void Print(string tabs) {
			Console.WriteLine(tabs + Name + " " + Change);
			tabs += "\t";
			foreach (Traversable t in children) {
				t.Print(tabs);
			}
		}
	}
	class TreeItem : Traversable {
		//members
		//constructors
		internal TreeItem(string fileName) : base(fileName) {
		}
		internal TreeItem(TreeItem copyFrom) : base(copyFrom.Name){
		}
		//methods
		public override void Print(string tabs) {
			Console.WriteLine(tabs + Name + " " + Change);
		}
	}
	enum Difference { UNKNOWN, SAME, NEW, MISSING }
	abstract class Traversable {
		//members
		private string name;
		private Difference diff;
		//properties
		public string Name { get { return name; } }
		public Difference Change { get { return diff; } set { diff = value; } }
		//constructors
		internal Traversable(string name) {
			this.name = name;
			diff = Difference.UNKNOWN;
		}
		//methods
		public abstract void Print(string tabs);
		public bool Compare(Traversable t) {
			return this.name.Equals(t.name);
		}
	}
}
