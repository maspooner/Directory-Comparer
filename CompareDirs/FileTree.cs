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
		//constructors
		internal TreeRoot(string rootFile) {
			root = new FileTree(Directory.GetFileSystemEntries(rootFile), new FileInfo(rootFile).Name);
		}
		//methods
		public void Print() {
			root.Print("");
		}
	}
	class FileTree : Traversable{
		//members
		private string folderName;
		private Traversable[] children;
		//constructors
		internal FileTree(string[] fileNames, string folderName) {
			this.folderName = folderName;
			children = new Traversable[fileNames.Length];
			for(int i = 0; i < children.Length; i++) {
				FileInfo fi = new FileInfo(fileNames[i]);
				if(fi.Attributes.HasFlag(FileAttributes.Directory)) {
					children[i] = new FileTree(Directory.GetFileSystemEntries(fileNames[i]), fi.Name);
				}
				else {
					children[i] = new TreeItem(fi.Name);
				}
				Console.WriteLine(fi.Extension);
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
		//methods
		public void Print(string tabs) {
			Console.WriteLine(tabs + fileName);
		}
	}
	enum Difference { UNKNOWN, SAME, NEW, MISSING }
	interface Traversable {
		void Print(string tabs);
		bool Compare(Traversable t);
	}
}
