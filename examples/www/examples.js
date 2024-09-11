export const shapeExample = "shapes";
export const ecs = "ecs";
export const input = "input";
export const audio = "audio";
export const imgui = "imgui";
export const breakout = "breakout";
export const drawTriangle = "draw-triangle";
export const collision = "collision";
export const frameGraph  = "frame-graph";
export const spriteBench = "sprite-benchmark";

export function setupExampleButtons() {
    var container = document.getElementsByClassName('buttons')[0];

    var examples = [
        shapeExample,
        ecs,
        input,
        audio,
        imgui,
        breakout,
        drawTriangle,
        collision,
        frameGraph,
        spriteBench,
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