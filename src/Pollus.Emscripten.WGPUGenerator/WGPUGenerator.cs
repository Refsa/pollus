namespace Pollus.Emscripten.WGPUGenerator;

using System;
using System.IO;
using System.Linq;
using System.Text;
using CppAst;

public class WGPUGenerator
{
    static readonly string[] callbacks = [
        "WGPUBufferMapCallback",
        "WGPUCompilationInfoCallback",
        "WGPUCreateComputePipelineAsyncCallback",
        "WGPUCreateRenderPipelineAsyncCallback",
        "WGPUDeviceLostCallback",
        "WGPUErrorCallback",
        "WGPUProc",
        "WGPUQueueWorkDoneCallback",
        "WGPURequestAdapterCallback",
        "WGPURequestDeviceCallback",
    ];

    public void GenerateWGPU()
    {
        var source = File.ReadAllText("Resources/webgpu.h", Encoding.UTF8);
        if (source == null) return;

        var ast = CppParser.Parse(source, new()
        {
            ParseMacros = true,
            Defines =
            {
                "WGPU_SHARED_LIBRARY",
                "_WIN32",
                "WGPU_SKIP_PROCS"
            },
        });

        if (ast.HasErrors)
        {
            foreach (var error in ast.Diagnostics.Messages)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine(error);
                Console.ResetColor();
            }
            return;
        }

        foreach (var @enum in ast.Enums)
        {
            var code = GenerateEnum(ast, @enum);
            var fileName = $"{@enum.Name}.g.cs";
            WriteFile(fileName, code);
        }

        foreach (var @struct in ast.Classes)
        {
            var code = GenerateClass(ast, @struct);
            var fileName = $"{@struct.Name}.g.cs";
            WriteFile(fileName, code);
        }

        {
            var sb = new StringBuilder();
            sb.AppendLine("namespace Pollus.Emscripten.WGPU;");
            foreach (var typedef in ast.Typedefs)
            {
                if (!callbacks.Contains(typedef.Name)) continue;
                if (typedef.ElementType is CppPointerType pointerType && pointerType.ElementType is CppFunctionType @function) {
                    var args = string.Join(", ", @function.Parameters.Select(p => $"{TranslateType(p.Type)} {p.Name}"));
                    sb.AppendLine($"unsafe public delegate {TranslateType(@function.ReturnType)} {@typedef.Name}({args});");
                }
            }
            var fileName = "WGPUCallbacks.g.cs";
            WriteFile(fileName, sb.ToString());
        }

        var repoRoot = Environment.CurrentDirectory;
        for (int i = 0; i < 4; i++) repoRoot = Path.GetDirectoryName(repoRoot)!;
        var targetDir = Path.Combine(repoRoot, "Pollus.Emscripten", "WGPU", "generated");
        Console.WriteLine($"Target directory: {targetDir}");

        if (Directory.Exists(targetDir)) Directory.Delete(targetDir, true);
        Directory.Move(
            Path.Combine(Environment.CurrentDirectory, "generated"),
            targetDir
        );
    }

    void WriteFile(string fileName, string code)
    {
        var dir = Directory.CreateDirectory(Path.Combine(Environment.CurrentDirectory, "generated"));
        var target = Path.Combine(dir.FullName, fileName);
        File.WriteAllText(target, code);
        Console.WriteLine($"Generated {fileName}");
    }

    string TranslateType(CppType type)
    {
        return (type.FullName switch
        {
            "int32_t" => "int",
            "uint8_t" => "byte",
            "uint16_t" => "ushort",
            "uint32_t" => "uint",
            "uint64_t" => "ulong",
            "size_t" => "nuint",
            "WGPUBool" => "bool",
            "uint32_t const *" => "uint",
            "char" => "byte",
            "char const *" => "byte*",
            _ => type.FullName,
        }).Replace(" ", "").Replace("const", "").Replace("Flags", "");
    }

    string ToCamelCase(string name)
    {
        return $"{char.ToUpper(name[0])}{name[1..]}";
    }

    string GenerateClass(CppCompilation ast, CppClass @struct)
    {
        var sb = new StringBuilder();
        sb.AppendLine("namespace Pollus.Emscripten.WGPU;");

        sb.AppendLine($"unsafe public struct {@struct.Name.Replace("Impl", "")}");
        sb.AppendLine("{");
        foreach (var field in @struct.Fields)
        {
            var type = TranslateType(field.Type);
            sb.AppendLine($"    public {type} {ToCamelCase(field.Name)};");
        }
        sb.AppendLine("}");

        return sb.ToString();
    }

    string GenerateEnum(CppCompilation ast, CppEnum @enum)
    {
        var sb = new StringBuilder();
        sb.AppendLine("namespace Pollus.Emscripten.WGPU;");

        var integerType = TranslateType(@enum.IntegerType);

        var nameReplace = @enum.Name + "_";
        sb.AppendLine($"public enum {@enum.Name} : {integerType}");
        sb.AppendLine("{");
        foreach (var @enumValue in @enum.Items)
        {
            var name = @enumValue.Name.Replace(nameReplace, "");
            if (char.IsNumber(name[0])) name = "_" + name;
            sb.AppendLine($"    {name} = {@enumValue.Value},");
        }

        sb.AppendLine("}");

        return sb.ToString();
    }
}