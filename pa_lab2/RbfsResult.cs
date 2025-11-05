public class RbfsResult
{
    public RbfsNode? Node { get; }
    public int BackedUp_F_Cost { get; }

    public RbfsResult(RbfsNode? node, int backedUpCost)
    {
        Node = node;
        BackedUp_F_Cost = backedUpCost;
    }
}