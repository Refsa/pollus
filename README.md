# Pollus Game Engine

## What is Pollus?
A small endevour into the possibilities of a "pure" C# game engine that can compile to both native and web platforms. It aims to create a smaller 2D engine as a complete package, but still supporting selecting the underlying core modules to create other types of engines.

You can find the Breakout clone example running [here](https://refsa.github.io/pollus/)

## WARNING
This project is still very early in development and any api surface and underlying features might change at any point.  
There are still a lot of important features missing and most of the WASM and browser support is still a prototype.  

## Features
- ECS with some inspiration from bevy
- Plugin based features
- WGPU rendering backend
- Simple audio plugin
- Simple input plugin
- Build to Windows and Browser (WASM)
    - Nothing should stop it from running on Linux and OSX
    - Mobile support in the future, but also runs in mobile browsers through WASM
- Dear ImGui renderer, which is the only form of UI right now
    - Some function mismatches with ImGui.NET bindings for web
    - f.ex. need to use `ImGui.TextUnformatted` instead of `ImGui.Text`

Currently requires `net8.0` and `net8.0-browser` targets.  
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
        new InputPlugin(),
    ])
    .AddSystems(CoreStage.PostInit, SystemBuilder.FnSystem("SetupEntities",
    static (Commands commands) => 
    {
        commands.Spawn(Entity.With(Transform2.Default, new Player()));
    }))
    .AddSystems(CoreStage.Update, SystemBuilder.FnSystem("UpdateEntities",
    static (Query<Transform2>.Filter<All<Player>> query, ButtonInput<Key> keys) => 
    {
        query.ForEach((ref Transform2 transform) =>
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
- [Silk.NET.OpenAL](https://github.com/dotnet/Silk.NET) (for native OpenAL bindings)
- [SixLabors.ImageSharp](https://github.com/SixLabors/ImageSharp/) (image assets)
- [ImGui.NET](https://github.com/ImGuiNET/ImGui.NET) (Dear ImGui bindings)
- [cimgui/imgui](https://github.com/cimgui/cimgui) (for wasm builds)

## License
This project is licensed under 'MIT License' except where noted. Parts of the project with a different license is either notified in the file itself or in the directory of the file.