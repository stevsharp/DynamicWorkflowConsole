using DynamicWorkflowConsole.Handlers;
using DynamicWorkflowConsole.Models;

namespace DynamicWorkflowConsole.Tests.Fakes;

public sealed class RecordingHandler : IWorkflowHandler
{
    public int CallCount { get; private set; }
    public string? LastOrderId { get; private set; }

    public Task HandleAsync(OrderContext context, Func<Task> next)
    {
        CallCount++;
        LastOrderId = context.OrderId;
        return next();
    }
}
