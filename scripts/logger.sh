#!/bin/bash

# Log levels with colors
function logInfo() {
    printf "\033[1;34m[INFO]\033[0m %s\n" "$@"
}

function logSuccess() {
    printf "\033[1;32m[SUCCESS]\033[0m %s\n" "$@"
}

function logWarning() {
    printf "\033[1;33m[WARNING]\033[0m %s\n" "$@"
}

function logError() {
    printf "\033[1;31m[ERROR]\033[0m %s\n" "$@"
}

function logDebug() {
    printf "\033[1;35m[DEBUG]\033[0m %s\n" "$@"
}