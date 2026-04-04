#!/bin/bash

if [ "$1" = "release" ]; then
    shift
    dotnet run --project PollusGame.Desktop/PollusGame.Desktop.csproj -c Release -- $@
    exit 0
fi

dotnet run --project PollusGame.Desktop/PollusGame.Desktop.csproj -- $@
