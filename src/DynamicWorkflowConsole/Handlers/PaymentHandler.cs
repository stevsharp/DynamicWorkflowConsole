using DynamicWorkflowConsole.Models;

namespace DynamicWorkflowConsole.Handlers;

public class PaymentHandler : IWorkflowHandler
{
    public async Task HandleAsync(OrderContext context, Func<Task> next)
    {
        Console.WriteLine($"[Step: Payment] Payment processed for {context.Amount:C}.");
        await next();
    }
}
