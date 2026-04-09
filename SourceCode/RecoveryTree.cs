using System;

namespace CST2550
{
	// This is my custom Binary Search Tree implementation.
	// It stores RecoveryRecords sorted alphabetically by number plate.
	// The reason I used a BST is so searching is O(log n) rather than
	// looping through every single record every time someone searches.
	public class RecoveryTree
	{
		public Node Root;

		// Adds a new record into the tree
		public void Add(RecoveryRecord newRecord)
		{
			Root = InsertRecursive(Root, newRecord);
		}

		// Recursively finds the right place to insert the new record.
		// Goes left if the plate comes before the current node alphabetically,
		// right if it comes after.
		private Node InsertRecursive(Node current, RecoveryRecord record)
		{
			if (current == null)
				return new Node(record);

			int comparison = string.Compare(record.NumberPlate, current.Data.NumberPlate);

			if (comparison < 0)
				current.Left = InsertRecursive(current.Left, record);
			else if (comparison > 0)
				current.Right = InsertRecursive(current.Right, record);
			// if comparison == 0 then it's a duplicate plate, so just ignore it

			return current;
		}

		// Searches for a record by number plate and returns it (or null if not found)
		public RecoveryRecord Search(string plate)
		{
			return SearchRecursive(Root, plate);
		}

		private RecoveryRecord SearchRecursive(Node current, string plate)
		{
			// Reached the end of a branch without finding it
			if (current == null)
				return null;

			int comparison = string.Compare(plate, current.Data.NumberPlate);

			if (comparison == 0)
				return current.Data; // found it

			if (comparison < 0)
				return SearchRecursive(current.Left, plate);
			else
				return SearchRecursive(current.Right, plate);
		}

		// Removes a record from the tree by number plate.
		// Returns true if something was actually deleted, false if the plate wasn't found.
		public bool Delete(string plate)
		{
			bool wasDeleted = false;
			Root = DeleteRecursive(Root, plate, ref wasDeleted);
			return wasDeleted;
		}

		// This was the trickiest part to implement. There are three cases:
		// 1. The node has no children - just remove it
		// 2. The node has one child - replace the node with its child
		// 3. The node has two children - replace with the in-order successor
		private Node DeleteRecursive(Node current, string plate, ref bool wasDeleted)
		{
			if (current == null)
				return null;

			int comparison = string.Compare(plate, current.Data.NumberPlate);

			if (comparison < 0)
			{
				current.Left = DeleteRecursive(current.Left, plate, ref wasDeleted);
			}
			else if (comparison > 0)
			{
				current.Right = DeleteRecursive(current.Right, plate, ref wasDeleted);
			}
			else
			{
				// This is the node we want to delete
				wasDeleted = true;

				// Case 1: leaf node, no children
				if (current.Left == null && current.Right == null)
					return null;

				// Case 2: only has a right child
				if (current.Left == null)
					return current.Right;

				// Case 2: only has a left child
				if (current.Right == null)
					return current.Left;

				// Case 3: has both children
				// Find the in-order successor - the smallest node in the right subtree.
				// This is the next plate alphabetically so it's safe to replace with.
				Node successor = current.Right;
				while (successor.Left != null)
					successor = successor.Left;

				current.Data = successor.Data;
				current.Right = DeleteRecursive(current.Right, successor.Data.NumberPlate, ref wasDeleted);
			}

			return current;
		}
	}
}
