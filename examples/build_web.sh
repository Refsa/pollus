if [ "$1" == "net9" ]; then
    dotnet publish --framework net9.0-browser -c Release
elif [ "$1" == "net10" ]; then
    dotnet publish --framework net10.0-browser -c Release
else
    dotnet publish --framework net8.0-browser -c Release
fi