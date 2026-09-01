using DynamicWorkflowConsole.Engine;
using DynamicWorkflowConsole.Handlers;
using DynamicWorkflowConsole.Models;
using Microsoft.Extensions.DependencyInjection;

public class Program
{
    public static async Task Main()
    {
        var services = new ServiceCollection();

        services.AddKeyedTransient<IWorkflowHandler, InventoryHandler>("InventoryService");
        services.AddKeyedTransient<IWorkflowHandler, PaymentHandler>("PaymentService");
        services.AddTransient<DynamicWorkflowEngine>();

        var provider = services.BuildServiceProvider();
        var engine = provider.GetRequiredService<DynamicWorkflowEngine>();

        var context = new OrderContext
        {
            OrderId = "ORD-999",
            Amount = 6500.00m,
            CustomerAge = 22,
            CreditScore = 650
        };

        await engine.ExecuteWorkflowAsync("OrderProcessing", context);
    }
}
