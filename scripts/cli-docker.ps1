param(
  [Parameter(ValueFromRemainingArguments = $true)]
  [string[]]$CommandArgs
)

$ErrorActionPreference = "Stop"

$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Definition
$RepoRoot = Resolve-Path (Join-Path $ScriptDir "..")
Set-Location $RepoRoot

if (-not $CommandArgs -or $CommandArgs.Count -eq 0) {
  Write-Host "Usage: ./scripts/cli-docker.ps1 <command> [args...]"
  Write-Host "Example: ./scripts/cli-docker.ps1 nukeAndPave"
  exit 1
}

# Run CLI inside the API container to use container network/env (DB_HOST=postgres, DB_PORT=5432).
docker compose -f compose.yml exec api dotnet /app/cli/CLI.dll @CommandArgs

