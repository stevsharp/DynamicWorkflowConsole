using DynamicWorkflowConsole.Models;

namespace DynamicWorkflowConsole.Handlers;

public class InventoryHandler : IWorkflowHandler
{
    public async Task HandleAsync(OrderContext context, Func<Task> next)
    {
        Console.WriteLine($"[Step: Inventory] Stock reserved for Order {context.OrderId}.");
        await next();
    }
}
