#!/usr/bin/env bash
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(cd "${SCRIPT_DIR}/.." && pwd)"
POLICY_PATH="$REPO_ROOT/governance.config.json"
SPEC_PATH="$REPO_ROOT/spec/SPECIFICATION.md"

log_json() {
  local level="$1"; local message="$2"; local data="$3"
  printf '{"timestamp":"%s","level":"%s","message":"%s","data":%s}\n' "$(date -u +%Y-%m-%dT%H:%M:%SZ)" "$level" "$message" "$data"
}

if [ ! -f "$POLICY_PATH" ]; then
  log_json "error" "specstart.missing_policy" "{\"path\":\"$POLICY_PATH\"}"
  echo "Missing governance.config.json. Run the Governance Bootstrap task first." >&2
  exit 1
fi

if [ ! -f "$SPEC_PATH" ]; then
  log_json "error" "specstart.missing_spec" "{\"path\":\"$SPEC_PATH\"}"
  echo "Missing spec/SPECIFICATION.md. Create it before starting from spec." >&2
  exit 1
fi

echo "Spec execution can begin. Follow spec/SPECIFICATION.md using the active governance policy."
log_json "info" "specstart.ready" "{\"policy\":\"$POLICY_PATH\",\"spec\":\"$SPEC_PATH\"}"
