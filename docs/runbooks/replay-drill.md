# Runbook: Replay drill

## Goal

Confirm that **ReplayRecovery** can exercise a replay job and that projections remain consistent with expectations after a controlled replay (learning-platform scope).

## Preconditions

- PostgreSQL available for `replay_recovery_dev` (see `ReplayRecovery.Api` `appsettings.json`).
- **QueryReadModel** (or target projection owner) database reachable if validating rebuilt projections.
- Correlation and tenant headers understood by callers (`docs/platform/integration-event-contract.md`).

## Steps

1. Start **ReplayRecovery.Api** (default port `5015` in `launchSettings.json`).
2. Start **QueryReadModel.Api** if validating session overview or dashboard projections (`5009`).
3. **Start replay job**  
   `POST /api/v1/replay-recovery/replay-jobs/start` with body `{ "replayMode": "Deterministic", "projectionSetName": "<set>" }` and a valid write scope / bypass per environment.
4. **Advance checkpoints** as needed: `POST /api/v1/replay-recovery/replay-jobs/{id}/checkpoints`.
5. **Complete or fail** the job intentionally once behaviour is verified: `.../complete` or `.../fail`.
6. Compare projection row counts or sample keys before/after (application-specific queries or DB inspection).
7. Record outcome, job id, projection set, and timestamp in change or drill log.

## Failure modes

- **Job stuck running**: check PostgreSQL connectivity, outbox relay config in `RealtimePlatform:OutboxRelay`, and logs for poison messages.
- **Wrong projection set**: cancel job and restart with corrected `projectionSetName`.

## Evidence

- Replay job id (ULID), final state, final checkpoint sequence.
- Screenshots or query output for projection sanity checks.
