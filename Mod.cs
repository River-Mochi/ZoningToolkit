// Mod.cs
// Entry point for Zone Tools – logging, settings, localization, systems, and Shift+Z panel hotkey.

namespace ZoningToolkit
{
    using System;
    using System.Reflection;
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

        // Active settings (Options UI)
        public static Setting? Settings
        {
            get; private set;
        }

        // The ProxyAction for Shift+Z (rebindable). Used by ZoneToolKeybindSystem.
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

            GameManager? gm = GameManager.instance;
            LocalizationManager? lm = gm?.localizationManager;
            lm?.AddSource("en-US", new LocaleEN(setting));

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
            updateSystem.UpdateAt<ZoningToolkitModToolSystem>(SystemUpdatePhase.ToolUpdate);

            // Core zoning logic that applies the selected zoning mode to blocks.
            updateSystem.UpdateAt<ZoningToolkitModSystem>(SystemUpdatePhase.Modification4B);

            // Cohtml UI bridge (C# <-> React panel).
            updateSystem.UpdateAt<ZoningToolkitModUISystem>(SystemUpdatePhase.UIUpdate);

            // Hotkey system – listens to Shift+Z and toggles the panel.
            updateSystem.UpdateAt<ZoneToolKeybindSystem>(SystemUpdatePhase.ToolUpdate);
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
    }
}
