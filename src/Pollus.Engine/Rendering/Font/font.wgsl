struct VertexInput {
    @location(0) vertex: vec2f,
    @location(1) uv: vec2f,
    @location(2) color: vec4f,
};

struct ModelInput {
    @location(3) model_0: vec4f,
    @location(4) model_1: vec4f,
    @location(5) model_2: vec4f,
    @location(6) color: vec4f,
};

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
@group(0) @binding(1) var atlas_texture: texture_2d<f32>;
@group(0) @binding(2) var atlas_texture_sampler: sampler;

var<private> model: mat4x4f;
fn vs_setup(input: ModelInput) {
    model = transpose(mat4x4f(input.model_0, input.model_1, input.model_2, vec4f(0.0, 0.0, 0.0, 1.0)));
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

    let dist = textureSample(atlas_texture, atlas_texture_sampler, input.uv).r;

    let texels_per_pixel = fwidth(input.uv) * vec2f(textureDimensions(atlas_texture));
    let pixel_to_sdf = min(max(texels_per_pixel.x, texels_per_pixel.y) * (8.0 / 255.0), 0.15);
    let alpha = smoothstep(0.5 - pixel_to_sdf, 0.5 + pixel_to_sdf, dist);

    if (alpha * input.color.a < 0.01) {
        discard;
    }

    out.color = vec4f(1.0, 1.0, 1.0, alpha) * input.color;

    return out;
}
