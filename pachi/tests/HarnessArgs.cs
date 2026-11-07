using System;
using System.Collections.Generic;
using System.Globalization;
using Godot;

/// <summary>
/// Reads harness options from the Godot command line, as either
/// <c>--name=value</c> or <c>--name value</c> (before or after <c>--</c>).
/// </summary>
public static class HarnessArgs
{
    /// Seed used when a harness gets no --seed, so runs are reproducible by default.
    public const int DefaultSeed = 1;

    public static List<string> All()
    {
        var all = new List<string>(OS.GetCmdlineArgs());
        all.AddRange(OS.GetCmdlineUserArgs());
        return all;
    }

    public static string? Get(string name)
    {
        List<string> args = All();
        string flag = $"--{name}";
        for (int i = 0; i < args.Count; i++)
        {
            if (args[i].StartsWith(flag + "=", StringComparison.Ordinal))
            {
                return args[i][(flag.Length + 1)..];
            }
            if (args[i] == flag && i + 1 < args.Count)
            {
                return args[i + 1];
            }
        }
        return null;
    }

    /// Seeds GameConfig's gameplay RNG and Godot's global RNG (cosmetic randomness).
    public static int ApplySeed()
    {
        string? raw = Get("seed");
        int seed = raw == null ? DefaultSeed : int.Parse(raw, CultureInfo.InvariantCulture);
        GameConfig.SeedOverride = seed;
        GD.Seed((ulong)seed);
        GD.Print($"Harness seed: {seed}");
        return seed;
    }
}
