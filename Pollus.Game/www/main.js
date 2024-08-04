import { dotnet } from './_framework/dotnet.js'

const { getAssemblyExports, getConfig } = await dotnet.create();

let exports = await getAssemblyExports(getConfig().mainAssemblyName);

await dotnet.run();