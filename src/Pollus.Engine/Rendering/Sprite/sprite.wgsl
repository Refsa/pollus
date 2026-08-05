struct VertexInput {
    @builtin(vertex_index) index: u32,

    // Instance bound data
    // basis X, basis Y, packed Z, translation X.
    @location(0) i_model_0: vec4f,
    // basis X, basis Y, unused, translation Y.
    @location(1) i_model_1: vec3f,

    @location(2) i_slice: vec4u,
    @location(3) i_color: u32,
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
    let model_row_0 = vec4f( input.i_model_0.x, input.i_model_0.y, 0.0, input.i_model_0.w );
    let model_row_1 = vec4f( input.i_model_1.x, input.i_model_1.y, 0.0, input.i_model_1.z );
    let model_row_2 = vec4f( 0.0, 0.0, 1.0, input.i_model_0.z );

    model = transpose(mat4x4f(
        model_row_0,
        model_row_1,
        model_row_2,
        vec4f(0.0, 0.0, 0.0, 1.0)
    ));

    model[0] *= f32(input.i_slice.z);
    model[1] *= f32(input.i_slice.w);
    model *= FLIP_Y;

    vertex = vec4f(
        f32(input.index & 0x1u),
        f32((input.index & 0x2u) >> 1u),
        0.0,
        1.0
    );
}

@vertex
fn vs_main(
    input: VertexInput
) -> VertexOutput {
    vs_setup(input);

    var out: VertexOutput;
    out.pos = scene_uniform.projection * scene_uniform.view * model * (vertex + vec4f(-0.5, -0.5, 0.0, 0.0));
    out.uv = vertex.xy * vec2f(input.i_slice.zw) + vec2f(input.i_slice.xy);
    out.color = unpack4x8unorm(input.i_color);
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