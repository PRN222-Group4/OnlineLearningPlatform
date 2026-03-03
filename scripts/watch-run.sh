#!/usr/bin/env bash
# Kill any dotnet process that references the Presentation project path and start dotnet watch
SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJ_DIR="$SCRIPT_DIR/../OnlineLearningPlatform.Presentation"
CSPROJ="$PROJ_DIR/OnlineLearningPlatform.Presentation.csproj"

echo "Project dir: $PROJ_DIR"

# Find processes with command line containing project dir
pids=$(ps aux | grep dotnet | grep "$PROJ_DIR" | grep -v grep | awk '{print $2}')
if [ -n "$pids" ]; then
  echo "Killing processes: $pids"
  kill -9 $pids || true
  sleep 0.5
fi

echo "Starting dotnet watch run..."
dotnet watch --project "$CSPROJ" run
