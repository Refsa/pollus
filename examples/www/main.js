import { dotnet } from './_framework/dotnet.js'

const { getAssemblyExports, getConfig } = await dotnet.create();
let exports = await getAssemblyExports(getConfig().mainAssemblyName);

const canvas = document.getElementById('canvas');
while (canvas === null) {
    await new Promise(r => setTimeout(r, 100));
}
canvas.addEventListener('click', (e) => e.preventDefault());
canvas.addEventListener('contextmenu', (e) => e.preventDefault());

dotnet.withApplicationArguments('shapes');
await dotnet.run();