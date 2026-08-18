using Godot;
using System.Diagnostics;

[GlobalClass]
public partial class CardUI : PanelContainer
{
    [Export]
    public CardData CardData
    {
        get => _cardData;
        set
        {
            _cardData = value;
            UpdateDisplay();
        }
    }

    private CardData _cardData;
    private Label _titleLabel;
    private Label _descriptionLabel;
    private Control _indicatorContainer;
    private bool _isPressed = false;
    private Vector2 _pressPosition;
    private const float DragThreshold = 5.0f;

    public override void _Ready()
    {
        _titleLabel = GetNodeOrNull<Label>("%TitleLabel");
        _descriptionLabel = GetNodeOrNull<Label>("%DescriptionLabel");
        _indicatorContainer = GetNodeOrNull<Control>("%IndicatorContainer");
        UpdateDisplay();
    }

    public override void _GuiInput(InputEvent @event)
    {
        if (CardData == null) return;

        if (@event is InputEventMouseButton mouseButton && mouseButton.ButtonIndex == MouseButton.Left)
        {
            if (mouseButton.Pressed)
            {
                _isPressed = true;
                _pressPosition = mouseButton.GlobalPosition;
            }
            else
            {
                _isPressed = false;
            }
        }
        else if (@event is InputEventMouseMotion mouseMotion && _isPressed)
        {
            if (_pressPosition.DistanceTo(mouseMotion.GlobalPosition) >= DragThreshold)
            {
                _isPressed = false;
                Debug.Assert(CardDragController.Instance != null, "CardDragController.Instance must not be null when starting a drag");
                CardDragController.Instance.StartDrag(CardData, mouseMotion.GlobalPosition);
            }
        }
    }

    private void UpdateDisplay()
    {
        if (_cardData == null) return;

        if (_titleLabel != null)
        {
            _titleLabel.Text = _cardData.Title;
        }
        if (_descriptionLabel != null)
        {
            _descriptionLabel.Text = _cardData.Description;
        }

        if (_indicatorContainer != null)
        {
            foreach (Node child in _indicatorContainer.GetChildren())
            {
                child.QueueFree();
            }

            _cardData.PopulateCardUI(_indicatorContainer);
        }

        StyleBoxFlat style = new StyleBoxFlat();
        style.BgColor = _cardData.CardColor;
        style.SetCornerRadiusAll(6);
        style.SetBorderWidthAll(1);
        style.BorderColor = new Color(1, 1, 1, 0.4f);
        style.ContentMarginLeft = 4;
        style.ContentMarginRight = 4;
        style.ContentMarginTop = 4;
        style.ContentMarginBottom = 4;
        AddThemeStyleboxOverride("panel", style);
    }
}
