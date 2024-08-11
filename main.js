import { dotnet } from './_framework/dotnet.js'

const { getAssemblyExports, getConfig } = await dotnet.create();
let exports = await getAssemblyExports(getConfig().mainAssemblyName);

while (document.getElementById('canvas') === null) {
    await new Promise(r => setTimeout(r, 100));
}

await dotnet.run();