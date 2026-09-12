using Godot;
using Godot.Collections;
using System.Collections.Generic;
using System.Diagnostics;

[GlobalClass]
public partial class CardSidebar : Control
{
    public static CardSidebar? Instance { get; private set; }

    [Export]
    public PackedScene? CardUIScene { get; set; }

    private Container? _cardsContainer;
    private readonly List<CardData> _activeCards = new();
    private const int MaxCards = 8;
    private const int InitialCardCount = 4;

    public override void _Ready()
    {
        Instance = this;
        _cardsContainer = GetNodeOrNull<Container>("%CardsContainer");

        Debug.Assert(GlobalEvents.Instance != null, "GlobalEvents.Instance must not be null");
        GlobalEvents.Instance.BallEnteredPocket += OnBallEnteredPocket;

        CallDeferred(nameof(InitInitialCards));
    }

    public override void _ExitTree()
    {
        if (Instance == this)
        {
            Instance = null;
        }

        if (GlobalEvents.Instance != null)
        {
            GlobalEvents.Instance.BallEnteredPocket -= OnBallEnteredPocket;
        }
    }

    private void InitInitialCards()
    {
        _activeCards.Clear();
        for (int i = 0; i < InitialCardCount; i++)
        {
            AddPlayableCard();
        }
        PopulateCards();
    }

    private void OnBallEnteredPocket(Node pocketNode, Node ballNode)
    {
        AddPlayableCard();
        PopulateCards();
    }

    public void AddPlayableCard()
    {
        var activePockets = GetActivePockets();
        CardData? newCard = CardGenerator.GeneratePlayableCard(activePockets);
        if (newCard == null) return;

        if (_activeCards.Count >= MaxCards)
        {
            // FIFO queue rule: remove oldest card (bottom of list)
            _activeCards.RemoveAt(_activeCards.Count - 1);
        }

        // Insert new card at top of list
        _activeCards.Insert(0, newCard);
    }

    public void RemoveCard(CardData card)
    {
        if (card != null && _activeCards.Remove(card))
        {
            PopulateCards();
        }
    }

    private List<Pocket> GetActivePockets()
    {
        var pockets = new List<Pocket>();
        var nodes = GetTree().GetNodesInGroup(Pocket.GroupPockets);
        foreach (Node node in nodes)
        {
            if (node is Pocket pocket && pocket.IsInsideTree())
            {
                pockets.Add(pocket);
            }
        }
        return pockets;
    }

    private void PopulateCards()
    {
        if (_cardsContainer == null || CardUIScene == null) return;

        foreach (Node child in _cardsContainer.GetChildren())
        {
            child.QueueFree();
        }

        foreach (CardData card in _activeCards)
        {
            if (CardUIScene.Instantiate() is CardUI cardUI)
            {
                _cardsContainer.AddChild(cardUI);
                cardUI.CardData = card;
            }
        }
    }
}
