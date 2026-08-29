using Godot;
using System;
using System.Collections.Generic;
using System.Diagnostics;

public partial class CardDragController : CanvasLayer
{
    public static CardDragController? Instance { get; private set; }

    public bool IsDragging { get; private set; } = false;
    public CardData? CurrentCard { get; private set; }

    private Control? _dragPreview;
    private Label? _previewTitleLabel;
    private Panel? _previewPanel;
    private Node2D? _currentHoverTarget;
    private readonly Dictionary<Node2D, float> _targets = [];

    public override void _Ready()
    {
        Instance = this;
        CreateDragPreviewNode();
    }

    public override void _ExitTree()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    public void RegisterTarget(Node2D target, float radius = 40.0f)
    {
        if (target != null && !_targets.ContainsKey(target))
        {
            _targets[target] = radius;
        }
    }

    public void UnregisterTarget(Node2D target)
    {
        if (target != null)
        {
            _targets.Remove(target);
            if (_currentHoverTarget == target)
            {
                SetTargetHighlight(_currentHoverTarget, false);
                _currentHoverTarget = null;
            }
        }
    }

    public void StartDrag(CardData cardData, Vector2 initialScreenPosition)
    {
        if (cardData == null || IsDragging) return;

        IsDragging = true;
        CurrentCard = cardData;

        if (_previewTitleLabel != null)
        {
            _previewTitleLabel.Text = cardData.Title;
        }

        StyleBoxFlat style = new StyleBoxFlat();
        style.BgColor = cardData.CardColor;
        style.SetCornerRadiusAll(6);
        style.SetBorderWidthAll(2);
        style.BorderColor = Colors.White;
        if (_previewPanel != null)
        {
            _previewPanel.AddThemeStyleboxOverride("panel", style);
        }

        if (_dragPreview != null)
        {
            _dragPreview.GlobalPosition = initialScreenPosition - (_dragPreview.Size / 2.0f);
            _dragPreview.Visible = true;
        }
    }

    public override void _Input(InputEvent @event)
    {
        if (!IsDragging) return;

        if (@event is InputEventMouseMotion motion)
        {
            if (_dragPreview != null)
            {
                _dragPreview.GlobalPosition = motion.GlobalPosition - (_dragPreview.Size / 2.0f);
            }
            UpdateHoverTarget(motion.GlobalPosition);
        }
        else if (@event is InputEventMouseButton mouseButton && mouseButton.ButtonIndex == MouseButton.Left && !mouseButton.Pressed)
        {
            EndDrag(mouseButton.GlobalPosition);
        }
    }

    private void UpdateHoverTarget(Vector2 screenPosition)
    {
        Node2D? newHover = null;

        foreach (var (target, radius) in _targets)
        {
            if (target == null || !target.IsInsideTree()) continue;

            if (GetScreenBounds(target, radius).HasPoint(screenPosition) && CurrentCard != null && CurrentCard.CanApply(target))
            {
                newHover = target;
                break;
            }
        }

        if (_currentHoverTarget != newHover)
        {
            if (_currentHoverTarget != null)
            {
                SetTargetHighlight(_currentHoverTarget, false);
            }
            _currentHoverTarget = newHover;
            if (_currentHoverTarget != null)
            {
                SetTargetHighlight(_currentHoverTarget, true);
            }
        }
    }

    private void EndDrag(Vector2 releaseScreenPosition)
    {
        if (!IsDragging) return;

        if (_currentHoverTarget != null)
        {
            if (CurrentCard != null && CurrentCard.CanApply(_currentHoverTarget))
            {
                if (CurrentCard.Apply(_currentHoverTarget))
                {
                    Debug.Assert(CardSidebar.Instance != null, "CardSidebar.Instance must not be null when card is applied");
                    CardSidebar.Instance.RemoveCard(CurrentCard);
                }
            }
            SetTargetHighlight(_currentHoverTarget, false);
            _currentHoverTarget = null;
        }

        IsDragging = false;
        CurrentCard = null;
        if (_dragPreview != null)
        {
            _dragPreview.Visible = false;
        }
    }

    private static Rect2 GetScreenBounds(Node2D target, float radius)
    {
        Viewport viewport = target.GetViewport();
        if (viewport != null)
        {
            Vector2 viewportLocalPos = target.GetViewportTransform() * target.GlobalPosition;
            Vector2 screenOffset = Vector2.Zero;
            Vector2 scale = Vector2.One;

            if (viewport.GetParent() is SubViewportContainer container)
            {
                screenOffset = container.GetGlobalPosition();
                if (container.Stretch && viewport is SubViewport subViewport && subViewport.Size.X > 0 && subViewport.Size.Y > 0)
                {
                    scale = (Vector2)container.Size / (Vector2)subViewport.Size;
                }
            }
            else if (viewport.GetParent() is Control parentControl)
            {
                screenOffset = parentControl.GetGlobalPosition();
            }

            Vector2 screenPos = screenOffset + (viewportLocalPos * scale);
            float scaledRadius = radius * scale.X;
            return new Rect2(screenPos - new Vector2(scaledRadius, scaledRadius), new Vector2(scaledRadius * 2, scaledRadius * 2));
        }

        return new Rect2(target.GlobalPosition - new Vector2(radius, radius), new Vector2(radius * 2, radius * 2));
    }

    private static void SetTargetHighlight(Node2D target, bool highlighted)
    {
        if (target != null && target.IsInsideTree())
        {
            target.Modulate = highlighted ? new Color(1.4f, 1.4f, 1.4f) : Colors.White;
        }
    }

    private void CreateDragPreviewNode()
    {
        _dragPreview = new Control();
        _dragPreview.Name = "DragPreview";
        _dragPreview.CustomMinimumSize = new Vector2(120, 70);
        _dragPreview.Size = new Vector2(120, 70);
        _dragPreview.MouseFilter = Control.MouseFilterEnum.Ignore;
        _dragPreview.Visible = false;

        _previewPanel = new Panel();
        _previewPanel.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        _previewPanel.MouseFilter = Control.MouseFilterEnum.Ignore;
        _dragPreview.AddChild(_previewPanel);

        _previewTitleLabel = new Label();
        _previewTitleLabel.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        _previewTitleLabel.HorizontalAlignment = HorizontalAlignment.Center;
        _previewTitleLabel.VerticalAlignment = VerticalAlignment.Center;
        _previewTitleLabel.AutowrapMode = TextServer.AutowrapMode.WordSmart;
        _previewTitleLabel.MouseFilter = Control.MouseFilterEnum.Ignore;
        _previewTitleLabel.AddThemeColorOverride("font_color", Colors.White);
        _previewTitleLabel.AddThemeFontSizeOverride("font_size", 12);
        _dragPreview.AddChild(_previewTitleLabel);

        AddChild(_dragPreview);
    }
}
