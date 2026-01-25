using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
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

    private SceneFrame _previousScene = new();
    private SceneFrame _currentScene = new();
    private int _tickIndex;
    private string _sandboxStatus = "Disabled";
    private string _saveStatus = "";

    public DemoViewModel()
    {
        DemoLogger.Info("demo", "viewmodel-ctor");
        _clock = new SimulationClock(1);
        _scheduler = new TickScheduler(_clock);
        _offlineReconciler = new OfflineReconciler(_scheduler);

        var actionDefinitions = new ActionDefinition("gather", "Gather", TimeSpan.FromSeconds(1), TimeSpan.Zero, new[] { "gather" });
        _actionRegistry = new ActionRegistry();
        _actionRegistry.RegisterDefinition(actionDefinitions);
        _actionRunner = new ActionRunner(_actionRegistry);

        var skillRegistry = new SkillRegistry();
        skillRegistry.Register(new SkillDefinition("gathering", "Gathering", 10, new Dictionary<int, string> { [2] = "Unlocked: Faster gather" }));
        _skillSystem = new SkillSystem(skillRegistry, new LinearXpCurve(50, 25));

        var questRegistry = new QuestRegistry();
        questRegistry.Register(new QuestDefinition("gather", "Gather Logs", "Collect 5 logs", 5));
        _questService = new QuestService(questRegistry);

        var achievementRegistry = new AchievementRegistry();
        achievementRegistry.Register(new AchievementDefinition("wealth", "Wealth", "Earn 10 gold", 10));
        _achievementService = new AchievementService(achievementRegistry);

        var collectionRegistry = new CollectionRegistry();
        collectionRegistry.Register(new CollectionDefinition("starter", "Starter Set", new[] { "log", "sword", "shield" }));
        _collectionService = new CollectionService(collectionRegistry);

        var itemRegistry = new ItemRegistry();
        itemRegistry.Register(new ItemDefinition("log", "Log", true));
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
        _craftingService.Register(new RecipeDefinition("plank", "Plank", new Dictionary<string, int> { ["log"] = 2 }, "log", 1));

        _tradeService = new TradeService(_inventoryService, _walletService);
        _tradeService.ListOffer(new TradeOffer("offer-log", "log", 1, 2));

        _bestiaryRegistry.Register(new BestiaryEntry("slime", "Slime", "A weak creature"));
        _itemCompendiumRegistry.Register(new ItemCompendiumEntry("log", "Log", "Basic wood"));

        _combatSystem = new CombatSystem();
        _combatEncounter = new CombatEncounter(new List<CombatantState>
        {
            new("player", new CombatantStats(20, 5, 1)),
            new("enemy", new CombatantStats(12, 3, 0))
        }, new SimpleCombatAi());
        _combatEncounter.Combatants[0].Effects.Add(new StatusEffect("battle-focus", 3, 1, 0));

        _audioService = new AudioService(new AudioRegistry());
        _audioService.Mixer.SetMaster(0.8f);

        var snapshotPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "idle-sdk-demo.db");
        var snapshotStore = new SqliteSnapshotStore($"Data Source={snapshotPath}");
        _snapshotService = new SnapshotService<DemoState>(snapshotStore, new JsonStateSerializer<DemoState>());

        SeedWorld();
        SeedEquipment();
        SeedScene();
        ConfigureActions();
        ConfigureSandbox();
        ConfigureHotkeys();
        _sandboxConsole.Enable();
        SandboxStatus = "Enabled";
        UpdateAllDisplays();

        TickCommand = new RelayCommand(TickOnce);
        OfflineCommand = new RelayCommand(() => RunOffline(TimeSpan.FromSeconds(10)));
        ToggleSandboxCommand = new RelayCommand(ToggleSandbox);
        SandboxAddGoldCommand = new RelayCommand(() => ExecuteSandbox("add-gold"));
        SaveCommand = new RelayCommand(SaveSnapshot);
        LoadCommand = new RelayCommand(LoadSnapshot);
        DemoLogger.Info("demo", "viewmodel-ready");
    }

    public ObservableCollection<string> SkillLines { get; } = new();
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

    public RelayCommand TickCommand { get; }
    public RelayCommand OfflineCommand { get; }
    public RelayCommand ToggleSandboxCommand { get; }
    public RelayCommand SandboxAddGoldCommand { get; }
    public RelayCommand SaveCommand { get; }
    public RelayCommand LoadCommand { get; }

    public string SandboxStatus
    {
        get => _sandboxStatus;
        private set
        {
            _sandboxStatus = value;
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
        _actionRegistry.RegisterHandler(new GatherActionHandler(this));
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
            DemoLogger.Info("demo", "offline", new { durationSeconds = duration.TotalSeconds });
            _offlineReconciler.Reconcile(new OfflineReconcileRequest(ProfileId, DateTimeOffset.UtcNow - duration, DateTimeOffset.UtcNow), _ => ExecuteAction(), duration);
            UpdateAllDisplays();
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
            _tickIndex++;
            DemoLogger.Info("demo", "execute-action", new { tick = _tickIndex });
            var actionResult = _actionRunner.Step("gather", new ActionContext(ProfileId, DateTimeOffset.UtcNow), TimeSpan.FromSeconds(1));
            DemoLogger.Info("demo", "action-result", new { action = "gather", actionResult.Completed, actionResult.Output });
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
    }

    private void UpdateSkills()
    {
        SkillLines.Clear();
        var progress = _skillSystem.GetOrCreateProgress("gathering");
        SkillLines.Add($"Gathering L{progress.Level} ({progress.TotalXp} XP)");
        if (progress.Unlocks.Count > 0)
        {
            SkillLines.Add($"Unlocks: {string.Join(", ", progress.Unlocks)}");
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
        var progress = _questService.GetOrCreate(ProfileId, "gather");
        QuestLines.Add($"Gather Logs: {progress.CurrentValue}/5");
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
        CraftingLines.Add("Recipe: 2x log -> 1x log (demo)");
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
                inventory.GetQuantity("log"),
                _skillSystem.GetOrCreateProgress("gathering").Level,
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
            var logCount = inventory.GetQuantity("log");
            if (loaded.Logs > logCount)
            {
                _inventoryService.AddItem(ProfileId, "log", loaded.Logs - logCount);
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

    private Guid ProfileId => Guid.Parse("11111111-1111-1111-1111-111111111111");

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
        => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

    private sealed record DemoState(
        long Gold,
        int Logs,
        int Level,
        IReadOnlyDictionary<string, ActionCooldownState> ActionCooldowns,
        IReadOnlyDictionary<string, List<StatusEffect>> CombatEffects);

    private sealed class GatherActionHandler : IActionHandler
    {
        private readonly DemoViewModel _vm;

        public GatherActionHandler(DemoViewModel vm)
        {
            _vm = vm;
        }

        public string ActionId => "gather";

        public ActionResult Execute(ActionContext context, TimeSpan delta)
        {
            _vm._skillSystem.AddXp("gathering", 25);
            _vm._inventoryService.AddItem(context.ProfileId, "log", 1);
            _vm._walletService.Credit(context.ProfileId, "gold", 1);
            _vm._questService.AddProgress(context.ProfileId, "gather", 1);
            _vm._achievementService.AddProgress(context.ProfileId, "wealth", 1);
            _vm._collectionService.AddItem(context.ProfileId, "starter", "log");
            return new ActionResult(true, "gathered");
        }
    }
}
