#!/bin/bash
set -e

# 0. Initial message (requirement)
echo "NOTE: Before running this script, make sure AWS CLI v2 is installed."
echo "The command 'ecs execute-command' requires AWS CLI v2."
echo ""

# 1. Use env credentials if present; otherwise prompt
if [[ -n "$AWS_ACCESS_KEY_ID" && -n "$AWS_SECRET_ACCESS_KEY" ]]; then
  echo "Using AWS credentials from environment (AWS_ACCESS_KEY_ID / AWS_SECRET_ACCESS_KEY)."
else
  read -p "AWS_ACCESS_KEY_ID: " AWS_ACCESS_KEY_ID
  read -p "AWS_SECRET_ACCESS_KEY: " AWS_SECRET_ACCESS_KEY
  if [[ -z "$AWS_ACCESS_KEY_ID" ]] || [[ -z "$AWS_SECRET_ACCESS_KEY" ]]; then
    echo "Error: AWS_ACCESS_KEY_ID and AWS_SECRET_ACCESS_KEY are required." >&2
    exit 1
  fi
  export AWS_ACCESS_KEY_ID
  export AWS_SECRET_ACCESS_KEY
fi

# Configure AWS CLI for this session (default region and output)
export AWS_DEFAULT_REGION=us-west-2
export AWS_DEFAULT_OUTPUT=json
export AWS_PROFILE=""

# --- Prerequisites check: credentials and Session Manager Plugin ---
abort_with_note() {
  echo "" >&2
  echo "NOTE: Before running this script, make sure AWS CLI v2 is installed." >&2
  echo "The command 'ecs execute-command' requires AWS CLI v2." >&2
  echo "Also ensure your AWS credentials are valid (use valid access keys) and that the Session Manager Plugin is installed." >&2
  echo "Session Manager Plugin: https://docs.aws.amazon.com/systems-manager/latest/userguide/session-manager-working-with-install-plugin.html" >&2
  echo "" >&2
  echo "Waiting 30 seconds before exiting..." >&2
  sleep 30
  exit 1
}

# Check AWS CLI v2
if ! aws --version 2>/dev/null | grep -q "aws-cli/2"; then
  echo "Error: AWS CLI v2 is required (or not found)." >&2
  abort_with_note
fi

# Check credentials (valid session) - uses env vars only (no profile)
if ! aws sts get-caller-identity --region us-west-2 >/dev/null 2>&1; then
  echo "Error: AWS credentials are invalid or expired. Check your access keys." >&2
  abort_with_note
fi

# Check Session Manager Plugin
if ! command -v session-manager-plugin >/dev/null 2>&1; then
  echo "Error: Session Manager Plugin is not installed or not in PATH." >&2
  abort_with_note
fi

echo "Prerequisites verified (AWS CLI v2, credentials, Session Manager Plugin)."
echo ""

# 3. Get the active ECS task
echo ""
echo "Getting RUNNING task from ECS service..."
TASK_ARN=$(aws ecs list-tasks \
  --cluster an-blumberg-dev-backend-api-e85c-bc1e4de \
  --service-name an-blumberg-dev-backend-api-e85c-fb7406a \
  --desired-status RUNNING \
  --region us-west-2 \
  --query "taskArns[0]" \
  --output text)

if [[ -z "$TASK_ARN" ]] || [[ "$TASK_ARN" == "None" ]]; then
  echo "Error: No RUNNING task found." >&2
  exit 1
fi

echo "Task ARN: $TASK_ARN"
echo ""

# 4. Connect to the container
aws ecs execute-command \
  --region us-west-2 \
  --cluster an-blumberg-dev-backend-api-e85c-bc1e4de \
  --task "${TASK_ARN}" \
  --container an-blumberg-dev-backend-api-e85c \
  --interactive \
  --command "/bin/sh"
