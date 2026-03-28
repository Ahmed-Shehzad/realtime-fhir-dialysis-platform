#!/usr/bin/env bash
set -euo pipefail

# Default ports from platform service launchSettings.json (5001–5016).
ports=(5001 5002 5003 5004 5005 5006 5007 5008 5009 5010 5011 5012 5013 5014 5015 5016)
host="${HEALTH_CHECK_HOST:-127.0.0.1}"
failures=0

for p in "${ports[@]}"; do
  code=$(curl -s -o /dev/null -w "%{http_code}" --connect-timeout 2 --max-time 5 \
    "http://${host}:${p}/health" || printf '%s' "000")
  if [[ "$code" != "200" ]]; then
    echo "FAIL http://${host}:${p}/health -> HTTP $code"
    failures=$((failures + 1))
  else
    echo "OK   http://${host}:${p}/health"
  fi
done

if [[ "$failures" -gt 0 ]]; then
  echo "Health check failed for $failures endpoint(s)."
  exit 1
fi

echo "All ${#ports[@]} endpoints returned HTTP 200."
