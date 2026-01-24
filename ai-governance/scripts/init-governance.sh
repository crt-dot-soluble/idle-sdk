#!/usr/bin/env bash
set -euo pipefail

log_json() {
  local level="$1"; local message="$2"; local data="$3"
  printf '{"timestamp":"%s","level":"%s","message":"%s","data":%s}\n' "$(date -u +%Y-%m-%dT%H:%M:%SZ)" "$level" "$message" "$data"
}

if [ "$#" -ne 1 ]; then
  log_json "error" "initgovernance.invalid_args" "{}"
  echo "Exactly one argument is required: path to SPECIFICATION.md" >&2
  exit 1
fi

SPEC_PATH="$1"
if [ ! -f "$SPEC_PATH" ]; then
  log_json "error" "initgovernance.missing_spec" "{\"path\":\"$SPEC_PATH\"}"
  echo "SPECIFICATION.md not found at $SPEC_PATH" >&2
  exit 1
fi

SPEC_BASENAME="$(basename "$SPEC_PATH")"
if [ "$SPEC_BASENAME" != "SPECIFICATION.md" ]; then
  log_json "error" "initgovernance.invalid_spec_name" "{\"name\":\"$SPEC_BASENAME\"}"
  echo "Invalid spec filename. Expected SPECIFICATION.md" >&2
  exit 1
fi

SPEC_DIR="$(cd "$(dirname "$SPEC_PATH")" && pwd)"
if [ "$(basename "$SPEC_DIR")" = "spec" ]; then
  TARGET_ROOT="$(cd "$SPEC_DIR/.." && pwd)"
else
  TARGET_ROOT="$SPEC_DIR"
fi

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
REPO_ROOT="$(cd "${SCRIPT_DIR}/.." && pwd)"


required_dirs=(.ai .vscode scripts templates docs plans src spec .github)
for dir in "${required_dirs[@]}"; do
  if [ -e "$TARGET_ROOT/$dir" ]; then
    log_json "error" "initgovernance.target_conflict" "{\"path\":\"$TARGET_ROOT/$dir\"}"
    echo "Target already contains $dir at $TARGET_ROOT/$dir. Aborting to avoid overwrite." >&2
    exit 1
  fi
done


required_files=(CONSTITUTION.md CHANGELOG.md MEMORY-LEDGER.md TODO-LEDGER.md README.md governance.config.json ai-governance.code-workspace)
for file in "${required_files[@]}"; do
  if [ -e "$TARGET_ROOT/$file" ]; then
    log_json "error" "initgovernance.target_conflict" "{\"path\":\"$TARGET_ROOT/$file\"}"
    echo "Target already contains $file at $TARGET_ROOT/$file. Aborting to avoid overwrite." >&2
    exit 1
  fi
done


cp -R "$REPO_ROOT/.ai" "$TARGET_ROOT/.ai"
cp -R "$REPO_ROOT/.vscode" "$TARGET_ROOT/.vscode"
cp -R "$REPO_ROOT/scripts" "$TARGET_ROOT/scripts"
cp -R "$REPO_ROOT/templates" "$TARGET_ROOT/templates"
cp -R "$REPO_ROOT/docs" "$TARGET_ROOT/docs"
cp -R "$REPO_ROOT/plans" "$TARGET_ROOT/plans"
cp -R "$REPO_ROOT/src" "$TARGET_ROOT/src"
cp -R "$REPO_ROOT/.github" "$TARGET_ROOT/.github"

cp "$REPO_ROOT/templates/CONSTITUTION.md" "$TARGET_ROOT/CONSTITUTION.md"
cp "$REPO_ROOT/templates/CHANGELOG.md" "$TARGET_ROOT/CHANGELOG.md"
cp "$REPO_ROOT/templates/MEMORY-LEDGER.md" "$TARGET_ROOT/MEMORY-LEDGER.md"
cp "$REPO_ROOT/templates/TODO-LEDGER.md" "$TARGET_ROOT/TODO-LEDGER.md"
cp "$REPO_ROOT/templates/README.md" "$TARGET_ROOT/README.md"
cp "$REPO_ROOT/governance.config.json" "$TARGET_ROOT/governance.config.json"
cp "$REPO_ROOT/templates/ai-governance.code-workspace" "$TARGET_ROOT/ai-governance.code-workspace"

mkdir -p "$TARGET_ROOT/spec"

cp "$SPEC_PATH" "$TARGET_ROOT/spec/SPECIFICATION.md"

echo "Initialized governance repository in $TARGET_ROOT.\n\nPlaced SPECIFICATION.md in the 'spec' folder."
log_json "info" "initgovernance.done" "{\"target\":\"$TARGET_ROOT\"}"
