#!/usr/bin/env bash
# Restores, builds, prepares client assets, then runs Workplan.WebApi and Workplan.Client.
# Usage: ./run-all.sh [https|http]
set -euo pipefail

SCHEME="${1:-https}"
ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
CLIENT_DIR="$ROOT_DIR/src/Workplan.Client"
SOLUTION="$ROOT_DIR/Workplan.sln"
WEBAPI_PROJECT="$ROOT_DIR/src/Workplan.WebApi/Workplan.WebApi.csproj"
CLIENT_PROJECT="$CLIENT_DIR/Workplan.Client.csproj"

if [[ "$SCHEME" != "https" && "$SCHEME" != "http" ]]; then
  echo "Usage: $0 [https|http]" >&2
  exit 1
fi

echo "==> Restoring solution"
dotnet restore "$SOLUTION"

echo "==> Ensuring PostgreSQL is running"
docker compose up -d db

echo "==> Preparing client assets"
if [[ ! -d "$CLIENT_DIR/node_modules" ]]; then
  npm install --prefix "$CLIENT_DIR"
fi
npm run --prefix "$CLIENT_DIR" tailwind:build

echo "==> Building solution"
dotnet build "$SOLUTION" --no-restore

pids=()
cleanup() {
  for pid in "${pids[@]}"; do
    kill "$pid" 2>/dev/null || true
  done
}
trap cleanup EXIT INT TERM

echo "==> Starting API ($SCHEME)"
ASPNETCORE_ENVIRONMENT=Development \
  dotnet run --no-build --launch-profile "$SCHEME" --project "$WEBAPI_PROJECT" &
pids+=("$!")

echo "==> Starting Client ($SCHEME)"
ASPNETCORE_ENVIRONMENT=Development \
  dotnet run --no-build --launch-profile "$SCHEME" --project "$CLIENT_PROJECT" &
pids+=("$!")

if [[ "$SCHEME" == "https" ]]; then
  echo "==> API:    https://localhost:7272/scalar"
  echo "==> Client: https://localhost:7193"
else
  echo "==> API:    http://localhost:5291/scalar"
  echo "==> Client: http://localhost:5276"
fi

wait
