namespace Pollus.Debugging;

public static class GizmoShaders
{
    public const string GIZMO_SHADER = """
    const LINE: u32 = 1u;
    const LINE_STRING: u32 = 2u;
    const RECT: u32 = 3u;
    const CIRCLE: u32 = 4u;
    const TRIANGLE: u32 = 5u;
    const GRID: u32 = 6u;

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

    fn has_flag(flag: u32, mask: u32) -> bool {
        return (flag & mask) != 0u;
    }

    @fragment
    fn fs_main(input: VertexOutput) -> @location(0) vec4f {
        var color: vec4f = input.color;
        return color;
    }
    """;
}