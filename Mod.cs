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

            lm?.AddSource("en-US", new LocaleEN(setting));

            // FileLocation path is "ModsSettings/ZoneTools/ZoneTools"
            AssetDatabase.global.LoadSettings("ZoneTools", setting, new Setting(this));
            setting.RegisterInOptionsUI();

            // ----- System registration -----
            updateSystem.UpdateAt<ZoningToolkitModToolSystem>(SystemUpdatePhase.ToolUpdate);
            updateSystem.UpdateAt<ZoningToolkitModSystem>(SystemUpdatePhase.Modification4B);
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
        }
    }
}
