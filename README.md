# Dynamic Workflow Console

A .NET 9 console sample that runs a **database-driven order workflow**: each step is resolved from DI by a handler key, then gated by JSON rules evaluated with [RulesEngine](https://github.com/microsoft/RulesEngine).

## What it demonstrates

- Keyed services (`.NET 8+`) to map a string such as `InventoryService` to an `IWorkflowHandler`
- Per-step rule JSON (the shape you would store in a database)
- Short-circuiting the pipeline when a rule fails (`OrderContext.Abort`)
- A two-step order flow: **inventory reservation** then **payment**

```text
OrderContext
    │
    ▼
Step 1  InventoryRules  →  InventoryHandler
    │
    ▼
Step 2  PaymentRules    →  PaymentHandler
```

## Rules

Rules are Newtonsoft JSON arrays of `Workflow` documents.

| Step | Handler key | Rule | Expression |
|------|-------------|------|------------|
| 1 | `InventoryService` | Customer must be 18+ | `CustomerAge >= 18` |
| 2 | `PaymentService` | Orders over $5,000 need credit ≥ 700 | `Amount <= 5000 \|\| CreditScore >= 700` |

The default `Program` scenario is a **passing age check** and a **failing high-value credit check** (`$6,500` / score `650`).

## Requirements

- [.NET 9 SDK](https://dotnet.microsoft.com/download)

## Run

```bash
dotnet run --project src/DynamicWorkflowConsole/DynamicWorkflowConsole.csproj
```

Expected console output (abbreviated):

```text
=== Executing Step 1: InventoryService ===
 --> Evaluating DB Rules Engine...
     [PASSED] Rule 'CustomerAgeCheck'
[Step: Inventory] Stock reserved for Order ORD-999.

=== Executing Step 2: PaymentService ===
 --> Evaluating DB Rules Engine...
     [FAILED] Rule 'HighValueCreditCheck': Orders over $5,000 require a Credit Score of at least 700.
```

Payment is not executed after the credit rule fails.

## Tests

```bash
dotnet test DynamicWorkflowConsole.sln
```

Open `DynamicWorkflowConsole.sln` in Visual Studio — the console app and the xUnit project appear as **two separate projects**.

Coverage includes:

- `OrderContext` abort state
- `InventoryHandler` / `PaymentHandler` calling `next`
- Engine paths: both steps succeed, underage abort, high-value credit abort, amount under $5,000, missing keyed handler

## Solution layout

```text
DynamicWorkflowConsole.sln
├── src/DynamicWorkflowConsole/
│   ├── Program.cs
│   ├── Models/          OrderContext, DbWorkflowStep
│   ├── Handlers/        IWorkflowHandler, Inventory, Payment
│   └── Engine/          DynamicWorkflowEngine
├── tests/DynamicWorkflowConsole.Tests/
│   ├── Models/
│   ├── Handlers/
│   ├── Engine/
│   └── Fakes/
└── README.md
```

## Packages

| Package | Role |
|---------|------|
| `Microsoft.Extensions.DependencyInjection` | Keyed handler registration |
| `Newtonsoft.Json` | Deserialize step rule JSON |
| `RulesEngine` 6.x | Evaluate `Workflow` / `Rule` expressions |

RulesEngine 6 uses `RulesEngine.Models.Workflow` (the older name was `WorkflowRules`).
