using DynamicWorkflowConsole.Handlers;
using DynamicWorkflowConsole.Models;

namespace DynamicWorkflowConsole.Tests.Handlers;

public class HandlerTests
{
    [Fact]
    public async Task InventoryHandler_CallsNextAndWritesOrderId()
    {
        var handler = new InventoryHandler();
        var context = new OrderContext { OrderId = "ORD-INV" };
        var nextCalled = false;
        var output = await CaptureConsoleAsync(() =>
            handler.HandleAsync(context, () =>
            {
                nextCalled = true;
                return Task.CompletedTask;
            }));

        Assert.True(nextCalled);
        Assert.Contains("ORD-INV", output);
        Assert.Contains("Inventory", output);
    }

    [Fact]
    public async Task PaymentHandler_CallsNextAndWritesAmount()
    {
        var handler = new PaymentHandler();
        var context = new OrderContext { Amount = 1250.50m };
        var nextCalled = false;
        var output = await CaptureConsoleAsync(() =>
            handler.HandleAsync(context, () =>
            {
                nextCalled = true;
                return Task.CompletedTask;
            }));

        Assert.True(nextCalled);
        Assert.Contains("Payment", output);
    }

    private static async Task<string> CaptureConsoleAsync(Func<Task> action)
    {
        var original = Console.Out;
        using var writer = new StringWriter();
        Console.SetOut(writer);
        try
        {
            await action();
        }
        finally
        {
            Console.SetOut(original);
        }

        return writer.ToString();
    }
}
