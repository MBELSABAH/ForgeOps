# Shared by run.sh, test.sh and check.sh after they set repo_dir.
export DOTNET_CLI_HOME="$repo_dir/.local/dotnet"
export NUGET_PACKAGES="$repo_dir/.local/nuget"
export DOTNET_SKIP_FIRST_TIME_EXPERIENCE=1
export DOTNET_NOLOGO=1
export DOTNET_CLI_TELEMETRY_OPTOUT=1
cd "$repo_dir"
