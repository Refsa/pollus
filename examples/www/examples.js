export const shapeExample = "shapes";
export const ecs = "ecs";
export const input = "input";
export const audio = "audio";
export const imgui = "imgui";
export const breakout = "breakout";
export const drawTriangle = "draw-triangle";
export const collision = "collision";
export const frameGraph = "frame-graph";
export const spriteBench = "sprite-benchmark";
export const compute = "compute";
export const meshRendering = "mesh-rendering";
export const flocking = "flocking";

export function setupExampleButtons() {
    var container = document.getElementsByClassName('buttons')[0];

    var examples = [
        "shapes",
        "ecs",
        "input",
        "audio",
        "imgui",
        "breakout",
        "draw-triangle",
        "collision",
        "frame-graph",
        "sprite-benchmark",
        "compute",
        "mesh-rendering",
        "coroutine",
        "change-tracking",
        "ecs-spawn",
        "hierarchy",
        "transform",
        "flocking",
        "gizmo",
    ];

    for (const example of examples) {
        var button = document.createElement('button');
        button.classList.add('button');
        button.textContent = example;
        button.onclick = () => {
            window.location.href = `?${example}`;
        };
        container.appendChild(button);
    }
}