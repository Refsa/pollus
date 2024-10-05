namespace Pollus.Debugging;

public static class GizmoShaders
{
    public const string GIZMO_SHADER = """
    struct VertexInput {
        @location(0) position : vec2f,
        @location(1) uv : vec2f,
        @location(2) color : vec4f,
    };

    struct VertexOutput {
        @builtin(position) position : vec4f,
        @location(0) uv : vec2f,
        @location(1) color : vec4f,
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
        return input.color;
    }
    """;
}