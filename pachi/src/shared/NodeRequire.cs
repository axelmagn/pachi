using Godot;
using System;

public static class NodeRequire
{
    /// Throws if a required export is unassigned. Unlike Debug.Assert, this
    /// also fires in release builds, failing loudly at the misconfigured node.
    public static T Require<T>(this Node node, T? value, string name) where T : class =>
        value ?? throw new InvalidOperationException($"{node.GetPath()}: missing required export '{name}'");
}
