using Godot;
using System.Collections.Generic;
using static TestAssert;

public static class CardShopUITests
{
    public static void RunAllTests()
    {
        TestCardSlotUIEmptyAndPopulated();
        TestCardSlotAffordability();
        TestCardShopUISynchronization();
    }

    private static PackageDealCard CreateMockCard(string id, string title, SocketCategory cat, int costTier, int costCount)
    {
        return new PackageDealCard
        {
            CardId = id,
            Title = title,
            Category = cat,
            BallCostTier = costTier,
            BallCostCount = costCount,
            Description = "Mock card description"
        };
    }

    public static void TestCardSlotUIEmptyAndPopulated()
    {
        var slot = new CardSlotUI();
        slot.InitControls();

        // Empty state
        slot.SetCard(null, 0, 0);
        Assert(slot.Card == null, "Slot card should be null.");
        Assert(slot.VisibleCardContainer != null && !slot.VisibleCardContainer.Visible, "Card container should be hidden when empty.");
        Assert(slot.EmptyLabel != null && slot.EmptyLabel.Visible, "Empty label should be visible when slot is empty.");

        // Populated state
        var card = CreateMockCard("test_pocket", "Super Pocket", SocketCategory.BeetlePocket, 1, 2);
        slot.SetCard(card, 0, 0);
        Assert(slot.Card == card, "Slot card should match.");
        Assert(slot.VisibleCardContainer != null && slot.VisibleCardContainer.Visible, "Card container should be visible when populated.");
        Assert(slot.EmptyLabel != null && !slot.EmptyLabel.Visible, "Empty label should be hidden when populated.");
        Assert(slot.TitleLabel != null && slot.TitleLabel.Text == "Super Pocket", $"Title should match 'Super Pocket', got {slot.TitleLabel?.Text}.");

        slot.QueueFree();
    }

    public static void TestCardSlotAffordability()
    {
        var slot = new CardSlotUI();
        slot.InitControls();

        var card = CreateMockCard("test_pocket", "Super Pocket", SocketCategory.BeetlePocket, 2, 3);
        slot.SetCard(card, 1, 2);

        slot.UpdateAffordability(true);
        Assert(slot.IsAffordable, "Slot should be marked affordable.");
        Assert(slot.SelectButton != null && !slot.SelectButton.Disabled, "Select button should be enabled when affordable.");

        slot.UpdateAffordability(false);
        Assert(!slot.IsAffordable, "Slot should be marked not affordable.");
        Assert(slot.SelectButton != null && slot.SelectButton.Disabled, "Select button should be disabled when not affordable.");

        slot.QueueFree();
    }

    public static void TestCardShopUISynchronization()
    {
        var shopUI = new CardShopUI();
        shopUI.InitControls();

        var shop = new CardShop();
        var meter = new DealMeter();
        var hopper = new Hopper();

        var deck = new List<PackageDealCard>();
        for (int i = 0; i < 9; i++)
        {
            deck.Add(CreateMockCard($"card_{i}", $"Card {i}", SocketCategory.BeetlePocket, 1, 1));
        }

        shop.Initialize(deck);
        shopUI.Bind(shop, meter, hopper);

        // Header tests
        Assert(shopUI.DeckCountLabel != null && shopUI.DeckCountLabel.Text.Contains('6'), $"Deck count label should show 6 remaining, got {shopUI.DeckCountLabel?.Text}.");
        Assert(shopUI.ActiveTargetRow == 1, $"Active target row should be 1, got {shopUI.ActiveTargetRow}.");

        // Row 0 slots should be populated
        CardSlotUI slot00 = shopUI.GetSlot(0, 0);
        Assert(slot00.Card != null && slot00.Card.CardId == "card_0", "Slot [0,0] should have card_0.");

        // Deal next row
        shop.DealNextRow();
        Assert(shopUI.ActiveTargetRow == 2, $"Active target row should now be 2, got {shopUI.ActiveTargetRow}.");
        Assert(shopUI.DeckCountLabel != null && shopUI.DeckCountLabel.Text.Contains('3'), $"Deck count label should show 3 remaining, got {shopUI.DeckCountLabel?.Text}.");

        CardSlotUI slot10 = shopUI.GetSlot(1, 0);
        Assert(slot10.Card != null && slot10.Card.CardId == "card_3", "Slot [1,0] should have card_3.");

        shopUI.QueueFree();
        shop.QueueFree();
        meter.QueueFree();
        hopper.QueueFree();
    }
}
