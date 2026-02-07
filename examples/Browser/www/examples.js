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
        "tween",
        "ecs-iter",
        "query-filter",
        "font",
        "scene",
        "render-order",
        "sprite-material",
        "sprite-animation"
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