# Pollus Game Engine

## What is Pollus?
A small endevour into the possibilities of a "pure" C# game engine that can compile to both native and web platforms. It aims to create a smaller 2D engine as a complete package, but still supporting selecting the underlying core modules to create other types of engines.

## Features
- ECS with some inspiration from bevy
- Plugin based features
- WGPU rendering backend
    - WebGPU on web
    - Other WGPU supported backends on native
- Simple audio plugin
- Simple input plugin
- Build to Windows, Linux and Browser (WASM)
- Dear ImGui renderer
- UI based on taffy, SDF based renderer (still WIP)

Currently requires `net10.0` targets and the `wasm-tools`/`wasm-experimental` workload for browser support.

## WIP
This project is still very early in development and any api surface and underlying features might change at any point.  
There are still a lot of important features missing and most of the WASM and browser support is still a prototype.  

Status of the different areas of the project is currently tracked in [TODO](TODO.md).

## Getting Started
`Pollus.Engine` is the glue project and contains all the required Plugins and systems to get up and running.  
Latest known working version can be found under the tag `v0.1.0`, but can be a bit out of date.

### Building WASM for web
Browser is built with `dotnet publish --framework net10.0-browser -c Release` and can be hosted locally with `dotnet serve -S -p <port>`.  
Install the `wasm-tools` and `wasm-experimental` workloads with `dotnet workload install <tool>`

### Examples
The [/examples/Application](./examples/Application/) folder in the root directory contains a few different examples for each aspect of the engine.  
**You can find the examples running [here](https://refsa.github.io/pollus/?breakout)**  

## Known Issues
- ImGui has some function signature mismatch between Silk.NET generated bindings and the wasm native library file.
    - `ImGui.Text` might have to be replace with `ImGui.TextUnformatted`
- Fullscreen mode in browser is broken
- `dotnet build` does not work for `net10-browser` as the target framework. Issue is that `browser-wasm` is not the default runtime identifier and as such it tries to build with an incompatible build pipeline. To build for the browser use `dotnet publish --framework net10.0-browser -c Release`

## [Documentation](./DOCS.md)

### Minimal example
```cs
Application.Builder
    .AddPlugins([
        AssetPlugin.Default,
        new RenderingPlugin(),
        new InputPlugin(),
    ])
    .AddSystems(CoreStage.PostInit, FnSystem.Create("SetupEntities",
    static (Commands commands, AssetServer assetServer) => 
    {
        commands.Spawn(Camera2D.Bundle);

        var shapeMaterial = shapeMaterials.Add(new ShapeMaterial
        {
            ShaderSource = assetServer.LoadAsync<ShaderAsset>("shaders/builtin/shape.wgsl"),
        });

        commands.Spawn(ShapeDraw.Bundle
            .Set(Transform2D.Default with
            {
                Position = new Vec2f(128f, 128f),
            })
            .Set(new ShapeDraw()
            {
                MaterialHandle = shapeMaterial,
                ShapeHandle = shapes.Add(Shape.Rectangle(Vec2f.Zero, Vec2f.One * 64f)),
                Color = Color.RED,
            }));
    }))
    .AddSystems(CoreStage.Update, FnSystem.Create("UpdateEntities",
    static (Query<Transform2D>.Filter<All<Player>> query, ButtonInput<Key> keys) => 
    {
        query.ForEach(keys, static (in keys, ref transform) =>
        {
            if (keys.Pressed(Key.ArrowLeft))
            {
                transform.Position += Vec2f.Left;
            }
            if (keys.Pressed(Key.ArrowRight))
            {
                transform.Position += Vec2f.Right;
            }
        });
    }))
    .Run()
```

## Libraries
The aim is to have as few dependencies as possible. This will allow the project to more easily adjust to any differing requirements between web and native. Most pure dotnet projects will work with WASM out of the box, but anything that is built on top of native libraries will not.

- [Silk.NET.SDL](https://github.com/dotnet/Silk.NET) (for SDL bindings)
- [Silk.NET.WebGPU](https://github.com/dotnet/Silk.NET) (for native WebGPU bindings)
- [WebGPU native](https://github.com/emscripten-core/emscripten/blob/3.1.56/system/include/webgpu/webgpu.h) (for browser WebGPU bindings)
- [Silk.NET.OpenAL](https://github.com/dotnet/Silk.NET) (for native OpenAL bindings)
- [StbImageSharp](https://github.com/StbSharp/StbImageSharp/) (image assets)
- [StbTrueTypeSharp](https://github.com/StbSharp/StbTrueTypeSharp) (font loading)
- [ImGui.NET](https://github.com/ImGuiNET/ImGui.NET) (Dear ImGui bindings)
- [cimgui/imgui](https://github.com/cimgui/cimgui) (for wasm builds)

## License
This project is licensed under 'MIT License' except where noted. Parts of the project with a different license is either notified in the file itself or in the directory of the file.
