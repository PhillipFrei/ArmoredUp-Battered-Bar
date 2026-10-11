#!/usr/bin/env bash
set -euo pipefail
ROOT="$(cd "$(dirname "$0")" && pwd)"
dotnet build "$ROOT/Source/BatteredBar/BatteredBar.csproj" -c Release
echo "Built DLL: $ROOT/1.6/Assemblies/ArmoredUpBatteredBar.dll"
