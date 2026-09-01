using DynamicWorkflowConsole.Models;

namespace DynamicWorkflowConsole.Tests.Models;

public class OrderContextTests
{
    [Fact]
    public void Abort_SetsAbortedFlagAndReason()
    {
        var context = new OrderContext { OrderId = "ORD-1" };

        context.Abort("credit check failed");

        Assert.True(context.IsAborted);
        Assert.Equal("credit check failed", context.AbortReason);
    }

    [Fact]
    public void NewContext_IsNotAborted()
    {
        var context = new OrderContext();

        Assert.False(context.IsAborted);
        Assert.Equal(string.Empty, context.AbortReason);
        Assert.False(string.IsNullOrWhiteSpace(context.WorkflowId));
    }
}
