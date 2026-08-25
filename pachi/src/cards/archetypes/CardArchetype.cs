using Godot;
using System.Collections.Generic;

[Tool]
[GlobalClass]
public abstract partial class CardArchetype : Resource
{
    [Export]
    public float BaseWeight { get; set; } = 1.0f;

    public virtual float GetWeight(CardGenerationContext context) => BaseWeight;

    public abstract List<CardData> GenerateValidOptions(CardGenerationContext context);
}
