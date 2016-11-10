using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompareDirs {
	enum ChangeState { SAME, ADDED, DELETED, MIXED }
	interface INodeVisitor {
		Node accept(DirNode n1, DirNode n2);
		Node accept(FileNode n1, DirNode n2);
		Node accept(DirNode n1, FileNode n2);
		Node accept(FileNode n1, FileNode n2);
	}
	class NodeVisitor : INodeVisitor {
		public Node accept(FileNode n1, FileNode n2) {
			if (n1.SharesName(n2)) {
				return new FileNode(n1.Name);
			}
			else {
				return new DirNode(new FileNode(n1.Name, ChangeState.DELETED),
								   new FileNode(n2.Name, ChangeState.ADDED));
			}
		}

		public Node accept(DirNode n1, FileNode n2) {
			n1.MarkAll(ChangeState.DELETED);
			int match = n1.ChildMatching(n2);
			if(match != -1) {
				n1[match].State = ChangeState.SAME;
			}
			else {
				n2.State = ChangeState.ADDED;
				n1.Append(n2);
			}
			return n1;
		}

		public Node accept(FileNode n1, DirNode n2) {
			throw new NotImplementedException();
		}

		public Node accept(DirNode n1, DirNode n2) {
			
		}
	}
	class TreeComparer {
		//properties
		internal Node BaseTree { get; private set; }
		internal Node OtherTree { get; private set; }
		//constructors
		internal TreeComparer(Node baseTree, Node otherTree) {
			BaseTree = baseTree;
			OtherTree = otherTree;
		}
		internal Node GenerateResult() {
			foreach(Node b in BaseTree) {
				if(OtherTree.)
			}
		}
	}
	abstract class Node {
		//properties
		internal string Name { get; private set; }
		internal ChangeState State { get; set; }
		//constructors
		internal Node(string name, ChangeState state) {
			Name = name;
			State = state;
		}
		internal Node(string name) : this(name, ChangeState.SAME) { }
		//methods
		/// <summary>
		/// Do this Node and another share the same name?
		/// </summary>
		/// <param name="other">The node to compare to</param>
		/// <returns></returns>
		internal bool SharesName(Node other) {
			return Name.Equals(other.Name);
		}
		internal abstract Node Compare(Node other);
		internal abstract bool ContainsState(ChangeState state);
	}
	class FileNode : Node {
		//constructors
		internal FileNode(string name) : base(name) { }
		internal FileNode(string name, ChangeState state) : base(name, state) { }
		//methods
		internal override Node Compare(Node other) {
			throw new NotImplementedException();
		}

		internal override bool ContainsState(ChangeState state) {
			return State.Equals(state);
		}
	}
	class DirNode : Node {
		//properties
		internal List<Node> Children { get; private set; }
		internal Node this[int i] { get { return Children[i]; } }
		//constructors
		internal DirNode(string name) : base(name) {
			Children = new List<Node>();
		}
		internal DirNode(params Node[] children) : this("") {
			Children.AddRange(children);
		}
		internal override Node Compare(Node other) {
			throw new NotImplementedException();
		}
		internal void MarkAll(ChangeState state) {
			foreach(Node c in Children) {
				c.State = state;
			}
		}
		internal void Append(Node n) {
			Children.Add(n);
		}
		internal int ChildMatching(Node other) {

		}

		internal override bool ContainsState(ChangeState state) {
			foreach(Node n in Children) {
				if (n.ContainsState(state))
					return true;
			}
			return false;
		}
	}

	class TreeRoot {
		//members
		private FileTree root;
		//properties
		public string Name { get { return root.Name; } }
		public Difference Change { get { return root.Change; } }
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
		private Difference FindParentDifference(Difference currDiff, Difference childDiff) {
			if (currDiff.Equals(Difference.UNKNOWN) || currDiff.Equals(Difference.SAME))
				return childDiff;
			else if (!currDiff.Equals(childDiff))
				return Difference.BOTH;
			return currDiff;
		}
		public void CompareLevels(FileTree otherRoot) {
			Difference currDiff1 = Difference.UNKNOWN, currDiff2 = Difference.UNKNOWN;
			foreach(Traversable t in children) {
				//find a match in the other tree at this level
				Traversable match = FindMatch(t, otherRoot);
				//if a match was found
				if (match != null) {
					//the children are the same
					t.Change = Difference.SAME;
                    match.Change = Difference.SAME;
					//if both children are file trees
					if(t is FileTree) {
						//compare them, setting their Change in the process
						(t as FileTree).CompareLevels(match as FileTree);
					}
					//find the difference the parent should have based on new children difference data
					currDiff1 = FindParentDifference(currDiff1, t.Change);
					currDiff2 = FindParentDifference(currDiff2, match.Change);
				}
				else {
					//this element is new in the original tree
					t.Change = Difference.NEW;
					//if it's a file tree, all children are also new
					if (t is FileTree) {
						(t as FileTree).SetChildrenAs(Difference.NEW);
					}
					//find the difference the parent should have based on NEW children data
					currDiff1 = FindParentDifference(currDiff1, Difference.NEW);
				}
			}
			foreach(Traversable t in otherRoot.children) {
				//element was not hit in other loop and is new in the other tree
				if (t.Change.Equals(Difference.UNKNOWN)) {
					Traversable newT;
					if(t is TreeItem) {
						//create a copy to attach to the original tree
						newT = new TreeItem(t as TreeItem);
					}
					else {
						newT = new FileTree(t as FileTree);
						(newT as FileTree).SetChildrenAs(Difference.MISSING);
					}
					newT.Change = Difference.MISSING;
					//find the difference the parent should have based on MISSING children data
					currDiff1 = FindParentDifference(currDiff1, Difference.MISSING);
					//add the new node
					this.children.Add(newT);
				}
			}
			this.Change = currDiff1;
			otherRoot.Change = currDiff2;
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
	enum Difference { UNKNOWN, SAME, NEW, MISSING, BOTH }
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
			return this.GetType().Equals(t.GetType()) && this.name.Equals(t.name);
		}
	}
}
