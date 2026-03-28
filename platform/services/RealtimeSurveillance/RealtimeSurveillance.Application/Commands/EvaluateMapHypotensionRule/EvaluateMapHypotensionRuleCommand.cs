using Intercessor.Abstractions;

namespace RealtimeSurveillance.Application.Commands.EvaluateMapHypotensionRule;

public sealed record EvaluateMapHypotensionRuleCommand(
    Ulid CorrelationId,
    string TreatmentSessionId,
    string RuleCode,
    double MetricValueMmHg,
    string? AuthenticatedUserId = null) : ICommand<EvaluateMapHypotensionRuleResult>;
