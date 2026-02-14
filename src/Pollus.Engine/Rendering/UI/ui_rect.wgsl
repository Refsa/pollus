struct VertexInput {
    @builtin(vertex_index) index: u32,

    // Instance data
    @location(0) i_pos_size: vec4f,       // xy=position, zw=size (screen pixels)
    @location(1) i_bg_color: vec4f,       // background RGBA
    @location(2) i_border_color: vec4f,   // border RGBA
    @location(3) i_border_radius: vec4f,  // topLeft, topRight, bottomRight, bottomLeft
    @location(4) i_border_widths: vec4f,  // top, right, bottom, left
    @location(5) i_extra: vec4f,          // x=ShapeType (0=RoundedRect, 1=Circle, 2=Checkmark, 3=DownArrow), yzw=reserved
};

struct VertexOutput {
    @builtin(position) pos: vec4f,
    @location(0) local_pos: vec2f,         // position within rect [0, size]
    @location(1) size: vec2f,
    @location(2) @interpolate(flat) bg_color: vec4f,
    @location(3) @interpolate(flat) border_color: vec4f,
    @location(4) @interpolate(flat) border_radius: vec4f,
    @location(5) @interpolate(flat) border_widths: vec4f,
    @location(6) @interpolate(flat) extra: vec4f,
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
    out.extra = input.i_extra;
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

// SDF for a circle
fn sd_circle(p: vec2f, r: f32) -> f32 {
    return length(p) - r;
}

// SDF for a checkmark (two line segments)
fn sd_checkmark(p: vec2f, size: f32) -> f32 {
    let s = size * 0.4;
    // Segment 1: bottom-left to bottom-center (the short leg)
    let a1 = vec2f(-s, 0.0);
    let b1 = vec2f(-s * 0.2, s * 0.7);
    // Segment 2: bottom-center to top-right (the long leg)
    let a2 = vec2f(-s * 0.2, s * 0.7);
    let b2 = vec2f(s, -s * 0.5);

    let d1 = sd_segment(p, a1, b1);
    let d2 = sd_segment(p, a2, b2);
    return min(d1, d2);
}

// SDF for a line segment
fn sd_segment(p: vec2f, a: vec2f, b: vec2f) -> f32 {
    let pa = p - a;
    let ba = b - a;
    let h = clamp(dot(pa, ba) / dot(ba, ba), 0.0, 1.0);
    return length(pa - ba * h);
}

// SDF for a down arrow (chevron/triangle)
fn sd_down_arrow(p: vec2f, size: f32) -> f32 {
    let s = size * 0.3;
    // Two line segments forming a V pointing down
    let a1 = vec2f(-s, -s * 0.4);
    let b1 = vec2f(0.0, s * 0.4);
    let a2 = vec2f(0.0, s * 0.4);
    let b2 = vec2f(s, -s * 0.4);

    let d1 = sd_segment(p, a1, b1);
    let d2 = sd_segment(p, a2, b2);
    return min(d1, d2);
}

struct FragmentOutput {
    @location(0) color: vec4f,
};

@fragment
fn fs_main(input: VertexOutput) -> FragmentOutput {
    let size = input.size;
    let half_size = size * 0.5;
    let shape_type = u32(input.extra.x);

    // Position relative to box center
    let p = input.local_pos - half_size;

    // --- Compute ALL SDF values and fwidth in uniform control flow ---
    // (WebGPU forbids fwidth inside non-uniform branches)

    // Circle SDF
    let circle_r = min(half_size.x, half_size.y);
    let d_circle = sd_circle(p, circle_r);
    let aa_circle = max(fwidth(d_circle), 0.5);

    // Checkmark SDF
    let check_s = min(half_size.x, half_size.y);
    let d_check = sd_checkmark(p, check_s);
    let check_thickness = check_s * 0.15;
    let aa_check = max(fwidth(d_check), 0.5);

    // Down arrow SDF
    let arrow_s = min(half_size.x, half_size.y);
    let d_arrow = sd_down_arrow(p, arrow_s);
    let arrow_thickness = arrow_s * 0.12;
    let aa_arrow = max(fwidth(d_arrow), 0.5);

    // Rounded rect SDF (outer + inner)
    let max_radius = min(half_size.x, half_size.y);
    let radii = min(input.border_radius, vec4f(max_radius));
    let d_outer = sd_rounded_box(p, half_size, radii);
    let aa_outer = max(fwidth(d_outer), 0.5);

    let border = input.border_widths;
    let inner_min = vec2f(border.w, border.x);
    let inner_max = vec2f(border.y, border.z);
    let inner_size = size - inner_min - inner_max;
    let inner_half = max(inner_size * 0.5, vec2f(0.0));
    let inner_center = (inner_min + (size - inner_max)) * 0.5;
    let inner_p = input.local_pos - inner_center;
    let inner_radii = max(radii - vec4f(
        max(border.w, border.x),
        max(border.y, border.x),
        max(border.y, border.z),
        max(border.w, border.z),
    ), vec4f(0.0));
    let d_inner = sd_rounded_box(inner_p, inner_half, inner_radii);
    let aa_inner = max(fwidth(d_inner), 0.5);

    // --- Now branch on shape_type (no derivative calls below) ---

    if (shape_type == 1u) {
        // Circle
        let alpha = 1.0 - smoothstep(-aa_circle, aa_circle, d_circle);
        if (alpha < 0.001) { discard; }
        var out: FragmentOutput;
        out.color = vec4f(input.bg_color.rgb, input.bg_color.a * alpha);
        return out;
    }

    if (shape_type == 2u) {
        // Checkmark
        let alpha = 1.0 - smoothstep(check_thickness - aa_check, check_thickness + aa_check, d_check);
        if (alpha < 0.001) { discard; }
        var out: FragmentOutput;
        out.color = vec4f(input.bg_color.rgb, input.bg_color.a * alpha);
        return out;
    }

    if (shape_type == 3u) {
        // Down arrow
        let alpha = 1.0 - smoothstep(arrow_thickness - aa_arrow, arrow_thickness + aa_arrow, d_arrow);
        if (alpha < 0.001) { discard; }
        var out: FragmentOutput;
        out.color = vec4f(input.bg_color.rgb, input.bg_color.a * alpha);
        return out;
    }

    // Default: RoundedRect (shape_type == 0)
    let outer_alpha = 1.0 - smoothstep(-aa_outer, aa_outer, d_outer);

    if (outer_alpha < 0.001) {
        discard;
    }

    let inner_alpha = 1.0 - smoothstep(-aa_inner, aa_inner, d_inner);

    let has_border = step(0.001, border.x + border.y + border.z + border.w);
    let blended = mix(input.border_color, input.bg_color, inner_alpha);
    let pixel_color = mix(input.bg_color, blended, has_border);

    var out: FragmentOutput;
    out.color = vec4f(pixel_color.rgb, pixel_color.a * outer_alpha);
    return out;
}
