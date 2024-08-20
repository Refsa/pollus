# Pollus Game Engine

## What is Pollus?
A small endevour into the possibilities of a "pure" C# game engine that can compile to both native and web platforms. It aims to create a smaller 2D engine as a complete package, but still supporting selecting the underlying core modules to create other types of engines.

Currently requires `net8.0` and `net8.0-browser` targets.

## WARNING
This project is still very early in development and any api surface and underlying features might change at any point. There are still a lot of important features missing and most of the WASM and browser support is still a prototype. There are bugs and marshalling problems between C# and JS might crop up when they might not make sense.

## Features
- ECS with extensibility through plugins, inspired by Bevy
- WGPU rendering backend
- Simple audio plugin
- Simple input plugin
- Build to Windows and Browser (WASM)
    - Nothing should stop it from running on Linux and OSX
    - Mobile support in the future, but also runs in mobile browsers through WASM
- Dear ImGui renderer

Status of the different areas of the project is currently tracked in [TODO](TODO.md).

## Getting Started
`Pollus.Engine` is the glue project and contains all the required Plugins and systems to get up and running.  
Requires `wasm-experimental` and `wasm-tools` workloads to be installed, which can be done with `dotnet workload install <package>`.  
Browser is built with `dotnet publish --framework net8.0-browser -c Release` and can be hosted locally with `dotnet serve -S -p <port>`.  
WebGPU is currently only working in Chromium-based browsers.

### Example folder
The `/examples/` folder in the root directory contains a few different examples for each aspect of the engine.  
A lot of the contents in `Pollus.Examples.csproj` file is required to build for web and will be templated to be more usable in the future.

### Minimal example
```cs
Application.Builder
    .AddPlugins([
        new AssetPlugin { RootPath = "assets" },
        new RenderingPlugin(),
        new InputPlugin(),
    ])
    .AddSystems(CoreStage.PostInit, SystemBuilder.FnSystem("SetupEntities",
    static (World world) => 
    {
        world.Spawn(Transform2.Default, new Player());
    }))
    .AddSystems(CoreStage.Update, SystemBuilder.FnSystem("UpdateEntities",
    static (Query<Transform2>.Filter<All<Player>> query) => 
    {
        query.ForEach((ref Transform2 transform) =>
        {
            transform.Position += Vec2f.Up;
        });
    }))
    .Run()
```

## Libraries
The aim is to have as few dependencies as possible. This will allow the project to more easily adjust to any differing requirements between web and native. Most pure dotnet projects will work with WASM out of the box, but anything that is built on top of native libraries will not.

- Silk.NET.SDL (for SDL bindings)
- Silk.NET.WebGPU (for native WebGPU bindings)
- Silk.NET.OpenAL (for native OpenAL bindings)
- SixLabors.ImageSharp (image assets)
- ImGui.NET (Dear ImGui)

## License
This project is licensed under 'MIT License' except where noted. Parts of the project with a different license is either notified in the file itself or in the directory of the file.