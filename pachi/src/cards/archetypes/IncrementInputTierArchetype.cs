using Godot;
using System.Collections.Generic;

[GlobalClass]
public partial class IncrementInputTierArchetype : CardArchetype
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
                if (idx >= 0 && idx < context.BallTiers.Count - 1)
                {
                    var resultTier = context.BallTiers[idx + 1];
                    string key = $"{ball.ResourcePath}_{resultTier.ResourcePath}";
                    if (seen.Add(key))
                    {
                        options.Add(new ModifyInputTierCardData
                        {
                            TargetTier = ball,
                            ResultTier = resultTier,
                            Title = $"In: T{idx + 1} -> T{idx + 2}",
                            Description = $"Increase 1 Tier {idx + 1} input requirement to Tier {idx + 2}. Gives a Ball Pack to hopper!",
                            CardColor = new Color(0.2f, 0.45f, 0.75f),
                            BonusBalls = CardGenerator.GeneratePackBalls(context)
                        });
                    }
                }
            }
        }
        return options;
    }
}
