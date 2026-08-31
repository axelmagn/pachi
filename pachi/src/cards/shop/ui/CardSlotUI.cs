using Godot;
using System;

[GlobalClass]
public partial class CardSlotUI : PanelContainer
{
    [Signal]
    public delegate void SlotPressedEventHandler(int row, int col);

    [Export]
    public Label? TitleLabel { get; set; }

    [Export]
    public Label? CategoryLabel { get; set; }

    [Export]
    public Label? CostLabel { get; set; }

    [Export]
    public Label? DescLabel { get; set; }

    [Export]
    public Label? EmptyLabel { get; set; }

    [Export]
    public Control? VisibleCardContainer { get; set; }

    [Export]
    public Button? SelectButton { get; set; }

    public PackageDealCard? Card { get; private set; }
    public int Row { get; private set; } = 0;
    public int Col { get; private set; } = 0;
    public bool IsAffordable { get; private set; } = true;
    public bool IsSelected { get; private set; } = false;

    public override void _Ready()
    {
        InitControls();
        if (SelectButton != null)
        {
            SelectButton.Pressed += OnButtonPressed;
        }
    }

    public void InitControls()
    {
        if (VisibleCardContainer != null && EmptyLabel != null && SelectButton != null && TitleLabel != null)
        {
            return;
        }

        CustomMinimumSize = new Vector2(86, 125);
        SizeFlagsHorizontal = SizeFlags.ExpandFill;
        SizeFlagsVertical = SizeFlags.ExpandFill;

        if (VisibleCardContainer == null)
        {
            var vbox = new VBoxContainer();
            vbox.Name = "CardVBox";
            vbox.SizeFlagsHorizontal = SizeFlags.ExpandFill;
            vbox.SizeFlagsVertical = SizeFlags.ExpandFill;
            AddChild(vbox);
            VisibleCardContainer = vbox;

            TitleLabel = new Label { Name = "TitleLabel", HorizontalAlignment = HorizontalAlignment.Center, ClipText = true };
            TitleLabel.AddThemeFontSizeOverride("font_size", 10);
            vbox.AddChild(TitleLabel);

            CategoryLabel = new Label { Name = "CategoryLabel", HorizontalAlignment = HorizontalAlignment.Center };
            CategoryLabel.AddThemeFontSizeOverride("font_size", 9);
            CategoryLabel.Modulate = new Color(0.7f, 0.7f, 0.7f);
            vbox.AddChild(CategoryLabel);

            CostLabel = new Label { Name = "CostLabel", HorizontalAlignment = HorizontalAlignment.Center };
            CostLabel.AddThemeFontSizeOverride("font_size", 10);
            vbox.AddChild(CostLabel);

            DescLabel = new Label { Name = "DescLabel", HorizontalAlignment = HorizontalAlignment.Center, ClipText = true, AutowrapMode = TextServer.AutowrapMode.WordSmart };
            DescLabel.AddThemeFontSizeOverride("font_size", 8);
            DescLabel.Modulate = new Color(0.6f, 0.6f, 0.6f);
            DescLabel.SizeFlagsVertical = SizeFlags.ExpandFill;
            vbox.AddChild(DescLabel);

            SelectButton = new Button { Name = "SelectButton", Text = "Buy" };
            SelectButton.AddThemeFontSizeOverride("font_size", 10);
            vbox.AddChild(SelectButton);
        }

        if (EmptyLabel == null)
        {
            EmptyLabel = new Label
            {
                Name = "EmptyLabel",
                Text = "— Empty —",
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                SizeFlagsHorizontal = SizeFlags.ExpandFill,
                SizeFlagsVertical = SizeFlags.ExpandFill
            };
            EmptyLabel.AddThemeFontSizeOverride("font_size", 10);
            EmptyLabel.Modulate = new Color(0.4f, 0.4f, 0.4f);
            AddChild(EmptyLabel);
        }

        UpdateView();
    }

    public void SetCard(PackageDealCard? card, int row, int col)
    {
        InitControls();
        Card = card;
        Row = row;
        Col = col;
        IsSelected = false;
        UpdateView();
    }

    public void UpdateAffordability(bool canAfford)
    {
        InitControls();
        IsAffordable = canAfford;
        if (SelectButton != null)
        {
            SelectButton.Disabled = !canAfford || Card == null;
            SelectButton.Text = IsSelected ? "Cancel" : "Select";
        }
        Modulate = canAfford || Card == null ? Colors.White : new Color(0.6f, 0.6f, 0.6f, 0.8f);
    }

    public void SetSelected(bool selected)
    {
        InitControls();
        IsSelected = selected;
        if (SelectButton != null)
        {
            SelectButton.Text = selected ? "Cancel" : "Select";
        }
    }

    private void UpdateView()
    {
        if (Card == null)
        {
            if (VisibleCardContainer != null) VisibleCardContainer.Visible = false;
            if (EmptyLabel != null) EmptyLabel.Visible = true;
            if (SelectButton != null) SelectButton.Disabled = true;
            return;
        }

        if (VisibleCardContainer != null) VisibleCardContainer.Visible = true;
        if (EmptyLabel != null) EmptyLabel.Visible = false;

        if (TitleLabel != null) TitleLabel.Text = Card.Title;
        if (CategoryLabel != null) CategoryLabel.Text = Card.Category.ToString();
        if (CostLabel != null)
        {
            string tierName = Card.BallCostTier switch
            {
                1 => "Steel",
                2 => "Brass",
                3 => "Cobalt",
                4 => "Obsidian",
                _ => $"T{Card.BallCostTier}"
            };
            CostLabel.Text = $"Cost: {Card.BallCostCount}x {tierName}";
        }
        if (DescLabel != null) DescLabel.Text = Card.Description;
        if (SelectButton != null)
        {
            SelectButton.Disabled = !IsAffordable;
            SelectButton.Text = IsSelected ? "Cancel" : "Select";
        }
    }

    private void OnButtonPressed()
    {
        if (Card != null)
        {
            EmitSignal(SignalName.SlotPressed, Row, Col);
        }
    }
}
