namespace pa_lab2;
public class SolveResult
{
    public StateNode? FinalNode { get; set; } 
    public long Iterations { get; set; }
    public long GeneratedNodes { get; set; }
    public int NodesInMemory { get; set; }
    
    public bool IsSuccess => FinalNode != null && FinalNode.Score == 0;
}