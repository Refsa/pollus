struct VertexInput {
    @builtin(vertex_index) index: u32,

    // Instance bound data
    @location(0) i_model_0: vec4f,
    @location(1) i_model_1: vec4f,
    @location(2) i_model_2: vec4f,

    @location(3) i_slice: vec4f,
    @location(4) i_color: vec4f,
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
@group(0) @binding(1) var texture: texture_2d<f32>;
@group(0) @binding(2) var texture_sampler: sampler;

const FLIP_Y: mat4x4f = mat4x4f(
    1.0, 0.0, 0.0, 0.0,
    0.0, -1.0, 0.0, 0.0,
    0.0, 0.0, 1.0, 0.0,
    0.0, 0.0, 0.0, 1.0
);

var<private> model: mat4x4f;
var<private> vertex: vec4f;
var<private> slice_origin: vec2f;
var<private> slice_extent: vec2f;
fn vs_setup(input: VertexInput) {
    model = transpose(mat4x4f(
        input.i_model_0, 
        input.i_model_1, 
        input.i_model_2, 
        vec4f(0.0, 0.0, 0.0, 1.0)
    ));
    model *= FLIP_Y;

    vertex = vec4f(f32(input.index & 0x1u), f32((input.index & 0x2u) >> 1u), 0.0, 1.0);
}

@vertex
fn vs_main(
    input: VertexInput
) -> VertexOutput {
    vs_setup(input);

    var out: VertexOutput;
    out.pos = scene_uniform.projection * scene_uniform.view * model * (vertex + vec4f(-0.5, -0.5, 0.0, 0.0));
    out.uv = vertex.xy * input.i_slice.zw + input.i_slice.xy;
    out.color = input.i_color;
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

    let texDims = textureDimensions(texture).xy;
    let uv = input.uv / vec2f(texDims);
    
    out.color = textureSample(texture, texture_sampler, uv);
    if (out.color.a == 0.0) {
        discard;
    }
    out.color = out.color * input.color;

    return out;
}
