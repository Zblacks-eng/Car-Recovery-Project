using System;

namespace CST2550
{
	// tree logic- Updat3: add the search funtion
	public class RecoveryTree
	{
		public Node Root;

		// Just a simple way to start adding cars to the tree
		public void Add(RecoveryRecord newRecord)
		{
			Root = InsertRecursive(Root, newRecord);
		}

		// It just checks if the plate should go left or right.
		private Node InsertRecursive(Node root, RecoveryRecord record)
		{
			// If we hit an empty spot, drop the new car record here
			if (root == null)
			{
				return new Node(record);
			}

			// compare plates alphabetically - left if it's "smaller", right if "bigger"
			int comparison = string.Compare(record.NumberPlate, root.Data.NumberPlate);

			if (comparison < 0)
			{
				root.Left = InsertRecursive(root.Left, record);
			}
			else if (comparison > 0)
			{
				root.Right = InsertRecursive(root.Right, record);
			}
			else
			{
				//Duplicate plate, do nothing
				return root;
			}

		// The public search we call from Program.cs
		public RecoveryRecord Search(string plate)
		{
			return SearchRecursive(Root, plate);
		}

		// This looks through the branches. If it finds the plate, it returns the whole car record.
		private RecoveryRecord SearchRecursive(Node root, string plate)
		{
			// If we get to the end of a branch and find nothing, return null
			if (root == null)
			{
				return null;
			}

			// Check if this node is the one we want
			int comparison = string.Compare(plate, root.Data.NumberPlate);

			if (comparison == 0)
			{
				return root.Data; // Found it! 
			}

			// If what we're looking for is "smaller", look left. If not, look right.
			if (comparison < 0)
			{
				return SearchRecursive(root.Left, plate);
			}
			else
			{
				return SearchRecursive(root.Right, plate);
			}
		}
	}
}
