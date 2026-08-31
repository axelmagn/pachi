using Godot;
using System;

[Tool]
[GlobalClass]
public partial class PackageDealCard : Resource
{
    private int _ballCostCount = 1;
    private int _ballCostTier = 1;
    private int _draftWeight = 100;

    [Export]
    public string CardId { get; set; } = string.Empty;

    [Export]
    public string Title { get; set; } = string.Empty;

    [Export(PropertyHint.MultilineText)]
    public string Description { get; set; } = string.Empty;

    [Export]
    public SocketCategory Category { get; set; } = SocketCategory.BeetlePocket;

    [Export]
    public PackedScene ComponentScene { get; set; } = null!;

    [Export(PropertyHint.Range, "1,4,1")]
    public int BallCostCount
    {
        get => _ballCostCount;
        set => _ballCostCount = Math.Clamp(value, 1, 4);
    }

    [Export(PropertyHint.Range, "1,4,1")]
    public int BallCostTier
    {
        get => _ballCostTier;
        set => _ballCostTier = Math.Clamp(value, 1, 4);
    }

    [Export]
    public Texture2D? IconTexture { get; set; }

    [Export]
    public Color AccentColor { get; set; } = Colors.White;

    [Export]
    public int DraftWeight
    {
        get => _draftWeight;
        set => _draftWeight = value;
    }

    [Export]
    public int BaseWeight
    {
        get => _draftWeight;
        set => _draftWeight = value;
    }
}
