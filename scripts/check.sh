#!/bin/sh
set -eu
repo_dir=$(CDPATH= cd -- "$(dirname -- "$0")/.." && pwd)
. "$repo_dir/scripts/environment.sh"
dotnet restore ForgeOps.slnx
dotnet build ForgeOps.slnx -c Release --no-restore
dotnet test ForgeOps.slnx -c Release --no-build --no-restore --logger "trx;LogFileName=forgeops.trx"
dotnet publish src/ForgeOps.Web -c Release --no-build --no-restore -o artifacts/publish
