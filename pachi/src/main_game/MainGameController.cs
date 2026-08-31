using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;
using System.Diagnostics;

[GlobalClass]
public partial class MainGameController : Node
{
    [Export]
    public CardShopUI? ShopUI { get; set; }

    [Export]
    public CardShop? Shop { get; set; }

    [Export]
    public DealMeter? Meter { get; set; }

    [Export]
    public Hopper? Hopper { get; set; }

    [Export]
    public Level? Level { get; set; }

    [Export]
    public Array<PackageDealCard>? MasterDeckCards { get; set; }

    private int _selectedRow = -1;
    private int _selectedCol = -1;
    private PackageDealCard? _selectedCard;
    private readonly List<Socket2D> _sockets = new();

    public override void _Ready()
    {
        if (Shop == null)
        {
            Shop = new CardShop { Name = "CardShop" };
            AddChild(Shop);
        }

        if (Meter == null)
        {
            Meter = new DealMeter { Name = "DealMeter" };
            AddChild(Meter);
        }

        // Connect DealMeter to CardShop
        Meter.DealThresholdReached += OnDealThresholdReached;

        // Build Master Deck
        var deck = new List<PackageDealCard>();
        if (MasterDeckCards != null && MasterDeckCards.Count > 0)
        {
            deck.AddRange(MasterDeckCards);
        }
        else
        {
            deck.AddRange(CreateDefaultMasterDeck());
        }

        Shop.Initialize(deck);

        if (Hopper != null && ShopUI != null)
        {
            ShopUI.Bind(Shop, Meter, Hopper);
            ShopUI.CardSlotSelected += OnCardSlotSelected;
            ShopUI.SelectionCancelled += OnSelectionCancelled;
        }

        // Discover and connect sockets
        DiscoverSockets();
    }

    public override void _ExitTree()
    {
        if (Meter != null)
        {
            Meter.DealThresholdReached -= OnDealThresholdReached;
        }

        if (ShopUI != null)
        {
            ShopUI.CardSlotSelected -= OnCardSlotSelected;
            ShopUI.SelectionCancelled -= OnSelectionCancelled;
        }

        foreach (Socket2D socket in _sockets)
        {
            socket.SocketClicked -= OnSocketClicked;
        }
        _sockets.Clear();
    }

    public override void _UnhandledInput(InputEvent @event)
    {
        if (_selectedCard != null && @event.IsActionPressed("ui_cancel"))
        {
            ShopUI?.ClearSelection();
            OnSelectionCancelled();
            GetViewport()?.SetInputAsHandled();
        }
    }

    private void DiscoverSockets()
    {
        if (Level == null) return;

        foreach (Node node in Level.GetTree().GetNodesInGroup(Socket2D.GroupSockets))
        {
            if (node is Socket2D socket && !_sockets.Contains(socket))
            {
                _sockets.Add(socket);
                socket.SocketClicked += OnSocketClicked;
            }
        }
    }

    private void OnDealThresholdReached()
    {
        Shop?.DealNextRow();
    }

    private void OnCardSlotSelected(int row, int col, PackageDealCard card)
    {
        _selectedRow = row;
        _selectedCol = col;
        _selectedCard = card;

        // Refresh socket discovery if needed
        if (_sockets.Count == 0)
        {
            DiscoverSockets();
        }

        // Highlight matching sockets
        foreach (Socket2D socket in _sockets)
        {
            socket.SetTargetHighlighted(socket.Category == card.Category);
        }
    }

    private void OnSelectionCancelled()
    {
        _selectedRow = -1;
        _selectedCol = -1;
        _selectedCard = null;

        foreach (Socket2D socket in _sockets)
        {
            socket.SetTargetHighlighted(false);
        }
    }

    private void OnSocketClicked(Socket2D socket)
    {
        if (_selectedCard == null || Shop == null || socket.Category != _selectedCard.Category)
        {
            return;
        }

        PackageDealCard? bought = Shop.PurchaseCard(_selectedRow, _selectedCol, Hopper);
        if (bought != null)
        {
            socket.MountPackageDeal(bought, Hopper);
        }

        ShopUI?.ClearSelection();
        OnSelectionCancelled();
    }

    private static List<PackageDealCard> CreateDefaultMasterDeck()
    {
        var list = new List<PackageDealCard>();
        var pocketCard = ResourceLoader.Load<PackageDealCard>("res://src/cards/starter_pocket_card.tres");
        var spinnerCard = ResourceLoader.Load<PackageDealCard>("res://src/cards/starter_spinner_card.tres");
        var yakumonoCard = ResourceLoader.Load<PackageDealCard>("res://src/cards/starter_yakumono_card.tres");

        if (pocketCard != null)
        {
            for (int i = 0; i < 6; i++) list.Add(pocketCard);
        }
        if (spinnerCard != null)
        {
            for (int i = 0; i < 4; i++) list.Add(spinnerCard);
        }
        if (yakumonoCard != null)
        {
            for (int i = 0; i < 2; i++) list.Add(yakumonoCard);
        }

        return list;
    }
}
