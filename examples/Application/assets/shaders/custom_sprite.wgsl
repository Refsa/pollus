struct VertexInput {
    @builtin(vertex_index) index: u32,

    // Instance bound data
    @location(0) i_model_0: vec4<f32>,
    @location(1) i_model_1: vec4<f32>,
    @location(2) i_model_2: vec4<f32>,
    @location(3) i_slice: vec4<f32>,
    @location(4) i_color: vec4<f32>,
};

struct VertexOutput {
    @builtin(position) pos: vec4<f32>,
    @location(0) uv: vec2<f32>,
    @location(1) @interpolate(flat) color: vec4<f32>,
};

struct SceneUniform {
    view: mat4x4<f32>,
    projection: mat4x4<f32>,
    time: f32,
};

@group(0) @binding(0) var<uniform> scene_uniform: SceneUniform;
@group(0) @binding(1) var texture: texture_2d<f32>;
@group(0) @binding(2) var texture_sampler: sampler;

const FLIP_Y: mat4x4<f32> = mat4x4<f32>(
    1.0, 0.0, 0.0, 0.0,
    0.0, -1.0, 0.0, 0.0,
    0.0, 0.0, 1.0, 0.0,
    0.0, 0.0, 0.0, 1.0
);

var<private> model: mat4x4<f32>;
var<private> vertex: vec4<f32>;
var<private> slice_origin: vec2<f32>;
var<private> slice_extent: vec2<f32>;
fn vs_setup(input: VertexInput) {
    model = transpose(mat4x4<f32>(
        input.i_model_0,
        input.i_model_1,
        input.i_model_2,
        vec4<f32>(0.0, 0.0, 0.0, 1.0)
    ));
    model[0] *= input.i_slice.z;
    model[1] *= input.i_slice.w;
    model *= FLIP_Y;

    vertex = vec4<f32>(f32(input.index & 0x1u), f32((input.index & 0x2u) >> 1u), 0.0, 1.0);
}

@vertex
fn vs_main(
    input: VertexInput
) -> VertexOutput {
    vs_setup(input);

    var out: VertexOutput;
    out.pos = scene_uniform.projection * scene_uniform.view * model * (vertex + vec4<f32>(-0.5, -0.5, 0.0, 0.0));
    out.uv = vertex.xy * input.i_slice.zw + input.i_slice.xy;
    out.color = input.i_color;
    return out;
}

struct FragmentOutput {
    @location(0) color: vec4<f32>,
};

fn rand22(n: vec2<f32>) -> f32 {
    return fract(sin(dot(n, vec2<f32>(12.9898, 4.1414))) * 43758.5453);
}

fn noise2(n: vec2<f32>) -> f32 {
    let d = vec2<f32>(0., 1.);
    let b = floor(n);
    let f = smoothstep(vec2<f32>(0.), vec2<f32>(1.), fract(n));
    return mix(mix(rand22(b), rand22(b + d.yx), f.x), mix(rand22(b + d.xy), rand22(b + d.yy), f.x), f.y);
}

@fragment
fn fs_main(
    input: VertexOutput
) -> FragmentOutput {
    var out: FragmentOutput;

    let texDims = textureDimensions(texture).xy;
    let uv = input.uv / vec2<f32>(texDims);

    out.color = textureSample(texture, texture_sampler, uv);
    if out.color.a == 0.0 {
        discard;
    }

    let noise = noise2((uv + scene_uniform.time * 0.1) * 32.0);
    let noise_color = noise * vec4<f32>(1.0, 0.0, 0.0, 1.0);

    out.color = out.color * input.color * noise_color;

    return out;
}