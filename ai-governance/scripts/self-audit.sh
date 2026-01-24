#!/usr/bin/env bash
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(cd "${SCRIPT_DIR}/.." && pwd)"
OUTPUT_PATH="${1:-$REPO_ROOT/.vscode/audit.json}"

log_json() {
  local level="$1"; local message="$2"; local data="$3"
  printf '{"timestamp":"%s","level":"%s","message":"%s","data":%s}\n' "$(date -u +%Y-%m-%dT%H:%M:%SZ)" "$level" "$message" "$data"
}

log_json "info" "selfaudit.start" "{}"

errors=()
warnings=()

add_error() { errors+=("$1"); }

required_dirs=(.ai .vscode scripts templates docs plans src spec .github schemas)
for d in "${required_dirs[@]}"; do
  if [ ! -d "$REPO_ROOT/$d" ]; then
    add_error "Missing required directory: $d"
  fi
done

required_files=(CONSTITUTION.md MEMORY-LEDGER.md TODO-LEDGER.md CHANGELOG.md)
for f in "${required_files[@]}"; do
  if [ ! -f "$REPO_ROOT/$f" ]; then
    add_error "Missing required file: $f"
  fi
done

if [ ! -f "$REPO_ROOT/governance.config.json" ] && [ ! -f "$REPO_ROOT/POLICY.md" ]; then
  add_error "Missing policy file: governance.config.json or POLICY.md"
fi

if [ ! -f "$REPO_ROOT/spec/SPECIFICATION.md" ]; then
  add_error "Missing spec/SPECIFICATION.md"
fi

if [ ! -f "$REPO_ROOT/schemas/governance.schema.json" ]; then
  add_error "Missing schema: schemas/governance.schema.json"
fi

if [ ! -f "$REPO_ROOT/manifest.json" ]; then
  add_error "Missing manifest.json"
fi

if [ ! -f "$REPO_ROOT/.vscode/tasks.json" ]; then
  add_error "Missing .vscode/tasks.json"
else
  required_tasks=(
    "Governance Preflight"
    "Governance Init Repository"
    "Governance Policy Validate"
    "Governance Git Init"
    "Governance Wiki Sync"
    "Governance Self-Audit"
    "Governance Bootstrap"
    "Governance Bootstrap (Defaults)"
    "Governance Policy Revision"
    "Set Autonomy Policy"
    "Set Workflow Mode"
    "Start Spec Implementation"
    "Governance Report Bundle"
  )
  for label in "${required_tasks[@]}"; do
    if ! grep -q "\"label\"[[:space:]]*:[[:space:]]*\"$label\"" "$REPO_ROOT/.vscode/tasks.json"; then
      add_error "Missing VS Code task: $label"
    fi
  done
fi

if [ -f "$REPO_ROOT/governance.config.json" ]; then
  required_keys=(version policyGeneratedBy bootstrap versionControl testing documentation language autonomy phases ciCdEnforced remoteRequired)
  for key in "${required_keys[@]}"; do
    if ! grep -q "\"$key\"" "$REPO_ROOT/governance.config.json"; then
      add_error "Missing policy key: $key"
    fi
  done
fi

if [ -d "$REPO_ROOT/scripts" ]; then
  for f in "$REPO_ROOT"/scripts/*.ps1; do
    [ -e "$f" ] || continue
    base="$(basename "$f" .ps1)"
    if [ ! -f "$REPO_ROOT/scripts/$base.sh" ]; then
      add_error "Missing .sh counterpart for scripts/$base.ps1"
    fi
  done
  for f in "$REPO_ROOT"/scripts/*.sh; do
    [ -e "$f" ] || continue
    base="$(basename "$f" .sh)"
    if [ ! -f "$REPO_ROOT/scripts/$base.ps1" ]; then
      add_error "Missing .ps1 counterpart for scripts/$base.sh"
    fi
  done
fi

mkdir -p "$(dirname "$OUTPUT_PATH")"

json_escape() {
  printf '%s' "$1" | sed 's/\\/\\\\/g; s/"/\\"/g'
}

errors_json=""
for err in "${errors[@]}"; do
  esc="$(json_escape "$err")"
  if [ -n "$errors_json" ]; then
    errors_json+=" , "
  fi
  errors_json+="\"$esc\""
done

ok="true"
if [ ${#errors[@]} -gt 0 ]; then
  ok="false"
fi

timestamp="$(date -u +%Y-%m-%dT%H:%M:%SZ)"
cat > "$OUTPUT_PATH" <<JSON
{
  "timestamp": "${timestamp}",
  "ok": ${ok},
  "errors": [${errors_json}],
  "warnings": []
}
JSON

if [ ${#errors[@]} -gt 0 ]; then
  log_json "error" "selfaudit.failed" "{\"output\":\"$OUTPUT_PATH\"}"
  echo "Self-audit failed. See $OUTPUT_PATH" >&2
  exit 1
fi

log_json "info" "selfaudit.passed" "{\"output\":\"$OUTPUT_PATH\"}"
echo "Self-audit passed. Report written to $OUTPUT_PATH"
