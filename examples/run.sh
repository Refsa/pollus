#!/bin/bash

if [ "$1" = "release" ]; then
    shift
    dotnet run -c Release --framework net9.0 -- $@
    exit 0
fi

dotnet run --framework net9.0 -- $@