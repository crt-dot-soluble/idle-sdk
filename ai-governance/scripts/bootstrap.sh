#!/usr/bin/env bash
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(cd "${SCRIPT_DIR}/.." && pwd)"

log_json() {
  local level="$1"; local message="$2"; local data="$3"
  printf '{"timestamp":"%s","level":"%s","message":"%s","data":%s}\n' "$(date -u +%Y-%m-%dT%H:%M:%SZ)" "$level" "$message" "$data"
}


MODE="${1:-}"
VERSION_CONTROL="${2:-}"
TESTING="${3:-}"
DOCUMENTATION="${4:-}"
LANGUAGE="${5:-}"
FRAMEWORKS="${6:-}"
AUTONOMY="${7:-}"

log_json "info" "bootstrap.start" "{\"mode\":\"$MODE\"}"

if [ -z "$MODE" ]; then
  log_json "error" "bootstrap.missing_mode" "{}"
  echo "Missing bootstrap mode. Re-run the Governance Bootstrap task and select a mode." >&2
  exit 1
fi

if [ "$MODE" != "defaults" ] && [ "$MODE" != "customize" ]; then
  echo "Invalid Mode" >&2
  exit 1
fi

if [ "$MODE" != "customize" ]; then
  VERSION_CONTROL="git-remote-ci"
  TESTING="full"
  DOCUMENTATION="inline"
  LANGUAGE="unspecified"
  FRAMEWORKS=""
  AUTONOMY="feature"
else
  if [ -z "$VERSION_CONTROL" ] || [ -z "$TESTING" ] || [ -z "$DOCUMENTATION" ] || [ -z "$LANGUAGE" ] || [ -z "$AUTONOMY" ]; then
    echo "Missing bootstrap inputs. Re-run the Governance Bootstrap task and complete all pickers." >&2
    exit 1
  fi
fi

case "$VERSION_CONTROL" in
  git-local|git-remote|git-remote-ci) ;;
  *) echo "Invalid VersionControl" >&2; exit 1 ;;
esac

case "$TESTING" in
  full|baseline) ;;
  *) echo "Invalid Testing" >&2; exit 1 ;;
esac

case "$DOCUMENTATION" in
  inline|comments-only|generate) ;;
  *) echo "Invalid Documentation" >&2; exit 1 ;;
esac

case "$AUTONOMY" in
  feature|milestone|fully-autonomous) ;;
  *) echo "Invalid Autonomy" >&2; exit 1 ;;
esac

if [ "$FRAMEWORKS" = "None" ]; then
  FRAMEWORKS=""
fi

if [ -f "${REPO_ROOT}/README.md" ]; then
  if grep -q "AI AGENTS MUST IGNORE THIS FILE" "${REPO_ROOT}/README.md"; then
    rm -f "${REPO_ROOT}/README.md"
    echo "Removed default README.md"
  fi
fi

SPEC_PATH="${REPO_ROOT}/spec/SPECIFICATION.md"
if [ ! -f "$SPEC_PATH" ]; then
  log_json "error" "bootstrap.missing_spec" "{\"path\":\"$SPEC_PATH\"}"
  echo "Missing spec/SPECIFICATION.md. Create it before running bootstrap." >&2
  exit 1
fi

json_escape() {
  printf '%s' "$1" | sed 's/\\/\\\\/g; s/"/\\"/g'
}

frameworks_json="[]"
if [ -n "$FRAMEWORKS" ]; then
  IFS=',' read -r -a fw_items <<< "$FRAMEWORKS"
  fw_join=""
  for item in "${fw_items[@]}"; do
    item="$(printf '%s' "$item" | sed 's/^[[:space:]]*//;s/[[:space:]]*$//')"
    [ -z "$item" ] && continue
    esc="$(json_escape "$item")"
    if [ -n "$fw_join" ]; then
      fw_join+=" , "
    fi
    fw_join+="\"$esc\""
  done
  if [ -n "$fw_join" ]; then
    frameworks_json="[$fw_join]"
  fi
fi

timestamp="$(date +%Y-%m-%d)"
skipped="false"
if [ "$MODE" != "customize" ]; then
  skipped="true"
fi

remote_required="true"
if [ "$VERSION_CONTROL" = "git-local" ]; then
  remote_required="false"
fi

cat > "${REPO_ROOT}/governance.config.json" <<JSON
{
  "version": "1.0.0",
  "policyGeneratedBy": "bootstrap",
  "bootstrap": {
    "mode": "${MODE}",
    "skipped": ${skipped},
    "timestamp": "${timestamp}"
  },
  "versionControl": "${VERSION_CONTROL}",
  "testing": "${TESTING}",
  "documentation": "${DOCUMENTATION}",
  "language": {
    "primary": "${LANGUAGE}",
    "frameworks": ${frameworks_json}
  },
  "autonomy": "${AUTONOMY}",
  "phases": {"required": true, "list": [0, 1, 2, 3, 4, 5]},
  "ciCdEnforced": true,
  "remoteRequired": ${remote_required}
}
JSON

echo "Wrote governance policy to governance.config.json"

log_json "info" "bootstrap.policy_written" "{\"path\":\"$REPO_ROOT/governance.config.json\"}"
