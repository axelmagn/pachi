using Godot;
using System.Collections.Generic;

[GlobalClass]
public partial class BallPackArchetype : CardArchetype
{
    public override float GetWeight(CardGenerationContext context)
    {
        return Mathf.Clamp(6.0f - (context.HopperBallCount * 0.35f), 0.5f, 6.0f) * BaseWeight;
    }

    public override List<CardData> GenerateValidOptions(CardGenerationContext context)
    {
        var packBalls = CardGenerator.GeneratePackBalls(context);
        return new List<CardData>
        {
            new BallPackCardData
            {
                Title = $"Ball Pack ({packBalls.Count}x)",
                Description = $"Drag to Hopper to add {packBalls.Count} balls.",
                PackBalls = packBalls,
                CardColor = new Color(0.2f, 0.6f, 0.7f),
            }
        };
    }
}
