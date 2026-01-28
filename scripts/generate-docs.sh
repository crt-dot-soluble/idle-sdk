#!/usr/bin/env bash
set -euo pipefail

CONFIGURATION="${1:-Debug}"
ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")/.." && pwd)"
CSPROJ="$ROOT_DIR/src/idle-sdk.core/IdleSdk.Core.csproj"
XML_PATH="$ROOT_DIR/src/idle-sdk.core/bin/$CONFIGURATION/net10.0/IdleSdk.Core.xml"
DOC_PATH="$ROOT_DIR/docs/GENERATED-API.md"

dotnet build "$CSPROJ" -c "$CONFIGURATION" >/dev/null

if [ ! -f "$XML_PATH" ]; then
  echo "XML documentation not found at $XML_PATH" >&2
  exit 1
fi

python - <<'PY'
import xml.etree.ElementTree as ET
from pathlib import Path

root = Path("$XML_PATH")
xml = ET.parse(root).getroot()
entries = []
for member in xml.findall(".//member"):
    name = member.attrib.get("name", "")
    if name.startswith("T:IdleSdk.Core.Assets"):
        summary = (member.findtext("summary") or "").strip() or "(No summary)"
        entries.append((name.replace("T:", ""), summary))

lines = [
    "# Generated API Documentation",
    "",
    "Generated from inline XML documentation for IdleSdk.Core.Assets.",
    ""
]
for name, summary in entries:
    lines.append(f"## {name}")
    lines.append("")
    lines.append(summary)
    lines.append("")

Path("$DOC_PATH").write_text("\n".join(lines), encoding="utf-8")
print(f"Generated {Path('$DOC_PATH')}")
PY
