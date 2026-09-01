namespace DynamicWorkflowConsole.Models;

public class OrderContext
{
    public string WorkflowId { get; } = Guid.NewGuid().ToString("N");
    public string OrderId { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public int CustomerAge { get; set; }
    public int CreditScore { get; set; }

    public bool IsAborted { get; private set; }
    public string AbortReason { get; private set; } = string.Empty;

    public void Abort(string reason)
    {
        IsAborted = true;
        AbortReason = reason;
    }
}
