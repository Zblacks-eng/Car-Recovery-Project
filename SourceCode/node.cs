namespace CST2550
{
    // defining the structure for binary search tree
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
