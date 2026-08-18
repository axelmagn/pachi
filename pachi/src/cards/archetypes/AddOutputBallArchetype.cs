using Godot;
using System.Collections.Generic;

[GlobalClass]
public partial class AddOutputBallArchetype : CardArchetype
{
    public override List<CardData> GenerateValidOptions(CardGenerationContext context)
    {
        var options = new List<CardData>();
        var seen = new HashSet<string>();

        foreach (var pocket in context.Pockets)
        {
            if (pocket.OutputBalls != null && pocket.OutputBalls.Count >= 6) continue;

            var candidates = new List<BallVariant>();
            if (pocket.OutputBalls != null)
            {
                candidates.AddRange(pocket.OutputBalls);
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
                    options.Add(new AddOutputBallCardData
                    {
                        BallToAdd = ball,
                        Title = $"Out: +T{idx + 1}",
                        Description = $"Add a Tier {idx + 1} ball to pocket payout.",
                        CardColor = new Color(0.7f, 0.55f, 0.2f)
                    });
                }
            }
        }
        return options;
    }
}
