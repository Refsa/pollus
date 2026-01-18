# Documentation
__A brief overview of each aspect of the engine.__  
**This engine uses Source Generators for many parts of the engine. When you see `partial` as part of the type definition then parts of that type is source generated**

- [Engine architecture](#engine-architecture)
- [Dos and Donts](#dos-and-donts)
- [ECS](#ecs)
    - [Components](#components)
    - [Queries](#queries)
    - [Systems](#systems)
    - [Coroutines](#coroutines)
    - [Schedule](#schedule)
    - [Commands](#commands)
    - [Events](#events)
    - [Hierarchy](#hierarchy)
- [Plugins](#plugins)
- [Resources](#resources)
- [AssetServer](#assetserver)
- [Transform2D](#transform2d)
- [Rendering](#rendering)
    - [Shaders](#shaders)
    - [Materials](#materials)
    - [Compute Shaders](#compute-shaders)
    - [Frame Graph](#framegraph)
    - [Sprites](#sprites)
    - [Shapes](#shapes)
    - [Meshes](#meshes)
    - [Text](#text)
- [Scene](#scene)
- [Tween](#tween)
- [Audio](#audio)
- [Input](#input)

## Engine architecture
- Structured around the ECS World
- Plugin system to handle different features
- Resource container to handle shared data
- AssetServer to handle loading of assets

## Dos and Donts
### Do
- Use partial when defining components and assets
    - Pollus.Generators auto-generates the rest of the type
- Use `Handle<T>`/`Handle` when storing assets in components
    - Components are unmanaged/blittable, Handle is an indirect way to store a reference to an asset

### Dont
- Dont alias Resources in other objects
    - Pollus makes use of system parameters to define dependencies for systems. By aliasing a resource and using it from a context where the system scheduler does not know about it might lead to undefined behavior.
- Dont alias Assets in other objects
    - The AssetServer is responsible for the lifetime of assets
    - `Handle` is used to keep a reference to assets
- Dont use non-static methods in Query.ForEach methods
    - ForEach methods should not reference objects directly from the outside to avoid GC allocations
    - Use the `ForEach(userData, predicate)` variant to pass data to the ForEach method

## ECS
- Archetype based ECS with chunked storage
- Systems and queries are separate concepts

### Components
- Components are required to be blittable, i.e. they should only contain value types.
- Pollus handles reference types in components via `Handle<T>`, more information can be found in the Assets section
```cs
public partial struct Health : IComponent
{
    public int Value;
    public int MaxValue;
    public Handle<Texture2D> Texture;
}
```

#### Default and Required components
- `Required<T>` is used to define a component that must be present on an entity
    - Recursively added to the entity if not present in the EntityBuilder
    - Allows setting a custom factory method to be used when auto-adding the component
- `IDefault<T>` is used to define a default constructor for a component
    - This is auto-generated if not present, with a value of `default`
    - Override to set the default value when being auto-constructed by Required components
```cs
[Required<Health>]
public partial struct Player : IComponent, IDefault<Player>
{
    public static Player Default => new() { Level = 1 };

    public int Level;
}

[Required<Health>(nameof(HealthConstructor))]
public partial struct Enemy : IComponent
{
    public int Damage;

    static Health HealthConstructor() => new() { MaxValue = 100};
}
```

#### Queries
- Queries can be created manually, but are usually injected in systems
```cs
Query<Health, Thing> query;
Query<Health>.Filter<None<Player>> filteredQuery;

// ForEach predicates should be static for performance reasons
query.ForEach(static (ref health, ref thing) =>
{
    health.Value -= thing.Damage;
});

// ForEach can access entity
query.ForEach(static (in entity, ref health, ref thing) =>
{
    health.Value -= thing.Damage;
});

// Any data can be passed to a ForEach call, which allows the predicate to stay static
query.ForEach((deltaTime, keyDown), static (in userData, ref health, ref thing) =>
{
    health.Value -= thing.Damage * userData.DeltaTime;
    if (userData.keyDown) health.Value += 10;
});
```

#### Filters
- Filters are used to exclude entities from queries
```cs
None<C0, C1, ..> // None of C0, C1, ..
Any<C0, C1, ..> // Any of C0, C1, ..
All<C0, C1, ..> // All of C0, C1, ..
Added<C0> // C0 was added this frame
Removed<C0> // C0 was removed this frame
```

### Systems
- Systems should contain the higher level logic
- Systems support dependency injection, via the Resources container
- Systems can either be a class, a delegate or part of a source generated SystemSet
- Systems are added to the Schedule, more information under the Schedule section below

#### Delegate Systems
- Simple setup, easy to change and refactor
- Messier code that can be harder to read as most of the systems are inlined
- `Local<T>` used to store and retrieve system scoped data
```cs
// Delegate systems allows quick setup and easy configuration of systems.
// A huge benefit is that very little generic typing has to be defined. 
// Pick types in the parameters and everything else is handled internally
world.Schedule.AddSystems(CoreStage.PostInit, FnSystem.Create("Label",
static (Time time, Query<Velocity> qVelocity) => 
{
    qVelocity.ForEach(time.DeltaTimeF, static (in deltaTime, ref velocity) =>
    {
        velocity.Value -= deltaTime;
    });
}));

// The first parameter of FnSystem.Create is either just the label or a SystemBuilderDescriptor
// SystemBuilderDescriptor is used to define order, dependencies, local variables, etc.
world.Schedule.AddSystems(CoreStage.PostInit, FnSystem.Create(new("Label")
{
    RunsBefore = ["BeforeThis"],
    RunsAfter = ["AfterThis"],
    Locals = [Local.From(10)],
}, static (Local<int> local) => { }));
```

#### Class Systems
- More verbose setup, but can be easier to read and organize
- Allows storing data on the system, without use of `Local<T>`
```cs
class ExampleSystem : SystemBase<Time, Query<Velocity>>
{
    public ExampleSystem() : base(new("Label")
    {
        RunsBefore = ["BeforeThis"],
        RunsAfter = ["AfterThis"],
    }) {}

    protected override void OnTick(Time time, Query<Velocity> qVelocity)
    {
        qVelocity.ForEach(time.DeltaTimeF, static (in deltaTime, ref velocity) =>
        {
            velocity.Value -= deltaTime;
        });
    }
}
```

#### SystemSet
- Similar to delegate systems, but makes use of Source Generators
- Easier to organize code
- Easier to swap out method that runs in system
```cs
[SystemSet]
static partial class GeneratedSystemSet
{
    // `nameof(ExampleSystem)` is the method that should run for this system
    [System(nameof(ExampleSystem))] 
    static readonly SystemBuilderDescriptor ExampleSystemDescriptor = new()
    {
        Stage = CoreStage.Update,
        RunsBefore = ["BeforeThis"],
        RunsAfter = ["AfterThis"],
        Locals = [Local.From(10)],
    };

    static void ExampleSystem(Local<int> local, Time time, Query<Velocity> qVelocity)
    {
        qVelocity.ForEach(time.DeltaTimeF, static (in deltaTime, ref velocity) =>
        {
            velocity.Value -= deltaTime;
        });
    }
}
```

#### RunCriteria
- Allows defining a criteria for when a system should run
- Default RunCriteria is RunAlways
```cs
// Runs system at 120 fps
new RunFixed(120),
// Runs system once
new RunOnce(),
```

#### Fetch
- `IFetch<T>` is used to configure how data is injected into systems
- If you add data to `world.Resources` that is now able to be used as a system parameter.
- More advanced use cases can set up access to sub-data in Resources, allowing for better automatic scheduling of systems.
    - As an example there is `Assets<T>` that is fetching from `AssetServer` that is stored in the `Resources` on `World`

### Coroutines
- Special type of system that allows step-by-step actions
- Makes use of a custom `Yield` type to handle control flow
- Supports custom yield actions, example found in [Coroutine Example](./examples/Application/Examples/ECS/CoroutineExample.cs)

```cs
Coroutine.Create(new("TestCoroutine"),
static (param) =>
{
    return Routine();

    static IEnumerable<Yield> Routine()
    {
        yield return Yield.WaitForSeconds(1f);
        yield return Coroutine.WaitForEnterState(TestState.First);
        yield return Coroutine.WaitForExitState(TestState.First);
    }
})
```

Coroutines can also be auto generated in system sets
```cs
[SystemSet]
static partial class GeneratedSystemSet
{
    // `nameof(ExampleSystem)` is the method that should run for this coroutine
    [Coroutine(nameof(ExampleSystem))] 
    static readonly SystemBuilderDescriptor ExampleSystemDescriptor = new()
    {
        Stage = CoreStage.Update,
    };

    static IEnumerable<Yield> ExampleSystem(Time time)
    {
        yield return Yield.WaitForSeconds(1f);
    }
}
```

### Schedule
- Schedule defines system execution order
- Schedule is split up into Stages
- The default schedule is found in CoreStage
```cs
// CoreStage
Init // Runs once
PostInit // Runs once

First
PreUpdate
Update
PostUpdate
Last

PreRender
Render
PostRender
```

#### StageGraph scheduling
Systems are automatically scheduled depending on the `RunsBefore`, `RunsAfter` and `Dependencies` of the systems. A systems parameters are automatically set as dependencies, including the arguments of a `Query<T>`. This ensures that all the dependencies of all systems are known statically before the schedule has ran, allowing us to more safely run systems in parallel in the future (parallel execution is currently not implemented).  

This means there are a few things to keep in mind when creating systems:
- Use what is needed
- Granular dependencies can be benefitial (as seen with Assets later)
- All Queries a system uses should be injected as a system parameter

### Commands
- Command buffers are used to record ECS operations
- Commands are flushed at the end of each Stage in the Schedule
- Commands can be injected into systems
```cs
// Inject into systems to use
static void SomeSystem(Commands commands) {}

// Spawn entity
var entityBuilder = commands.Spawn(Entity.With(Transform2D.Default, Health.Default));

// Despawn entity
commands.Despawn(entity);

// Add component
commands.AddComponent(entity, Health.Default);

// Remove component
commands.RemoveComponent<Health>(entity);

// Set parent
commands.SetParent(entity, parentEntity);

// Set children
commands.AddChild(entity, child);
commands.AddChildren(entity, children);

// remove children
commands.RemoveChild(entity, child);
commands.RemoveChildren(entity, children);

// Despawn hierarchy
commands.DespawnHierarchy(entity);
```

### Events
- Events are used to communicate between systems
- `EventReader<T>` and `EventWriter<T>` are used to read and write events
- Each reader has it's own cursor, so multiple systems can read off the latest events
- Events are kept for 2 frames before being overwritten
    - This means that systems running at a fixed frame rate different from the main loop it can miss out on events

```cs
// Can pre-init the events to avoid first-use initialization
world.Events.InitEvent<SomeEvent>();

static void WriteSystem(EventWriter<SomeEvent> writer)
{
    writer.Write(new SomeEvent());
}

static void ReadSystem(EventReader<SomeEvent> reader)
{
    // Calling .Read() will consume all the events since the last call to .Read()
    // There is also .Consume() which is a more optimal way to consume all events
    // .Peek() can be used to peek at the events without consuming them
    foreach (scoped ref readonly var e in reader.Read())
    {

    }
}
```

#### EventRunCriteria
- `EventRunCriteria<SomeEvent>.Create` can be used as a RunCriteria for systems
- Ensures the system only runs when there are new events of type `SomeEvent`

### Hierarchy
- Parent/Child relationships are supported
- One-to-many relationship
- More commands are listed under the [Commands](#commands) section
```cs
commands.Spawn(Entity.With(new MyComponent()))
    .AddChildren([
        commands.Spawn(Entity.With(new MyChildComponent())).Entity,
        commands.Spawn(Entity.With(new MyChildComponent())).Entity,
    ]);

commands.Spawn(Entity.With(new MyChildComponent()))
    .AddParent(parentEntity);

foreach (var child in new Query(world).HierarchyDFS())
{
    // iterate over hierarchy depth first
}
```

## Plugins
- Pollus is a plugin based engine, meaning almost every feature is a plugin
- Right now some core things like Window and GraphicsContext are not controlled by Plugins

```cs
class MyPlugin : IPlugin
{
    // Define required dependencies of the plugin
    // Dependencies will be pulled from here if they are not imported from other parts of the program
    public PluginDependency[] Dependencies =>
    [
        PluginDependency.From(() => SomeOtherPlugin.Default),
        PluginDependency.From<MyOtherPlugin>(),
    ];

    public void Apply(World world)
    {
        // Configure plugin on the world
    }
}
```

## Resources
- Resources stores shared data on the `World` to be accessed by `Systems`
- Supports adding by interface
```cs
world.Resources.Add(new MyResource());
world.Resources.Add<IMyResource>(new MyResource());

static void MySystem(MyResource myResource, IMyResource myResourceByInterface)
{
    // access MyResource here
}
```

## AssetServer
- AssetServer is used to load assets from AssetIOs
- AssetIO can be implemented to load from any source
    - FileAssetIO is the default AssetIO
- AssetLoaders can be implemented to load assets by extension
- `AssetEvent<TAsset>` used to listen for asset changes
- `AssetPlugin.Default` for easy setup
    - Assets are loaded from the `assets` directory in root

```cs
// Can easily be added to World as a Plugin
world.AddPlugin(AssetPlugin.Default);

// AssetServer lives in the Resources container
var assetServer = world.Resources.Get<AssetServer>();

// Assets are partial and can be autogenerated
// Dependencies are recursively generated for automatically when using AssetAttribute
// This allows assets to not send the Loaded event until their dependencies are loaded
[Asset]
partial class MyAsset { }

static void MySystem(AssetServer assetServer, Assets<MyAsset> myAssets)
{
    // Load assets synchronously
    var assetHandle = assetServer.Load<MyAsset>("path/to/asset.mine");

    // Load assets asynchronously
    var assetHandleAsync = assetServer.LoadAsync<MyAsset>("path/to/asset.mine");

    // Add directly to the Assets container
    var assetHandleDirect = myAssets.Add(new MyAsset());

    // Assets can be retrieved from the Assets<T> container
    var asset = myAssets.Get(assetHandle);
    // obviously have to wait until asset is loaded when calling LoadAsync
    var assetAsync = myAssets.Get(assetHandleAsync);
    var assetDirect = myAssets.Get(assetHandleDirect);
}

// Reading asset events
static void MySystem(EventReader<AssetEvent<MyAsset>> myAssetEvents)
{
    foreach (scoped ref readonly var e in myAssetEvents.Read())
    {
        if (e.Type is AssetEventType.Loaded)   
        {
            // Asset is ready for use
        }
    }
}
```

### AssetLoader
- AssetLoaders are responsible for converting raw data into assets
- AssetLoaders are registered on the AssetServer
- Supports asset hot-reload on supported systems via FileSystemWatcher
    - Shaders, Textures, Scenes, and other internal engine assets supports hot-reload
    - Custom asset types needs to implement handling of hot-reload

```cs
world.Resources.Get<AssetServer>().AddLoader(new TextAssetLoader());

public class TextAssetLoader : AssetLoader<TextAsset>
{
    public override string[] Extensions => [".txt"];

    protected override void Load(ReadOnlySpan<byte> data, ref LoadContext context)
    {
        var asset = new TextAsset(Encoding.UTF8.GetString(data));
        context.SetAsset(asset);
    }
}
```

## Transform2D
- Handles 2D transformations
- `TransformPlugin<Transform2D>` to enable hierarchy support
- `GlobalTransform` is required for transform hierarchy
    - Final transform value is calculated into the `Matrix` on `GlobalTransform`
- `ZIndex` to control render order

## Rendering
- TODO

### Materials
- Materials contains information about the pipeline for rendering objects with a shader

### Shaders
- WGSL is the shader language

### Compute Shaders
- Engine has support for compute shaders
- Functions in the same way as a regular material
- Configured via ComputeCommands and dispatched on a compute encoder
- Example can be found in [Compute Example](./examples/ComputeExample.cs)

### FrameGraph
- TODO

### Sprites
- TODO

### Shapes
- TODO

### Meshes
- TODO

### Text
- TODO

## Scene
- Scenes is a serializable collection of entities
- Can save and load flat and hierarchical entities
- Serialized as JSON
- Supports hot-reload of scene asset and any used sub-assets
- Full example can be found in the [Scene Example](./examples/SceneExample.cs)

## Tween
- Engine supports tweening object properties in an efficient ECS way
- Can tween any component property
- Currently have to manually register components you want to tween
- Supports sequencing of tweens

```cs
// Single tween that runs forever
Tween.Create(entity, (Transform2D comp) => comp.Position)
    .WithFromTo(pos, pos + Vec2f.Up * 64f)
    .WithDuration(2f)
    .WithEasing(Easing.Quartic)
    .WithFlags(TweenFlag.PingPong)
    .Append(commands);
```

```cs
// Sequence of tweens
Tween.Sequence(commands)
    .WithFlags(TweenFlag.Loop)
    .Then(Tween.Create(entity, (Transform2D comp) => comp.Position)
        .WithFromTo(pos, pos + Vec2f.Up * 64f)
        .WithDuration(2f)
        .WithFlags(TweenFlag.None)
        .WithEasing(Easing.Quartic)
    )
    .Then(Tween.Create(entity, (Transform2D comp) => comp.Scale)
        .WithFromTo(Vec2f.One * 4, Vec2f.One * 8)
        .WithDuration(1f)
        .WithFlags(TweenFlag.None)
        .WithEasing(Easing.Quartic)
    )
    .Append();
```

## Audio
- There is a limited audio system that allows playing sounds
- Very basic right now
- Example can be found in the [Audio Example](./examples/AudioExample.cs)

## Input
- Keyboard, Mouse and Gamepad input is supported (uses SDL)
- Inputs are read via the Event system, but can be read via polling as well
    - `EventReader<ButtonEvent<Key>>` to read via events
    - `ButtonInput<Key>` to read via polling
- Example can be found in the [Input Example](./examples/InputExample.cs)