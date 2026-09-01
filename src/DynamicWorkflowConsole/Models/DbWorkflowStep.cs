namespace DynamicWorkflowConsole.Models;

public class DbWorkflowStep
{
    public int StepOrder { get; set; }
    public string HandlerKey { get; set; } = string.Empty;
    public string StepRulesJson { get; set; } = string.Empty;
}
