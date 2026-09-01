using DynamicWorkflowConsole.Engine;
using DynamicWorkflowConsole.Handlers;
using DynamicWorkflowConsole.Models;
using DynamicWorkflowConsole.Tests.Fakes;
using Microsoft.Extensions.DependencyInjection;

namespace DynamicWorkflowConsole.Tests.Engine;

public class DynamicWorkflowEngineTests
{
    [Fact]
    public async Task ExecuteWorkflowAsync_WhenAgeAndCreditPass_RunsInventoryAndPayment()
    {
        var inventory = new RecordingHandler();
        var payment = new RecordingHandler();
        var engine = CreateEngine(inventory, payment);
        var context = new OrderContext
        {
            OrderId = "ORD-OK",
            Amount = 6500.00m,
            CustomerAge = 22,
            CreditScore = 720
        };

        await engine.ExecuteWorkflowAsync("OrderProcessing", context);

        Assert.False(context.IsAborted);
        Assert.Equal(1, inventory.CallCount);
        Assert.Equal(1, payment.CallCount);
        Assert.Equal("ORD-OK", inventory.LastOrderId);
        Assert.Equal("ORD-OK", payment.LastOrderId);
    }

    [Fact]
    public async Task ExecuteWorkflowAsync_WhenAmountIsUnderThreshold_RunsPaymentWithoutHighCredit()
    {
        var inventory = new RecordingHandler();
        var payment = new RecordingHandler();
        var engine = CreateEngine(inventory, payment);
        var context = new OrderContext
        {
            OrderId = "ORD-LOW",
            Amount = 4999.99m,
            CustomerAge = 30,
            CreditScore = 500
        };

        await engine.ExecuteWorkflowAsync("OrderProcessing", context);

        Assert.False(context.IsAborted);
        Assert.Equal(1, inventory.CallCount);
        Assert.Equal(1, payment.CallCount);
    }

    [Fact]
    public async Task ExecuteWorkflowAsync_WhenCustomerIsUnderage_AbortsBeforeInventory()
    {
        var inventory = new RecordingHandler();
        var payment = new RecordingHandler();
        var engine = CreateEngine(inventory, payment);
        var context = new OrderContext
        {
            OrderId = "ORD-AGE",
            Amount = 100m,
            CustomerAge = 16,
            CreditScore = 800
        };

        await engine.ExecuteWorkflowAsync("OrderProcessing", context);

        Assert.True(context.IsAborted);
        Assert.Contains("InventoryService", context.AbortReason);
        Assert.Equal(0, inventory.CallCount);
        Assert.Equal(0, payment.CallCount);
    }

    [Fact]
    public async Task ExecuteWorkflowAsync_WhenHighValueCreditFails_RunsInventoryThenAbortsPayment()
    {
        var inventory = new RecordingHandler();
        var payment = new RecordingHandler();
        var engine = CreateEngine(inventory, payment);
        var context = new OrderContext
        {
            OrderId = "ORD-999",
            Amount = 6500.00m,
            CustomerAge = 22,
            CreditScore = 650
        };

        await engine.ExecuteWorkflowAsync("OrderProcessing", context);

        Assert.True(context.IsAborted);
        Assert.Contains("PaymentService", context.AbortReason);
        Assert.Equal(1, inventory.CallCount);
        Assert.Equal(0, payment.CallCount);
    }

    [Fact]
    public async Task ExecuteWorkflowAsync_WhenHandlerKeyIsMissing_DoesNotThrow()
    {
        var services = new ServiceCollection();
        services.AddTransient<DynamicWorkflowEngine>();
        var engine = services.BuildServiceProvider().GetRequiredService<DynamicWorkflowEngine>();
        var context = new OrderContext
        {
            OrderId = "ORD-MISS",
            Amount = 100m,
            CustomerAge = 25,
            CreditScore = 800
        };

        var exception = await Record.ExceptionAsync(() =>
            engine.ExecuteWorkflowAsync("OrderProcessing", context));

        Assert.Null(exception);
        Assert.False(context.IsAborted);
    }

    private static DynamicWorkflowEngine CreateEngine(RecordingHandler inventory, RecordingHandler payment)
    {
        var services = new ServiceCollection();
        services.AddKeyedSingleton<IWorkflowHandler>("InventoryService", inventory);
        services.AddKeyedSingleton<IWorkflowHandler>("PaymentService", payment);
        services.AddTransient<DynamicWorkflowEngine>();
        return services.BuildServiceProvider().GetRequiredService<DynamicWorkflowEngine>();
    }
}
