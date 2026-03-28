---
name: Iteration 18 Hardening pre-production
overview: Add operational runbooks, pre-production acceptance checklist, CI test TRX artifacts as release evidence, and a local multi-service health script aligned with iteration 18 deliverables.
todos:
  - id: i18-plan-docs
    content: Runbooks + PRE-PRODUCTION-CHECKLIST under docs/
    status: completed
  - id: i18-script
    content: scripts/verify-local-api-health.sh for documented API ports
    status: completed
  - id: i18-ci
    content: GitHub Actions upload TestResults TRX artifacts (always)
    status: completed
isProject: true
---

## Context

Roadmap Iteration 18 (hardening, failover/replay/audit procedures, release gates, OAC). Repository is a learning PDMS; deliver **actionable ops docs** and **automated regression evidence** without requiring a full perf lab in CI.

## Files

- `docs/runbooks/*.md`, `docs/PRE-PRODUCTION-CHECKLIST.md`
- `scripts/verify-local-api-health.sh`
- `.github/workflows/dotnet.yml`
