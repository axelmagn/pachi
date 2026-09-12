using Godot;
using System.Collections.Generic;

[GlobalClass]
public partial class RemoveOutputBallArchetype : CardArchetype
{
    public override List<CardData> GenerateValidOptions(CardGenerationContext context)
    {
        var options = new List<CardData>();
        var seen = new HashSet<string>();

        foreach (var pocket in context.Pockets)
        {
            if (pocket.OutputBalls == null || pocket.OutputBalls.Count <= 1) continue;

            foreach (var ball in pocket.OutputBalls)
            {
                if (ball == null) continue;
                int idx = context.BallTiers.IndexOf(ball);
                if (idx < 0) continue;
                string key = $"{ball.ResourcePath}";
                if (seen.Add(key))
                {
                    options.Add(new RemoveOutputBallCardData
                    {
                        BallToRemove = ball,
                        Title = $"Out: -T{idx + 1}",
                        Description = $"Remove a Tier {idx + 1} payout ball. Gives a Ball Pack to hopper!",
                        CardColor = new Color(0.6f, 0.3f, 0.3f),
                        BonusBalls = CardGenerator.GeneratePackBalls(context)
                    });
                }
            }
        }
        return options;
    }
}
