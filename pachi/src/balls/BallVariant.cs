using Godot;
using System;

/// Configuration for a ball variant
[GlobalClass]
public partial class BallVariant : Resource
{
    /// The color the ball's placeholder circle renders as when a sprite is not available.
    [Export]
    public Color PlaceholderColor = Colors.White;

    /// A hidden price associated with the ball, used to track the exchange rate of different balls
    /// as well as certain background point calculations.
    [Export]
    public int BasePrice = 10;

    // TODO: add sprite attribute when art is ready
}
