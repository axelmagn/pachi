using Godot;
using System.Collections.Generic;
using static TestAssert;

public static class CardShopIntegrationTests
{
    public static void RunAllTests()
    {
        TestCardShopDealMeterTrigger();
        TestPurchaseAndSocketMountFlow();
    }

    private static PackageDealCard CreateTestCard(string id, SocketCategory cat, int costTier, int costCount, PackedScene scene)
    {
        return new PackageDealCard
        {
            CardId = id,
            Title = id,
            Category = cat,
            BallCostTier = costTier,
            BallCostCount = costCount,
            ComponentScene = scene
        };
    }

    public static void TestCardShopDealMeterTrigger()
    {
        var meter = new DealMeter { BaselinePeriod = 20.0f };
        var shop = new CardShop();
        var deck = new List<PackageDealCard>();
        var pocketScene = GD.Load<PackedScene>("res://src/pockets/starter_pocket_center.tscn");

        for (int i = 0; i < 9; i++)
        {
            deck.Add(CreateTestCard($"card_{i}", SocketCategory.BeetlePocket, 1, 1, pocketScene));
        }

        shop.Initialize(deck);
        meter.DealThresholdReached += () => shop.DealNextRow();

        Assert(shop.CurrentTargetRow == 1, "Target row should initially be 1.");

        // Advance meter past 100% -> should trigger DealNextRow
        meter.Advance(21.0);

        Assert(shop.CurrentTargetRow == 2, $"Target row should advance to 2 after meter trigger, got {shop.CurrentTargetRow}.");
        Assert(shop.GetCard(1, 0)?.CardId == "card_3", "Row 1 should now be populated with card_3.");

        meter.QueueFree();
        shop.QueueFree();
    }

    public static void TestPurchaseAndSocketMountFlow()
    {
        var shop = new CardShop();
        var hopper = new Hopper();
        var ballsRoot = new Node2D();
        hopper.AddChild(ballsRoot);
        hopper.BallsRoot = ballsRoot;

        // Give hopper 3x Tier-1 balls
        for (int i = 0; i < 3; i++)
        {
            var ball = new Ball { Variant = new BallVariant { Tier = 1, PlaceholderColor = Colors.White } };
            ballsRoot.AddChild(ball);
        }

        var pocketScene = GD.Load<PackedScene>("res://src/pockets/starter_pocket_center.tscn");
        var card = CreateTestCard("pocket_upgrade", SocketCategory.BeetlePocket, 1, 2, pocketScene);

        var deck = new List<PackageDealCard> { card, CreateTestCard("c2", SocketCategory.BeetlePocket, 1, 1, pocketScene), CreateTestCard("c3", SocketCategory.BeetlePocket, 1, 1, pocketScene) };
        shop.Initialize(deck);

        // Setup socket with starter pocket
        var socket = new Socket2D { Category = SocketCategory.BeetlePocket };
        var starterPocket = pocketScene.Instantiate<Node2D>();
        socket.AddChild(starterPocket);
        socket.AdoptChildComponent();

        Assert(socket.CurrentComponent == starterPocket, "Socket should have adopted starter pocket.");
        Assert(hopper.GetTierCount(1) == 3, "Hopper should have 3 Tier-1 balls.");

        // Purchase card at [0, 0] (cost: 2 Tier-1)
        PackageDealCard? bought = shop.PurchaseCard(0, 0, hopper);
        Assert(bought != null, "Purchase should succeed.");
        Assert(hopper.GetTierCount(1) == 1, $"Hopper should have 1 Tier-1 ball left after spending 2, got {hopper.GetTierCount(1)}.");

        // Mount to socket
        bool mounted = socket.MountPackageDeal(bought!, hopper);
        Assert(mounted, "MountPackageDeal should succeed.");
        Assert(socket.CurrentComponent != starterPocket, "Socket should have new component.");
        Assert(shop.GetCard(0, 0) == null, "Row 0 Col 0 should be null.");
        Assert(shop.GetCard(0, 1) == null, "Row 0 Col 1 should be discarded.");

        socket.QueueFree();
        hopper.QueueFree();
        shop.QueueFree();
    }
}
