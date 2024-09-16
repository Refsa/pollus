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
    
    var vo = vec2f(f32(in.vertex_index & 0x1u), f32((in.vertex_index & 0x2u) >> 1u)) * 24.0;
    vo = vo - vec2f(12.0, 12.0);
    let vertex = scene_uniform.projection * scene_uniform.view * vec4f(in.position + vo, 0.0, 1.0);

    out.position = vertex;
    out.uv = vec2f(
        f32(in.vertex_index & 1u),
        f32((in.vertex_index & 2u) >> 1u)
    );
    out.uv.y = 1.0 - out.uv.y;

    return out;
}

@fragment
fn fs_main(in: VertexOutput) -> @location(0) vec4f {
    let dist = length(in.uv - vec2f(0.5, 0.5));
    let dist_inv = 1.0 - dist;
    let color = vec3f(1.0, 1.0, 1.0) * pow(1.0 - dist, 10.0);
    return vec4f(color, pow(dist_inv, 10.0));

    // return vec4(in.uv, 0.0, 1.0);
}