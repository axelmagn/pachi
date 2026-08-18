using Godot;
using System.Collections.Generic;

[GlobalClass]
public partial class RemoveInputBallArchetype : CardArchetype
{
    public override List<CardData> GenerateValidOptions(CardGenerationContext context)
    {
        var options = new List<CardData>();
        var seen = new HashSet<string>();

        foreach (var pocket in context.Pockets)
        {
            if (pocket.InputBalls == null || pocket.InputBalls.Count <= 1) continue;

            foreach (var ball in pocket.InputBalls)
            {
                if (ball == null) continue;
                int idx = context.BallTiers.IndexOf(ball);
                if (idx < 0) continue;
                string key = $"{ball.ResourcePath}";
                if (seen.Add(key))
                {
                    options.Add(new RemoveInputBallCardData
                    {
                        BallToRemove = ball,
                        Title = $"In: -T{idx + 1}",
                        Description = $"Remove a Tier {idx + 1} ball from pocket input requirement.",
                        CardColor = new Color(0.5f, 0.3f, 0.6f)
                    });
                }
            }
        }
        return options;
    }
}
