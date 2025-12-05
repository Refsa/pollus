set -- "net10"

if [ "$2" == "skip-build" ]; then
    shift
else
    ./build_web.sh $1
fi

if [ $? -ne 0 ]; then
    exit 1
fi

(cd "bin/Release/$1.0-browser/browser-wasm/AppBundle" && dotnet serve -S -p 50000 -h "Cross-Origin-Embedder-Policy:require-corp" -h "Cross-Origin-Opener-Policy:same-origin")
