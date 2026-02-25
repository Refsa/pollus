import { dotnet } from './_framework/dotnet.js'
import * as Examples from './examples.js';

// for WasmEnableThreads we need to handle offscreen canvas support since dotnet10 uses an older emscripten version
let canvas = document.getElementById('canvas');
if (canvas && canvas.transferControlToOffscreen) {
    let offscreen = canvas.transferControlToOffscreen();

    let w = canvas.width, h = canvas.height;
    Object.defineProperty(canvas, 'width', { get() { return w; }, set(v) { w = v; } });
    Object.defineProperty(canvas, 'height', { get() { return h; }, set(v) { h = v; } });

    let origPostMessage = Worker.prototype.postMessage;
    Worker.prototype.postMessage = function (msg, transfer) {
        if (offscreen && msg && msg.cmd === 'run') {
            msg.offscreenCanvases = { canvas: { offscreenCanvas: offscreen } };
            transfer = [...transfer, offscreen];
            offscreen = null;
            Worker.prototype.postMessage = origPostMessage;
            return origPostMessage.call(this, msg, transfer);
        }
        return origPostMessage.call(this, msg, transfer);
    };
}

canvas.addEventListener('contextmenu', (e) => e.preventDefault());

const { runMain } = await dotnet
    .withApplicationArguments(window.location.search.substring(1))
    .create();

Examples.setupExampleButtons();
await runMain();
