using Godot;
using System.Diagnostics;

[Tool]
[GlobalClass]
public partial class CardUI : PanelContainer
{
	private static readonly StringName PanelStyleName = new("panel");

	private readonly VisualConfigBinding _binding;
	private VisualConfig? _configOverride;

	public CardUI()
	{
		_binding = new VisualConfigBinding(ApplyVisualConfig);
	}

	[Export]
	public VisualConfig? ConfigOverride
	{
		get => _configOverride;
		set
		{
			_configOverride = value;
			if (IsInsideTree())
			{
				_binding.Bind(_configOverride);
			}
		}
	}

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

	private CardData? _cardData;
	private Label? _titleLabel;
	private Label? _descriptionLabel;
	private Control? _indicatorContainer;
	private bool _isPressed = false;
	private Vector2 _pressPosition;
	private const float DragThreshold = 5.0f;

	public override void _EnterTree()
	{
		_binding.Bind(_configOverride);
	}

	public override void _ExitTree()
	{
		_binding.Unbind();
	}

	public override void _Ready()
	{
		_titleLabel = GetNodeOrNull<Label>("%TitleLabel");
		_descriptionLabel = GetNodeOrNull<Label>("%DescriptionLabel");
		_indicatorContainer = GetNodeOrNull<Control>("%IndicatorContainer");

		if (_binding.ActiveConfig != null)
		{
			ApplyVisualConfig(_binding.ActiveConfig);
		}
		else
		{
			UpdateDisplay(repopulateIndicators: true);
		}
	}

	public void ApplyVisualConfig(VisualConfig? config)
	{
		if (config == null) return;
		UpdateDisplay(config, repopulateIndicators: false);
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

	private void UpdateDisplay(VisualConfig? explicitConfig = null, bool repopulateIndicators = false)
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

		var activeConfig = explicitConfig ?? _binding?.ActiveConfig;
		if (_indicatorContainer != null && activeConfig != null)
		{
			foreach (Node child in _indicatorContainer.GetChildren())
			{
				if (child is BallAwardIndicator bai)
				{
					bai.ApplyVisualConfig(activeConfig);
				}
				else if (child is PocketBallsIndicator pbi)
				{
					pbi.ApplyVisualConfig(activeConfig);
				}
			}
		}

		Color bgColor = (activeConfig != null)
			? activeConfig.CardBackgroundColor
			: (_cardData != null ? _cardData.CardColor : new Color(0.2f, 0.4f, 0.8f, 1.0f));

		Color borderColor = (activeConfig != null)
			? activeConfig.CardBorderColor
			: new Color(1.0f, 1.0f, 1.0f, 0.4f);

		// StyleBoxFlat style = new StyleBoxFlat();
		// style.BgColor = bgColor;
		// style.SetCornerRadiusAll(6);
		// style.SetBorderWidthAll(1);
		// style.BorderColor = borderColor;
		// style.ContentMarginLeft = 4;
		// style.ContentMarginRight = 4;
		// style.ContentMarginTop = 4;
		// style.ContentMarginBottom = 4;
		// AddThemeStyleboxOverride(PanelStyleName, style);
	}
}
