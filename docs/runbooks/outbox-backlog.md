# Runbook: Outbox backlog

## Goal

Diagnose when **outbox messages** are not published or are retrying excessively.

## Quick checks

1. **Database**: Table `outbox_messages` (schema from `RealtimePlatform:OutboxStorage`) — look for rows with `SentAt` null and rising `PublishAttemptCount`.
2. **Relay**: `RealtimePlatform:OutboxRelay:Enabled` and `PollInterval` / `MaxBatchSize` in the service `appsettings`.
3. **Source address**: `RealtimePlatform:OutboxPublisher:SourceAddress` should match the service base URL consumers expect.
4. **Logs**: search for outbox relay errors and serialization failures.

## Corrective actions

- Fix broker or HTTP endpoint targeted by the relay; redeploy.
- Quarantine or delete poison payloads only under a controlled procedure and with audit note (learning platform: document in drill log).
- Temporarily increase `MaxPublishAttempts` only while root cause is fixed (not a substitute for fixing the sink).

## Evidence

- Sample `MessageId`, `MessageType`, last error text, and counts before/after mitigation.
