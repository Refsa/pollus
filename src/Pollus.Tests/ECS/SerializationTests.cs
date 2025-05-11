#pragma warning disable CA1416
namespace Pollus.Tests.ECS;

using Pollus.ECS;
using Pollus.Engine.Assets;
using Pollus.Engine.Serialization;

public class SerializationTests
{
    [Fact]
    public void Test_Serialize_Deserialize()
    {
        WorldSnapshot? worldSnapshot = null;

        {
            using var serializeWorld = new World();
            serializeWorld.AddPlugin(new AssetPlugin { RootPath = "assets", });
            serializeWorld.AddPlugin<SerializationPlugin<BinarySerialization>>();
            serializeWorld.AddPlugin<SnapshotSerializationPlugin>();
            serializeWorld.Prepare();

            for (int i = 0; i < 1_000; i++)
            {
                Entity.With(new TestComponent1 { Value = i + 1 }, new SerializeTag()).Spawn(serializeWorld);
            }

            serializeWorld.Events.GetWriter<SnapshotSerializeEvent>().Write(new SnapshotSerializeEvent { Path = "snapshots/test.bin" });
            serializeWorld.Update();

            var serializedResult = serializeWorld.Events.GetReader<SnapshotSerializeResultEvent>()?.Read()[0];
            Assert.NotNull(serializedResult);

            worldSnapshot = serializeWorld.Resources.Get<AssetServer>().GetAssets<WorldSnapshot>().Get(serializedResult.Value.SnapshotHandle);
            Assert.NotNull(worldSnapshot);
        }

        {
            using var deserializeWorld = new World();
            deserializeWorld.AddPlugin<SerializationPlugin<BinarySerialization>>();
            deserializeWorld.AddPlugin<SnapshotSerializationPlugin>();
            deserializeWorld.AddPlugin(new AssetPlugin { RootPath = "assets", });
            deserializeWorld.Prepare();

            var worldSnapshotHandle = deserializeWorld.Resources.Get<AssetServer>().Assets.Add(worldSnapshot, "snapshots/test.bin");
            deserializeWorld.Events.GetWriter<SnapshotDeserializeEvent>().Write(new() { Snapshot = worldSnapshotHandle });
            deserializeWorld.Update();

            var index = 0;
            var q = new Query<TestComponent1>.Filter<All<SerializeTag>>(deserializeWorld);
            Assert.Equal(1_000, q.EntityCount());
            foreach (var entity in q)
            {
                Assert.Equal(index++, entity.Entity.ID);
                Assert.Equal(index, entity.Component0.Value);
            }
        }
    }
}
#pragma warning restore CA1416