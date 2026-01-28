using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Text.Json;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using IdleSdk.Core.Assets;
using IdleSdk.Core;
using IdleSdk.Core.Actions;
using IdleSdk.Core.Achievements;
using IdleSdk.Core.Audio;
using IdleSdk.Core.Collections;
using IdleSdk.Core.Combat;
using IdleSdk.Core.Compendium;
using IdleSdk.Core.Economy;
using IdleSdk.Core.Equipment;
using IdleSdk.Core.Crafting;
using IdleSdk.Core.Input;
using IdleSdk.Core.Items;
using IdleSdk.Core.Offline;
using IdleSdk.Core.Persistence;
using IdleSdk.Core.Quests;
using IdleSdk.Core.Scene;
using IdleSdk.Core.Trade;
using IdleSdk.Core.Skills;
using IdleSdk.Core.Sandbox;
using IdleSdk.Core.Timing;
using IdleSdk.Core.World;
using IdleSdk.Core.Generator;
using IdleSdk.Demo.Infrastructure;

namespace IdleSdk.Demo.ViewModels;

public sealed class DemoViewModel : INotifyPropertyChanged
{
    private readonly SimulationClock _clock;
    private readonly TickScheduler _scheduler;
    private readonly OfflineReconciler _offlineReconciler;
    private readonly ActionRunner _actionRunner;
    private readonly ActionRegistry _actionRegistry;
    private readonly SkillRegistry _skillRegistry;
    private readonly SkillSystem _skillSystem;
    private readonly QuestService _questService;
    private readonly AchievementService _achievementService;
    private readonly CollectionService _collectionService;
    private readonly InventoryService _inventoryService;
    private readonly EquipmentService _equipmentService;
    private readonly WalletService _walletService;
    private readonly CombatSystem _combatSystem;
    private readonly CombatEncounter _combatEncounter;
    private readonly CraftingService _craftingService;
    private readonly TradeService _tradeService;
    private readonly LayeredGenerator _layeredGenerator = new();
    private readonly BestiaryRegistry _bestiaryRegistry = new();
    private readonly ItemCompendiumRegistry _itemCompendiumRegistry = new();
    private readonly SnapshotService<DemoState> _snapshotService;
    private readonly SceneDiffEngine _sceneDiffEngine = new();
    private readonly SandboxConsole _sandboxConsole = new();
    private readonly AudioService _audioService;
    private readonly DispatcherTimer _trainingTimer;
    private readonly AssetRegistry _assetRegistry = new();
    private readonly Dictionary<string, ImageAssetEntry> _imageAssets = new(StringComparer.OrdinalIgnoreCase);
    private readonly Stopwatch _frameStopwatch = Stopwatch.StartNew();
    private TimeSpan _lastFrameTime = TimeSpan.Zero;
    private TimeSpan _frameAccumulator = TimeSpan.Zero;
    private static readonly TimeSpan SimulationStep = TimeSpan.FromSeconds(1);
    private static readonly TimeSpan FrameInterval = TimeSpan.FromSeconds(1d / 60d);

    private SceneFrame _previousScene = new();
    private SceneFrame _currentScene = new();
    private int _tickIndex;
    private string _sandboxStatus = "Disabled";
    private string _saveStatus = "";
    private bool _sandboxEnabled;
    private bool _lockValuesEnabled;
    private string _lockStatus = "Live";
    private string _sandboxIcon = "🧫";
    private string _lockIcon = "🔓";
    private readonly Dictionary<string, SkillEntry> _skillEntries = new(StringComparer.OrdinalIgnoreCase);
    private SkillEntry? _selectedSkill;
    private string _selectedSkillName = "Select a skill";
    private string _selectedSkillIcon = "✨";
    private Bitmap? _selectedSkillIconImage;
    private bool _selectedSkillHasIconImage;
    private int _selectedSkillIconWidth = 18;
    private int _selectedSkillIconHeight = 18;
    private string _selectedSkillDetails = "";
    private string _selectedSkillActionLabel = "Train";
    private bool _selectedSkillActionEnabled;
    private string _selectedSkillActionHint = "";
    private string? _activeSkillId;
    private SkillTaskEntry? _selectedSkillTask;
    private string _selectedSkillTaskHeader = "Mode";
    private readonly Dictionary<string, string> _selectedTaskBySkill = new(StringComparer.OrdinalIgnoreCase);
    private double _zoomLevel = 1.0;
    private bool _isDebugOverlayOpen;
    private string _debugTickRateText = "";
    private string _debugFpsText = "";
    private string _debugOnlineStatus = "Online";
    private string _debugLastOffline = "Last offline: n/a";
    private string _debugUiUpdatesText = "UI updates/frame: --";
    private int _fpsFrameCount;
    private TimeSpan _fpsAccumulator = TimeSpan.Zero;

    public DemoViewModel()
    {
        DemoLogger.Info("demo", "viewmodel-ctor");
        _clock = new SimulationClock(1);
        _scheduler = new TickScheduler(_clock);
        _offlineReconciler = new OfflineReconciler(_scheduler);

        _actionRegistry = new ActionRegistry();
        _actionRegistry.RegisterDefinition(new ActionDefinition("idle", "Idle", TimeSpan.FromSeconds(1), TimeSpan.Zero, new[] { "idle" }));
        _actionRegistry.RegisterDefinition(new ActionDefinition("woodcut", "Woodcut", TimeSpan.FromSeconds(1), TimeSpan.Zero, new[] { "woodcutting" }));
        _actionRunner = new ActionRunner(_actionRegistry);

        _skillRegistry = new SkillRegistry();
        foreach (var definition in LoadSkillDefinitions())
        {
            _skillRegistry.Register(definition);
        }
        _skillSystem = new SkillSystem(_skillRegistry, new ExponentialXpCurve(60, 1.5));

        var questRegistry = new QuestRegistry();
        questRegistry.Register(new QuestDefinition("woodcut", "Chop Logs", "Collect 5 logs", 5));
        _questService = new QuestService(questRegistry);

        var achievementRegistry = new AchievementRegistry();
        achievementRegistry.Register(new AchievementDefinition("wealth", "Wealth", "Earn 10 gold", 10));
        _achievementService = new AchievementService(achievementRegistry);

        var collectionRegistry = new CollectionRegistry();
        collectionRegistry.Register(new CollectionDefinition("starter", "Starter Set", new[] { "oak-log", "sword", "shield" }));
        _collectionService = new CollectionService(collectionRegistry);

        var itemRegistry = new ItemRegistry();
        itemRegistry.Register(new ItemDefinition("moment", "Moment", true));
        itemRegistry.Register(new ItemDefinition("oak-log", "Oak Log", true));
        itemRegistry.Register(new ItemDefinition("willow-log", "Willow Log", true));
        itemRegistry.Register(new ItemDefinition("maple-log", "Maple Log", true));
        itemRegistry.Register(new ItemDefinition("yew-log", "Yew Log", true));
        itemRegistry.Register(new ItemDefinition("magic-log", "Magic Log", true));
        itemRegistry.Register(new ItemDefinition("sword", "Sword", false));
        itemRegistry.Register(new ItemDefinition("shield", "Shield", false));
        _inventoryService = new InventoryService(itemRegistry);

        var equipmentRegistry = new EquipmentRegistry();
        equipmentRegistry.Register(new EquipmentItemDefinition("sword", EquipmentSlot.Weapon, 3, 0));
        equipmentRegistry.Register(new EquipmentItemDefinition("shield", EquipmentSlot.Offhand, 0, 2));
        _equipmentService = new EquipmentService(equipmentRegistry);

        var currencyRegistry = new CurrencyRegistry();
        currencyRegistry.Register(new CurrencyDefinition("gold", "Gold", false));
        _walletService = new WalletService(currencyRegistry);

        _craftingService = new CraftingService(_inventoryService);
        _craftingService.Register(new RecipeDefinition("plank", "Plank", new Dictionary<string, int> { ["oak-log"] = 2 }, "oak-log", 1));

        _tradeService = new TradeService(_inventoryService, _walletService);
        _tradeService.ListOffer(new TradeOffer("offer-oak", "oak-log", 1, 2));

        _bestiaryRegistry.Register(new BestiaryEntry("slime", "Slime", "A weak creature"));
        _itemCompendiumRegistry.Register(new ItemCompendiumEntry("moment", "Moment", "A fleeting spark"));
        _itemCompendiumRegistry.Register(new ItemCompendiumEntry("oak-log", "Oak Log", "Basic wood"));
        _itemCompendiumRegistry.Register(new ItemCompendiumEntry("willow-log", "Willow Log", "Lightweight wood"));

        _combatSystem = new CombatSystem();
        _combatEncounter = new CombatEncounter(new List<CombatantState>
        {
            new("player", new CombatantStats(20, 5, 1)),
            new("enemy", new CombatantStats(12, 3, 0))
        }, new SimpleCombatAi());
        _combatEncounter.Combatants[0].Effects.Add(new StatusEffect("battle-focus", 3, 1, 0));

        _audioService = new AudioService(new AudioRegistry());
        _audioService.Mixer.SetMaster(0.8f);

        _trainingTimer = new DispatcherTimer
        {
            Interval = FrameInterval
        };
        _trainingTimer.Tick += (_, _) => OnFrame();

        var snapshotPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "idle-sdk-demo.db");
        var snapshotStore = new SqliteSnapshotStore($"Data Source={snapshotPath}");
        _snapshotService = new SnapshotService<DemoState>(snapshotStore, new JsonStateSerializer<DemoState>());

        LoadImageAssets();
        SeedWorld();
        SeedEquipment();
        SeedScene();
        ConfigureActions();
        ConfigureSandbox();
        ConfigureHotkeys();
        _sandboxConsole.Enable();
        SandboxStatus = "Enabled";
        SandboxEnabled = true;
        SandboxIcon = "🧪";
        DebugTickRateText = $"Tick rate: {_clock.TickRate}/s";
        DebugFpsText = "FPS: --";
        DebugOnlineStatus = "Online";
        DebugLastOffline = "Last offline: n/a";
        UpdateAllDisplays();

        TickCommand = new RelayCommand(TickOnce);
        OfflineCommand = new RelayCommand(() => RunOffline(TimeSpan.FromSeconds(10)));
        ToggleSandboxCommand = new RelayCommand(ToggleSandbox);
        SandboxAddGoldCommand = new RelayCommand(() => ExecuteSandbox("add-gold"));
        SaveCommand = new RelayCommand(SaveSnapshot);
        LoadCommand = new RelayCommand(LoadSnapshot);
        ToggleLockCommand = new RelayCommand(ToggleLock);
        SkillActionCommand = new RelayCommand(ExecuteSelectedSkillAction);
        ToggleDebugOverlayCommand = new RelayCommand(() => IsDebugOverlayOpen = !IsDebugOverlayOpen);
        DemoLogger.Info("demo", "viewmodel-ready");
    }

    public ObservableCollection<SkillEntry> Skills { get; } = new();
    public ObservableCollection<string> SelectedSkillUnlocks { get; } = new();
    public ObservableCollection<SkillTaskEntry> SelectedSkillTasks { get; } = new();
    public ObservableCollection<string> CombatLines { get; } = new();
    public ObservableCollection<string> InventoryLines { get; } = new();
    public ObservableCollection<string> EquipmentLines { get; } = new();
    public ObservableCollection<string> QuestLines { get; } = new();
    public ObservableCollection<string> AchievementLines { get; } = new();
    public ObservableCollection<string> CollectionLines { get; } = new();
    public ObservableCollection<string> WalletLines { get; } = new();
    public ObservableCollection<string> WorldLines { get; } = new();
    public ObservableCollection<string> SceneLines { get; } = new();
    public ObservableCollection<string> SceneDiffLines { get; } = new();
    public ObservableCollection<string> CraftingLines { get; } = new();
    public ObservableCollection<string> TradeLines { get; } = new();
    public ObservableCollection<string> GeneratorLines { get; } = new();
    public ObservableCollection<string> CompendiumLines { get; } = new();
    public ObservableCollection<DebugAdjustEntry> DebugWalletEntries { get; } = new();
    public ObservableCollection<DebugAdjustEntry> DebugInventoryEntries { get; } = new();
    public ObservableCollection<DebugSkillEntry> DebugSkillEntries { get; } = new();

    public RelayCommand TickCommand { get; }
    public RelayCommand OfflineCommand { get; }
    public RelayCommand ToggleSandboxCommand { get; }
    public RelayCommand SandboxAddGoldCommand { get; }
    public RelayCommand SaveCommand { get; }
    public RelayCommand LoadCommand { get; }
    public RelayCommand ToggleLockCommand { get; }
    public RelayCommand SkillActionCommand { get; }
    public RelayCommand ToggleDebugOverlayCommand { get; }

    public string SandboxStatus
    {
        get => _sandboxStatus;
        private set
        {
            _sandboxStatus = value;
            OnPropertyChanged();
        }
    }

    public bool SandboxEnabled
    {
        get => _sandboxEnabled;
        private set
        {
            _sandboxEnabled = value;
            OnPropertyChanged();
        }
    }

    public string SandboxIcon
    {
        get => _sandboxIcon;
        private set
        {
            _sandboxIcon = value;
            OnPropertyChanged();
        }
    }

    public bool LockValuesEnabled
    {
        get => _lockValuesEnabled;
        private set
        {
            _lockValuesEnabled = value;
            OnPropertyChanged();
        }
    }

    public string LockIcon
    {
        get => _lockIcon;
        private set
        {
            _lockIcon = value;
            OnPropertyChanged();
        }
    }

    public string LockStatus
    {
        get => _lockStatus;
        private set
        {
            _lockStatus = value;
            OnPropertyChanged();
        }
    }

    public SkillEntry? SelectedSkill
    {
        get => _selectedSkill;
        set
        {
            if (_selectedSkill == value)
            {
                return;
            }

            _selectedSkill = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(HasSelectedSkill));
            OnPropertyChanged(nameof(NoSelectedSkill));
            OnPropertyChanged(nameof(SelectedSkillActiveClass));
            UpdateSelectedSkillDetails();
        }
    }

    public bool HasSelectedSkill => SelectedSkill is not null;
    public bool NoSelectedSkill => SelectedSkill is null;

    public string SelectedSkillActiveClass
        => SelectedSkill is not null && string.Equals(_activeSkillId, SelectedSkill.Id, StringComparison.OrdinalIgnoreCase)
            ? "active"
            : string.Empty;

    public string SelectedSkillName
    {
        get => _selectedSkillName;
        private set
        {
            _selectedSkillName = value;
            OnPropertyChanged();
        }
    }

    public string SelectedSkillIcon
    {
        get => _selectedSkillIcon;
        private set
        {
            _selectedSkillIcon = value;
            OnPropertyChanged();
        }
    }

    public Bitmap? SelectedSkillIconImage
    {
        get => _selectedSkillIconImage;
        private set
        {
            _selectedSkillIconImage = value;
            OnPropertyChanged();
        }
    }

    public bool SelectedSkillHasIconImage
    {
        get => _selectedSkillHasIconImage;
        private set
        {
            _selectedSkillHasIconImage = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(SelectedSkillHasEmojiIcon));
        }
    }

    public bool SelectedSkillHasEmojiIcon => !SelectedSkillHasIconImage;

    public int SelectedSkillIconWidth
    {
        get => _selectedSkillIconWidth;
        private set
        {
            _selectedSkillIconWidth = value;
            OnPropertyChanged();
        }
    }

    public int SelectedSkillIconHeight
    {
        get => _selectedSkillIconHeight;
        private set
        {
            _selectedSkillIconHeight = value;
            OnPropertyChanged();
        }
    }

    public string SelectedSkillDetails
    {
        get => _selectedSkillDetails;
        private set
        {
            _selectedSkillDetails = value;
            OnPropertyChanged();
        }
    }

    public string SelectedSkillActionLabel
    {
        get => _selectedSkillActionLabel;
        private set
        {
            _selectedSkillActionLabel = value;
            OnPropertyChanged();
        }
    }

    public bool SelectedSkillActionEnabled
    {
        get => _selectedSkillActionEnabled;
        private set
        {
            _selectedSkillActionEnabled = value;
            OnPropertyChanged();
        }
    }

    public string SelectedSkillActionHint
    {
        get => _selectedSkillActionHint;
        private set
        {
            _selectedSkillActionHint = value;
            OnPropertyChanged();
        }
    }

    public SkillTaskEntry? SelectedSkillTask
    {
        get => _selectedSkillTask;
        set
        {
            if (value is null || !value.IsUnlocked)
            {
                return;
            }

            if (_selectedSkillTask == value)
            {
                return;
            }

            _selectedSkillTask = value;
            if (SelectedSkill is not null)
            {
                _selectedTaskBySkill[SelectedSkill.Id] = value.Id;
            }
            OnPropertyChanged();
            UpdateSelectedSkillDetails();
        }
    }

    public string SelectedSkillTaskHeader
    {
        get => _selectedSkillTaskHeader;
        private set
        {
            _selectedSkillTaskHeader = value;
            OnPropertyChanged();
        }
    }

    public double ZoomLevel
    {
        get => _zoomLevel;
        set
        {
            var clamped = Math.Clamp(value, 0.5, 2.0);
            if (Math.Abs(_zoomLevel - clamped) < 0.001)
            {
                return;
            }

            _zoomLevel = clamped;
            OnPropertyChanged();
        }
    }

    public bool IsDebugOverlayOpen
    {
        get => _isDebugOverlayOpen;
        set
        {
            if (_isDebugOverlayOpen == value)
            {
                return;
            }

            _isDebugOverlayOpen = value;
            OnPropertyChanged();
        }
    }

    public string DebugTickRateText
    {
        get => _debugTickRateText;
        private set
        {
            _debugTickRateText = value;
            OnPropertyChanged();
        }
    }

    public string DebugFpsText
    {
        get => _debugFpsText;
        private set
        {
            _debugFpsText = value;
            OnPropertyChanged();
        }
    }

    public string DebugUiUpdatesText
    {
        get => _debugUiUpdatesText;
        private set
        {
            _debugUiUpdatesText = value;
            OnPropertyChanged();
        }
    }

    public string DebugOnlineStatus
    {
        get => _debugOnlineStatus;
        private set
        {
            _debugOnlineStatus = value;
            OnPropertyChanged();
        }
    }

    public string DebugLastOffline
    {
        get => _debugLastOffline;
        private set
        {
            _debugLastOffline = value;
            OnPropertyChanged();
        }
    }

    public string SaveStatus
    {
        get => _saveStatus;
        private set
        {
            _saveStatus = value;
            OnPropertyChanged();
        }
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    private void ConfigureActions()
    {
        _actionRegistry.RegisterHandler(new IdleActionHandler(this));
        _actionRegistry.RegisterHandler(new WoodcutActionHandler(this));
    }

    private void ConfigureSandbox()
    {
        _sandboxConsole.Register("add-gold", _ =>
        {
            _walletService.Credit(ProfileId, "gold", 5);
            UpdateWallets();
            return new SandboxResult(true, "Added 5 gold.");
        });
    }

    private void ConfigureHotkeys()
    {
        var hotkeys = new HotkeyManager();
        hotkeys.Register(new HotkeyBinding("tick", "Space"));
        hotkeys.Register(new HotkeyBinding("sandbox", "Ctrl+S"));
    }

    private void SeedWorld()
    {
        var universe = new UniverseDefinition("u1", "Prime");
        var world = new WorldDefinition("w1", "Idle Realm");
        var region = new WorldRegion("r1", "Northlands");
        var zone = new WorldZone("z1", "Forest");
        zone.AddNode(new WorldNode("n1", "Clearing"));
        region.AddZone(zone);
        world.AddRegion(region);
        universe.AddWorld(world);

        WorldLines.Add($"Universe: {universe.Name}");
        WorldLines.Add($"World: {world.Name}");
        WorldLines.Add($"Region: {region.Name}");
        WorldLines.Add($"Zone: {zone.Name}");
        WorldLines.Add("Node: Clearing");
    }

    private void SeedEquipment()
    {
        _equipmentService.Equip(ProfileId, "sword");
        _equipmentService.Equip(ProfileId, "shield");
        UpdateEquipment();
    }

    private void SeedScene()
    {
        _currentScene = new SceneFrame();
        _currentScene.Add(new SceneElement("bg", SceneLayer.Background, "forest", 0, 0));
        _currentScene.Add(new SceneElement("player", SceneLayer.Foreground, "hero", 5, 5));
        _previousScene = new SceneFrame();
        RefreshSceneDisplay();
    }

    private void TickOnce()
    {
        try
        {
            if (LockValuesEnabled)
            {
                DemoLogger.Info("demo", "tick-skipped", new { reason = "locked" });
                return;
            }
            DemoLogger.Info("demo", "tick", new { tick = _tickIndex });
            _scheduler.Step(TimeSpan.FromSeconds(1), _ => ExecuteAction());
            UpdateAllDisplays();
        }
        catch (Exception ex)
        {
            DemoLogger.Error("demo", "tick-failed", ex);
            throw;
        }
    }

    private void RunOffline(TimeSpan duration)
    {
        try
        {
            if (LockValuesEnabled)
            {
                DemoLogger.Info("demo", "offline-skipped", new { reason = "locked" });
                return;
            }
            DebugOnlineStatus = "Offline (reconciling)";
            DebugLastOffline = $"Last offline: {duration.TotalSeconds:0}s";
            DemoLogger.Info("demo", "offline", new { durationSeconds = duration.TotalSeconds });
            _offlineReconciler.Reconcile(new OfflineReconcileRequest(ProfileId, DateTimeOffset.UtcNow - duration, DateTimeOffset.UtcNow), _ => ExecuteAction(), duration);
            UpdateAllDisplays();
            DebugOnlineStatus = "Online";
        }
        catch (Exception ex)
        {
            DemoLogger.Error("demo", "offline-failed", ex);
            throw;
        }
    }

    private void ExecuteAction()
    {
        try
        {
            if (LockValuesEnabled)
            {
                DemoLogger.Info("demo", "action-skipped", new { reason = "locked" });
                return;
            }
            _tickIndex++;
            DemoLogger.Info("demo", "execute-action", new { tick = _tickIndex });
            if (!string.IsNullOrWhiteSpace(_activeSkillId))
            {
                ExecuteActiveTraining();
            }
            var combatResult = _combatSystem.Step(_combatEncounter);
            UpdateCombat(combatResult);
            UpdateScene();
        }
        catch (Exception ex)
        {
            DemoLogger.Error("demo", "execute-action-failed", ex);
            throw;
        }
    }

    private void UpdateScene()
    {
        var next = new SceneFrame();
        foreach (var element in _currentScene.Elements)
        {
            next.Add(element);
        }

        next.Add(new SceneElement($"drop-{_tickIndex}", SceneLayer.Overlay, "drop", _tickIndex % 5, 1));
        var diff = _sceneDiffEngine.Compute(_currentScene, next);
        _previousScene = _currentScene;
        _currentScene = next;

        SceneLines.Clear();
        foreach (var element in _currentScene.Elements)
        {
            SceneLines.Add($"{element.Layer}: {element.AssetKey} ({element.X},{element.Y})");
        }

        SceneDiffLines.Clear();
        foreach (var element in diff.Added)
        {
            SceneDiffLines.Add($"Added: {element.Id}");
        }
        foreach (var element in diff.Removed)
        {
            SceneDiffLines.Add($"Removed: {element.Id}");
        }
    }

    private void UpdateCombat(CombatTickResult result)
    {
        DemoLogger.Info("demo", "combat-update", new { entries = result.LogEntries.Count, aiDecisions = result.AiDecisions.Count });
        CombatLines.Clear();
        foreach (var entry in result.LogEntries)
        {
            CombatLines.Add($"{entry.AttackerId} hit {entry.DefenderId} for {entry.Damage}");
        }

        foreach (var decision in result.AiDecisions)
        {
            CombatLines.Add($"AI: {decision.AttackerId} -> {decision.TargetId} (seed {decision.Seed?.ToString() ?? "n/a"})");
        }

        foreach (var combatant in _combatEncounter.Combatants)
        {
            var effects = combatant.Effects.Effects.Select(effect => $"{effect.Id}({effect.DurationTicks})");
            var joined = string.Join(", ", effects);
            CombatLines.Add($"{combatant.Id} effects: {(string.IsNullOrWhiteSpace(joined) ? "none" : joined)}");
        }
    }

    private void UpdateAllDisplays()
    {
        DemoLogger.Info("demo", "update-displays");
        UpdateSkills();
        UpdateInventory();
        UpdateEquipment();
        UpdateQuests();
        UpdateAchievements();
        UpdateCollections();
                UpdateWallets();
        UpdateCrafting();
        UpdateTrade();
        UpdateGenerator();
        UpdateCompendium();
                RefreshSceneDisplay();
        UpdateSelectedSkillDetails();
        UpdateDebugExplorer();
    }

    private void UpdateDebugExplorer()
    {
        DebugWalletEntries.Clear();
        var wallet = _walletService.GetOrCreateWallet(ProfileId);
        var gold = wallet.GetBalance("gold");
        DebugWalletEntries.Add(new DebugAdjustEntry(
            "Gold",
            gold.ToString(),
            new RelayCommand(() => AdjustGold(10)),
            new RelayCommand(() => AdjustGold(-10))));

        DebugInventoryEntries.Clear();
        var inventory = _inventoryService.GetOrCreate(ProfileId);
        foreach (var itemId in GetDebugItemIds())
        {
            var quantity = inventory.GetQuantity(itemId);
            DebugInventoryEntries.Add(new DebugAdjustEntry(
                itemId,
                quantity.ToString(),
                new RelayCommand(() => AdjustItem(itemId, 1)),
                new RelayCommand(() => AdjustItem(itemId, -1))));
        }

        DebugSkillEntries.Clear();
        foreach (var definition in OrderSkills(_skillRegistry.Definitions))
        {
            var progress = _skillSystem.GetOrCreateProgress(definition.Id);
            DebugSkillEntries.Add(new DebugSkillEntry(
                definition.Name,
                $"L{progress.Level} • {progress.TotalXp} XP",
                new RelayCommand(() => AddSkillXp(definition.Id, 100)),
                new RelayCommand(() => AddSkillXp(definition.Id, 1000))));
        }
    }

    private static IReadOnlyList<string> GetDebugItemIds()
        => new[]
        {
            "moment",
            "oak-log",
            "willow-log",
            "maple-log",
            "yew-log",
            "magic-log"
        };

    private void AdjustGold(long delta)
    {
        try
        {
            if (delta >= 0)
            {
                _walletService.Credit(ProfileId, "gold", delta);
            }
            else
            {
                _walletService.Debit(ProfileId, "gold", Math.Abs(delta));
            }
        }
        catch (Exception ex)
        {
            DemoLogger.Warn("demo", "debug-adjust-gold-failed", new { error = ex.Message });
        }

        UpdateAllDisplays();
    }

    private void AdjustItem(string itemId, int delta)
    {
        try
        {
            if (delta >= 0)
            {
                _inventoryService.AddItem(ProfileId, itemId, delta);
            }
            else
            {
                _inventoryService.RemoveItem(ProfileId, itemId, Math.Abs(delta));
            }
        }
        catch (Exception ex)
        {
            DemoLogger.Warn("demo", "debug-adjust-item-failed", new { itemId, error = ex.Message });
        }

        UpdateAllDisplays();
    }

    private void AddSkillXp(string skillId, int amount)
    {
        try
        {
            _skillSystem.AddXp(skillId, amount);
        }
        catch (Exception ex)
        {
            DemoLogger.Warn("demo", "debug-add-skill-xp-failed", new { skillId, error = ex.Message });
        }

        UpdateAllDisplays();
    }

    private void UpdateSkills()
    {
        var selectedId = SelectedSkill?.Id;
        Skills.Clear();

        var ordered = OrderSkills(_skillRegistry.Definitions).ToList();
        foreach (var definition in ordered)
        {
            var progress = _skillSystem.GetOrCreateProgress(definition.Id);
            var iconImage = GetIconBitmap(definition.Icon, out var width, out var height, out var hasImage);
            var currentLevelXp = progress.XpCurve.GetTotalXpForLevel(progress.Level);
            var nextLevelXp = progress.Level >= definition.MaxLevel
                ? currentLevelXp
                : progress.XpCurve.GetTotalXpForLevel(progress.Level + 1);
            var progressRatio = nextLevelXp <= currentLevelXp
                ? 1.0
                : Math.Clamp((progress.TotalXp - currentLevelXp) / (double)(nextLevelXp - currentLevelXp), 0, 1);

            if (!_skillEntries.TryGetValue(definition.Id, out var entry))
            {
                entry = new SkillEntry(definition.Id);
                _skillEntries[definition.Id] = entry;
            }

            entry.Update(
                definition.Name,
                definition.Icon,
                iconImage,
                width,
                height,
                hasImage,
                string.Equals(_activeSkillId, definition.Id, StringComparison.OrdinalIgnoreCase),
                progress.Level,
                progress.TotalXp,
                definition.MaxLevel,
                progressRatio,
                progress.Unlocks.ToList());

            Skills.Add(entry);
        }

        if (!string.IsNullOrWhiteSpace(selectedId))
        {
            SelectedSkill = Skills.FirstOrDefault(skill => skill.Id.Equals(selectedId, StringComparison.OrdinalIgnoreCase));
        }

        if (SelectedSkill is null && Skills.Count > 0)
        {
            SelectedSkill = Skills[0];
        }
    }

    private void UpdateInventory()
    {
        InventoryLines.Clear();
        var inventory = _inventoryService.GetOrCreate(ProfileId);
        foreach (var slot in inventory.GetSlots())
        {
            InventoryLines.Add($"{slot.ItemId}: {slot.Quantity}");
        }
    }

    private void UpdateEquipment()
    {
        EquipmentLines.Clear();
        var loadout = _equipmentService.GetOrCreateLoadout(ProfileId);
        foreach (var slot in loadout.Slots)
        {
            EquipmentLines.Add($"{slot.Key}: {slot.Value}");
        }

        var totals = _equipmentService.GetTotalBonuses(ProfileId);
        EquipmentLines.Add($"Bonuses: ATK +{totals.Attack}, DEF +{totals.Defense}");
    }

    private void UpdateQuests()
    {
        QuestLines.Clear();
        var progress = _questService.GetOrCreate(ProfileId, "woodcut");
        QuestLines.Add($"Chop Logs: {progress.CurrentValue}/5");
    }

    private void UpdateAchievements()
    {
        AchievementLines.Clear();
        var progress = _achievementService.GetOrCreate(ProfileId, "wealth");
        AchievementLines.Add($"Wealth: {progress.CurrentValue}/10");
    }

    private void UpdateCollections()
    {
        CollectionLines.Clear();
        var progress = _collectionService.GetOrCreate(ProfileId, "starter");
        CollectionLines.Add($"Starter Set: {progress.Collected.Count}/{progress.Definition.ItemIds.Count}");
    }

    private void UpdateWallets()
    {
        WalletLines.Clear();
        var wallet = _walletService.GetOrCreateWallet(ProfileId);
        WalletLines.Add($"Gold: {wallet.GetBalance("gold")}");
    }

    private void UpdateCrafting()
    {
        CraftingLines.Clear();
        CraftingLines.Add("Recipe: 2x oak log -> 1x oak log (demo)");
    }

    private void UpdateTrade()
    {
        TradeLines.Clear();
        foreach (var offer in _tradeService.Offers)
        {
            TradeLines.Add($"{offer.Id}: {offer.Quantity}x {offer.ItemId} for {offer.Price} gold");
        }
    }

    private void UpdateGenerator()
    {
        GeneratorLines.Clear();
        var layers = new List<LayerDefinition>
        {
            new("base", new Dictionary<string, int> { ["A"] = 1, ["B"] = 1 }),
            new("hat", new Dictionary<string, int> { ["Cap"] = 2, ["Crown"] = 1 })
        };

        var result = _layeredGenerator.Generate(layers, 7);
        foreach (var entry in result)
        {
            GeneratorLines.Add($"{entry.Key}: {entry.Value}");
        }
    }

    private void UpdateCompendium()
    {
        CompendiumLines.Clear();
        foreach (var entry in _bestiaryRegistry.Entries)
        {
            CompendiumLines.Add($"Bestiary: {entry.Name}");
        }
        foreach (var entry in _itemCompendiumRegistry.Entries)
        {
            CompendiumLines.Add($"Item: {entry.Name}");
        }
    }

    private void ExecuteSandbox(string command)
    {
        var result = _sandboxConsole.Execute(new SandboxCommand(command, new Dictionary<string, string>()));
        if (result.Success)
        {
            UpdateWallets();
        }
    }

    private void RefreshSceneDisplay()
    {
        SceneLines.Clear();
        foreach (var element in _currentScene.Elements)
        {
            SceneLines.Add($"{element.Layer}: {element.AssetKey} ({element.X},{element.Y})");
        }

        SceneDiffLines.Clear();
    }

    private void ToggleSandbox()
    {
        if (_sandboxConsole.Enabled)
        {
            _sandboxConsole.Disable();
        }
        else
        {
            _sandboxConsole.Enable();
        }

        SandboxStatus = _sandboxConsole.Enabled ? "Enabled" : "Disabled";
        SandboxEnabled = _sandboxConsole.Enabled;
        SandboxIcon = SandboxEnabled ? "🧪" : "🧫";
    }

    private void ToggleLock()
    {
        LockValuesEnabled = !LockValuesEnabled;
        LockStatus = LockValuesEnabled ? "Locked" : "Live";
        LockIcon = LockValuesEnabled ? "🔒" : "🔓";
        UpdateSelectedSkillDetails();
        UpdateTrainingTimer();
    }

    private void ExecuteSelectedSkillAction()
    {
        if (SelectedSkill is null || LockValuesEnabled)
        {
            return;
        }

        if (string.Equals(_activeSkillId, SelectedSkill.Id, StringComparison.OrdinalIgnoreCase))
        {
            DemoLogger.Info("demo", "skill-stop", new { skill = SelectedSkill.Id });
            _activeSkillId = null;
        }
        else
        {
            DemoLogger.Info("demo", "skill-start", new { skill = SelectedSkill.Id });
            _activeSkillId = SelectedSkill.Id;
        }

        OnPropertyChanged(nameof(SelectedSkillActiveClass));
        UpdateSkills();
        UpdateTrainingTimer();
    }

    private void UpdateTrainingTimer()
    {
        if (!string.IsNullOrWhiteSpace(_activeSkillId) && !LockValuesEnabled)
        {
            if (!_trainingTimer.IsEnabled)
            {
                _frameAccumulator = TimeSpan.Zero;
                _lastFrameTime = _frameStopwatch.Elapsed;
                _trainingTimer.Start();
            }

            return;
        }

        if (_trainingTimer.IsEnabled)
        {
            _trainingTimer.Stop();
        }
    }

    private void OnFrame()
    {
        if (LockValuesEnabled)
        {
            return;
        }

        var now = _frameStopwatch.Elapsed;
        var delta = now - _lastFrameTime;
        _lastFrameTime = now;

        if (delta <= TimeSpan.Zero)
        {
            return;
        }

        _fpsFrameCount++;
        _fpsAccumulator += delta;
        if (_fpsAccumulator >= TimeSpan.FromSeconds(1))
        {
            var fps = _fpsFrameCount / _fpsAccumulator.TotalSeconds;
            DebugFpsText = $"FPS: {fps:0}";
            _fpsFrameCount = 0;
            _fpsAccumulator = TimeSpan.Zero;
        }

        var uiUpdates = SmoothSkillProgress(delta);
        DebugUiUpdatesText = $"UI updates/frame: {uiUpdates}";

        _frameAccumulator += delta;
        while (_frameAccumulator >= SimulationStep)
        {
            _frameAccumulator -= SimulationStep;
            TickOnce();
        }
    }

    private int SmoothSkillProgress(TimeSpan delta)
    {
        var seconds = delta.TotalSeconds;
        if (seconds <= 0)
        {
            return 0;
        }

        var smoothing = 1 - Math.Exp(-seconds / 0.18);
        var updates = 0;
        foreach (var entry in Skills)
        {
            if (entry.SmoothProgress(smoothing))
            {
                updates++;
            }
        }

        return updates;
    }

    private void ExecuteActiveTraining()
    {
        if (string.IsNullOrWhiteSpace(_activeSkillId))
        {
            return;
        }

        var now = DateTimeOffset.UtcNow;
        if (_activeSkillId.Equals("idling", StringComparison.OrdinalIgnoreCase))
        {
            var result = _actionRunner.Step("idle", new ActionContext(ProfileId, now), TimeSpan.FromSeconds(1));
            DemoLogger.Info("demo", "action-result", new { action = "idle", result.Completed, result.Output });
            return;
        }

        if (_activeSkillId.Equals("woodcutting", StringComparison.OrdinalIgnoreCase))
        {
            var result = _actionRunner.Step("woodcut", new ActionContext(ProfileId, now), TimeSpan.FromSeconds(1));
            DemoLogger.Info("demo", "action-result", new { action = "woodcut", result.Completed, result.Output });
        }
    }

    private void SaveSnapshot()
    {
        try
        {
            DemoLogger.Info("demo", "save-snapshot");
            var wallet = _walletService.GetOrCreateWallet(ProfileId);
            var inventory = _inventoryService.GetOrCreate(ProfileId);
            var actionCooldowns = _actionRunner.GetCooldownSnapshot();
            var combatEffects = _combatEncounter.Combatants
                .ToDictionary(combatant => combatant.Id, combatant => combatant.Effects.GetSnapshot().ToList());
            var state = new DemoState(
                wallet.GetBalance("gold"),
                inventory.GetQuantity("oak-log"),
                _skillSystem.GetOrCreateProgress("idling").Level,
                actionCooldowns,
                combatEffects);
            _snapshotService.SaveAsync(ProfileId.ToString(), state, "v1").GetAwaiter().GetResult();
            SaveStatus = "Saved";
        }
        catch (Exception ex)
        {
            DemoLogger.Error("demo", "save-snapshot-failed", ex);
            throw;
        }
    }

    private void LoadSnapshot()
    {
        try
        {
            DemoLogger.Info("demo", "load-snapshot");
            var loaded = _snapshotService.LoadLatestAsync(ProfileId.ToString()).GetAwaiter().GetResult();
            if (loaded is null)
            {
                SaveStatus = "No snapshot";
                DemoLogger.Warn("demo", "load-snapshot-empty");
                return;
            }

            var wallet = _walletService.GetOrCreateWallet(ProfileId);
            var current = wallet.GetBalance("gold");
            if (loaded.Gold > current)
            {
                _walletService.Credit(ProfileId, "gold", loaded.Gold - current);
            }

            var inventory = _inventoryService.GetOrCreate(ProfileId);
            var logCount = inventory.GetQuantity("oak-log");
            if (loaded.Logs > logCount)
            {
                _inventoryService.AddItem(ProfileId, "oak-log", loaded.Logs - logCount);
            }

            if (loaded.ActionCooldowns is not null)
            {
                _actionRunner.RestoreCooldownSnapshot(loaded.ActionCooldowns);
            }

            if (loaded.CombatEffects is not null)
            {
                foreach (var combatant in _combatEncounter.Combatants)
                {
                    if (loaded.CombatEffects.TryGetValue(combatant.Id, out var effects))
                    {
                        combatant.Effects.RestoreSnapshot(effects);
                    }
                }
            }

            SaveStatus = "Loaded";
            UpdateAllDisplays();
        }
        catch (Exception ex)
        {
            DemoLogger.Error("demo", "load-snapshot-failed", ex);
            throw;
        }
    }

    private IEnumerable<SkillDefinition> LoadSkillDefinitions()
    {
        var packSkills = TryLoadDeveloperSkills();
        if (packSkills.Count > 0)
        {
            return packSkills;
        }

        return new[]
        {
            new SkillDefinition("idling", "Idling", "💤", 20, new Dictionary<int, string>
            {
                [1] = "Idle",
                [5] = "Flip a coin",
                [10] = "Daydream",
                [15] = "Power nap",
                [20] = "Lucid drift"
            }),
            new SkillDefinition("woodcutting", "Woodcutting", "🪓", 20, new Dictionary<int, string>
            {
                [1] = "Oak",
                [5] = "Willow",
                [10] = "Maple",
                [15] = "Yew",
                [20] = "Magic"
            }),
        };
    }

    private IReadOnlyList<SkillDefinition> TryLoadDeveloperSkills()
    {
        var skillsPath = FindDeveloperPackFile("skills.json");
        if (skillsPath is null)
        {
            return Array.Empty<SkillDefinition>();
        }

        try
        {
            var json = File.ReadAllText(skillsPath);
            var entries = JsonSerializer.Deserialize<List<SkillPackEntry>>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (entries is null || entries.Count == 0)
            {
                return Array.Empty<SkillDefinition>();
            }

            return entries.Select(entry => new SkillDefinition(
                entry.Id,
                entry.Name,
                entry.Icon,
                entry.MaxLevel,
                NormalizeUnlocks(entry.Unlocks))).ToList();
        }
        catch (Exception ex)
        {
            DemoLogger.Warn("demo", "developer-pack-skills-failed", ex);
            return Array.Empty<SkillDefinition>();
        }
    }

    private void LoadImageAssets()
    {
        var imagesPath = FindDeveloperPackFile("images.json");
        if (imagesPath is null)
        {
            return;
        }

        var packRoot = Directory.GetParent(Path.GetDirectoryName(imagesPath)!)?.FullName;
        if (string.IsNullOrWhiteSpace(packRoot))
        {
            return;
        }

        try
        {
            var json = File.ReadAllText(imagesPath);
            var entries = JsonSerializer.Deserialize<List<ImageAssetEntry>>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (entries is null)
            {
                return;
            }

            foreach (var entry in entries)
            {
                var absolutePath = Path.Combine(packRoot, entry.Path.Replace('/', Path.DirectorySeparatorChar));
                var type = entry.Type.ToLowerInvariant() switch
                {
                    "png" => ImageAssetType.Png,
                    "jpg" => ImageAssetType.Jpg,
                    "jpeg" => ImageAssetType.Jpeg,
                    _ => ImageAssetType.Unknown
                };

                var image = new ImageAssetDefinition(entry.Id, absolutePath, type, entry.RenderWidth, entry.RenderHeight);
                _assetRegistry.RegisterImage(image);
                _imageAssets[entry.Id] = entry with { Path = absolutePath };
            }
        }
        catch (Exception ex)
        {
            DemoLogger.Warn("demo", "developer-pack-images-failed", new { error = ex.Message });
        }
    }

    private Bitmap? GetIconBitmap(string iconId, out int width, out int height, out bool hasImage)
    {
        if (_imageAssets.TryGetValue(iconId, out var asset))
        {
            width = asset.RenderWidth ?? 16;
            height = asset.RenderHeight ?? 16;
            hasImage = true;
            try
            {
                return new Bitmap(asset.Path);
            }
            catch (Exception ex)
            {
                DemoLogger.Warn("demo", "icon-load-failed", new { iconId, error = ex.Message });
            }
        }

        width = 16;
        height = 16;
        hasImage = false;
        return null;
    }

    private static IReadOnlyDictionary<int, string> NormalizeUnlocks(Dictionary<string, string>? unlocks)
    {
        if (unlocks is null || unlocks.Count == 0)
        {
            return new Dictionary<int, string>();
        }

        var normalized = new Dictionary<int, string>();
        foreach (var pair in unlocks)
        {
            if (int.TryParse(pair.Key, out var level))
            {
                normalized[level] = pair.Value;
            }
        }

        return normalized;
    }

    private static string? FindDeveloperPackFile(string fileName)
    {
        var current = AppContext.BaseDirectory;
        for (var i = 0; i < 8; i++)
        {
            var candidate = Path.Combine(current, "content-packs", "Developer", "data", fileName);
            if (File.Exists(candidate))
            {
                return candidate;
            }

            var parent = Directory.GetParent(current);
            if (parent is null)
            {
                break;
            }

            current = parent.FullName;
        }

        return null;
    }

    private static IEnumerable<SkillDefinition> OrderSkills(IEnumerable<SkillDefinition> definitions)
    {
        var order = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
        {
            ["idling"] = 0,
            ["woodcutting"] = 1
        };

        return definitions
            .OrderBy(def => order.TryGetValue(def.Id, out var rank) ? rank : int.MaxValue)
            .ThenBy(def => def.Name, StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    private void BuildSelectedSkillTasks(int level)
    {
        SelectedSkillTasks.Clear();

        if (SelectedSkill is null)
        {
            SelectedSkillTaskHeader = "Mode";
            return;
        }

        if (SelectedSkill.Id.Equals("idling", StringComparison.OrdinalIgnoreCase))
        {
            SelectedSkillTaskHeader = "Mode";
            AddTask("idle", "Idle", 1, level, "1-3 Moments");
            AddTask("flip", "Flip a coin", 5, level, "2-4 Moments");
            AddTask("daydream", "Daydream", 10, level, "3-5 Moments");
            AddTask("nap", "Power nap", 15, level, "4-6 Moments");
            AddTask("lucid", "Lucid drift", 20, level, "5-7 Moments");
        }
        else if (SelectedSkill.Id.Equals("woodcutting", StringComparison.OrdinalIgnoreCase))
        {
            SelectedSkillTaskHeader = "Mode";
            AddTask("oak", "Oak tree", 1, level, "1-3 Oak Logs");
            AddTask("willow", "Willow tree", 10, level, "1-3 Willow Logs");
            AddTask("maple", "Maple tree", 20, level, "1-3 Maple Logs");
            AddTask("yew", "Yew tree", 30, level, "1-3 Yew Logs");
            AddTask("magic", "Magic tree", 40, level, "1-3 Magic Logs");
        }
        else
        {
            SelectedSkillTaskHeader = "Mode";
        }

        SelectDefaultTask();
    }

    private void AddTask(string id, string name, int requiredLevel, int currentLevel, string reward)
    {
        var unlocked = currentLevel >= requiredLevel;
        var selected = SelectedSkill is not null
            && _selectedTaskBySkill.TryGetValue(SelectedSkill.Id, out var taskId)
            && taskId.Equals(id, StringComparison.OrdinalIgnoreCase);

        SelectedSkillTasks.Add(new SkillTaskEntry(id, name, requiredLevel, reward, unlocked, selected));
    }

    private void SelectDefaultTask()
    {
        if (SelectedSkill is null)
        {
            return;
        }

        var selectedId = _selectedTaskBySkill.TryGetValue(SelectedSkill.Id, out var current)
            ? current
            : SelectedSkill.Id.Equals("idling", StringComparison.OrdinalIgnoreCase) ? "idle" : "oak";

        var entry = SelectedSkillTasks.FirstOrDefault(task => task.Id.Equals(selectedId, StringComparison.OrdinalIgnoreCase))
            ?? SelectedSkillTasks.FirstOrDefault(task => task.IsUnlocked);

        if (entry is not null)
        {
            _selectedSkillTask = entry;
            _selectedTaskBySkill[SelectedSkill.Id] = entry.Id;
            OnPropertyChanged(nameof(SelectedSkillTask));
        }
    }

    private int RollMoments()
    {
        var task = _selectedTaskBySkill.TryGetValue("idling", out var mode) ? mode : "idle";
        return task switch
        {
            "flip" => Random.Shared.Next(2, 5),
            "daydream" => Random.Shared.Next(3, 6),
            "nap" => Random.Shared.Next(4, 7),
            "lucid" => Random.Shared.Next(5, 8),
            _ => Random.Shared.Next(1, 4)
        };
    }

    private int GetIdleXp()
    {
        var task = _selectedTaskBySkill.TryGetValue("idling", out var mode) ? mode : "idle";
        return task switch
        {
            "flip" => 20,
            "daydream" => 25,
            "nap" => 30,
            "lucid" => 35,
            _ => 15
        };
    }

    private (string TaskId, string LogItemId) GetSelectedTree()
    {
        var task = _selectedTaskBySkill.TryGetValue("woodcutting", out var tree) ? tree : "oak";
        return task switch
        {
            "willow" => ("willow", "willow-log"),
            "maple" => ("maple", "maple-log"),
            "yew" => ("yew", "yew-log"),
            "magic" => ("magic", "magic-log"),
            _ => ("oak", "oak-log")
        };
    }

    private int RollLogs((string TaskId, string LogItemId) tree)
        => tree.TaskId switch
        {
            "willow" => Random.Shared.Next(1, 4),
            "maple" => Random.Shared.Next(1, 4),
            "yew" => Random.Shared.Next(1, 4),
            "magic" => Random.Shared.Next(1, 4),
            _ => Random.Shared.Next(1, 4)
        };

    private int GetWoodcutXp(string tree)
        => tree switch
        {
            "willow" => 22,
            "maple" => 25,
            "yew" => 28,
            "magic" => 32,
            _ => 18
        };

    private void UpdateSelectedSkillDetails()
    {
        if (SelectedSkill is null)
        {
            SelectedSkillName = "Select a skill";
            SelectedSkillIcon = "✨";
            SelectedSkillIconImage = null;
            SelectedSkillHasIconImage = false;
            SelectedSkillIconWidth = 32;
            SelectedSkillIconHeight = 32;
            SelectedSkillDetails = "";
            SelectedSkillActionLabel = "Train";
            SelectedSkillActionHint = "";
            SelectedSkillActionEnabled = false;
            SelectedSkillUnlocks.Clear();
            SelectedSkillTasks.Clear();
            SelectedSkillTaskHeader = "Mode";
            return;
        }

        var progress = _skillSystem.GetOrCreateProgress(SelectedSkill.Id);
        var nextTotalXp = progress.Level >= SelectedSkill.MaxLevel
            ? progress.XpCurve.GetTotalXpForLevel(SelectedSkill.MaxLevel)
            : progress.XpCurve.GetTotalXpForLevel(progress.Level + 1);
        SelectedSkillName = SelectedSkill.Name;
        SelectedSkillIcon = SelectedSkill.Icon;
        SelectedSkillIconImage = SelectedSkill.IconImage;
        SelectedSkillHasIconImage = SelectedSkill.HasIconImage;
        SelectedSkillIconWidth = SelectedSkill.IconWidth * 2;
        SelectedSkillIconHeight = SelectedSkill.IconHeight * 2;
        SelectedSkillDetails = progress.Level >= SelectedSkill.MaxLevel
            ? $"Level {progress.Level}/{SelectedSkill.MaxLevel} • {progress.TotalXp} XP (Max)"
            : $"Level {progress.Level}/{SelectedSkill.MaxLevel} • {progress.TotalXp}/{nextTotalXp} XP";

        BuildSelectedSkillTasks(progress.Level);

        var isActive = string.Equals(_activeSkillId, SelectedSkill.Id, StringComparison.OrdinalIgnoreCase);
        var verb = SelectedSkill.Id.Equals("idling", StringComparison.OrdinalIgnoreCase) ? "Idle" : "Chop";
        SelectedSkillActionLabel = isActive ? "Stop" : verb;
        var modeName = SelectedSkillTask?.Name ?? "";
        SelectedSkillActionHint = isActive
            ? $"Training {modeName} in the background."
            : SelectedSkill.Id.Equals("idling", StringComparison.OrdinalIgnoreCase)
                ? $"Start {modeName.ToLowerInvariant()} to gain XP each tick."
                : $"Start {modeName.ToLowerInvariant()} to gain XP and resources each tick.";
        SelectedSkillActionEnabled = !LockValuesEnabled;

        SelectedSkillUnlocks.Clear();
        foreach (var unlock in progress.Unlocks)
        {
            SelectedSkillUnlocks.Add(unlock);
        }
    }

    private Guid ProfileId => Guid.Parse("11111111-1111-1111-1111-111111111111");

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    private sealed record DemoState(
        long Gold,
        int Logs,
        int Level,
        IReadOnlyDictionary<string, ActionCooldownState> ActionCooldowns,
        IReadOnlyDictionary<string, List<StatusEffect>> CombatEffects);

    private sealed record SkillPackEntry(
        string Id,
        string Name,
        string Icon,
        int MaxLevel,
        Dictionary<string, string>? Unlocks);

    private sealed record ImageAssetEntry(
        string Id,
        string Path,
        string Type,
        int? RenderWidth,
        int? RenderHeight);

    public sealed class SkillEntry : INotifyPropertyChanged
    {
        private string _name = string.Empty;
        private string _icon = string.Empty;
        private Bitmap? _iconImage;
        private int _iconWidth = 16;
        private int _iconHeight = 16;
        private bool _hasIconImage;
        private bool _isActive;
        private int _level;
        private long _totalXp;
        private int _maxLevel;
        private double _progressRatio;
        private double _targetProgressRatio;
        private bool _hasProgressRatio;
        private IReadOnlyCollection<string> _unlocks = Array.Empty<string>();

        public SkillEntry(string id)
        {
            Id = id;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public string Id { get; }

        public string Name
        {
            get => _name;
            private set => SetField(ref _name, value);
        }

        public string Icon
        {
            get => _icon;
            private set => SetField(ref _icon, value);
        }

        public Bitmap? IconImage
        {
            get => _iconImage;
            private set => SetField(ref _iconImage, value);
        }

        public int IconWidth
        {
            get => _iconWidth;
            private set => SetField(ref _iconWidth, value);
        }

        public int IconHeight
        {
            get => _iconHeight;
            private set => SetField(ref _iconHeight, value);
        }

        public bool HasIconImage
        {
            get => _hasIconImage;
            private set
            {
                if (SetField(ref _hasIconImage, value))
                {
                    OnPropertyChanged(nameof(HasEmojiIcon));
                }
            }
        }

        public bool IsActive
        {
            get => _isActive;
            private set
            {
                if (SetField(ref _isActive, value))
                {
                    OnPropertyChanged(nameof(IsActiveSkill));
                }
            }
        }

        public int Level
        {
            get => _level;
            private set
            {
                if (SetField(ref _level, value))
                {
                    OnPropertyChanged(nameof(LevelText));
                }
            }
        }

        public long TotalXp
        {
            get => _totalXp;
            private set
            {
                if (SetField(ref _totalXp, value))
                {
                    OnPropertyChanged(nameof(LevelText));
                }
            }
        }

        public int MaxLevel
        {
            get => _maxLevel;
            private set => SetField(ref _maxLevel, value);
        }

        public double ProgressRatio
        {
            get => _progressRatio;
            private set => SetField(ref _progressRatio, value);
        }

        public IReadOnlyCollection<string> Unlocks
        {
            get => _unlocks;
            private set => SetField(ref _unlocks, value);
        }

        public string LevelText => $"L{Level} • {TotalXp} XP";
        public bool IsActiveSkill => IsActive;
        public bool HasEmojiIcon => !HasIconImage;

        public void Update(
            string name,
            string icon,
            Bitmap? iconImage,
            int iconWidth,
            int iconHeight,
            bool hasIconImage,
            bool isActive,
            int level,
            long totalXp,
            int maxLevel,
            double targetProgressRatio,
            IReadOnlyCollection<string> unlocks)
        {
            Name = name;
            Icon = icon;
            IconImage = iconImage;
            IconWidth = iconWidth;
            IconHeight = iconHeight;
            HasIconImage = hasIconImage;
            IsActive = isActive;
            Level = level;
            TotalXp = totalXp;
            MaxLevel = maxLevel;
            Unlocks = unlocks;

            _targetProgressRatio = targetProgressRatio;
            if (!_hasProgressRatio)
            {
                ProgressRatio = targetProgressRatio;
                _hasProgressRatio = true;
            }
        }

        public bool SmoothProgress(double smoothing)
        {
            var next = ProgressRatio + ((_targetProgressRatio - ProgressRatio) * smoothing);
            var clamped = Math.Clamp(next, 0, 1);
            if (Math.Abs(clamped - ProgressRatio) < 0.0001)
            {
                return false;
            }

            ProgressRatio = clamped;
            return true;
        }

        private bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
            {
                return false;
            }

            field = value;
            OnPropertyChanged(propertyName);
            return true;
        }

        private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
    public sealed record SkillTaskEntry(
        string Id,
        string Name,
        int RequiredLevel,
        string Reward,
        bool IsUnlocked,
        bool IsSelected)
    {
        public string Detail => $"Level {RequiredLevel} • {Reward}";
        public bool IsLocked => !IsUnlocked;
    }

    public sealed class DebugAdjustEntry : INotifyPropertyChanged
    {
        private string _valueText;

        public DebugAdjustEntry(string name, string valueText, RelayCommand addCommand, RelayCommand removeCommand)
        {
            Name = name;
            _valueText = valueText;
            AddCommand = addCommand;
            RemoveCommand = removeCommand;
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        public string Name { get; }

        public string ValueText
        {
            get => _valueText;
            set
            {
                if (_valueText == value)
                {
                    return;
                }

                _valueText = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ValueText)));
            }
        }

        public RelayCommand AddCommand { get; }
        public RelayCommand RemoveCommand { get; }
    }

    public sealed class DebugSkillEntry
    {
        public DebugSkillEntry(string name, string valueText, RelayCommand addSmallCommand, RelayCommand addLargeCommand)
        {
            Name = name;
            ValueText = valueText;
            AddSmallCommand = addSmallCommand;
            AddLargeCommand = addLargeCommand;
        }

        public string Name { get; }
        public string ValueText { get; }
        public RelayCommand AddSmallCommand { get; }
        public RelayCommand AddLargeCommand { get; }
    }

    private sealed class IdleActionHandler : IActionHandler
    {
        private readonly DemoViewModel _vm;

        public IdleActionHandler(DemoViewModel vm)
        {
            _vm = vm;
        }

        public string ActionId => "idle";

        public ActionResult Execute(ActionContext context, TimeSpan delta)
        {
            var xp = _vm.GetIdleXp();
            _vm._skillSystem.AddXp("idling", xp);
            var moments = _vm.RollMoments();
            _vm._inventoryService.AddItem(context.ProfileId, "moment", moments);
            return new ActionResult(true, "idled");
        }
    }

    private sealed class WoodcutActionHandler : IActionHandler
    {
        private readonly DemoViewModel _vm;

        public WoodcutActionHandler(DemoViewModel vm)
        {
            _vm = vm;
        }

        public string ActionId => "woodcut";

        public ActionResult Execute(ActionContext context, TimeSpan delta)
        {
            var tree = _vm.GetSelectedTree();
            var xp = _vm.GetWoodcutXp(tree.TaskId);
            _vm._skillSystem.AddXp("woodcutting", xp);
            var logs = _vm.RollLogs(tree);
            _vm._inventoryService.AddItem(context.ProfileId, tree.LogItemId, logs);
            _vm._walletService.Credit(context.ProfileId, "gold", 1);
            _vm._questService.AddProgress(context.ProfileId, "woodcut", 1);
            _vm._achievementService.AddProgress(context.ProfileId, "wealth", 1);
            _vm._collectionService.AddItem(context.ProfileId, "starter", "oak-log");
            return new ActionResult(true, "woodcut");
        }
    }
}
