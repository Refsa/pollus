struct VertexInput {
    @location(0) vertex: vec2f,
};

struct VertexOutput {
    @builtin(position) clip_position: vec4f,
};

@vertex
fn vs_main(
    input: VertexInput
) -> VertexOutput {
    var out: VertexOutput;
    out.clip_position = vec4f(input.vertex, 0.0, 1.0);
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
    out.color = vec4f(1.0, 1.0, 0.0, 1.0);
    return out;
}