using Godot;
using System.Collections.Generic;

[GlobalClass]
public partial class IncrementOutputTierArchetype : CardArchetype
{
    public override List<CardData> GenerateValidOptions(CardGenerationContext context)
    {
        var options = new List<CardData>();
        var seen = new HashSet<string>();

        foreach (var pocket in context.Pockets)
        {
            if (pocket.OutputBalls == null) continue;
            foreach (var ball in pocket.OutputBalls)
            {
                if (ball == null) continue;
                int idx = context.BallTiers.IndexOf(ball);
                if (idx >= 0 && idx < context.BallTiers.Count - 1)
                {
                    var resultTier = context.BallTiers[idx + 1];
                    string key = $"{ball.ResourcePath}_{resultTier.ResourcePath}";
                    if (seen.Add(key))
                    {
                        options.Add(new ModifyOutputTierCardData
                        {
                            TargetTier = ball,
                            ResultTier = resultTier,
                            Title = $"Out: T{idx + 1} -> T{idx + 2}",
                            Description = $"Upgrade 1 Tier {idx + 1} output payout to Tier {idx + 2}.",
                            CardColor = new Color(0.75f, 0.45f, 0.2f)
                        });
                    }
                }
            }
        }
        return options;
    }
}
