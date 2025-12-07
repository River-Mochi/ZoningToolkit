// Mod.cs
// Entry point for ZoneTools (ZT) – logging, settings, localization, system registration.

namespace ZoningToolkit
{
    using System.Reflection;            // AssemblyVersion
    using Colossal.IO.AssetDatabase;    // AssetDatabase.global
    using Colossal.Localization;        // LocalizationManager
    using Colossal.Logging;             // ILog
    using Game;                         // UpdateSystem
    using Game.Common;                  // SystemUpdatePhase
    using Game.Modding;                 // IMod
    using Game.SceneFlow;               // GameManager
    using ZoningToolkit.Systems;        // ECS systems

    public sealed class Mod : IMod
    {
        // Metadata
        public const string ModName = "ZoneTools";
        public const string ModId = "ZoneTools";
        public const string ModTag = "[ZT]";

        // Version from assembly (3-part)
        public static readonly string ModVersion =
            Assembly.GetExecutingAssembly().GetName().Version?.ToString(3) ?? "1.0.0";

        // Once-only banner
        private static bool s_BannerLogged;

        // Single shared logger for the whole mod (CO-style name)
        public static readonly ILog s_Log = LogManager.GetLogger(ModId);

        // Static ctor to configure logger based on build
        static Mod()
        {
#if DEBUG
            s_Log.SetShowsErrorsInUI(true);
#else
            s_Log.SetShowsErrorsInUI(false);
#endif
        }

        // Active settings (Options UI)
        public static Setting? Settings
        {
            get; private set;
        }

        public void OnLoad(UpdateSystem updateSystem)
        {
            // Once-only log banner
            if (!s_BannerLogged)
            {
                s_BannerLogged = true;
                s_Log.Info($"{ModName} {ModTag} v{ModVersion} OnLoad");
            }

            // ----- Settings + localization -----
            var setting = new Setting(this);
            Settings = setting;

            GameManager? gm = GameManager.instance;
            LocalizationManager? lm = gm?.localizationManager;

            // Add English locale source
            lm?.AddSource("en-US", new LocaleEN(setting));

            // Load saved settings, then show Options UI (simple style like your other mods)
            // FileLocation path is "ModsSettings/ZoneTools/ZoneTools"
            AssetDatabase.global.LoadSettings("ZoneTools", setting, new Setting(this));
            setting.RegisterInOptionsUI();

            // ----- System registration -----
            // Tool: used for existing-road zoning updates / UI-driven tool on/off state.
            updateSystem.UpdateAt<ZoningToolkitModToolSystem>(SystemUpdatePhase.ToolUpdate);

            // Core zoning logic: applies zoning behaviour to blocks.
            updateSystem.UpdateAt<ZoningToolkitModSystem>(SystemUpdatePhase.Modification4B);

            // UI bridge: C# <-> Cohtml/React UI.
            updateSystem.UpdateAt<ZoningToolkitModUISystem>(SystemUpdatePhase.UIUpdate);
        }

        public void OnDispose()
        {
            s_Log.Info(nameof(OnDispose));

            if (Settings != null)
            {
                Settings.UnregisterInOptionsUI();
                Settings = null;
            }

            // Do NOT remove locales here; the game manages the localization manager lifecycle.
        }
    }
}
