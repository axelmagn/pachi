using Godot;

public partial class GameEvents : RefCounted
{
    [Signal]
    public delegate void AddDefaultBallsEventHandler(int numBalls);

    [Signal]
    public delegate void CardClickedEventHandler(Card card);
}
