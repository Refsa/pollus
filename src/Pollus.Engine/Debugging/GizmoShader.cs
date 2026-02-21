namespace Pollus.Debugging;

public static class GizmoShaders
{
    public const string GIZMO_SHADER =
        """
        struct VertexInput {
            @builtin(instance_index) instance_index: u32,
            @location(0) position : vec2f,
            @location(1) uv : vec2f,
            @location(2) color : vec4f,
        };

        struct VertexOutput {
            @builtin(position) position : vec4f,
            @location(0) @interpolate(linear) uv : vec2f,
            @location(1) @interpolate(linear) color : vec4f,
        };

        struct SceneUniform {
            view: mat4x4f,
            projection: mat4x4f,
            time: f32,
        };

        @group(0) @binding(0) var<uniform> scene_uniform: SceneUniform;

        @vertex
        fn vs_main(input: VertexInput) -> VertexOutput {
            var output: VertexOutput;

            output.position = scene_uniform.projection * scene_uniform.view * vec4f(input.position, 0.0, 1.0);
            output.uv = input.uv;
            output.color = input.color;

            return output;
        }

        @fragment
        fn fs_main(input: VertexOutput) -> @location(0) vec4f {
            var color: vec4f = input.color;
            return color;
        }
        """;

    public const string GIZMO_FONT_SHADER =
        """
        struct VertexInput {
            @builtin(instance_index) instance_index: u32,
            @location(0) position : vec2f,
            @location(1) uv : vec2f,
            @location(2) color : vec4f,
        };

        struct VertexOutput {
            @builtin(position) position : vec4f,
            @location(0) @interpolate(linear) uv : vec2f,
            @location(1) @interpolate(linear) color : vec4f,
        };

        struct SceneUniform {
            view: mat4x4f,
            projection: mat4x4f,
            time: f32,
        };

        @group(0) @binding(0) var<uniform> scene_uniform: SceneUniform;
        @group(0) @binding(1) var texture: texture_2d<f32>;
        @group(0) @binding(2) var texture_sampler: sampler;

        @vertex
        fn vs_main(input: VertexInput) -> VertexOutput {
            var output: VertexOutput;

            output.position = scene_uniform.projection * scene_uniform.view * vec4f(input.position, 0.0, 1.0);
            output.uv = input.uv;
            output.color = input.color;

            return output;
        }

        @fragment
        fn fs_main(input: VertexOutput) -> @location(0) vec4f {
            let dist = textureSample(texture, texture_sampler, input.uv).r;

            let fw = fwidth(input.uv);
            let w = min(max(fw.x, fw.y) * 128.0, 0.15);
            let edge = 0.5;
            let alpha = smoothstep(edge - w, edge + w, dist);

            if (alpha * input.color.a < 0.01) {
                discard;
            }

            return vec4f(1.0, 1.0, 1.0, alpha) * input.color;
        }
        """;
    
    public const string GIZMO_TEXTURE_SHADER =
        """
        struct VertexInput {
            @builtin(instance_index) instance_index: u32,
            @location(0) position : vec2f,
            @location(1) uv : vec2f,
            @location(2) color : vec4f,
        };

        struct VertexOutput {
            @builtin(position) position : vec4f,
            @location(0) @interpolate(linear) uv : vec2f,
            @location(1) @interpolate(linear) color : vec4f,
        };

        struct SceneUniform {
            view: mat4x4f,
            projection: mat4x4f,
            time: f32,
        };

        @group(0) @binding(0) var<uniform> scene_uniform: SceneUniform;
        @group(0) @binding(1) var texture: texture_2d<f32>;
        @group(0) @binding(2) var texture_sampler: sampler;

        @vertex
        fn vs_main(input: VertexInput) -> VertexOutput {
            var output: VertexOutput;

            output.position = scene_uniform.projection * scene_uniform.view * vec4f(input.position, 0.0, 1.0);
            output.uv = input.uv;
            output.color = input.color;

            return output;
        }

        @fragment
        fn fs_main(input: VertexOutput) -> @location(0) vec4f {
            var color: vec4f = input.color;

            color = textureSample(texture, texture_sampler, input.uv) * input.color;

            return color;
        }
        """;
}