using Godot;
using System;
using System.Diagnostics;

[Tool]
[GlobalClass]
public partial class Socket2D : Node2D
{
    [Signal]
    public delegate void ComponentMountingEventHandler(Socket2D socket, Node2D incomingComponent);

    [Signal]
    public delegate void ComponentMountedEventHandler(Socket2D socket, Node2D mountedComponent);

    [Signal]
    public delegate void ComponentUnmountingEventHandler(Socket2D socket, Node2D outgoingComponent);

    [Signal]
    public delegate void ComponentUnmountedEventHandler(Socket2D socket, Node2D unmountedComponent);

    [Signal]
    public delegate void SocketClickedEventHandler(Socket2D socket);

    public static readonly StringName GroupSockets = new("sockets");

    private SocketCategory _category = SocketCategory.BeetlePocket;
    private string _socketId = string.Empty;
    private Vector2 _boundsSize = new(100, 140);

    [Export]
    public SocketCategory Category
    {
        get => _category;
        set
        {
            _category = value;
            QueueRedraw();
        }
    }

    [Export]
    public string SocketId
    {
        get => _socketId;
        set
        {
            _socketId = value;
            QueueRedraw();
        }
    }

    [Export]
    public Vector2 BoundsSize
    {
        get => _boundsSize;
        set
        {
            _boundsSize = value;
            QueueRedraw();
        }
    }

    [Export]
    public PackedScene? DefaultStarterScene { get; set; }

    public bool IsTargetHighlighted { get; private set; } = false;

    public void SetTargetHighlighted(bool highlighted)
    {
        if (IsTargetHighlighted == highlighted) return;
        IsTargetHighlighted = highlighted;
        QueueRedraw();
    }

    private Node2D? _currentComponent;

    public Node2D? CurrentComponent
    {
        get
        {
            if (_currentComponent == null)
            {
                AdoptChildComponent();
            }
            return _currentComponent;
        }
        private set => _currentComponent = value;
    }

    public override void _Ready()
    {
        AdoptChildComponent();

        if (Engine.IsEditorHint())
        {
            QueueRedraw();
            return;
        }

        AddToGroup(GroupSockets);
    }

    public void AdoptChildComponent()
    {
        if (_currentComponent != null) return;

        foreach (Node child in GetChildren())
        {
            if (child is Node2D node2D && child is ISocketComponent component)
            {
                _currentComponent = node2D;
                component.OnMounted(this);
                break;
            }
        }
    }

    public bool CanMount(PackageDealCard? card)
    {
        return card != null && card.Category == Category && card.ComponentScene != null;
    }

    public bool MountPackageDeal(PackageDealCard card, Hopper? hopper = null)
    {
        if (!CanMount(card))
        {
            return false;
        }

        if (CurrentComponent != null)
        {
            Node2D outgoing = CurrentComponent;
            EmitSignal(SignalName.ComponentUnmounting, this, outgoing);

            if (outgoing is ISocketComponent outgoingComp)
            {
                outgoingComp.OnUnmounting(this);
                outgoingComp.FlushActiveBalls(variant =>
                {
                    if (variant != null && hopper != null)
                    {
                        hopper.AddQueuedBalls(new[] { variant });
                    }
                });
            }

            outgoing.ProcessMode = ProcessModeEnum.Disabled;
            DisableDescendantColliders(outgoing);
            RemoveChild(outgoing);
            outgoing.QueueFree();
            CurrentComponent = null;
            EmitSignal(SignalName.ComponentUnmounted, this, outgoing);
        }

        Node2D? incoming = card.ComponentScene.Instantiate<Node2D>();
        if (incoming == null)
        {
            return false;
        }
        incoming.Position = Vector2.Zero;
        incoming.Rotation = 0.0f;
        CurrentComponent = incoming;
        AddChild(incoming);

        EmitSignal(SignalName.ComponentMounting, this, incoming);

        if (incoming is ISocketComponent incomingComp)
        {
            incomingComp.OnMounted(this);
        }

        EmitSignal(SignalName.ComponentMounted, this, incoming);
        TriggerMountFeedback();

        if (Engine.IsEditorHint())
        {
            QueueRedraw();
        }

        return true;
    }

    public bool ResetToStarter(Hopper? hopper = null)
    {
        if (DefaultStarterScene == null)
        {
            string defaultPath = Category switch
            {
                SocketCategory.BeetlePocket => "res://src/pockets/starter_pocket_center.tscn",
                SocketCategory.Spinner => "res://src/spinner/starter_spinner_left.tscn",
                SocketCategory.Yakumono => "res://src/yakumono/starter_yakumono.tscn",
                _ => string.Empty
            };
            if (!string.IsNullOrEmpty(defaultPath) && ResourceLoader.Exists(defaultPath))
            {
                DefaultStarterScene = ResourceLoader.Load<PackedScene>(defaultPath);
            }
        }

        if (DefaultStarterScene == null)
        {
            return false;
        }

        var starterCard = new PackageDealCard
        {
            Category = Category,
            ComponentScene = DefaultStarterScene,
            BallCostTier = 1,
            BallCostCount = 0
        };

        return MountPackageDeal(starterCard, hopper);
    }

    private void TriggerMountFeedback()
    {
        if (Engine.IsEditorHint() || !IsInsideTree()) return;
        // Hook for future audiovisual latch feedback
    }

    private static void DisableDescendantColliders(Node node)
    {
        if (node is CollisionShape2D shape)
        {
            shape.SetDeferred(CollisionShape2D.PropertyName.Disabled, true);
            shape.Disabled = true;
        }
        else if (node is CollisionPolygon2D polygon)
        {
            polygon.SetDeferred(CollisionPolygon2D.PropertyName.Disabled, true);
            polygon.Disabled = true;
        }

        foreach (Node child in node.GetChildren())
        {
            DisableDescendantColliders(child);
        }
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (!IsTargetHighlighted) return;

        if (@event is InputEventMouseButton mouseEvent && mouseEvent.Pressed && mouseEvent.ButtonIndex == MouseButton.Left)
        {
            Vector2 localPos = ToLocal(GetGlobalMousePosition());
            Vector2 half = BoundsSize / 2.0f;
            if (Mathf.Abs(localPos.X) <= half.X && Mathf.Abs(localPos.Y) <= half.Y)
            {
                EmitSignal(SignalName.SocketClicked, this);
                GetViewport().SetInputAsHandled();
            }
        }
    }

    public override void _Draw()
    {
        if (!Engine.IsEditorHint() && !IsTargetHighlighted) return;

        Color catColor = Category switch
        {
            SocketCategory.BeetlePocket => new Color(1.0f, 0.75f, 0.2f, 0.8f),
            SocketCategory.Spinner => new Color(0.7f, 0.3f, 1.0f, 0.8f),
            SocketCategory.Yakumono => new Color(1.0f, 0.2f, 0.6f, 0.8f),
            _ => Colors.White
        };

        if (IsTargetHighlighted)
        {
            catColor = Colors.Yellow;
        }
        else if (CurrentComponent != null || GetChildCount() > 0)
        {
            catColor.A *= 0.4f;
        }

        Vector2 half = BoundsSize / 2.0f;
        Vector2 topLeft = new(-half.X, -half.Y);
        Vector2 topRight = new(half.X, -half.Y);
        Vector2 bottomRight = new(half.X, half.Y);
        Vector2 bottomLeft = new(-half.X, half.Y);

        if (IsTargetHighlighted)
        {
            DrawRect(new Rect2(-half, BoundsSize), new Color(1.0f, 0.9f, 0.2f, 0.15f), filled: true);
            DrawRect(new Rect2(-half, BoundsSize), Colors.Yellow, filled: false, width: 3.0f);
        }
        else
        {
            DrawDashedLine(topLeft, topRight, catColor, 2.0f, 6.0f);
            DrawDashedLine(topRight, bottomRight, catColor, 2.0f, 6.0f);
            DrawDashedLine(bottomRight, bottomLeft, catColor, 2.0f, 6.0f);
            DrawDashedLine(bottomLeft, topLeft, catColor, 2.0f, 6.0f);
        }

        string label = string.IsNullOrEmpty(SocketId) ? Category.ToString() : $"{Category}\n({SocketId})";
        DrawString(
            ThemeDB.FallbackFont,
            new Vector2(-BoundsSize.X / 2.0f + 4, 0),
            label,
            HorizontalAlignment.Center,
            BoundsSize.X - 8,
            12,
            catColor
        );
    }
}
