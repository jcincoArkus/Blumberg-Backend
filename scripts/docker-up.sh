# Idempotent Docker startup for backend (Linux/macOS).
# Usage:
#   ./scripts/docker-up.sh
#   ./scripts/docker-up.sh --build
set -euo pipefail

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
ROOT_DIR="$(cd "${SCRIPT_DIR}/.." && pwd)"
cd "${ROOT_DIR}"

if [ ! -f ".env" ] && [ -f ".env.example" ]; then
  echo "No .env found; copying .env.example -> .env (edit it to match your setup)."
  cp .env.example .env
fi

COMPOSE_ARGS=(-f compose.yml up -d)
if [ "${1:-}" = "--build" ]; then
  COMPOSE_ARGS+=("--build")
fi

echo "Starting containers (idempotent): docker compose ${COMPOSE_ARGS[*]}"
docker compose "${COMPOSE_ARGS[@]}"

echo "Waiting for Postgres to be healthy..."
timeout_s=120
start_s="$(date +%s)"
while true; do
  now_s="$(date +%s)"
  elapsed=$((now_s - start_s))
  if [ "$elapsed" -gt "$timeout_s" ]; then
    echo "Timed out waiting for Postgres health after ${timeout_s}s" >&2
    exit 1
  fi

  if docker compose -f compose.yml exec -T database sh -lc "pg_isready -U blumberg -d blumberg" >/dev/null 2>&1; then
    echo "Postgres is ready."
    break
  fi

  sleep 2
done

echo "Backend is up. API should be available at http://localhost:5000"

