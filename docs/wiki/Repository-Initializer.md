# Repository Initializer (CLI)

Use the initializer to create a ready-to-use repository with the required AI governance files and structure.

## Scripts
- scripts/init-governance.ps1
- scripts/init-governance.sh

## Usage
Provide exactly one argument: the path to SPECIFICATION.md.
The filename **must** be SPECIFICATION.md.

Run the script from a cloned copy of this governance repository so it can copy the required files.

### Example
- PowerShell:
	```powershell
	.\scripts\init-governance.ps1 .\SPECIFICATION.md
	```
- Bash:
	```bash
	./scripts/init-governance.sh ./SPECIFICATION.md
	```

The initializer:
- Creates the required governance structure
- Copies governance files (excluding any .git data)
- Places the provided SPECIFICATION.md at /spec/SPECIFICATION.md
- Aborts if the target already contains required files or directories

## Target Root Resolution
- If the provided SPECIFICATION.md is located under a spec/ folder, the target root is the parent of spec/
- Otherwise, the target root is the directory containing SPECIFICATION.md

## Notes
- This is the recommended way to reuse the governance system in downstream projects.
- After initialization, run the VS Code tasks to bootstrap policy and start the workflow.
