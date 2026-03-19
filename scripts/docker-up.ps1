# Idempotent Docker startup for the backend (Postgres + API).
# Usage:
#   ./scripts/docker-up.ps1
#   ./scripts/docker-up.ps1 -Build
param(
  [switch]$Build,
  [int]$TimeoutSeconds = 120
)

$ErrorActionPreference = "Stop"

$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Definition
$RepoRoot = Resolve-Path (Join-Path $ScriptDir "..")
Set-Location $RepoRoot

if (-not (Test-Path ".env") -and (Test-Path ".env.example")) {
  Write-Host "No .env found; copying .env.example -> .env (edit it to match your setup)."
  Copy-Item ".env.example" ".env" -Force
}

$composeArgs = @("-f", "compose.yml", "up", "-d")
if ($Build) {
  $composeArgs += @("--build")
}

Write-Host "Starting containers (idempotent): docker compose $($composeArgs -join ' ')"
docker compose @composeArgs

Write-Host "Waiting for Postgres to be healthy..."
$start = Get-Date
while ($true) {
  $elapsed = (Get-Date) - $start
  if ($elapsed.TotalSeconds -gt $TimeoutSeconds) {
    throw "Timed out waiting for Postgres health after $TimeoutSeconds seconds."
  }

  # pg_isready is available in the postgres image
  $cmd = "pg_isready -U blumberg -d blumberg"
  $result = docker compose -f compose.yml exec -T database sh -lc $cmd 2>$null

  if ($LASTEXITCODE -eq 0) {
    Write-Host "Postgres is ready."
    break
  }

  Start-Sleep -Seconds 2
}

Write-Host "Backend is up. API should be available at http://localhost:5000"

