# Runbook: Load smoke (lightweight performance check)

## Goal

Quick sanity check that APIs respond under light concurrency **before** a demo, class, or tagged release — not a substitute for formal load testing.

## Preconditions

- Target base URLs known (local ports in each `launchSettings.json`, or staging hostnames).
- JWT or `DevelopmentBypass` behaviour understood for the environment.

## Option A: Sequential curl

From repo root with services running:

```bash
for p in $(seq 5001 5016); do curl -s -o /dev/null -w "%{http_code} :$p\n" "http://127.0.0.1:$p/health"; done
```

Investigate any non-`200` lines.

## Option B: Short burst (hey)

If [hey](https://github.com/rakyll/hey) is installed:

```bash
hey -n 200 -c 10 http://127.0.0.1:5001/health
```

Record p95 latency and error count. Repeat for critical write paths only on non-production data.

## Evidence

- Command, environment, date, p95 / error rate (or HTTP status summary).
