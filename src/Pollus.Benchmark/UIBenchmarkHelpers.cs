namespace Pollus.Benchmark;

using Pollus.ECS;
using Pollus.Input;
using Pollus.UI;
using Pollus.UI.Layout;
using LayoutStyle = Pollus.UI.Layout.Style;

public static class UIBenchmarkHelpers
{
    public static World CreateUIWorld()
    {
        var world = new World();
        world.AddPlugin(new UISystemsPlugin(), addDependencies: true);
        world.Resources.Add(new CurrentDevice<Mouse>());
        world.Resources.Add(new ButtonInput<MouseButton>());
        world.Resources.Add(new ButtonInput<Key>());
        world.Prepare();
        return world;
    }

    public static Entity SpawnRoot(Commands commands, float width, float height)
    {
        return commands.Spawn(Entity.With(
            new UINode(),
            new UILayoutRoot { Size = new Size<float>(width, height) },
            new UIStyle
            {
                Value = LayoutStyle.Default with
                {
                    FlexDirection = FlexDirection.Column,
                    Size = new Size<Length>(Length.Percent(100), Length.Percent(100)),
                }
            }
        )).Entity;
    }

    public static void SpawnFlatChildren(Commands commands, Entity parent, int count)
    {
        for (int i = 0; i < count; i++)
        {
            var child = commands.Spawn(Entity.With(
                new UINode(),
                new UIStyle
                {
                    Value = LayoutStyle.Default with
                    {
                        Size = new Size<Length>(Length.Percent(100), Length.Px(40)),
                    }
                }
            )).Entity;
            commands.AddChild(parent, child);
        }
    }

    public static Entity SpawnDeepChain(Commands commands, Entity parent, int depth)
    {
        var current = parent;
        for (int i = 0; i < depth; i++)
        {
            var child = commands.Spawn(Entity.With(
                new UINode(),
                new UIStyle
                {
                    Value = LayoutStyle.Default with
                    {
                        FlexDirection = FlexDirection.Column,
                        Size = new Size<Length>(Length.Percent(100), Length.Auto),
                        Padding = Rect<Length>.All(Length.Px(2)),
                    }
                }
            )).Entity;
            commands.AddChild(current, child);
            current = child;
        }
        return current;
    }

    public static void SpawnGrid(Commands commands, Entity parent, int cols, int rows)
    {
        for (int r = 0; r < rows; r++)
        {
            var row = commands.Spawn(Entity.With(
                new UINode(),
                new UIStyle
                {
                    Value = LayoutStyle.Default with
                    {
                        FlexDirection = FlexDirection.Row,
                        FlexWrap = FlexWrap.Wrap,
                        Size = new Size<Length>(Length.Percent(100), Length.Auto),
                    }
                }
            )).Entity;
            commands.AddChild(parent, row);

            for (int c = 0; c < cols; c++)
            {
                var cell = commands.Spawn(Entity.With(
                    new UINode(),
                    new UIStyle
                    {
                        Value = LayoutStyle.Default with
                        {
                            Size = new Size<Length>(Length.Px(40), Length.Px(40)),
                        }
                    }
                )).Entity;
                commands.AddChild(row, cell);
            }
        }
    }

    public static void SpawnInteractiveChildren(Commands commands, Entity parent, int count)
    {
        for (int i = 0; i < count; i++)
        {
            var child = commands.Spawn(Entity.With(
                new UINode(),
                new UIInteraction(),
                new UIStyle
                {
                    Value = LayoutStyle.Default with
                    {
                        Size = new Size<Length>(Length.Percent(100), Length.Px(40)),
                    }
                }
            )).Entity;
            commands.AddChild(parent, child);
        }
    }

    public static Entity SpawnInteractiveDeepChain(Commands commands, Entity parent, int depth)
    {
        var current = parent;
        for (int i = 0; i < depth; i++)
        {
            var child = commands.Spawn(Entity.With(
                new UINode(),
                new UIInteraction(),
                new UIStyle
                {
                    Value = LayoutStyle.Default with
                    {
                        FlexDirection = FlexDirection.Column,
                        Size = new Size<Length>(Length.Percent(100), Length.Auto),
                        Padding = Rect<Length>.All(Length.Px(2)),
                    }
                }
            )).Entity;
            commands.AddChild(current, child);
            current = child;
        }
        return current;
    }
}
