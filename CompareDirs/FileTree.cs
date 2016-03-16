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
		private int depth;
		//constructors
		internal TreeRoot(string rootFile) : this(rootFile, int.MaxValue) { }
		internal TreeRoot(string rootFile, int depth) {
			root = new FileTree(Directory.GetFileSystemEntries(rootFile), new FileInfo(rootFile).Name, 0, depth);
		}
		internal TreeRoot(TreeRoot copyFrom) {
			root = new FileTree(root);
		}
		//methods
		public void CompareWith(TreeRoot otherTree) {

		}
		public void Print() {
			root.Print("");
		}
	}
	class FileTree : Traversable{
		//members
		private string folderName;
		private Traversable[] children;
		//constructors
		internal FileTree(string[] fileNames, string folderName, int currentDepth, int maxDepth) {
			this.folderName = folderName;
			children = new Traversable[fileNames.Length];
			for(int i = 0; i < children.Length; i++) {
				FileInfo fi = new FileInfo(fileNames[i]);
				if(currentDepth < maxDepth || fi.Attributes.HasFlag(FileAttributes.Directory)) {
					children[i] = new FileTree(Directory.GetFileSystemEntries(fileNames[i]), fi.Name, currentDepth + 1, maxDepth);
				}
				else {
					children[i] = new TreeItem(fi.Name);
				}
			}
		}
		internal FileTree(FileTree copyFrom) {
			this.folderName = copyFrom.folderName;
			this.children = new Traversable[copyFrom.children.Length];
			for(int i = 0; i < this.children.Length; i++) {
				FileTree fi = copyFrom.children[i] as FileTree;
				if(fi == null) {
					children[i] = new TreeItem(copyFrom.children[i] as TreeItem);
				}
				else {
					children[i] = new FileTree(fi);
				}
			}
		}
		//methods
		public void Print(string tabs) {
			Console.WriteLine(tabs + folderName);
			tabs += "\t";
			foreach (Traversable t in children) {
				t.Print(tabs);
			}
		}
		public bool Compare(Traversable t) {
			FileTree ft = t as FileTree;
			return ft != null && ft.folderName.Equals(this.folderName);
		}
	}
	class TreeItem : Traversable {
		//members
		private string fileName;
		private Difference diff;
		//constructors
		internal TreeItem(string fileName) {
			this.fileName = fileName;
			diff = Difference.UNKNOWN;
		}
		internal TreeItem(TreeItem copyFrom) {
			this.fileName = copyFrom.fileName;
			diff = Difference.UNKNOWN;
		}
		//methods
		public void Print(string tabs) {
			Console.WriteLine(tabs + fileName);
		}
		public bool Compare(Traversable t) {
			TreeItem ti = t as TreeItem;
			return ti != null && ti.fileName.Equals(this.fileName);
		}
	}
	enum Difference { UNKNOWN, SAME, NEW, MISSING }
	interface Traversable {
		void Print(string tabs);
		bool Compare(Traversable t);
	}
}
