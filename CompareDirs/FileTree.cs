using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompareDirs {
	public enum ChangeState { SAME, ADDED, DELETED, MIXED }
	public abstract class Node {
		//properties
		public string Name { get; private set; }
		public ChangeState State { get; set; }
		//constructors
		public Node(string name, ChangeState state) {
			Name = name;
			State = state;
		}
		//methods
		/// <summary>
		/// Do this Node and another share the same name and type?
		/// </summary>
		/// <param name="other">The node to compare to</param>
		/// <returns></returns>
		public bool SharesName(Node other) {
			return Name.Equals(other.Name) && other.GetType().Equals(this.GetType());
		}
		public int CompareTo(Node n) {
			return (n is BranchNode) ? CompareTo(n as BranchNode) : CompareTo(n as LeafNode);
		}
		public override string ToString() {
			return ToString(true);
		}
		public string ToString(bool showState) {
			return Print("", showState);
		}
		public ChangeState MeshStates(ChangeState oldState, ChangeState newState) {
			if (oldState.Equals(ChangeState.MIXED) || newState.Equals(ChangeState.MIXED))
				return ChangeState.MIXED;
			if (oldState.Equals(newState)) return newState;
			if (oldState.Equals(ChangeState.SAME)) return newState;
			if (newState.Equals(ChangeState.SAME)) return oldState;
			return ChangeState.MIXED;
		}
		public string PrintTitle(string tabs, bool showState) {
			return tabs + Name +  (showState ? " (" + State.ToString() + ")" : "");
		}
		public abstract Node MergeTrees(Node second);
		public abstract Node CreateCopy(ChangeState state);
		public abstract int CompareTo(LeafNode other);
		public abstract int CompareTo(BranchNode other);
		public abstract void MarkAs(ChangeState state);
		public abstract string Print(string tabs, bool showState);
		public abstract bool EqualToNode(Node other);
	}
	public class LeafNode : Node {
		//constructors
		public LeafNode(string name, ChangeState state) : base(name, state) { }
		//methods
		public override void MarkAs(ChangeState state) {
			State = state;
		}
		public override int CompareTo(BranchNode other) {
			return 1;
		}
		public override int CompareTo(LeafNode other) {
			return Name.CompareTo(other.Name);
		}
		public override string Print(string tabs, bool showState) {
			return PrintTitle(tabs, showState);
		}

		public override bool EqualToNode(Node other) {
			return SharesName(other) && State.Equals(other.State);
		}

		public override Node MergeTrees(Node second) {
			return CreateCopy(ChangeState.SAME);
		}

		public override Node CreateCopy(ChangeState state) {
			return new LeafNode(Name, state);
		}
	}
	/// <summary>
	/// A BranchNode consists of a Node that has Children
	/// Invariant: Children are sorted alphabetically by Name
	/// BranchNodes always come before LeafNodes
	/// </summary>
	public class BranchNode : Node {
		//properties
		public List<Node> Children { get; private set; }
		public Node this[int i] { get { return Children[i]; } }
		//constructors
		public BranchNode(string name, ChangeState state, List<Node> children) : base(name, state) {
			Children = children;
		}
		public BranchNode(string name, ChangeState state, params Node[] nodes) : this(name, state, new List<Node>()) {
			Children.AddRange(nodes);
		}
		public void Append(Node n) {
			Children.Add(n);
		}
		public override void MarkAs(ChangeState state) {
			State = state;
			foreach(Node c in Children) {
				c.MarkAs(state);
			}
		}

		public override int CompareTo(LeafNode other) {
			return -1;
		}

		public override int CompareTo(BranchNode other) {
			return Name.CompareTo(other.Name);
		}
		public override string Print(string tabs, bool showState) {
			string soFar = PrintTitle(tabs, showState);
			tabs += showState ? "\t" : "  ";
			foreach (Node c in Children) {
				soFar += "\n" + c.Print(tabs, showState);
			}
			return soFar;
		}

		public override bool EqualToNode(Node other) {
			if (!State.Equals(other.State)) return false;
			if(other is BranchNode) {
				BranchNode bOther = other as BranchNode;
				if (bOther.Children.Count != Children.Count) return false;
				for(int i = 0; i < bOther.Children.Count; i++) {
					if (!Children[i].EqualToNode(bOther.Children[i])) {
						return false;
					}
				}
			}
			else {
				return other.SharesName(this);
            }
			return true;
		}
		public override Node MergeTrees(Node second) {
			BranchNode bSecond = second as BranchNode;
			List<Node> resultChildren = new List<Node>();
			ChangeState overallState = ChangeState.SAME;
			var firstIter = Children.GetEnumerator();
			var secondIter = bSecond.Children.GetEnumerator();
			bool firstHasNext = firstIter.MoveNext();
			bool secondHasNext = secondIter.MoveNext();
			while(firstHasNext && secondHasNext) {
				if (firstIter.Current.SharesName(secondIter.Current)) {
					resultChildren.Add(firstIter.Current.MergeTrees(secondIter.Current));
					firstHasNext = firstIter.MoveNext();
					secondHasNext = secondIter.MoveNext();
				}
				else if(firstIter.Current.CompareTo(secondIter.Current) < 0) {
					resultChildren.Add(firstIter.Current.CreateCopy(ChangeState.DELETED));
					firstHasNext = firstIter.MoveNext();
				}
				else {
					resultChildren.Add(secondIter.Current.CreateCopy(ChangeState.ADDED));
					secondHasNext = secondIter.MoveNext();
				}
				overallState = MeshStates(overallState, resultChildren[resultChildren.Count - 1].State);
			}
			while (firstHasNext) {
				resultChildren.Add(firstIter.Current.CreateCopy(ChangeState.DELETED));
				firstHasNext = firstIter.MoveNext();
				overallState = MeshStates(overallState, resultChildren[resultChildren.Count - 1].State);
			}
			while (secondHasNext) {
				resultChildren.Add(secondIter.Current.CreateCopy(ChangeState.ADDED));
				secondHasNext = secondIter.MoveNext();
				overallState = MeshStates(overallState, resultChildren[resultChildren.Count - 1].State);
			}
			return new BranchNode(Name, overallState, resultChildren);
		}

		public override Node CreateCopy(ChangeState state) {
			List<Node> copyChildren = new List<Node>();
			foreach(Node c in Children) {
				copyChildren.Add(c.CreateCopy(state));
			}
			return new BranchNode(Name, state);
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
		public TreeRoot(string rootFile) : this(rootFile, int.MaxValue) { }
		public TreeRoot(string rootFile, int depth) {
			root = new FileTree(Directory.GetFileSystemEntries(rootFile), new FileInfo(rootFile).Name, 0, depth);
		}
		public TreeRoot(TreeRoot copyFrom) {
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
		public FileTree(string[] fileNames, string folderName, int currentDepth, int maxDepth) : base(folderName) {
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
		public FileTree(FileTree copyFrom) : base(copyFrom.Name){
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
		public TreeItem(string fileName) : base(fileName) {
		}
		public TreeItem(TreeItem copyFrom) : base(copyFrom.Name){
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
		public Traversable(string name) {
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
