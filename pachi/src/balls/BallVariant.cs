using Godot;
using System;

/// Configuration for a ball variant
[Tool]
[GlobalClass]
public partial class BallVariant : Resource
{
    /// The color the ball's placeholder circle renders as when a sprite is not available.
    [Export]
    public Color PlaceholderColor = Colors.White;

    /// The sprite texture rendered for this ball variant. If null, ball falls back to PlaceholderColor.
    [Export]
    public Texture2D? Sprite { get; set; }

    /// A hidden price associated with the ball, used to track the exchange rate of different balls
    /// as well as certain background point calculations.
    [Export]
    public int BasePrice = 10;
}

