import { dotnet } from './_framework/dotnet.js'
import * as Examples from './examples.js';

const { getAssemblyExports, getConfig } = await dotnet
    .withApplicationArguments(window.location.search.substring(1))
    .create();
let exports = await getAssemblyExports(getConfig().mainAssemblyName);

const canvas = document.getElementById('canvas');
while (canvas === null) {
    await new Promise(r => setTimeout(r, 100));
}
canvas.addEventListener('click', (e) => e.preventDefault());
canvas.addEventListener('contextmenu', (e) => e.preventDefault());

Examples.setupExampleButtons();
await dotnet.run();