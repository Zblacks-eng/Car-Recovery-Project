using System;

namespace CST2550
{
	// A node is basically one "slot" in the binary search tree.
	// Each node holds one recovery record and has two child nodes (left and right).
	// If there's no child on that side, it just stays null.
	public class Node
	{
		public RecoveryRecord Data;
		public Node Left;
		public Node Right;

		public Node(RecoveryRecord data)
		{
			Data = data;
			Left = null;
			Right = null;
		}
	}
}
