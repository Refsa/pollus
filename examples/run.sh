#!/bin/bash

if [ "$1" = "release" ]; then
    shift
    dotnet run --project Desktop/Pollus.Examples.Desktop.csproj -c Release -- $@
    exit 0
fi

dotnet run --project Desktop/Pollus.Examples.Desktop.csproj -- $@