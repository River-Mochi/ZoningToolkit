// Mod.cs
// Entry point for Zone Tools – logging, settings, localization, systems, and Shift+Z panel hotkey.

namespace ZoningToolkit
{
    using System;
    using System.Reflection;
    using Colossal;                       // IDictionarySource
    using Colossal.IO.AssetDatabase;
    using Colossal.Localization;
    using Colossal.Logging;
    using Game;
    using Game.Input;
    using Game.Modding;
    using Game.SceneFlow;
    using ZoningToolkit.Systems;

    public sealed class Mod : IMod
    {
        // Metadata
        public const string ModName = "Zone Tools";
        public const string ModId = "ZoneTools";
        public const string ModTag = "[ZT]";

        // Version (3-part) from assembly
        public static readonly string ModVersion =
            Assembly.GetExecutingAssembly().GetName().Version?.ToString(3) ?? "1.0.0";

        // CO InputManager action ID for the panel toggle keybinding
        public const string kTogglePanelActionName = "ZoneToolsTogglePanel";

        // Once-only banner flag
        private static bool s_BannerLogged;

        // CO-style logger
        public static readonly ILog s_Log = LogManager.GetLogger(ModId);

        /// <summary>
        /// Debug logging flag for verbose logs (UI events, tool chatter, etc.).
        /// In DEBUG builds this defaults to true; in RELEASE builds it defaults to false.
        /// Can flip this at runtime if code added to expose it via settings toggle.
        /// </summary>
        public static bool DebugLoggingEnabled
        {
            get; set;
        } =
#if DEBUG
            true;
#else
            false;
#endif

        /// <summary>
        /// Helper for verbose debug logging routed through the CO logger.
        /// This keeps ZoneTools.log much smaller in Release builds.
        /// </summary>
        public static void Debug(string message)
        {
            if (!DebugLoggingEnabled)
            {
                return;
            }

            s_Log.Info(message);
        }

        // Active settings (Options UI)
        public static Setting? Settings
        {
            get; private set;
        }

        // The ProxyAction for Shift+Z (rebindable). Used by ZoneToolSystemKeybind.
        public static ProxyAction? TogglePanelAction
        {
            get; private set;
        }

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
            // Once-only log banner
            if (!s_BannerLogged)
            {
                s_BannerLogged = true;
                s_Log.Info($"{ModName} {ModTag} v{ModVersion} OnLoad");
            }

            // ----- Settings + localization -----------------------------------

            var setting = new Setting(this);
            Settings = setting;

            // Register languages via helper
            AddLocaleSource("en-US", new LocaleEN(setting));
            AddLocaleSource("fr-FR", new LocaleFR(setting));
            AddLocaleSource("es-ES", new LocaleES(setting));
            AddLocaleSource("de-DE", new LocaleDE(setting));
            AddLocaleSource("it-IT", new LocaleIT(setting));
            AddLocaleSource("ja-JP", new LocaleJA(setting));
            AddLocaleSource("ko-KR", new LocaleKO(setting));
            AddLocaleSource("pl-PL", new LocalePL(setting));
            AddLocaleSource("pt-BR", new LocalePT_BR(setting));
            AddLocaleSource("zh-HANS", new LocaleZH_CN(setting));        // Simplified Chinese
            AddLocaleSource("zh-HANT", new LocaleZH_HANT(setting));      // Traditional Chinese

            // Load saved values, then register Options UI.
            AssetDatabase.global.LoadSettings(ModId, setting, new Setting(this));
            setting.RegisterInOptionsUI();

            // Keybindings (CO wiki pattern: KeyboardAction + KeyboardBinding)
            try
            {
                setting.RegisterKeyBindings();

                TogglePanelAction = setting.GetAction(kTogglePanelActionName);
                if (TogglePanelAction != null)
                {
                    TogglePanelAction.shouldBeEnabled = true;
                    s_Log.Info($"{ModTag} Keybinding '{kTogglePanelActionName}' enabled (default Shift+Z).");
                }
                else
                {
                    s_Log.Warn($"{ModTag} Keybinding action '{kTogglePanelActionName}' not found.");
                }
            }
            catch (Exception ex)
            {
                s_Log.Warn($"{ModTag} Keybinding setup skipped: {ex.GetType().Name}: {ex.Message}");
            }

            // ----- ECS systems -----------------------------------------------

            // Tool for updating existing-road zoning (used by the "Update Tool" icon).
            updateSystem.UpdateAt<ZoneToolSystemExistingRoads>(SystemUpdatePhase.ToolUpdate);

            // Core zoning logic that applies the selected zoning mode to blocks.
            updateSystem.UpdateAt<ZoneToolSystemCore>(SystemUpdatePhase.Modification4B);

            // Cohtml UI bridge (C# <-> React panel).
            updateSystem.UpdateAt<ZoneToolBridgeUI>(SystemUpdatePhase.UIUpdate);

            // Hotkey system – listens to Shift+Z and toggles the panel.
            updateSystem.UpdateAt<ZoneToolSystemKeybind>(SystemUpdatePhase.ToolUpdate);
        }

        public void OnDispose()
        {
            s_Log.Info(nameof(OnDispose));

            if (TogglePanelAction != null)
            {
                TogglePanelAction.shouldBeEnabled = false;
                TogglePanelAction = null;
            }

            if (Settings != null)
            {
                Settings.UnregisterInOptionsUI();
                Settings = null;
            }
        }

        // --------------------------------------------------------------------
        // Localization helper
        // --------------------------------------------------------------------

        /// <summary>
        /// Wrapper for LocalizationManager.AddSource that catches exceptions
        /// so localization issues can't break mod loading.
        /// </summary>
        private static void AddLocaleSource(string localeId, IDictionarySource source)
        {
            if (string.IsNullOrEmpty(localeId))
            {
                return;
            }

            LocalizationManager? lm = GameManager.instance?.localizationManager;
            if (lm == null)
            {
                s_Log.Warn($"AddLocaleSource: No LocalizationManager; cannot add source for '{localeId}'.");
                return;
            }

            try
            {
                lm.AddSource(localeId, source);
            }
            catch (Exception ex)
            {
                s_Log.Warn($"AddLocaleSource: AddSource for '{localeId}' failed: {ex.GetType().Name}: {ex.Message}");
            }
        }
    }
}
