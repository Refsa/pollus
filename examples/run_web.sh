set -- "net10"

if [ "$2" == "skip-build" ]; then
    shift
else
    ./build_web.sh $1
fi

if [ $? -ne 0 ]; then
    exit 1
fi

SERVE_ARGS="-S -p 50000 -h \"Cross-Origin-Embedder-Policy:require-corp\" -h \"Cross-Origin-Opener-Policy:same-origin\""

if [ "$(uname)" == "Linux" ]; then
    SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
    PFX_PATH="$SCRIPT_DIR/localhost.pfx"

    if [ ! -f "$PFX_PATH" ]; then
        echo "No localhost.pfx found. Generating trusted certificate with mkcert..."
        mkcert -pkcs12 -p12-file "$PFX_PATH" localhost 127.0.0.1 ::1
    fi

    SERVE_ARGS="$SERVE_ARGS --pfx \"$PFX_PATH\" --pfx-pwd changeit"
fi

# Change directory to the new output location inside the Browser folder
(cd "Browser/bin/Release/net10.0-browser/browser-wasm/AppBundle" && eval dotnet serve $SERVE_ARGS)
