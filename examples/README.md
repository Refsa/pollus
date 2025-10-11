# Pollus Examples

## Running examples
### Native/Desktop
supports `net9.0` or `net10.0`
run `run.sh` or `dotnet run --framework net10.0` without args to see available examples  
run a specific example with `run.sh <example_name>` or `dotnet run --framework net9.0 -- <example_name>`  

### Browser
install [`dotnet serve`](https://github.com/natemcmaster/dotnet-serve) with `dotnet tool install --global dotnet-serve`  
run `run_web.sh` or `dotnet publish --framework net10.0-browser -c Release` and serve the files under `.\bin\Release\net10.0-browser\browser-wasm\AppBundle\index.html` with a local web server like `dotnet serve`  
web examples have buttons on the web page to select which example to run