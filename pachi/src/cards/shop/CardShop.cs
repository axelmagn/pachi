using Godot;
using System;
using System.Collections.Generic;

[GlobalClass]
public partial class CardShop : Node
{
    [Signal]
    public delegate void RowDealtEventHandler(int row);

    [Signal]
    public delegate void CursorMovedEventHandler(int targetRow);

    [Signal]
    public delegate void CardPurchasedEventHandler(PackageDealCard card, int row, int col);

    [Signal]
    public delegate void RowDiscardedEventHandler(int row);

    [Signal]
    public delegate void DeckExhaustedEventHandler();

    public const int RowCount = 3;
    public const int ColCount = 3;

    public List<PackageDealCard> MasterDeck { get; } = new();
    public List<PackageDealCard> DiscardPile { get; } = new();
    private readonly PackageDealCard?[,] _grid = new PackageDealCard?[RowCount, ColCount];

    public int CurrentTargetRow { get; private set; } = 0;
    public bool IsDeckExhausted { get; private set; } = false;

    public void Initialize(IEnumerable<PackageDealCard> initialDeck)
    {
        MasterDeck.Clear();
        DiscardPile.Clear();
        IsDeckExhausted = false;

        for (int r = 0; r < RowCount; r++)
        {
            for (int c = 0; c < ColCount; c++)
            {
                _grid[r, c] = null;
            }
        }

        if (initialDeck != null)
        {
            MasterDeck.AddRange(initialDeck);
        }

        // Deal initial 3 cards into Row 0
        CurrentTargetRow = 0;
        DealRowInternal(0);

        // Next deal will target Row 1
        CurrentTargetRow = 1;
        EmitSignal(SignalName.CursorMoved, CurrentTargetRow);
    }

    public PackageDealCard? GetCard(int row, int col)
    {
        if (row < 0 || row >= RowCount || col < 0 || col >= ColCount)
        {
            return null;
        }
        return _grid[row, col];
    }

    public PackageDealCard?[] GetRowCards(int row)
    {
        var result = new PackageDealCard?[ColCount];
        if (row >= 0 && row < RowCount)
        {
            for (int c = 0; c < ColCount; c++)
            {
                result[c] = _grid[row, c];
            }
        }
        return result;
    }

    public bool DealNextRow()
    {
        if (MasterDeck.Count == 0)
        {
            IsDeckExhausted = true;
            EmitSignal(SignalName.DeckExhausted);
            return false;
        }

        int targetRow = CurrentTargetRow;
        DealRowInternal(targetRow);

        CurrentTargetRow = (CurrentTargetRow + 1) % RowCount;
        EmitSignal(SignalName.CursorMoved, CurrentTargetRow);
        return true;
    }

    private void DealRowInternal(int row)
    {
        // 1. Move old unpurchased cards in this row to discard pile
        for (int c = 0; c < ColCount; c++)
        {
            PackageDealCard? oldCard = _grid[row, c];
            if (oldCard != null)
            {
                DiscardPile.Add(oldCard);
                _grid[row, c] = null;
            }
        }

        // 2. Draw up to ColCount cards from MasterDeck
        int countToDraw = Math.Min(ColCount, MasterDeck.Count);
        for (int c = 0; c < countToDraw; c++)
        {
            PackageDealCard card = MasterDeck[0];
            MasterDeck.RemoveAt(0);
            _grid[row, c] = card;
        }

        EmitSignal(SignalName.RowDealt, row);
    }

    public PackageDealCard? PurchaseCard(int row, int col, Hopper? hopper = null)
    {
        if (row < 0 || row >= RowCount || col < 0 || col >= ColCount)
        {
            return null;
        }

        PackageDealCard? card = _grid[row, col];
        if (card == null)
        {
            return null;
        }

        if (hopper != null)
        {
            if (!hopper.HasBallCost(card.BallCostTier, card.BallCostCount))
            {
                return null;
            }
            if (!hopper.DeductBallCost(card.BallCostTier, card.BallCostCount))
            {
                return null;
            }
        }

        // Remove purchased card from grid
        _grid[row, col] = null;

        // Discard the remaining cards in this row
        for (int c = 0; c < ColCount; c++)
        {
            PackageDealCard? other = _grid[row, c];
            if (other != null)
            {
                DiscardPile.Add(other);
                _grid[row, c] = null;
            }
        }

        EmitSignal(SignalName.CardPurchased, card, row, col);
        EmitSignal(SignalName.RowDiscarded, row);
        return card;
    }
}
