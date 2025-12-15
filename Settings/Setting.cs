// Settings/Setting.cs
// Options UI for Zone Tools – Actions + About (name/version/link) + keybinding.

namespace ZoningToolkit
{
    using System;
    using Colossal.IO.AssetDatabase;
    using Game.Input;
    using Game.Modding;
    using Game.Settings;
    using UnityEngine; // Application

    [FileLocation("ModsSettings/ZoneTools/ZoneTools")]
    [SettingsUITabOrder(kActionsTab, kAboutTab)]
    [SettingsUIGroupOrder(kActionsGroup, kBindingsGroup, kAboutGroup, kAboutLinksGroup)]
    // Only show "Links" header on the About tab (no redundant "About" header).
    [SettingsUIShowGroupName(kAboutLinksGroup)]
    [SettingsUIKeyboardAction(Mod.kTogglePanelActionName, ActionType.Button, usages: new[] { "Game" })]
    public sealed class Setting : ModSetting
    {
        // Tabs
        public const string kActionsTab = "Actions";
        public const string kAboutTab = "About";

        // Groups
        public const string kActionsGroup = "Actions";
        public const string kBindingsGroup = "Key bindings";
        public const string kAboutGroup = "About";
        public const string kAboutLinksGroup = "Links";

        // Defaults (first install)
        // NOTE: property initializers are what makes first-install defaults show up.
        private const bool kDefaultAutoOpenPanelForRoadTools = true;
        private const bool kDefaultProtectOccupiedCells = true;
        private const bool kDefaultProtectZonedCells = false;

        // External links
        private const string kUrlParadox =
            "https://mods.paradoxplaza.com/authors/River-mochi/cities_skylines_2?games=cities_skylines_2&orderBy=desc&sortBy=best&time=alltime";

        public Setting(IMod mod)
            : base(mod)
        {
        }

        public override void SetDefaults()
        {
            AutoOpenPanelForRoadTools = kDefaultAutoOpenPanelForRoadTools;
            ProtectOccupiedCells = kDefaultProtectOccupiedCells;
            ProtectZonedCells = kDefaultProtectZonedCells;

            // Ensure binding has a concrete value when resetting.
            TogglePanelBinding = new ProxyBinding { };
        }

        // ----- ABOUT TAB -----

        [SettingsUISection(kAboutTab, kAboutGroup)]
        public string ModName => Mod.ModName;

        [SettingsUISection(kAboutTab, kAboutGroup)]
        public string ModVersion => Mod.ModVersion;

        [SettingsUIButtonGroup(kAboutLinksGroup)]
        [SettingsUIButton]
        [SettingsUISection(kAboutTab, kAboutLinksGroup)]
        public bool OpenParadox
        {
            set
            {
                if (!value)
                {
                    return;
                }

                TryOpenUrl(kUrlParadox);
            }
        }

        // ----- ACTIONS TAB -----

        [SettingsUISection(kActionsTab, kActionsGroup)]
        public bool AutoOpenPanelForRoadTools { get; set; } = kDefaultAutoOpenPanelForRoadTools;

        [SettingsUISection(kActionsTab, kActionsGroup)]
        public bool ProtectOccupiedCells { get; set; } = kDefaultProtectOccupiedCells;

        [SettingsUISection(kActionsTab, kActionsGroup)]
        public bool ProtectZonedCells { get; set; } = kDefaultProtectZonedCells;

        // ----- KEYBINDINGS (Actions tab) -----

        [SettingsUISection(kActionsTab, kBindingsGroup)]
        [SettingsUIKeyboardBinding(BindingKeyboard.Z, Mod.kTogglePanelActionName, shift: true)]
        public ProxyBinding TogglePanelBinding { get; set; } = new ProxyBinding { };

        // ----- Helpers -----

        private static void TryOpenUrl(string url)
        {
            try
            {
                Application.OpenURL(url);
            }
            catch (Exception)
            {
                // ignore
            }
        }
    }
}
