#!/bin/sh
set -eu
repo_dir=$(CDPATH= cd -- "$(dirname -- "$0")/.." && pwd)
. "$repo_dir/scripts/environment.sh"
exec dotnet run --project src/ForgeOps.Web "$@"
