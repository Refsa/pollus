struct VertexInput {
    @builtin(vertex_index) index: u32,

    // Instance data
    @location(0) i_pos_size: vec4f,       // xy=position, zw=size (screen pixels)
    @location(1) i_bg_color: vec4f,       // background RGBA
    @location(2) i_border_color: vec4f,   // border RGBA
    @location(3) i_border_radius: vec4f,  // topLeft, topRight, bottomRight, bottomLeft
    @location(4) i_border_widths: vec4f,  // top, right, bottom, left
    @location(5) i_extra: vec4f,          // x=ShapeType (0=RoundedRect, 1=Circle, 2=Checkmark, 3=DownArrow), y=OutlineWidth, z=OutlineOffset, w=TextMode (>0.5=glyph)
    @location(6) i_outline_color: vec4f,  // outline RGBA
    @location(7) i_uv_rect: vec4f,       // minU, minV, sizeU, sizeV
};

struct VertexOutput {
    @builtin(position) pos: vec4f,
    @location(0) local_pos: vec2f,         // position within expanded rect [0, expanded_size]
    @location(1) size: vec2f,              // original element size (before expansion)
    @location(2) @interpolate(flat) bg_color: vec4f,
    @location(3) @interpolate(flat) border_color: vec4f,
    @location(4) @interpolate(flat) border_radius: vec4f,
    @location(5) @interpolate(flat) border_widths: vec4f,
    @location(6) @interpolate(flat) extra: vec4f,
    @location(7) @interpolate(flat) outline_color: vec4f,
    @location(8) uv: vec2f,
};

struct UIUniform {
    viewport_size: vec2f,
    time: f32,
    delta_time: f32,
    mouse_position: vec2f,
};

@group(0) @binding(0) var<uniform> ui: UIUniform;
@group(0) @binding(1) var tex: texture_2d<f32>;
@group(0) @binding(2) var samp: sampler;

@vertex
fn vs_main(input: VertexInput) -> VertexOutput {
    // Generate quad corner from vertex_index (0..5 for triangle strip producing 2 triangles)
    let u = f32(input.index & 0x1u);
    let v = f32((input.index & 0x2u) >> 1u);

    let outline_width = input.i_extra.y;
    let outline_offset = input.i_extra.z;
    let expand = outline_width + outline_offset;

    // Expand quad outward so fragment shader has pixels for the outline
    let expanded_size = input.i_pos_size.zw + vec2f(expand * 2.0);
    let expanded_pos = input.i_pos_size.xy - vec2f(expand);
    let pixel_pos = expanded_pos + vec2f(u, v) * expanded_size;

    // Convert screen pixels to NDC: x: [0, width] -> [-1, 1], y: [0, height] -> [1, -1]
    let ndc_x = pixel_pos.x / ui.viewport_size.x * 2.0 - 1.0;
    let ndc_y = 1.0 - pixel_pos.y / ui.viewport_size.y * 2.0;

    var out: VertexOutput;
    out.pos = vec4f(ndc_x, ndc_y, 0.0, 1.0);
    // local_pos is in the expanded coordinate space
    out.local_pos = vec2f(u, v) * expanded_size;
    // size is the original element size (unchanged)
    out.size = input.i_pos_size.zw;
    out.bg_color = input.i_bg_color;
    out.border_color = input.i_border_color;
    out.border_radius = input.i_border_radius;
    out.border_widths = input.i_border_widths;
    out.extra = input.i_extra;
    out.outline_color = input.i_outline_color;
    out.uv = input.i_uv_rect.xy + vec2f(u, v) * input.i_uv_rect.zw;
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
    let tex_color = textureSample(tex, samp, input.uv);

    // Text glyph mode: Extra.w > 0.5 → font atlas, red channel = alpha
    if (input.extra.w > 0.5) {
        let glyph_alpha = tex_color.r * input.bg_color.a;
        if (glyph_alpha < 0.001) { discard; }
        var out: FragmentOutput;
        out.color = vec4f(input.bg_color.rgb, glyph_alpha);
        return out;
    }

    let bg_color = tex_color * input.bg_color;

    let size = input.size;
    let half_size = size * 0.5;
    let shape_type = u32(input.extra.x);
    let outline_width = input.extra.y;
    let outline_offset = input.extra.z;
    let expand = outline_width + outline_offset;

    // Position relative to element center (accounting for the expansion)
    let p = input.local_pos - vec2f(expand) - half_size;

    // --- Compute ALL SDF values and fwidth in uniform control flow ---
    // (WebGPU forbids fwidth inside non-uniform branches)

    // Circle SDF
    let circle_r = min(half_size.x, half_size.y);
    let d_circle = sd_circle(p, circle_r);
    let aa_circle = max(fwidth(d_circle), 0.5);

    // Circle outline SDFs
    let d_circle_outline_outer = sd_circle(p, circle_r + outline_offset + outline_width);
    let aa_circle_outline_outer = max(fwidth(d_circle_outline_outer), 0.5);
    let d_circle_outline_inner = sd_circle(p, circle_r + outline_offset);
    let aa_circle_outline_inner = max(fwidth(d_circle_outline_inner), 0.5);

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
    let inner_p = input.local_pos - vec2f(expand) - inner_center;
    let inner_radii = max(radii - vec4f(
        max(border.w, border.x),
        max(border.y, border.x),
        max(border.y, border.z),
        max(border.w, border.z),
    ), vec4f(0.0));
    let d_inner = sd_rounded_box(inner_p, inner_half, inner_radii);
    let aa_inner = max(fwidth(d_inner), 0.5);

    // Outline SDF: use the outer SDF offset outward by outline_offset
    let outline_outer_radii = radii + vec4f(outline_offset + outline_width);
    let d_outline_outer = sd_rounded_box(p, half_size + vec2f(outline_offset + outline_width), outline_outer_radii);
    let aa_outline_outer = max(fwidth(d_outline_outer), 0.5);
    let outline_inner_radii = radii + vec4f(outline_offset);
    let d_outline_inner = sd_rounded_box(p, half_size + vec2f(outline_offset), outline_inner_radii);
    let aa_outline_inner = max(fwidth(d_outline_inner), 0.5);

    // --- Now branch on shape_type (no derivative calls below) ---

    if (shape_type == 1u) {
        // Circle
        let element_a = (1.0 - smoothstep(-aa_circle, aa_circle, d_circle)) * bg_color.a;

        // Circle outline band
        let co_outer = 1.0 - smoothstep(-aa_circle_outline_outer, aa_circle_outline_outer, d_circle_outline_outer);
        let co_inner = 1.0 - smoothstep(-aa_circle_outline_inner, aa_circle_outline_inner, d_circle_outline_inner);
        let co_band = co_outer * (1.0 - co_inner) * input.outline_color.a;

        let final_a = element_a + co_band * (1.0 - element_a);
        if (final_a < 0.001) { discard; }

        let final_rgb = select(
            input.outline_color.rgb,
            mix(input.outline_color.rgb, bg_color.rgb, element_a / final_a),
            final_a > 0.001
        );

        var out: FragmentOutput;
        out.color = vec4f(final_rgb, final_a);
        return out;
    }

    if (shape_type == 2u) {
        // Checkmark
        let alpha = 1.0 - smoothstep(check_thickness - aa_check, check_thickness + aa_check, d_check);
        if (alpha < 0.001) { discard; }
        var out: FragmentOutput;
        out.color = vec4f(bg_color.rgb, bg_color.a * alpha);
        return out;
    }

    if (shape_type == 3u) {
        // Down arrow
        let alpha = 1.0 - smoothstep(arrow_thickness - aa_arrow, arrow_thickness + aa_arrow, d_arrow);
        if (alpha < 0.001) { discard; }
        var out: FragmentOutput;
        out.color = vec4f(bg_color.rgb, bg_color.a * alpha);
        return out;
    }

    // Default: RoundedRect (shape_type == 0)
    let outer_alpha = 1.0 - smoothstep(-aa_outer, aa_outer, d_outer);

    let inner_alpha = 1.0 - smoothstep(-aa_inner, aa_inner, d_inner);

    let has_border = step(0.001, border.x + border.y + border.z + border.w);
    let blended = mix(input.border_color, bg_color, inner_alpha);
    let pixel_color = mix(bg_color, blended, has_border);
    let element_alpha = pixel_color.a * outer_alpha;

    // Outline band: between outline_inner and outline_outer SDFs
    let outline_outer_alpha = 1.0 - smoothstep(-aa_outline_outer, aa_outline_outer, d_outline_outer);
    let outline_inner_alpha = 1.0 - smoothstep(-aa_outline_inner, aa_outline_inner, d_outline_inner);
    let outline_band = outline_outer_alpha * (1.0 - outline_inner_alpha);
    let outline_alpha = outline_band * input.outline_color.a;

    // Composite: element over outline (element is drawn on top in overlapping AA regions)
    let final_alpha = element_alpha + outline_alpha * (1.0 - element_alpha);

    if (final_alpha < 0.001) {
        discard;
    }

    let final_rgb = select(
        input.outline_color.rgb,
        mix(input.outline_color.rgb, pixel_color.rgb, element_alpha / final_alpha),
        final_alpha > 0.001
    );

    var out: FragmentOutput;
    out.color = vec4f(final_rgb, final_alpha);
    return out;
}
