// Mod.cs
// Entry point for Zone Tools – logging, settings, localization, systems, and Shift+G hotkey.
namespace ZoningToolkit
{
    using System.Reflection;
    using Colossal.IO.AssetDatabase;
    using Colossal.Localization;
    using Colossal.Logging;
    using Game;
    using Game.Common;
    using Game.Input;
    using Game.Modding;
    using Game.SceneFlow;
    using Unity.Entities;
    using UnityEngine.InputSystem;
    using ZoningToolkit.Systems;

    public sealed class Mod : IMod
    {
        // Metadata
        public const string ModName = "Zone Tools";
        public const string ModId   = "ZoneTools";   // matches mod.json id and assembly name
        public const string ModTag  = "[ZT]";

        // Keybinding action name (used by Setting + Keybindings UI)
        public const string kToggleUpdateToolBindingName = "ToggleUpdateTool";

        // Version from assembly (3-part)
        public static readonly string ModVersion =
            Assembly.GetExecutingAssembly().GetName().Version?.ToString(3) ?? "1.0.0";

        // Once-only banner
        private static bool s_BannerLogged;

        // Shared logger
        public static readonly ILog s_Log = LogManager.GetLogger(ModId);

        // Active settings
        public static Setting? Settings
        {
            get;
            private set;
        }

        // ProxyAction for Shift+G
        private static ProxyAction? s_ToggleUpdateToolAction;

        static Mod()
        {
#if DEBUG
            s_Log.SetShowsErrorsInUI(true);
#else
            s_Log.SetShowsErrorsInUI(false);
#endif
        }

        public void OnLoad(UpdateSystem updateSystem)
        {
            // Once-only banner
            if (!s_BannerLogged)
            {
                s_BannerLogged = true;
                s_Log.Info($"{ModName} {ModTag} v{ModVersion} OnLoad");
            }

            // ----- Settings + localization ------------------------------------

            var setting = new Setting(this);
            Settings = setting;

            GameManager? gameManager = GameManager.instance;
            LocalizationManager? localizationManager = gameManager?.localizationManager;

            // Add English locale
            localizationManager?.AddSource("en-US", new LocaleEN(setting));

            // Load saved settings, then register Options UI.
            // FileLocation attribute handles the ModsSettings/ZoneTools/ZoneTools path.
            AssetDatabase.global.LoadSettings(ModId, setting, new Setting(this));
            setting.RegisterInOptionsUI();

            // Register key bindings so they show in the Keybindings tab.
            setting.RegisterKeyBindings();

            // Get our Shift+G action and hook a handler.
            s_ToggleUpdateToolAction = setting.GetAction(kToggleUpdateToolBindingName);
            if (s_ToggleUpdateToolAction != null)
            {
                s_ToggleUpdateToolAction.shouldBeEnabled = true;
                s_ToggleUpdateToolAction.onInteraction += OnToggleUpdateToolInteraction;
            }

            if (gameManager != null &&
                gameManager.modManager.TryGetExecutableAsset(this, out var asset))
            {
                s_Log.Info($"{ModTag} Asset path: {asset.path}");
            }

            // ----- ECS systems -------------------------------------------------

            // Tool: existing-road zoning editor.
            updateSystem.UpdateAt<ZoningToolkitModToolSystem>(SystemUpdatePhase.ToolUpdate);

            // Core zoning application system (new + updated blocks).
            updateSystem.UpdateAt<ZoningToolkitModSystem>(SystemUpdatePhase.Modification4B);

            // UI bridge: C# <-> Cohtml/React panel + button.
            updateSystem.UpdateAt<ZoningToolkitModUISystem>(SystemUpdatePhase.UIUpdate);

            s_Log.Info($"{ModTag} Systems registered.");
        }

        public void OnDispose()
        {
            s_Log.Info(nameof(OnDispose));

            // Keep OnDispose minimal – just remove Options UI.
            if (Settings != null)
            {
                Settings.UnregisterInOptionsUI();
                Settings = null;
            }

            // We intentionally do NOT explicitly tear down keybindings or locales;
            // CS2 cleans these up when unloading the mod.
        }

        private static void OnToggleUpdateToolInteraction(ProxyAction action, InputActionPhase phase)
        {
            if (phase != InputActionPhase.Performed)
            {
                return;
            }

            try
            {
                World world = World.DefaultGameObjectInjectionWorld;
                if (world == null || !world.IsCreated)
                {
                    return;
                }

                ZoningToolkitModToolSystem? toolSystem =
                    world.GetExistingSystemManaged<ZoningToolkitModToolSystem>();

                if (toolSystem == null)
                {
                    return;
                }

                bool enable = !toolSystem.toolEnabled;
                s_Log.Info($"{ModTag} Hotkey toggle update tool -> {enable}");

                if (enable)
                {
                    toolSystem.EnableTool();
                }
                else
                {
                    toolSystem.DisableTool();
                }
            }
            catch (System.Exception ex)
            {
                s_Log.Error($"{ModTag} ToggleUpdateTool exception: {ex}");
            }
        }
    }
}
