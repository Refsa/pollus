struct VertexInput {
    @location(0) vertex: vec3f,
    @location(1) uv: vec2f,
};

struct VertexOutput {
    @builtin(position) pos: vec4f,
    @location(0) uv: vec2f,
};

struct ModelInput {
    @location(5) model_0: vec4f,
    @location(6) model_1: vec4f,
    @location(7) model_2: vec4f,
    @location(8) model_3: vec4f,
}

struct SceneUniform {
    view: mat4x4f,
    projection: mat4x4f,
    time: f32,
};

@group(0) @binding(0) var<uniform> scene_uniform: SceneUniform;
@group(0) @binding(1) var texture: texture_2d<f32>;
@group(0) @binding(2) var texture_sampler: sampler;

const FLIP_Y: mat4x4f = mat4x4f(
    1.0, 0.0, 0.0, 0.0,
    0.0, -1.0, 0.0, 0.0,
    0.0, 0.0, 1.0, 0.0,
    0.0, 0.0, 0.0, 1.0
);

var<private> model: mat4x4f;
fn vs_setup(input: ModelInput) {
    model = mat4x4f(input.model_0, input.model_1, input.model_2, input.model_3);
    model *= FLIP_Y;
}

@vertex
fn vs_main(
    input: VertexInput,
    model_input: ModelInput
) -> VertexOutput {
    vs_setup(model_input);

    var out: VertexOutput;
    out.pos = scene_uniform.projection * scene_uniform.view * model * vec4f(input.vertex, 1.0);
    out.uv = input.uv;
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
    
    out.color = textureSample(texture, texture_sampler, input.uv);
    if (out.color.a == 0.0) {
        discard;
    }

    out.color = vec4f(vec3f(sin(scene_uniform.time) * 0.5 + 0.5), 1.0) * out.color;

    return out;
}
