using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using CompareDirs;
using System.Diagnostics;

namespace UnitTests {
	[TestClass]
	public class UnitTest1 {
		private BranchNode CreateNode1() {
			return new BranchNode("folder",
									new BranchNode("sub1", new LeafNode("ssuubb1", ChangeState.ADDED),
														   new LeafNode("ssuubb2", ChangeState.DELETED)),
									new LeafNode("folder2", ChangeState.SAME));
		}
		private BranchNode CreateNode2() {
			return new BranchNode("folder",
									new BranchNode("sub1", new LeafNode("ssuubb1", ChangeState.MIXED),
														   new LeafNode("ssuubb3", ChangeState.SAME)),
                                    new BranchNode("sub2", new LeafNode("ssuubb1", ChangeState.ADDED)),
									new LeafNode("folder4", ChangeState.SAME));
		}
		[TestMethod]
		public void CompareSameTreeYieldsSameTree() {
			BranchNode node1 = CreateNode1(), node2 = CreateNode1();
			node1.MarkAs(ChangeState.SAME);
			node2.MarkAs(ChangeState.ADDED);
			Trace.WriteLine(node1.ToString());
			Trace.WriteLine(node2.ToString());
			Assert.IsTrue(node1.EqualToNode(node1.MergeToResult(node2)));
		}
	}
}
