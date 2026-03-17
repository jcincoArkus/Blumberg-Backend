# Connect to ECS container (ECS Exec).
# Usage: .\scripts\connect-ecs-exec.ps1
#
# Credentials: if not in env, reads from encrypted file (DPAPI). If not saved, prompts and can save for next time.

param(
    [string] $Cluster = "an-blumberg-dev-backend-api-e85c-bc1e4de",
    [string] $Service = "an-blumberg-dev-backend-api-e85c-fb7406a",
    [string] $Container = "an-blumberg-dev-backend-api-e85c"
)

$ErrorActionPreference = "Stop"

$CredentialStoreDir = Join-Path $env:APPDATA "BlumbergEcsExec"
$CredentialStorePath = Join-Path $CredentialStoreDir "ecs-credentials.enc"

function Get-EncryptedCredentials {
    if (-not (Test-Path -LiteralPath $CredentialStorePath -PathType Leaf)) { return $null }
    try {
        Add-Type -AssemblyName System.Security -ErrorAction Stop
        $encryptedBytes = [System.IO.File]::ReadAllBytes($CredentialStorePath)
        $decryptedBytes = [System.Security.Cryptography.ProtectedData]::Unprotect(
            $encryptedBytes,
            $null,
            [System.Security.Cryptography.DataProtectionScope]::CurrentUser
        )
        $json = [System.Text.Encoding]::UTF8.GetString($decryptedBytes)
        return $json | ConvertFrom-Json
    } catch {
        return $null
    }
}

function Save-EncryptedCredentials {
    param([string]$AccessKey, [string]$SecretKey, [string]$Region, [string]$Output)
    try {
        Add-Type -AssemblyName System.Security -ErrorAction Stop
        $obj = @{
            AWS_ACCESS_KEY_ID     = $AccessKey
            AWS_SECRET_ACCESS_KEY = $SecretKey
            AWS_DEFAULT_REGION    = $Region
            AWS_DEFAULT_OUTPUT    = $Output
        }
        $json = $obj | ConvertTo-Json -Compress
        $bytes = [System.Text.Encoding]::UTF8.GetBytes($json)
        $encryptedBytes = [System.Security.Cryptography.ProtectedData]::Protect(
            $bytes,
            $null,
            [System.Security.Cryptography.DataProtectionScope]::CurrentUser
        )
        if (-not (Test-Path $CredentialStoreDir)) {
            New-Item -ItemType Directory -Path $CredentialStoreDir -Force | Out-Null
        }
        [System.IO.File]::WriteAllBytes($CredentialStorePath, $encryptedBytes)
        return $true
    } catch {
        return $false
    }
}

# --- 1. Check AWS CLI v2 and show version or NOTE message ---
$awsVersion = $null
try {
    $awsVersion = aws --version 2>&1 | Out-String
} catch {
    $awsVersion = $null
}

if (-not $awsVersion -or $awsVersion -notmatch "aws-cli/2") {
    Write-Host "NOTE: Before running this script, make sure AWS CLI v2 is installed." -ForegroundColor Yellow
    Write-Host "The command 'ecs execute-command' requires AWS CLI v2." -ForegroundColor Yellow
    Write-Host ""
    Write-Host "Download: https://aws.amazon.com/cli/" -ForegroundColor Gray
    Write-Host ""
    Write-Host "Waiting 30 seconds before exiting..." -ForegroundColor Yellow
    Start-Sleep -Seconds 30
    exit 1
}

Write-Host "AWS CLI: $($awsVersion.Trim())" -ForegroundColor Green
Write-Host ""

# --- 2. Use env vars, then encrypted store, then prompt; optionally save for next time ---
$weSetCredentials = $false
$weSetRegion = $false
$weSetOutput = $false
$wePromptedForCredentials = $false
$Region = $null
$Output = $null

if (-not [string]::IsNullOrWhiteSpace($env:AWS_ACCESS_KEY_ID) -and -not [string]::IsNullOrWhiteSpace($env:AWS_SECRET_ACCESS_KEY)) {
    Write-Host "Using credentials from environment (AWS_ACCESS_KEY_ID / AWS_SECRET_ACCESS_KEY)." -ForegroundColor Cyan
} else {
    $saved = Get-EncryptedCredentials
    if ($saved -and -not [string]::IsNullOrWhiteSpace($saved.AWS_ACCESS_KEY_ID) -and -not [string]::IsNullOrWhiteSpace($saved.AWS_SECRET_ACCESS_KEY)) {
        $env:AWS_ACCESS_KEY_ID = $saved.AWS_ACCESS_KEY_ID
        $env:AWS_SECRET_ACCESS_KEY = $saved.AWS_SECRET_ACCESS_KEY
        $Region = if ($saved.AWS_DEFAULT_REGION) { $saved.AWS_DEFAULT_REGION } else { "us-west-2" }
        $Output = if ($saved.AWS_DEFAULT_OUTPUT) { $saved.AWS_DEFAULT_OUTPUT } else { "json" }
        $env:AWS_DEFAULT_REGION = $Region
        $env:AWS_DEFAULT_OUTPUT = $Output
        $env:AWS_PROFILE = ""
        Write-Host "Using credentials from encrypted store ($CredentialStorePath)." -ForegroundColor Cyan
        $weSetCredentials = $true
        $weSetRegion = $true
        $weSetOutput = $true
    } else {
        $wePromptedForCredentials = $true
        $env:AWS_ACCESS_KEY_ID = Read-Host "AWS_ACCESS_KEY_ID"
        $env:AWS_SECRET_ACCESS_KEY = Read-Host "AWS_SECRET_ACCESS_KEY"
        if ([string]::IsNullOrWhiteSpace($env:AWS_ACCESS_KEY_ID) -or [string]::IsNullOrWhiteSpace($env:AWS_SECRET_ACCESS_KEY)) {
            Write-Host "Error: AWS_ACCESS_KEY_ID and AWS_SECRET_ACCESS_KEY are required." -ForegroundColor Red
            exit 1
        }
        $env:AWS_PROFILE = ""
        $weSetCredentials = $true
    }
}

if ($null -eq $Region) {
    if (-not [string]::IsNullOrWhiteSpace($env:AWS_DEFAULT_REGION)) {
        $Region = $env:AWS_DEFAULT_REGION
        Write-Host "Using region from environment: $Region" -ForegroundColor Cyan
    } elseif (-not [string]::IsNullOrWhiteSpace($env:AWS_REGION)) {
        $Region = $env:AWS_REGION
        Write-Host "Using region from environment: $Region" -ForegroundColor Cyan
    } else {
        $Region = Read-Host "AWS Region (default: us-west-2)"
        if ([string]::IsNullOrWhiteSpace($Region)) { $Region = "us-west-2" }
        $env:AWS_DEFAULT_REGION = $Region
        $weSetRegion = $true
    }
}

if ($null -eq $Output) {
    if (-not [string]::IsNullOrWhiteSpace($env:AWS_DEFAULT_OUTPUT)) {
        $Output = $env:AWS_DEFAULT_OUTPUT
    } else {
        $Output = Read-Host "Output format (default: json)"
        if ([string]::IsNullOrWhiteSpace($Output)) { $Output = "json" }
        $env:AWS_DEFAULT_OUTPUT = $Output
        $weSetOutput = $true
    }
}

# If we prompted for credentials (did not load from store), offer to save for next time
if ($wePromptedForCredentials) {
    $save = Read-Host "Save credentials for next time? (encrypted, same Windows user only) [Y/n]"
    if ([string]::IsNullOrWhiteSpace($save) -or $save -match '^[Yy]') {
        if (Save-EncryptedCredentials -AccessKey $env:AWS_ACCESS_KEY_ID -SecretKey $env:AWS_SECRET_ACCESS_KEY -Region $Region -Output $Output) {
            Write-Host "Credentials saved to encrypted store." -ForegroundColor Green
        } else {
            Write-Host "Could not save credentials (encryption not available)." -ForegroundColor Yellow
        }
    }
}

Write-Host ""
Write-Host "Region: $Region | Output: $Output" -ForegroundColor Cyan
Write-Host ""

try {
    # --- 3. Get RUNNING task (uses env credentials, no --profile) ---
    Write-Host "Getting RUNNING task from ECS service..." -ForegroundColor Cyan
    $taskArn = aws ecs list-tasks --cluster $Cluster --service-name $Service --desired-status RUNNING --region $Region --query "taskArns[0]" --output text
    if (-not $taskArn -or $taskArn -eq "None") {
        throw "No RUNNING task found in the service."
    }
    Write-Host "Task: $taskArn" -ForegroundColor Green
    Write-Host ""

    # --- 4. Connect to container ---
    Write-Host "Connecting to container (ECS Exec)..." -ForegroundColor Cyan
    aws ecs execute-command --region $Region --cluster $Cluster --task $taskArn --container $Container --interactive --command "/bin/sh"
}
catch {
    Write-Host ""
    Write-Host "ERROR: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}
finally {
    if ($weSetCredentials) {
        Remove-Item Env:AWS_ACCESS_KEY_ID -ErrorAction SilentlyContinue
        Remove-Item Env:AWS_SECRET_ACCESS_KEY -ErrorAction SilentlyContinue
    }
    if ($weSetRegion) { Remove-Item Env:AWS_DEFAULT_REGION -ErrorAction SilentlyContinue }
    if ($weSetOutput) { Remove-Item Env:AWS_DEFAULT_OUTPUT -ErrorAction SilentlyContinue }
}
