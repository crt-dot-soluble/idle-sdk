#!/usr/bin/env bash
set -euo pipefail

VERSION_CONTROL="${1:-}"
TESTING="${2:-}"
DOCUMENTATION="${3:-}"
LANGUAGE="${4:-}"
FRAMEWORKS="${5:-}"
AUTONOMY="${6:-}"

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(cd "${SCRIPT_DIR}/.." && pwd)"
POLICY_PATH="$REPO_ROOT/governance.config.json"

log_json() {
  local level="$1"; local message="$2"; local data="$3"
  printf '{"timestamp":"%s","level":"%s","message":"%s","data":%s}\n' "$(date -u +%Y-%m-%dT%H:%M:%SZ)" "$level" "$message" "$data"
}

if [ ! -f "$POLICY_PATH" ]; then
  log_json "error" "policy.missing" "{\"path\":\"$POLICY_PATH\"}"
  echo "Missing governance.config.json at $POLICY_PATH"
  exit 1
fi
get_string() {
  sed -n "s/^[[:space:]]*\"$1\"[[:space:]]*:[[:space:]]*\"\([^\"]*\)\".*/\1/p" "$POLICY_PATH" | head -n1
}

get_bool() {
  sed -n "s/^[[:space:]]*\"$1\"[[:space:]]*:[[:space:]]*\(true\|false\).*/\1/p" "$POLICY_PATH" | head -n1
}

get_array_raw() {
  sed -n "s/.*\"$1\"[[:space:]]*:[[:space:]]*\[\(.*\)\].*/\1/p" "$POLICY_PATH" | head -n1
}

json_escape() {
  printf '%s' "$1" | sed 's/\\/\\\\/g; s/"/\\"/g'
}

current_version="$(get_string version)"
current_generated="$(get_string policyGeneratedBy)"
bootstrap_mode="$(get_string mode)"
bootstrap_skipped="$(get_bool skipped)"
bootstrap_timestamp="$(get_string timestamp)"
current_version_control="$(get_string versionControl)"
current_testing="$(get_string testing)"
current_documentation="$(get_string documentation)"
current_language="$(get_string primary)"
current_frameworks_raw="$(get_array_raw frameworks)"
current_autonomy="$(get_string autonomy)"
current_phases_required="$(get_bool required)"
current_phases_list_raw="$(get_array_raw list)"
current_cicd="$(get_bool ciCdEnforced)"

current_version="${current_version:-1.0.0}"
current_generated="${current_generated:-bootstrap}"
bootstrap_mode="${bootstrap_mode:-defaults}"
bootstrap_skipped="${bootstrap_skipped:-true}"
bootstrap_timestamp="${bootstrap_timestamp:-$(date +%Y-%m-%d)}"
current_phases_required="${current_phases_required:-true}"
current_phases_list_raw="${current_phases_list_raw:-0, 1, 2, 3, 4, 5}"
current_cicd="${current_cicd:-true}"

version_control="$current_version_control"
testing="$current_testing"
documentation="$current_documentation"
language="$current_language"
autonomy="$current_autonomy"

if [ -n "$VERSION_CONTROL" ]; then
  case "$VERSION_CONTROL" in
    git-local|git-remote|git-remote-ci) ;;
    *) echo "Invalid VersionControl" >&2; exit 1 ;;
  esac
  version_control="$VERSION_CONTROL"
fi

if [ -n "$TESTING" ]; then
  case "$TESTING" in
    full|baseline) ;;
    *) echo "Invalid Testing" >&2; exit 1 ;;
  esac
  testing="$TESTING"
fi

if [ -n "$DOCUMENTATION" ]; then
  case "$DOCUMENTATION" in
    inline|comments-only|generate) ;;
    *) echo "Invalid Documentation" >&2; exit 1 ;;
  esac
  documentation="$DOCUMENTATION"
fi

if [ -n "$LANGUAGE" ]; then
  language="$LANGUAGE"
fi

if [ -n "$AUTONOMY" ]; then
  case "$AUTONOMY" in
    feature|milestone|fully-autonomous) ;;
    *) echo "Invalid Autonomy" >&2; exit 1 ;;
  esac
  autonomy="$AUTONOMY"
fi

frameworks_json="[]"
if [ -n "$FRAMEWORKS" ]; then
  if [ "$FRAMEWORKS" = "None" ]; then
    frameworks_json="[]"
  else
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
    frameworks_json="[$fw_join]"
  fi
else
  if [ -n "$current_frameworks_raw" ]; then
    frameworks_json="[$current_frameworks_raw]"
  fi
fi

phases_required="${current_phases_required:-true}"
phases_list="${current_phases_list_raw:-0, 1, 2, 3, 4, 5}"

remote_required="true"
if [ "$version_control" = "git-local" ]; then
  remote_required="false"
fi

cat > "$POLICY_PATH" <<JSON
{
  "version": "${current_version}",
  "policyGeneratedBy": "${current_generated}",
  "bootstrap": {
    "mode": "${bootstrap_mode}",
    "skipped": ${bootstrap_skipped},
    "timestamp": "${bootstrap_timestamp}"
  },
  "versionControl": "${version_control}",
  "testing": "${testing}",
  "documentation": "${documentation}",
  "language": {
    "primary": "${language}",
    "frameworks": ${frameworks_json}
  },
  "autonomy": "${autonomy}",
  "phases": {"required": ${phases_required}, "list": [${phases_list}]},
  "ciCdEnforced": ${current_cicd},
  "remoteRequired": ${remote_required}
}
JSON

echo "Updated governance policy at $POLICY_PATH"
log_json "info" "policy.update.done" "{\"path\":\"$POLICY_PATH\"}"
