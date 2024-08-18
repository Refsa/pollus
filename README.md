# Pollus Game Engine

## What is Pollus?
A small endevour into the possibilities of a "pure" C# game engine that can compile to both native and web platforms. It aims to create a smaller 2D engine as a complete package, but still supporting selecting the underlying core modules to create other types of engines.

## WARNING
This project is still very early in development and any api surface and underlying features might change at any point. There are still a lot of important features missing and most of the WASM and browser support is still a prototype.

## Features
- ECS with extensibility through plugins, inspired by Bevy
- WGPU rendering backend
- Simple audio plugin
- Simple input plugin
- Build to Windows and Browser (WASM)
    - Nothing should stop it from running on Linux and OSX
    - Mobile support in the future, but also runs in mobile browsers through WASM

## Getting Started
`Pollus.Engine` is the glue project and contains all the required Plugins and systems to get up and running.

### Example folder
The `/examples/` folder in the root directory contains a few different examples for each aspect of the engine.

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
    static (Query<Transform2>.Filter<All<Player>>> query) => 
    {
        query.ForEach((ref Transform2 transform) =>
        {
            transform.Position += Vec2f.Up;
        });
    }))
    .Run()
```

## Libraries
The aim is to have as few dependencies as possible. This will allow the project to more easily adjust to any differing requirements between web and native. WASM support is still very early in C# and as such many third-party libraries have limited support, especially those with native dependencies.

- Silk.NET.SDL (for SDL bindings)
- Silk.NET.WebGPU (for native WebGPU bindings)
- Silk.NET.OpenAL (for native OpenAL bindings)
- SixLabors.ImageSharp (image assets)

## License
This project is licensed under 'MIT License' except where noted. Parts of the project with a different license is either notified in the file itself or in the directory of the file.