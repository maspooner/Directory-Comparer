using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CompareDirs {
	/// <summary>
	/// Models a Difference between <seealso cref="Node"/> types
	/// </summary>
	public enum ChangeState { SAME, ADDED, DELETED, MIXED }
	/// <summary>
	/// Models a Node in a file tree
	/// </summary>
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
		public bool SharesName(Node other) {
			return Name.Equals(other.Name) && other.GetType().Equals(this.GetType());
		}
		/// <summary>
		/// Compares this Node to another
		/// </summary>
		/// <param name="n">the <seealso cref="Node"/> to compare to</param>
		public int CompareTo(Node n) {
			return (n is BranchNode) ? CompareTo(n as BranchNode) : CompareTo(n as LeafNode);
		}
		public override string ToString() {
			return ToString(true);
		}
		public string ToString(bool showState) {
			return Print("", showState);
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
			BranchBuilder builder = new BranchBuilder(Children, bSecond.Children);
			while (builder.HasNextLeft() && builder.HasNextRight()) {
				if (builder.Left.SharesName(builder.Right)) {
					builder.AcceptBoth();
				}
				else if (builder.Left.CompareTo(builder.Right) < 0) {
					builder.AcceptLeft();
				}
				else {
					builder.AcceptRight();
				}
			}
			while (builder.HasNextLeft()) {
				builder.AcceptLeft();
			}
			while (builder.HasNextRight()) {
				builder.AcceptRight();
			}
			return builder.Build(Name);
		}

		public override Node CreateCopy(ChangeState state) {
			List<Node> copyChildren = new List<Node>();
			foreach(Node c in Children) {
				copyChildren.Add(c.CreateCopy(state));
			}
			return new BranchNode(Name, state);
		}
	}
	public class BranchBuilder {
		//members
		private bool firstHasNext, secondHasNext;
		private ChangeState overallState;
		private List<Node>.Enumerator firstIter, secondIter;
		private List<Node> children;
		//properties
		public Node Left { get { return firstIter.Current; } }
		public Node Right { get { return secondIter.Current; } }
		//constructors
		public BranchBuilder(List<Node> first, List<Node> second) {
			children = new List<Node>();
			overallState = ChangeState.SAME;
			firstIter = first.GetEnumerator();
			secondIter = second.GetEnumerator();
			firstHasNext = firstIter.MoveNext();
			secondHasNext = secondIter.MoveNext();
		}
		//methods
		public bool HasNextLeft() {
			return firstHasNext;
		}
		public bool HasNextRight() {
			return secondHasNext;
		}
		public void AcceptBoth() {
			children.Add(Left.MergeTrees(Right));
			firstHasNext = firstIter.MoveNext();
			secondHasNext = secondIter.MoveNext();
			overallState = MeshStates(overallState, children[children.Count - 1].State);
		}
		public void AcceptLeft() {
			children.Add(Left.CreateCopy(ChangeState.DELETED));
			firstHasNext = firstIter.MoveNext();
			overallState = MeshStates(overallState, children[children.Count - 1].State);
		}
		public void AcceptRight() {
			children.Add(Right.CreateCopy(ChangeState.ADDED));
			secondHasNext = secondIter.MoveNext();
			overallState = MeshStates(overallState, children[children.Count - 1].State);
		}
		public ChangeState MeshStates(ChangeState oldState, ChangeState newState) {
			if (oldState.Equals(ChangeState.MIXED) || newState.Equals(ChangeState.MIXED))
				return ChangeState.MIXED;
			if (oldState.Equals(newState)) return newState;
			if (oldState.Equals(ChangeState.SAME)) return newState;
			if (newState.Equals(ChangeState.SAME)) return oldState;
			return ChangeState.MIXED;
		}
		public BranchNode Build(string name) {
			return new BranchNode(name, overallState, children);
		}
	}
}
