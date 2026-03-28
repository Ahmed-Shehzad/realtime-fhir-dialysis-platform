namespace RealtimeSurveillance.Application.Commands.EvaluateMapHypotensionRule;

public sealed record EvaluateMapHypotensionRuleResult(bool AlertRaised, Ulid? AlertId);
