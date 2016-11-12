using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CompareDirs;
using System.Diagnostics;

namespace UnitTests {
	[TestClass]
	public class UnitTest1 {
		private BranchNode CreateNode1() {
			return new BranchNode("1.", ChangeState.SAME,
									new BranchNode("A.", ChangeState.SAME,
														new LeafNode("a1", ChangeState.ADDED),
														new LeafNode("a2", ChangeState.DELETED)),
									new BranchNode("B.", ChangeState.SAME));
		}
		private BranchNode CreateNode2() {
			return new BranchNode("1.", ChangeState.ADDED,
									new BranchNode("A.", ChangeState.DELETED,
														new LeafNode("a1", ChangeState.ADDED),
														new LeafNode("a2", ChangeState.DELETED)),
									new BranchNode("B.", ChangeState.SAME),
									new LeafNode("C.", ChangeState.MIXED));
		}
		private BranchNode CreateNode3() {
			return new BranchNode("1.", ChangeState.MIXED,
									new BranchNode("A.", ChangeState.DELETED,
															new LeafNode("a1", ChangeState.MIXED),
															new LeafNode("a3", ChangeState.SAME)),
									new BranchNode("B.", ChangeState.DELETED,
														 new LeafNode("b1", ChangeState.ADDED)),
									new BranchNode("D.", ChangeState.SAME),
                                    new LeafNode("C.", ChangeState.SAME));
		}
		private BranchNode CreateResultNode1_3() {
			return new BranchNode("1.", ChangeState.MIXED,
									new BranchNode("A.", ChangeState.MIXED,
															new LeafNode("a1", ChangeState.SAME),
															new LeafNode("a2", ChangeState.DELETED),
															new LeafNode("a3", ChangeState.ADDED)),
									new BranchNode("B.", ChangeState.ADDED,
															new LeafNode("b1", ChangeState.ADDED)),
									new BranchNode("D.", ChangeState.ADDED),
									new LeafNode("C.", ChangeState.ADDED));
		}
		private void AssertNodesEqual(Node expected, Node actual) {
			Trace.WriteLine("Expected: ");
			Trace.WriteLine(expected.ToString());
			Trace.WriteLine("Actual: ");
			Trace.WriteLine(actual.ToString());
			Assert.IsTrue(expected.EqualToNode(actual));
		}
		[TestMethod]
		public void EqualToNode_EqualNodes() {
			Assert.IsTrue(CreateNode1().EqualToNode(CreateNode1()));
		}
		[TestMethod]
		public void EqualToNode_NotEqualNodes() {
			Assert.IsFalse(CreateNode1().EqualToNode(CreateNode2()));
			Assert.IsFalse(CreateNode1().EqualToNode(CreateNode3()));
		}
		private void AssertMergeTrees(BranchNode expected, BranchNode first, BranchNode second) {
			Trace.WriteLine("First:");
			Trace.WriteLine(first.ToString(false));
			Trace.WriteLine("Second:");
			Trace.WriteLine(second.ToString(false));
			AssertNodesEqual(expected, first.MergeTrees(second));
		}
		[TestMethod]
		public void MergeTrees_MergeWithSelfYieldsSelf() {
			BranchNode first = CreateNode1(), second = CreateNode1();
			first.MarkAs(ChangeState.SAME);
			AssertMergeTrees(first, first, second);
		}
		[TestMethod]
		public void MergeTrees_MergeAddsTopLevelLeaf() {
			BranchNode expected = CreateNode2();
			expected.MarkAs(ChangeState.SAME);
			expected.State = ChangeState.ADDED;
			expected.Children[expected.Children.Count - 1].MarkAs(ChangeState.ADDED);

			AssertMergeTrees(expected, CreateNode1(), CreateNode2());
		}
		[TestMethod]
		public void MergeTrees_MergeRemovesTopLevelLeaf() {
			BranchNode expected = CreateNode2();
			expected.MarkAs(ChangeState.SAME);
			expected.State = ChangeState.DELETED;
			expected.Children[expected.Children.Count - 1].MarkAs(ChangeState.DELETED);

			AssertMergeTrees(expected, CreateNode2(), CreateNode1());
		}
		[TestMethod]
		public void MergeTrees_ComplexTrees() {
			AssertMergeTrees(CreateResultNode1_3(), CreateNode1(), CreateNode3());
		}
		[TestMethod]
		public void MeshStates_CommonInsAndOuts() {
			BranchBuilder b = new BranchBuilder(CreateNode1().Children, CreateNode1().Children);
			Assert.AreEqual(ChangeState.DELETED, b.MeshStates(ChangeState.DELETED, ChangeState.SAME));
			Assert.AreEqual(ChangeState.DELETED, b.MeshStates(ChangeState.DELETED, ChangeState.DELETED));
			Assert.AreEqual(ChangeState.DELETED, b.MeshStates(ChangeState.SAME, ChangeState.DELETED));

			Assert.AreEqual(ChangeState.ADDED, b.MeshStates(ChangeState.ADDED, ChangeState.SAME));
			Assert.AreEqual(ChangeState.ADDED, b.MeshStates(ChangeState.ADDED, ChangeState.ADDED));
			Assert.AreEqual(ChangeState.ADDED, b.MeshStates(ChangeState.SAME, ChangeState.ADDED));

			Assert.AreEqual(ChangeState.SAME, b.MeshStates(ChangeState.SAME, ChangeState.SAME));

			Assert.AreEqual(ChangeState.MIXED, b.MeshStates(ChangeState.DELETED, ChangeState.ADDED));
			Assert.AreEqual(ChangeState.MIXED, b.MeshStates(ChangeState.ADDED, ChangeState.DELETED));

			Assert.AreEqual(ChangeState.MIXED, b.MeshStates(ChangeState.MIXED, ChangeState.MIXED));
			Assert.AreEqual(ChangeState.MIXED, b.MeshStates(ChangeState.MIXED, ChangeState.ADDED));
			Assert.AreEqual(ChangeState.MIXED, b.MeshStates(ChangeState.MIXED, ChangeState.DELETED));
			Assert.AreEqual(ChangeState.MIXED, b.MeshStates(ChangeState.MIXED, ChangeState.SAME));

			Assert.AreEqual(ChangeState.MIXED, b.MeshStates(ChangeState.MIXED, ChangeState.ADDED));
			Assert.AreEqual(ChangeState.MIXED, b.MeshStates(ChangeState.MIXED, ChangeState.DELETED));
			Assert.AreEqual(ChangeState.MIXED, b.MeshStates(ChangeState.MIXED, ChangeState.SAME));
		}
	}
}
