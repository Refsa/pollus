struct VertexInput {
    @location(0) vertex: vec2f,
    @location(1) uv: vec2f,
};

struct ModelInput {
    @location(2) model_0: vec4f,
    @location(3) model_1: vec4f,
    @location(4) model_2: vec4f,
    @location(5) color: vec4f,
}

struct VertexOutput {
    @builtin(position) pos: vec4f,
    @location(0) uv: vec2f,
    @location(1) @interpolate(flat) color: vec4f,
};

struct SceneUniform {
    view: mat4x4f,
    projection: mat4x4f,
    time: f32,
};

@group(0) @binding(0) var<uniform> scene_uniform: SceneUniform;

var<private> model: mat4x4f;
fn vs_setup(input: ModelInput) {
    model = transpose(mat4x4f(
        input.model_0, 
        input.model_1, 
        input.model_2, 
        vec4f(0.0, 0.0, 0.0, 1.0))
    );
}

@vertex
fn vs_main(
    input: VertexInput,
    model_input: ModelInput
) -> VertexOutput {
    vs_setup(model_input);

    var out: VertexOutput;
    out.pos = scene_uniform.projection * scene_uniform.view * model * vec4f(input.vertex, 0.0, 1.0);
    out.uv = input.uv;
    out.color = model_input.color;
    return out;
}

struct FragmentOutput {
    @location(0) color: vec4f,
};

@fragment
fn fs_main(
    input: VertexOutput
) -> FragmentOutput {
    var out: FragmentOutput;
    out.color = input.color;
    return out;
}
