using Godot;
using System.Diagnostics;

[Tool]
[GlobalClass]
public partial class CardUI : PanelContainer
{
    private CardData? _cardData;
    private Label? _titleLabel;
    private Label? _descriptionLabel;
    private Control? _indicatorContainer;
    private bool _isPressed = false;
    private Vector2 _pressPosition;

    [Export]
    public float DragThreshold { get; set; } = 5.0f;

    [Export]
    public CardData? CardData
    {
        get => _cardData;
        set
        {
            if (_cardData == value) return;
            _cardData = value;
            UpdateDisplay(repopulateIndicators: true);
        }
    }

    public override void _Ready()
    {
        _titleLabel = GetNodeOrNull<Label>("%TitleLabel");
        _descriptionLabel = GetNodeOrNull<Label>("%DescriptionLabel");
        _indicatorContainer = GetNodeOrNull<Control>("%IndicatorContainer");

        UpdateDisplay(repopulateIndicators: true);
    }

    public override void _GuiInput(InputEvent @event)
    {
        if (Engine.IsEditorHint()) return;
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

    private void UpdateDisplay(bool repopulateIndicators = false)
    {
        if (_titleLabel == null) _titleLabel = GetNodeOrNull<Label>("%TitleLabel");
        if (_descriptionLabel == null) _descriptionLabel = GetNodeOrNull<Label>("%DescriptionLabel");
        if (_indicatorContainer == null) _indicatorContainer = GetNodeOrNull<Control>("%IndicatorContainer");

        if (_cardData != null)
        {
            if (_titleLabel != null && _titleLabel.Text != _cardData.Title)
            {
                _titleLabel.Text = _cardData.Title;
            }
            if (_descriptionLabel != null && _descriptionLabel.Text != _cardData.Description)
            {
                _descriptionLabel.Text = _cardData.Description;
            }

            if (_indicatorContainer != null && (repopulateIndicators || _indicatorContainer.GetChildCount() == 0))
            {
                foreach (Node child in _indicatorContainer.GetChildren())
                {
                    _indicatorContainer.RemoveChild(child);
                    child.QueueFree();
                }

                _cardData.PopulateCardUI(_indicatorContainer);
            }
        }
    }
}
