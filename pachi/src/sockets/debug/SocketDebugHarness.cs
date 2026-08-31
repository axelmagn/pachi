using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;
using System.Diagnostics;

[GlobalClass]
public partial class SocketDebugHarness : CanvasLayer
{
    [Export]
    public OptionButton? SocketSelector { get; set; }

    [Export]
    public OptionButton? CardSelector { get; set; }

    [Export]
    public Button? HotSwapButton { get; set; }

    [Export]
    public Label? StatusLabel { get; set; }

    [Export]
    public Array<PackageDealCard> AvailableCards { get; set; } = [];

    private readonly List<Socket2D> _sockets = [];

    public override void _Ready()
    {
        if (!OS.IsDebugBuild())
        {
            Hide();
            QueueFree();
            return;
        }

        SocketSelector ??= GetNodeOrNull<OptionButton>("Panel/VBox/SocketSelector");
        CardSelector ??= GetNodeOrNull<OptionButton>("Panel/VBox/CardSelector");
        HotSwapButton ??= GetNodeOrNull<Button>("Panel/VBox/HotSwapButton");
        StatusLabel ??= GetNodeOrNull<Label>("Panel/VBox/StatusLabel");

        Debug.Assert(SocketSelector != null, "SocketSelector must not be null");
        Debug.Assert(CardSelector != null, "CardSelector must not be null");
        Debug.Assert(HotSwapButton != null, "HotSwapButton must not be null");
        Debug.Assert(StatusLabel != null, "StatusLabel must not be null");

        if (AvailableCards.Count == 0)
        {
            LoadDefaultCards();
        }

        PopulateSockets();
        PopulateCards();

        if (HotSwapButton != null) HotSwapButton.Pressed += OnHotSwapPressed;
    }

    private void LoadDefaultCards()
    {
        string[] cardPaths =
        [
            "res://src/cards/starter_pocket_card.tres",
            "res://src/cards/starter_spinner_card.tres",
            "res://src/cards/starter_yakumono_card.tres"
        ];

        foreach (string path in cardPaths)
        {
            if (ResourceLoader.Exists(path))
            {
                var card = ResourceLoader.Load<PackageDealCard>(path);
                if (card != null)
                {
                    AvailableCards.Add(card);
                }
            }
        }
    }

    public void PopulateSockets()
    {
        _sockets.Clear();
        if (SocketSelector != null) SocketSelector.Clear();

        var nodes = GetTree().GetNodesInGroup(Socket2D.GroupSockets);
        int index = 0;
        foreach (Node node in nodes)
        {
            if (node is Socket2D socket)
            {
                _sockets.Add(socket);
                string label = string.IsNullOrEmpty(socket.SocketId) ? socket.Name : $"{socket.SocketId} ({socket.Category})";
                SocketSelector?.AddItem(label, index++);
            }
        }
    }

    public void PopulateCards()
    {
        if (CardSelector != null)
        {
            CardSelector.Clear();
            for (int i = 0; i < AvailableCards.Count; i++)
            {
                var card = AvailableCards[i];
                CardSelector.AddItem($"{card.Title} [{card.Category}]", i);
            }
        }
    }

    private void OnHotSwapPressed()
    {
        if (SocketSelector == null || CardSelector == null) return;
        int socketIdx = SocketSelector.Selected;
        int cardIdx = CardSelector.Selected;

        if (socketIdx < 0 || socketIdx >= _sockets.Count || cardIdx < 0 || cardIdx >= AvailableCards.Count)
        {
            SetStatus("Invalid selection.", Colors.Red);
            return;
        }

        Socket2D socket = _sockets[socketIdx];
        PackageDealCard card = AvailableCards[cardIdx];
        Hopper? hopper = GetTree().GetFirstNodeInGroup(Hopper.GroupHoppers) as Hopper;

        bool success = socket.MountPackageDeal(card, hopper);
        if (success)
        {
            SetStatus($"Mounted '{card.Title}' to '{socket.SocketId}'!", Colors.Green);
        }
        else
        {
            SetStatus($"Failed to mount: category mismatch ({card.Category} vs {socket.Category})", Colors.Salmon);
        }
    }

    private void SetStatus(string message, Color color)
    {
        if (StatusLabel != null)
        {
            StatusLabel.Text = message;
            StatusLabel.Modulate = color;
        }
    }
}
