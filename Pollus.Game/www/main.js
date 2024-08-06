import { dotnet } from './_framework/dotnet.js'

const { getAssemblyExports, getConfig } = await dotnet.create();
let exports = await getAssemblyExports(getConfig().mainAssemblyName);

// exports.Pollus.Graphics.Window.SetCanvas(document.getElementById('pollusCanvas'));

await dotnet.run();