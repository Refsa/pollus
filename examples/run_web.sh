
if [ "$2" == "skip-build" ]; then
    shift
else
    ./build_web.sh $1
fi

(cd "bin/Release/$1.0-browser/browser-wasm/AppBundle" && dotnet serve -S -p 50000)
