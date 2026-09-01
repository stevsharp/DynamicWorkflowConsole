using DynamicWorkflowConsole.Models;

namespace DynamicWorkflowConsole.Handlers;

public interface IWorkflowHandler
{
    Task HandleAsync(OrderContext context, Func<Task> next);
}
