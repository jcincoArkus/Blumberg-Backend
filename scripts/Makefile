# Connect to ECS container (ECS Exec)
# From scripts folder:  make connect-ecs
# From repo root:        make -C scripts connect-ecs
#
# After connecting to the container, run these two commands separately (inside the container):
#   1. dotnet /app/cli/CLI.dll nukeAndPave
#   2. INGESTION_SIMULATOR_BASE_URL="https://d2cqcx3hu6v090.cloudfront.net" dotnet /app/cli/CLI.dll ingestion:simulate --interval 10

.PHONY: connect-ecs connect-ecs-bash

connect-ecs:
	powershell -NoProfile -ExecutionPolicy Bypass -File "connect-ecs-exec.ps1"

connect-ecs-bash:
	bash connect-ecs-exec.sh
