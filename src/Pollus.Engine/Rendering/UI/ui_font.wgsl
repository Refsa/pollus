struct VertexInput {
    @location(0) position: vec2f,
    @location(1) uv: vec2f,
    @location(2) color: vec4f,
};

struct InstanceInput {
    @location(3) offset: vec4f,   // xy=screen position, zw=unused
    @location(4) color: vec4f,
};

struct VertexOutput {
    @builtin(position) pos: vec4f,
    @location(0) uv: vec2f,
    @location(1) @interpolate(flat) color: vec4f,
};

struct UIViewportUniform {
    viewport_size: vec2f,
};

@group(0) @binding(0) var<uniform> viewport: UIViewportUniform;
@group(0) @binding(1) var atlas_texture: texture_2d<f32>;
@group(0) @binding(2) var atlas_texture_sampler: sampler;

@vertex
fn vs_main(input: VertexInput, inst: InstanceInput) -> VertexOutput {
    let pixel_pos = inst.offset.xy + input.position;

    // Convert screen pixels to NDC: x: [0, width] -> [-1, 1], y: [0, height] -> [1, -1]
    let ndc_x = pixel_pos.x / viewport.viewport_size.x * 2.0 - 1.0;
    let ndc_y = 1.0 - pixel_pos.y / viewport.viewport_size.y * 2.0;

    var out: VertexOutput;
    out.pos = vec4f(ndc_x, ndc_y, 0.0, 1.0);
    out.uv = input.uv;
    out.color = input.color * inst.color;
    return out;
}

struct FragmentOutput {
    @location(0) color: vec4f,
};

@fragment
fn fs_main(input: VertexOutput) -> FragmentOutput {
    var out: FragmentOutput;

    var sample = textureSample(atlas_texture, atlas_texture_sampler, input.uv).r;
    out.color = vec4f(1.0, 1.0, 1.0, sample) * input.color;

    if (out.color.a == 0.0) {
        discard;
    }

    return out;
}
