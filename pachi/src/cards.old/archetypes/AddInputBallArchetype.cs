using Godot;
using System.Collections.Generic;

[GlobalClass]
public partial class AddInputBallArchetype : CardArchetype
{
    public override List<CardData> GenerateValidOptions(CardGenerationContext context)
    {
        var options = new List<CardData>();
        var seen = new HashSet<string>();

        foreach (var pocket in context.Pockets)
        {
            if (pocket.InputBalls != null && pocket.InputBalls.Count >= Pocket.MaxInputCapacity) continue;

            var candidates = new List<BallVariant>();
            if (pocket.InputBalls != null)
            {
                candidates.AddRange(pocket.InputBalls);
            }
            candidates.Add(context.BallTiers[0]);

            foreach (var ball in candidates)
            {
                if (ball == null) continue;
                int idx = context.BallTiers.IndexOf(ball);
                if (idx < 0) continue;
                string key = $"{ball.ResourcePath}";
                if (seen.Add(key))
                {
                    options.Add(new AddInputBallCardData
                    {
                        BallToAdd = ball,
                        Title = $"In: +T{idx + 1}",
                        Description = $"Add a Tier {idx + 1} input requirement. Gives a Ball Pack to hopper!",
                        CardColor = new Color(0.2f, 0.6f, 0.5f),
                        BonusBalls = CardGenerator.GeneratePackBalls(context)
                    });
                }
            }
        }
        return options;
    }
}
