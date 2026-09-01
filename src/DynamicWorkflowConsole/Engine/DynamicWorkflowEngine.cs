using DynamicWorkflowConsole.Handlers;
using DynamicWorkflowConsole.Models;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using RulesEngine.Models;

namespace DynamicWorkflowConsole.Engine;

public class DynamicWorkflowEngine(IServiceProvider serviceProvider)
{
    private readonly IServiceProvider _serviceProvider = serviceProvider;

    public async Task ExecuteWorkflowAsync(string workflowName, OrderContext context)
    {
        List<DbWorkflowStep> dbSteps = FetchStepsFromDatabase();

        foreach (var step in dbSteps)
        {
            if (context.IsAborted)
            {
                Console.WriteLine($"\n[WORKFLOW TERMINATED] Reason: {context.AbortReason}");
                break;
            }

            Console.WriteLine($"\n=== Executing Step {step.StepOrder}: {step.HandlerKey} ===");

            if (!string.IsNullOrWhiteSpace(step.StepRulesJson))
            {
                bool rulesPassed = await EvaluateRulesAsync(step.StepRulesJson, context);
                if (!rulesPassed)
                {
                    context.Abort($"Rule validation failed at step '{step.HandlerKey}'.");
                    continue;
                }
            }

            var handler = _serviceProvider.GetKeyedService<IWorkflowHandler>(step.HandlerKey);
            if (handler != null)
            {
                await handler.HandleAsync(context, () => Task.CompletedTask);
            }
            else
            {
                Console.WriteLine($"[Error] Could not resolve service for key: {step.HandlerKey}");
            }
        }
    }

    private async Task<bool> EvaluateRulesAsync(string json, OrderContext context)
    {
        Console.WriteLine(" --> Evaluating DB Rules Engine...");

        var workflowList = JsonConvert.DeserializeObject<List<Workflow>>(json);
        if (workflowList == null || workflowList.Count == 0)
        {
            Console.WriteLine(" --> [RulesEngine] No valid rules found in JSON.");
            return true;
        }

        var rulesEngine = new RulesEngine.RulesEngine(workflowList.ToArray());

        List<RuleResultTree> results = await rulesEngine.ExecuteAllRulesAsync(workflowList[0].WorkflowName, context);

        bool allPassed = true;
        foreach (var result in results)
        {
            if (result.IsSuccess)
            {
                Console.WriteLine($"     [PASSED] Rule '{result.Rule.RuleName}'");
            }
            else
            {
                Console.WriteLine($"     [FAILED] Rule '{result.Rule.RuleName}': {result.Rule.ErrorMessage}");
                allPassed = false;
            }
        }

        return allPassed;
    }

    private List<DbWorkflowStep> FetchStepsFromDatabase()
    {
        return
        [
            new DbWorkflowStep
            {
                StepOrder = 1,
                HandlerKey = "InventoryService",
                StepRulesJson = @"[
                  {
                    'WorkflowName': 'InventoryRules',
                    'Rules': [
                      {
                        'RuleName': 'CustomerAgeCheck',
                        'ErrorMessage': 'Customer must be 18 or older to buy items.',
                        'Expression': 'CustomerAge >= 18'
                      }
                    ]
                  }
                ]"
            },
            new DbWorkflowStep
            {
                StepOrder = 2,
                HandlerKey = "PaymentService",
                StepRulesJson = @"[
                  {
                    'WorkflowName': 'PaymentRules',
                    'Rules': [
                      {
                        'RuleName': 'HighValueCreditCheck',
                        'ErrorMessage': 'Orders over $5,000 require a Credit Score of at least 700.',
                        'Expression': 'Amount <= 5000 || CreditScore >= 700'
                      }
                    ]
                  }
                ]"
            }
        ];
    }
}
