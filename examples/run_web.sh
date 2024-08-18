
if [ "$1" == "skip-build" ]; then
    shift
else
    ./build_web.sh
fi

(cd 'bin/Release/net8.0-browser/browser-wasm/AppBundle' && dotnet serve -S -p 50000)