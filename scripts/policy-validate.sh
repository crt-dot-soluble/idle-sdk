#!/usr/bin/env bash
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(cd "${SCRIPT_DIR}/.." && pwd)"
POLICY_PATH="$REPO_ROOT/governance.config.json"
SCHEMA_PATH="$REPO_ROOT/schemas/governance.schema.json"

log_json() {
  local level="$1"; local message="$2"; local data="$3"
  printf '{"timestamp":"%s","level":"%s","message":"%s","data":%s}\n' "$(date -u +%Y-%m-%dT%H:%M:%SZ)" "$level" "$message" "$data"
}

if [ ! -f "$POLICY_PATH" ]; then
  log_json "error" "policy.missing" "{\"path\":\"$POLICY_PATH\"}"
  echo "Missing governance.config.json at $POLICY_PATH"
  exit 1
fi

if [ ! -f "$SCHEMA_PATH" ]; then
  log_json "error" "schema.missing" "{\"path\":\"$SCHEMA_PATH\"}"
  echo "Missing schemas/governance.schema.json at $SCHEMA_PATH"
  exit 1
fi

get_string() {
  sed -n "s/.*\"$1\"[[:space:]]*:[[:space:]]*\"\([^\"]*\)\".*/\1/p" "$POLICY_PATH" | head -n1
}

get_bool() {
  sed -n "s/.*\"$1\"[[:space:]]*:[[:space:]]*\(true\|false\).*/\1/p" "$POLICY_PATH" | head -n1
}

errors=()

required_keys=(version policyGeneratedBy bootstrap versionControl testing documentation language autonomy phases ciCdEnforced remoteRequired)
for key in "${required_keys[@]}"; do
  if ! grep -q "\"$key\"" "$POLICY_PATH"; then
    errors+=("Missing policy key: $key")
  fi
  if ! grep -q "\"$key\"" "$SCHEMA_PATH"; then
    errors+=("Schema missing key: $key")
  fi
done

vc="$(get_string versionControl)"
testing="$(get_string testing)"
documentation="$(get_string documentation)"
autonomy="$(get_string autonomy)"
mode="$(get_string mode)"

case "$vc" in git-local|git-remote|git-remote-ci) ;; *) errors+=("Invalid versionControl: $vc");; esac
case "$testing" in full|baseline) ;; *) errors+=("Invalid testing: $testing");; esac
case "$documentation" in inline|comments-only|generate) ;; *) errors+=("Invalid documentation: $documentation");; esac
case "$autonomy" in feature|milestone|fully-autonomous) ;; *) errors+=("Invalid autonomy: $autonomy");; esac
case "$mode" in defaults|customize) ;; *) errors+=("Invalid bootstrap mode: $mode");; esac

if [ "$(get_bool ciCdEnforced)" != "true" ] && [ "$(get_bool ciCdEnforced)" != "false" ]; then
  errors+=("Invalid ciCdEnforced value")
fi
if [ "$(get_bool remoteRequired)" != "true" ] && [ "$(get_bool remoteRequired)" != "false" ]; then
  errors+=("Invalid remoteRequired value")
fi

if [ ${#errors[@]} -gt 0 ]; then
  log_json "error" "policy.validation_failed" "{}"
  printf '%s\n' "${errors[@]}" >&2
  exit 1
fi

echo "Policy validated: $POLICY_PATH"
log_json "info" "policy.validation_ok" "{\"path\":\"$POLICY_PATH\",\"schema\":\"$SCHEMA_PATH\"}"
