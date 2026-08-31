using Godot;
using System.Collections.Generic;
using static TestAssert;

public static class CardShopTests
{
    public static void RunAllTests()
    {
        TestInitialDealAndCursor();
        TestDealNextRowCyclesCursor();
        TestDealOverwriteDiscardsOldCards();
        TestPurchaseDiscardsRow();
        TestDeckExhaustion();
    }

    private static PackageDealCard CreateMockCard(string id, int costTier = 1, int costCount = 1)
    {
        return new PackageDealCard
        {
            CardId = id,
            Title = id,
            Category = SocketCategory.BeetlePocket,
            BallCostTier = costTier,
            BallCostCount = costCount
        };
    }

    public static void TestInitialDealAndCursor()
    {
        var shop = new CardShop();
        var deck = new List<PackageDealCard>();
        for (int i = 0; i < 9; i++)
        {
            deck.Add(CreateMockCard($"card_{i}"));
        }

        shop.Initialize(deck);

        Assert(shop.CurrentTargetRow == 1, $"Initial cursor should point to Row 1, got {shop.CurrentTargetRow}.");
        Assert(shop.MasterDeck.Count == 6, $"Remaining deck count should be 6, got {shop.MasterDeck.Count}.");
        Assert(shop.DiscardPile.Count == 0, "Discard pile should be empty.");

        // Row 0 should have 3 cards
        Assert(shop.GetCard(0, 0)?.CardId == "card_0", "Row 0 Col 0 should be card_0.");
        Assert(shop.GetCard(0, 1)?.CardId == "card_1", "Row 0 Col 1 should be card_1.");
        Assert(shop.GetCard(0, 2)?.CardId == "card_2", "Row 0 Col 2 should be card_2.");

        // Row 1 and Row 2 should be empty
        Assert(shop.GetCard(1, 0) == null, "Row 1 Col 0 should be null.");
        Assert(shop.GetCard(2, 0) == null, "Row 2 Col 0 should be null.");
    }

    public static void TestDealNextRowCyclesCursor()
    {
        var shop = new CardShop();
        var deck = new List<PackageDealCard>();
        for (int i = 0; i < 9; i++)
        {
            deck.Add(CreateMockCard($"card_{i}"));
        }

        shop.Initialize(deck);

        // Deal into Row 1
        bool dealt = shop.DealNextRow();
        Assert(dealt, "DealNextRow should succeed.");
        Assert(shop.CurrentTargetRow == 2, $"Cursor should advance to Row 2, got {shop.CurrentTargetRow}.");
        Assert(shop.GetCard(1, 0)?.CardId == "card_3", "Row 1 Col 0 should be card_3.");
        Assert(shop.GetCard(1, 1)?.CardId == "card_4", "Row 1 Col 1 should be card_4.");
        Assert(shop.GetCard(1, 2)?.CardId == "card_5", "Row 1 Col 2 should be card_5.");
        Assert(shop.MasterDeck.Count == 3, $"Remaining deck count should be 3, got {shop.MasterDeck.Count}.");

        // Deal into Row 2
        dealt = shop.DealNextRow();
        Assert(dealt, "DealNextRow should succeed.");
        Assert(shop.CurrentTargetRow == 0, $"Cursor should wrap to Row 0, got {shop.CurrentTargetRow}.");
        Assert(shop.GetCard(2, 0)?.CardId == "card_6", "Row 2 Col 0 should be card_6.");
        Assert(shop.GetCard(2, 1)?.CardId == "card_7", "Row 2 Col 1 should be card_7.");
        Assert(shop.GetCard(2, 2)?.CardId == "card_8", "Row 2 Col 2 should be card_8.");
        Assert(shop.MasterDeck.Count == 0, $"Remaining deck count should be 0, got {shop.MasterDeck.Count}.");
    }

    public static void TestDealOverwriteDiscardsOldCards()
    {
        var shop = new CardShop();
        var deck = new List<PackageDealCard>();
        for (int i = 0; i < 12; i++)
        {
            deck.Add(CreateMockCard($"card_{i}"));
        }

        shop.Initialize(deck); // deals card_0..2 into Row 0, cursor = 1
        shop.DealNextRow();    // deals card_3..5 into Row 1, cursor = 2
        shop.DealNextRow();    // deals card_6..8 into Row 2, cursor = 0

        // Now cursor points to Row 0. DealNextRow should discard card_0, card_1, card_2 and place card_9..11 in Row 0
        shop.DealNextRow();
        Assert(shop.DiscardPile.Count == 3, $"Discard pile should have 3 old cards from Row 0, got {shop.DiscardPile.Count}.");
        Assert(shop.GetCard(0, 0)?.CardId == "card_9", "Row 0 Col 0 should now be card_9.");
        Assert(shop.GetCard(0, 1)?.CardId == "card_10", "Row 0 Col 1 should now be card_10.");
        Assert(shop.GetCard(0, 2)?.CardId == "card_11", "Row 0 Col 2 should now be card_11.");
        Assert(shop.CurrentTargetRow == 1, $"Cursor should advance to Row 1, got {shop.CurrentTargetRow}.");
    }

    public static void TestPurchaseDiscardsRow()
    {
        var shop = new CardShop();
        var deck = new List<PackageDealCard>();
        for (int i = 0; i < 6; i++)
        {
            deck.Add(CreateMockCard($"card_{i}"));
        }

        shop.Initialize(deck); // Row 0 has card_0, card_1, card_2

        PackageDealCard? purchased = shop.PurchaseCard(0, 1);
        Assert(purchased != null, "Purchased card should not be null.");
        Assert(purchased!.CardId == "card_1", $"Purchased card should be card_1, got {purchased.CardId}.");

        // The whole Row 0 should now be empty (purchased card was bought, remaining 2 were discarded)
        Assert(shop.GetCard(0, 0) == null, "Row 0 Col 0 should be null after row discard.");
        Assert(shop.GetCard(0, 1) == null, "Row 0 Col 1 should be null after purchase.");
        Assert(shop.GetCard(0, 2) == null, "Row 0 Col 2 should be null after row discard.");
        Assert(shop.DiscardPile.Count == 2, $"Discard pile should have 2 unpurchased cards from Row 0, got {shop.DiscardPile.Count}.");
    }

    public static void TestDeckExhaustion()
    {
        var shop = new CardShop();
        var deck = new List<PackageDealCard>
        {
            CreateMockCard("card_0"),
            CreateMockCard("card_1"),
            CreateMockCard("card_2")
        };

        shop.Initialize(deck); // Deals 3 cards into Row 0, deck is now 0

        bool exhaustedTriggered = false;
        shop.DeckExhausted += () =>
        {
            exhaustedTriggered = true;
        };

        bool dealt = shop.DealNextRow();
        Assert(!dealt, "DealNextRow on empty deck should return false.");
        Assert(exhaustedTriggered, "DeckExhausted signal should fire.");
        Assert(shop.IsDeckExhausted, "IsDeckExhausted should be true.");
    }
}
