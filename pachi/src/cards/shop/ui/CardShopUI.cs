using Godot;
using System;
using System.Collections.Generic;

[GlobalClass]
public partial class CardShopUI : VBoxContainer
{
    [Signal]
    public delegate void CardSlotSelectedEventHandler(int row, int col, PackageDealCard card);

    [Signal]
    public delegate void SelectionCancelledEventHandler();

    [Export]
    public Label? TitleLabel { get; set; }

    [Export]
    public ProgressBar? DealProgressBar { get; set; }

    [Export]
    public Label? DealMeterLabel { get; set; }

    [Export]
    public Label? DeckCountLabel { get; set; }

    [Export]
    public Label? StatusLabel { get; set; }

    public int ActiveTargetRow { get; private set; } = 0;

    private readonly CardSlotUI[,] _slots = new CardSlotUI[CardShop.RowCount, CardShop.ColCount];
    private readonly Label[] _rowHeaderLabels = new Label[CardShop.RowCount];
    private readonly PanelContainer[] _rowPanels = new PanelContainer[CardShop.RowCount];

    private CardShop? _shop;
    private DealMeter? _meter;
    private Hopper? _hopper;

    private int _selectedRow = -1;
    private int _selectedCol = -1;

    public override void _Ready()
    {
        InitControls();
    }

    public void InitControls()
    {
        if (TitleLabel != null && DealProgressBar != null && DeckCountLabel != null)
        {
            return;
        }

        CustomMinimumSize = new Vector2(284, 508);
        SizeFlagsHorizontal = SizeFlags.ExpandFill;
        SizeFlagsVertical = SizeFlags.ExpandFill;

        // Clear children if re-initializing
        foreach (Node child in GetChildren())
        {
            child.QueueFree();
        }

        // Header
        var headerVBox = new VBoxContainer { Name = "HeaderVBox" };
        AddChild(headerVBox);

        var topRow = new HBoxContainer { Name = "TopRow" };
        headerVBox.AddChild(topRow);

        TitleLabel = new Label
        {
            Name = "TitleLabel",
            Text = "CARD SHOP",
            SizeFlagsHorizontal = SizeFlags.ExpandFill
        };
        TitleLabel.AddThemeFontSizeOverride("font_size", 14);
        topRow.AddChild(TitleLabel);

        DeckCountLabel = new Label
        {
            Name = "DeckCountLabel",
            Text = "Deck: 0",
            HorizontalAlignment = HorizontalAlignment.Right
        };
        DeckCountLabel.AddThemeFontSizeOverride("font_size", 11);
        DeckCountLabel.Modulate = new Color(0.8f, 0.8f, 0.8f);
        topRow.AddChild(DeckCountLabel);

        DealProgressBar = new ProgressBar
        {
            Name = "DealProgressBar",
            MinValue = 0,
            MaxValue = 100,
            Value = 0,
            ShowPercentage = false,
            CustomMinimumSize = new Vector2(0, 10)
        };
        headerVBox.AddChild(DealProgressBar);

        var meterInfoRow = new HBoxContainer { Name = "MeterInfoRow" };
        headerVBox.AddChild(meterInfoRow);

        DealMeterLabel = new Label
        {
            Name = "DealMeterLabel",
            Text = "Deal: 0% (1.0x)",
            SizeFlagsHorizontal = SizeFlags.ExpandFill
        };
        DealMeterLabel.AddThemeFontSizeOverride("font_size", 10);
        DealMeterLabel.Modulate = new Color(0.75f, 0.75f, 0.75f);
        meterInfoRow.AddChild(DealMeterLabel);

        StatusLabel = new Label
        {
            Name = "StatusLabel",
            Text = "",
            HorizontalAlignment = HorizontalAlignment.Right
        };
        StatusLabel.AddThemeFontSizeOverride("font_size", 10);
        StatusLabel.Modulate = new Color(0.9f, 0.8f, 0.4f);
        meterInfoRow.AddChild(StatusLabel);

        // 3 Rows
        var rowsContainer = new VBoxContainer
        {
            Name = "RowsContainer",
            SizeFlagsVertical = SizeFlags.ExpandFill
        };
        AddChild(rowsContainer);

        for (int r = 0; r < CardShop.RowCount; r++)
        {
            int rowIndex = r;
            var rowPanel = new PanelContainer
            {
                Name = $"RowPanel_{r}",
                SizeFlagsVertical = SizeFlags.ExpandFill
            };
            _rowPanels[r] = rowPanel;
            rowsContainer.AddChild(rowPanel);

            var rowVBox = new VBoxContainer();
            rowPanel.AddChild(rowVBox);

            var rowHeader = new Label
            {
                Name = $"RowHeader_{r}",
                Text = $"ROW {r + 1}"
            };
            rowHeader.AddThemeFontSizeOverride("font_size", 9);
            rowHeader.Modulate = new Color(0.6f, 0.6f, 0.6f);
            rowVBox.AddChild(rowHeader);
            _rowHeaderLabels[r] = rowHeader;

            var slotsHBox = new HBoxContainer
            {
                Name = $"SlotsHBox_{r}",
                SizeFlagsVertical = SizeFlags.ExpandFill
            };
            rowVBox.AddChild(slotsHBox);

            for (int c = 0; c < CardShop.ColCount; c++)
            {
                int colIndex = c;
                var slot = new CardSlotUI { Name = $"Slot_{r}_{c}" };
                slot.InitControls();
                slot.SlotPressed += (row, col) => OnSlotPressed(row, col);
                _slots[r, c] = slot;
                slotsHBox.AddChild(slot);
            }
        }

        UpdateCursorHighlight(0);
    }

    public void Bind(CardShop shop, DealMeter meter, Hopper hopper)
    {
        InitControls();

        if (_shop != null)
        {
            _shop.RowDealt -= OnShopRowDealt;
            _shop.CursorMoved -= OnShopCursorMoved;
            _shop.CardPurchased -= OnShopCardPurchased;
            _shop.RowDiscarded -= OnShopRowDiscarded;
            _shop.DeckExhausted -= OnShopDeckExhausted;
        }

        if (_meter != null)
        {
            _meter.ProgressChanged -= OnMeterProgressChanged;
        }

        if (_hopper != null)
        {
            _hopper.InventoryChanged -= OnHopperInventoryChanged;
        }

        _shop = shop;
        _meter = meter;
        _hopper = hopper;

        _shop.RowDealt += OnShopRowDealt;
        _shop.CursorMoved += OnShopCursorMoved;
        _shop.CardPurchased += OnShopCardPurchased;
        _shop.RowDiscarded += OnShopRowDiscarded;
        _shop.DeckExhausted += OnShopDeckExhausted;

        _meter.ProgressChanged += OnMeterProgressChanged;
        _hopper.InventoryChanged += OnHopperInventoryChanged;

        ActiveTargetRow = _shop.CurrentTargetRow;
        UpdateCursorHighlight(ActiveTargetRow);
        RefreshAllSlots();
        UpdateHeaderInfo();
    }

    public override void _ExitTree()
    {
        if (_shop != null)
        {
            _shop.RowDealt -= OnShopRowDealt;
            _shop.CursorMoved -= OnShopCursorMoved;
            _shop.CardPurchased -= OnShopCardPurchased;
            _shop.RowDiscarded -= OnShopRowDiscarded;
            _shop.DeckExhausted -= OnShopDeckExhausted;
        }

        if (_meter != null)
        {
            _meter.ProgressChanged -= OnMeterProgressChanged;
        }

        if (_hopper != null)
        {
            _hopper.InventoryChanged -= OnHopperInventoryChanged;
        }
    }

    public CardSlotUI GetSlot(int row, int col)
    {
        InitControls();
        return _slots[row, col];
    }

    public void ClearSelection()
    {
        if (_selectedRow >= 0 && _selectedCol >= 0)
        {
            _slots[_selectedRow, _selectedCol].SetSelected(false);
        }
        _selectedRow = -1;
        _selectedCol = -1;
        if (StatusLabel != null) StatusLabel.Text = "";
        EmitSignal(SignalName.SelectionCancelled);
    }

    private void OnSlotPressed(int row, int col)
    {
        if (_selectedRow == row && _selectedCol == col)
        {
            // Deselect
            ClearSelection();
            return;
        }

        ClearSelection();

        PackageDealCard? card = _shop?.GetCard(row, col);
        if (card == null) return;

        bool canAfford = _hopper == null || _hopper.HasBallCost(card.BallCostTier, card.BallCostCount);
        if (!canAfford) return;

        _selectedRow = row;
        _selectedCol = col;
        _slots[row, col].SetSelected(true);
        if (StatusLabel != null) StatusLabel.Text = "Target a socket...";
        EmitSignal(SignalName.CardSlotSelected, row, col, card);
    }

    private void RefreshAllSlots()
    {
        if (_shop == null) return;

        for (int r = 0; r < CardShop.RowCount; r++)
        {
            for (int c = 0; c < CardShop.ColCount; c++)
            {
                PackageDealCard? card = _shop.GetCard(r, c);
                _slots[r, c].SetCard(card, r, c);
                bool canAfford = card != null && (_hopper == null || _hopper.HasBallCost(card.BallCostTier, card.BallCostCount));
                _slots[r, c].UpdateAffordability(canAfford);
            }
        }
    }

    private void RefreshAffordability()
    {
        for (int r = 0; r < CardShop.RowCount; r++)
        {
            for (int c = 0; c < CardShop.ColCount; c++)
            {
                PackageDealCard? card = _slots[r, c].Card;
                bool canAfford = card != null && (_hopper == null || _hopper.HasBallCost(card.BallCostTier, card.BallCostCount));
                _slots[r, c].UpdateAffordability(canAfford);
            }
        }
    }

    private void UpdateHeaderInfo()
    {
        if (DeckCountLabel != null && _shop != null)
        {
            DeckCountLabel.Text = _shop.IsDeckExhausted ? "Deck: EXHAUSTED" : $"Deck: {_shop.MasterDeck.Count}";
        }

        if (DealProgressBar != null && _meter != null)
        {
            DealProgressBar.Value = _meter.Progress * 100.0f;
        }

        if (DealMeterLabel != null && _meter != null)
        {
            int percent = (int)(_meter.Progress * 100.0f);
            DealMeterLabel.Text = $"Deal: {percent}% ({_meter.EffectiveRateMultiplier:F1}x)";
        }
    }

    private void UpdateCursorHighlight(int targetRow)
    {
        ActiveTargetRow = targetRow;
        for (int r = 0; r < CardShop.RowCount; r++)
        {
            if (_rowHeaderLabels[r] == null) continue;

            if (r == targetRow)
            {
                _rowHeaderLabels[r].Text = $"> ROW {r + 1} [TARGET]";
                _rowHeaderLabels[r].Modulate = new Color(1.0f, 0.9f, 0.4f);
            }
            else
            {
                _rowHeaderLabels[r].Text = $"  ROW {r + 1}";
                _rowHeaderLabels[r].Modulate = new Color(0.6f, 0.6f, 0.6f);
            }
        }
    }

    private void OnShopRowDealt(int row)
    {
        for (int c = 0; c < CardShop.ColCount; c++)
        {
            PackageDealCard? card = _shop?.GetCard(row, c);
            _slots[row, c].SetCard(card, row, c);
            bool canAfford = card != null && (_hopper == null || _hopper.HasBallCost(card.BallCostTier, card.BallCostCount));
            _slots[row, c].UpdateAffordability(canAfford);
        }
        UpdateHeaderInfo();
    }

    private void OnShopCursorMoved(int targetRow)
    {
        UpdateCursorHighlight(targetRow);
    }

    private void OnShopCardPurchased(PackageDealCard card, int row, int col)
    {
        ClearSelection();
        RefreshAllSlots();
        UpdateHeaderInfo();
    }

    private void OnShopRowDiscarded(int row)
    {
        for (int c = 0; c < CardShop.ColCount; c++)
        {
            _slots[row, c].SetCard(null, row, c);
        }
    }

    private void OnShopDeckExhausted()
    {
        UpdateHeaderInfo();
    }

    private void OnMeterProgressChanged(float progress, float effectiveRateMultiplier)
    {
        UpdateHeaderInfo();
    }

    private void OnHopperInventoryChanged()
    {
        RefreshAffordability();
    }
}
