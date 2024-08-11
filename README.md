# Pollus Game Engine

## What is Pollus?
A small endevour into the possibilities of a "pure" C# game engine that can compile to both native and web platforms. It aims to create a smaller 2D engine as a complete package, but still supporting selecting the core modules to create other types of engines.

## WARNING
This project is still very early in development and any api surface and underlying features might change at any point. There are still a lot of important features missing and most of the WASM and browser support is still a prototype.

## Libraries
The aim is to have as few dependencies as possible. This will allow the project to more easily adjust to any differing requirements between web and native. WASM support is still very early in C# and as such many third-party libraries have limited support, especially those with native dependencies.

- Silk.NET.SDL (for SDL bindings)
- Silk.NET.WebGPU (for native WebGPU bindings)

## License
This project is licensed under 'MIT License' except where noted. Parts of the project with a different license is either notified in the file itself or in the directory of the file.