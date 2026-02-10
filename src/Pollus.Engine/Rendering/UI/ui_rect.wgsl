struct VertexInput {
    @builtin(vertex_index) index: u32,

    // Instance data
    @location(0) i_pos_size: vec4f,       // xy=position, zw=size (screen pixels)
    @location(1) i_bg_color: vec4f,       // background RGBA
    @location(2) i_border_color: vec4f,   // border RGBA
    @location(3) i_border_radius: vec4f,  // topLeft, topRight, bottomRight, bottomLeft
    @location(4) i_border_widths: vec4f,  // top, right, bottom, left
};

struct VertexOutput {
    @builtin(position) pos: vec4f,
    @location(0) local_pos: vec2f,         // position within rect [0, size]
    @location(1) size: vec2f,
    @location(2) @interpolate(flat) bg_color: vec4f,
    @location(3) @interpolate(flat) border_color: vec4f,
    @location(4) @interpolate(flat) border_radius: vec4f,
    @location(5) @interpolate(flat) border_widths: vec4f,
};

struct UIViewportUniform {
    viewport_size: vec2f,
};

@group(0) @binding(0) var<uniform> viewport: UIViewportUniform;

@vertex
fn vs_main(input: VertexInput) -> VertexOutput {
    // Generate quad corner from vertex_index (0..5 for triangle strip producing 2 triangles)
    let u = f32(input.index & 0x1u);
    let v = f32((input.index & 0x2u) >> 1u);

    let pixel_pos = input.i_pos_size.xy + vec2f(u, v) * input.i_pos_size.zw;

    // Convert screen pixels to NDC: x: [0, width] -> [-1, 1], y: [0, height] -> [1, -1]
    let ndc_x = pixel_pos.x / viewport.viewport_size.x * 2.0 - 1.0;
    let ndc_y = 1.0 - pixel_pos.y / viewport.viewport_size.y * 2.0;

    var out: VertexOutput;
    out.pos = vec4f(ndc_x, ndc_y, 0.0, 1.0);
    out.local_pos = vec2f(u, v) * input.i_pos_size.zw;
    out.size = input.i_pos_size.zw;
    out.bg_color = input.i_bg_color;
    out.border_color = input.i_border_color;
    out.border_radius = input.i_border_radius;
    out.border_widths = input.i_border_widths;
    return out;
}

// SDF for a rounded box (Inigo Quilez)
// p: point relative to box center, b: box half-extents, r: corner radii (tl, tr, br, bl)
fn sd_rounded_box(p: vec2f, b: vec2f, r: vec4f) -> f32 {
    // Select radius based on quadrant
    var rx: vec2f;
    if (p.x > 0.0) {
        rx = vec2f(r.y, r.z); // right side: topRight, bottomRight
    } else {
        rx = vec2f(r.x, r.w); // left side: topLeft, bottomLeft
    }
    var radius: f32;
    if (p.y > 0.0) {
        radius = rx.y; // bottom half
    } else {
        radius = rx.x; // top half
    }
    let q = abs(p) - b + vec2f(radius);
    return min(max(q.x, q.y), 0.0) + length(max(q, vec2f(0.0))) - radius;
}

struct FragmentOutput {
    @location(0) color: vec4f,
};

@fragment
fn fs_main(input: VertexOutput) -> FragmentOutput {
    let size = input.size;
    let half_size = size * 0.5;

    // Position relative to box center
    let p = input.local_pos - half_size;

    // Clamp radii so they don't exceed half the box dimension
    let max_radius = min(half_size.x, half_size.y);
    let radii = min(input.border_radius, vec4f(max_radius));

    // Outer SDF + anti-aliased alpha
    let d_outer = sd_rounded_box(p, half_size, radii);
    let aa_outer = max(fwidth(d_outer), 0.5);
    let outer_alpha = 1.0 - smoothstep(-aa_outer, aa_outer, d_outer);

    if (outer_alpha < 0.001) {
        discard;
    }

    // Inner SDF (shrink by border widths)
    let border = input.border_widths; // top, right, bottom, left
    let inner_min = vec2f(border.w, border.x);         // left, top
    let inner_max = vec2f(border.y, border.z);         // right, bottom
    let inner_size = size - inner_min - inner_max;
    let inner_half = max(inner_size * 0.5, vec2f(0.0));
    let inner_center = (inner_min + (size - inner_max)) * 0.5;
    let inner_p = input.local_pos - inner_center;

    // Shrink radii by border widths for inner shape
    let inner_radii = max(radii - vec4f(
        max(border.w, border.x),  // topLeft: max(left, top)
        max(border.y, border.x),  // topRight: max(right, top)
        max(border.y, border.z),  // bottomRight: max(right, bottom)
        max(border.w, border.z),  // bottomLeft: max(left, bottom)
    ), vec4f(0.0));

    let d_inner = sd_rounded_box(inner_p, inner_half, inner_radii);
    let aa_inner = max(fwidth(d_inner), 0.5);
    let inner_alpha = 1.0 - smoothstep(-aa_inner, aa_inner, d_inner);

    // Composite: lerp between border color and background using inner mask
    let has_border = step(0.001, border.x + border.y + border.z + border.w);
    let blended = mix(input.border_color, input.bg_color, inner_alpha);
    let pixel_color = mix(input.bg_color, blended, has_border);

    var out: FragmentOutput;
    out.color = vec4f(pixel_color.rgb, pixel_color.a * outer_alpha);
    return out;
}
