using Godot;
using System.Collections.Generic;

[GlobalClass]
public partial class DecrementInputTierArchetype : CardArchetype
{
    public override List<CardData> GenerateValidOptions(CardGenerationContext context)
    {
        var options = new List<CardData>();
        var seen = new HashSet<string>();

        foreach (var pocket in context.Pockets)
        {
            if (pocket.InputBalls == null) continue;
            foreach (var ball in pocket.InputBalls)
            {
                if (ball == null) continue;
                int idx = context.BallTiers.IndexOf(ball);
                if (idx > 0)
                {
                    var resultTier = context.BallTiers[idx - 1];
                    string key = $"{ball.ResourcePath}_{resultTier.ResourcePath}";
                    if (seen.Add(key))
                    {
                        options.Add(new ModifyInputTierCardData
                        {
                            TargetTier = ball,
                            ResultTier = resultTier,
                            Title = $"In: T{idx + 1} -> T{idx}",
                            Description = $"Decrease 1 Tier {idx + 1} input requirement to Tier {idx}.",
                            CardColor = new Color(0.35f, 0.4f, 0.65f)
                        });
                    }
                }
            }
        }
        return options;
    }
}
