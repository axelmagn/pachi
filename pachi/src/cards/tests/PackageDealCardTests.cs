using Godot;
using Godot.Collections;
using System;
using static TestAssert;

public static class PackageDealCardTests
{
    public static void RunAllTests()
    {
        TestPackageDealCardProperties();
        TestBallCostClamping();
        TestStarterCardResources();
        TestBallAwardIndicatorPresentation();
    }

    public static void TestPackageDealCardProperties()
    {
        var card = new PackageDealCard
        {
            CardId = "test_card",
            Title = "Test Card",
            Description = "Test description",
            Category = SocketCategory.Spinner,
            BallCostCount = 3,
            BallCostTier = 2,
            AccentColor = Colors.Green,
            DraftWeight = 75
        };

        Assert(card.CardId == "test_card", "CardId should match.");
        Assert(card.Title == "Test Card", "Title should match.");
        Assert(card.Description == "Test description", "Description should match.");
        Assert(card.Category == SocketCategory.Spinner, "Category should match.");
        Assert(card.BallCostCount == 3, "BallCostCount should be 3.");
        Assert(card.BallCostTier == 2, "BallCostTier should be 2.");
        Assert(card.AccentColor == Colors.Green, "AccentColor should match.");
        Assert(card.DraftWeight == 75, "DraftWeight should match.");
    }

    public static void TestBallCostClamping()
    {
        var card = new PackageDealCard();

        card.BallCostCount = 0;
        Assert(card.BallCostCount == 1, $"BallCostCount below 1 should clamp to 1, got {card.BallCostCount}.");

        card.BallCostCount = 5;
        Assert(card.BallCostCount == 4, $"BallCostCount above 4 should clamp to 4, got {card.BallCostCount}.");

        card.BallCostTier = -1;
        Assert(card.BallCostTier == 1, $"BallCostTier below 1 should clamp to 1, got {card.BallCostTier}.");

        card.BallCostTier = 10;
        Assert(card.BallCostTier == 4, $"BallCostTier above 4 should clamp to 4, got {card.BallCostTier}.");
    }

    public static void TestStarterCardResources()
    {
        var pocketCard = ResourceLoader.Load<PackageDealCard>("res://src/cards/starter_pocket_card.tres");
        Assert(pocketCard != null, "starter_pocket_card.tres should load.");
        Assert(pocketCard!.Category == SocketCategory.BeetlePocket, "Pocket card category should be BeetlePocket.");
        Assert(pocketCard.ComponentScene != null, "Pocket card ComponentScene should not be null.");
        Assert(pocketCard.BallCostCount >= 1 && pocketCard.BallCostCount <= 4, "BallCostCount must be between 1 and 4.");
        Assert(pocketCard.BallCostTier >= 1 && pocketCard.BallCostTier <= 4, "BallCostTier must be between 1 and 4.");

        var spinnerCard = ResourceLoader.Load<PackageDealCard>("res://src/cards/starter_spinner_card.tres");
        Assert(spinnerCard != null, "starter_spinner_card.tres should load.");
        Assert(spinnerCard!.Category == SocketCategory.Spinner, "Spinner card category should be Spinner.");
        Assert(spinnerCard.ComponentScene != null, "Spinner card ComponentScene should not be null.");

        var yakumonoCard = ResourceLoader.Load<PackageDealCard>("res://src/cards/starter_yakumono_card.tres");
        Assert(yakumonoCard != null, "starter_yakumono_card.tres should load.");
        Assert(yakumonoCard!.Category == SocketCategory.Yakumono, "Yakumono card category should be Yakumono.");
        Assert(yakumonoCard.ComponentScene != null, "Yakumono card ComponentScene should not be null.");
    }

    public static void TestBallAwardIndicatorPresentation()
    {
        var indicator = new BallAwardIndicator { MaxColumns = 6 };
        var variant = new BallVariant { PlaceholderColor = Colors.Cyan };

        // 1 ball: 1 row of 1 col -> 10x10 px (1*8 + 2)
        indicator.Balls = new Array<BallVariant> { variant };
        Assert(indicator.Size == new Vector2(10, 10), $"1 ball should be (10, 10), got {indicator.Size}");

        // 3 balls: 1 row of 3 cols -> 26x10 px (3*8 + 2)
        indicator.Balls = new Array<BallVariant> { variant, variant, variant };
        Assert(indicator.Size == new Vector2(26, 10), $"3 balls should be (26, 10), got {indicator.Size}");

        // 6 balls: 1 row of 6 cols -> 50x10 px (6*8 + 2)
        indicator.Balls = new Array<BallVariant> { variant, variant, variant, variant, variant, variant };
        Assert(indicator.Size == new Vector2(50, 10), $"6 balls should be (50, 10), got {indicator.Size}");

        // 10 balls: 2 rows of up to 6 cols -> 50x18 px (2*8 + 2)
        indicator.Balls = new Array<BallVariant> { variant, variant, variant, variant, variant, variant, variant, variant, variant, variant };
        Assert(indicator.Size == new Vector2(50, 18), $"10 balls should be (50, 18), got {indicator.Size}");
    }
}
