struct VertexInput {
    @builtin(vertex_index) vertex_index: u32, 
    @builtin(instance_index) instance_index: u32,
    @location(0) position: vec2f,
    @location(1) velocity: vec2f,
}

struct VertexOutput {
    @builtin(position) position : vec4f,
    @location(0) uv : vec2f,
}

struct Particle {
    position: vec2f,
    velocity: vec2f,
};

struct SceneUniform {
    view: mat4x4f,
    projection: mat4x4f,
    time: f32,
};

@group(0) @binding(0) var<uniform> scene_uniform: SceneUniform;
@group(0) @binding(1) var<storage> data: array<Particle>;

@vertex
fn vs_main(in: VertexInput) -> VertexOutput {
    var out: VertexOutput;
    
    let vo = vec2f(f32(in.vertex_index & 0x1u), f32((in.vertex_index & 0x2u) >> 1u)) * 16.0;
    let vertex = scene_uniform.projection * scene_uniform.view * vec4f(in.position + vo, 0.0, 1.0);

    out.position = vertex;
    out.uv = vec2f(
        f32((in.vertex_index << 1u) & 2u),
        f32(in.vertex_index & 2u)
    );
    out.uv.y = 1.0 - out.uv.y;

    return out;
}

@fragment
fn fs_main(in: VertexOutput) -> @location(0) vec4f {
    return vec4(in.uv, 0.0, 1.0);
}