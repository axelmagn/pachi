using System.Collections.Generic;
using Chickensoft.GoDotTest;
using Godot;

public abstract partial class IntegrationTestClass : TestClass
{
    private readonly List<Node> _trackedNodes = [];

    protected IntegrationTestClass(Node testScene) : base(testScene)
    {
    }

    protected T InstantiateAndTrack<T>(string scenePath) where T : Node
    {
        var packedScene = GD.Load<PackedScene>(scenePath);
        var instance = packedScene.Instantiate<T>();
        TrackNode(instance);
        TestScene.AddChild(instance);
        return instance;
    }

    protected T TrackNode<T>(T node) where T : Node
    {
        _trackedNodes.Add(node);
        return node;
    }

    [Cleanup]
    public void CleanupTrackedNodes()
    {
        // Calling QueueFree() while nodes remain in the scene tree allows Godot's SceneTree
        // deletion queue to properly free child hierarchies and release CanvasItem RIDs.
        // We avoid calling RemoveChild() before QueueFree() because orphaned nodes are not
        // processed by the SceneTree's frame cleanup.
        foreach (var node in _trackedNodes)
        {
            if (GodotObject.IsInstanceValid(node))
            {
                node.QueueFree();
            }
        }
        _trackedNodes.Clear();
    }
}
