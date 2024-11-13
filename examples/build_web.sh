if [ "$1" == "net8" ]; then
    dotnet publish --framework net8.0-browser -c Release
else
    dotnet publish --framework net9.0-browser -c Release
fi