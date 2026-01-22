#!/bin/bash

SCRIPT_DIR="$(dirname "${BASH_SOURCE[0]}")"
ROOT_DIR="$(cd "${SCRIPT_DIR}/.." && pwd)"
source "${SCRIPT_DIR}/logger.sh"

# Project paths
DATABASE_PROJECT="Adapters/Database"
STARTUP_PROJECT="Adapters/Database"
MIGRATIONS_DIR="${DATABASE_PROJECT}/Migrations"

# Load environment variables from .env file
function loadEnv() {
    local envFile="${ROOT_DIR}/.env"
    if [ -f "$envFile" ]; then
        set -a
        source "$envFile"
        set +a
    else
        logWarning ".env file not found at ${envFile}"
    fi
}

# Check if dotnet ef tools are installed
function checkEfTools() {
    loadEnv

    if ! dotnet ef --version &> /dev/null; then
        logError "EF Core tools not installed. Installing..."
        dotnet tool install --global dotnet-ef
        if [ $? -ne 0 ]; then
            logError "Failed to install EF Core tools"
            exit 1
        fi
        logSuccess "EF Core tools installed"
    fi
}

# Show migration help
function migrationHelp() {
    echo "Migration Commands:"
    echo "  migration:generate <name>  Generate a new migration from schema changes"
    echo ""
}

# Generate a new migration by comparing schema vs database
function migrationGenerate() {
    local name=$1

    if [ -z "$name" ]; then
        logError "Migration name is required"
        echo "Usage: ./scripts/cli migration:generate <MigrationName>"
        echo "Example: ./scripts/cli migration:generate AddUserTable"
        exit 1
    fi

    checkEfTools

    logInfo "Generating migration: $name"
    logInfo "Comparing schemas in ${DATABASE_PROJECT}/Schemas vs ModelSnapshot..."

    # EF Core automatically compares:
    # 1. Current model (from IEntityTypeConfiguration in Schemas/)
    # 2. ModelSnapshot (last known state)
    # 3. Generates only the differences

    dotnet ef migrations add "$name" \
        --project "$DATABASE_PROJECT" \
        --startup-project "$STARTUP_PROJECT" \
        --output-dir "Migrations"

    if [ $? -ne 0 ]; then
        logError "Failed to generate migration"
        exit 1
    fi

    # Find the generated migration file
    local migrationFile=$(find "${MIGRATIONS_DIR}" -name "*_${name}.cs" ! -name "*.Designer.cs" | head -1)

    if [ -z "$migrationFile" ]; then
        logError "Could not find generated migration file"
        exit 1
    fi

    # Check if migration is empty (Up method has no operations)
    # Look for actual migrationBuilder calls like "migrationBuilder.CreateTable"
    local upContent=$(grep "migrationBuilder\." "$migrationFile" || true)

    if [ -z "$upContent" ]; then
        logWarning "No schema changes detected. Removing empty migration..."

        dotnet ef migrations remove \
            --project "$DATABASE_PROJECT" \
            --startup-project "$STARTUP_PROJECT" \
            --force &> /dev/null

        logInfo "No migration needed - schemas are up to date"
        exit 0
    fi

    logSuccess "Migration '$name' generated successfully"
    logInfo "Files created in: ${MIGRATIONS_DIR}/"

    echo ""
    logInfo "Generated files:"
    ls -la "${MIGRATIONS_DIR}"/*.cs 2>/dev/null | tail -3
}
