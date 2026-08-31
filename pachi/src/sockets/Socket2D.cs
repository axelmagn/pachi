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

        AddToGroup("sockets");
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

    public bool MountFromCard(PackageDealCard card, Hopper? hopper = null)
    {
        return MountPackageDeal(card, hopper);
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
                if (hopper != null)
                {
                    outgoingComp.FlushActiveBalls(variant =>
                    {
                        if (variant != null)
                        {
                            hopper.AddQueuedBalls(new[] { variant });
                        }
                    });
                }
            }

            outgoing.ProcessMode = ProcessModeEnum.Disabled;
            DisableDescendantColliders(outgoing);
            RemoveChild(outgoing);
            outgoing.QueueFree();
            CurrentComponent = null;
            EmitSignal(SignalName.ComponentUnmounted, this, outgoing);
        }

        Node2D incoming = card.ComponentScene.Instantiate<Node2D>();
        incoming.Position = Vector2.Zero;
        incoming.Rotation = 0.0f;
        AddChild(incoming);
        CurrentComponent = incoming;

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

    private void TriggerMountFeedback()
    {
        if (Engine.IsEditorHint() || !IsInsideTree()) return;
        // Audiovisual latch feedback
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

    public override void _Draw()
    {
        if (!Engine.IsEditorHint()) return;

        Color catColor = Category switch
        {
            SocketCategory.BeetlePocket => new Color(1.0f, 0.75f, 0.2f, 0.8f),
            SocketCategory.Spinner => new Color(0.7f, 0.3f, 1.0f, 0.8f),
            SocketCategory.Yakumono => new Color(1.0f, 0.2f, 0.6f, 0.8f),
            _ => Colors.White
        };

        if (CurrentComponent != null || GetChildCount() > 0)
        {
            catColor.A *= 0.4f;
        }

        Rect2 rect = new(-BoundsSize / 2.0f, BoundsSize);
        DrawRect(rect, catColor, filled: false, width: 2.0f);

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
