// Settings/Setting.cs
// Options UI for Zone Tools – About info + keyboard shortcut + UI behaviour toggle.

namespace ZoningToolkit
{
    using Colossal.IO.AssetDatabase;
    using Game.Input;
    using Game.Modding;
    using Game.Settings;

    [FileLocation("ModsSettings/ZoneTools/ZoneTools")]
    [SettingsUITabOrder(kAboutTab)]
    [SettingsUIGroupOrder(kAboutGroup, kUiGroup, kBindingsGroup)]
    [SettingsUIShowGroupName(kAboutGroup)]
    // keyboard action (CO InputManager)
    [SettingsUIKeyboardAction(Mod.kTogglePanelActionName, ActionType.Button, usages: new[] { "Game" })]
    public sealed class Setting : ModSetting
    {
        // Tabs
        public const string kAboutTab = "About";

        // Groups
        public const string kAboutGroup = "About";
        public const string kUiGroup = "UI";
        public const string kBindingsGroup = "Key bindings";

        public Setting(IMod mod)
            : base(mod)
        {
        }

        public override void SetDefaults()
        {
            // Keep existing behaviour by default (panel auto-opens when selecting zonable road tools).
            AutoOpenPanelForRoadTools = true;
        }

        // ----- ABOUT TAB ------------------------------------------------------

        [SettingsUISection(kAboutTab, kAboutGroup)]
        public string ModName => Mod.ModName;

        [SettingsUISection(kAboutTab, kAboutGroup)]
        public string ModVersion => Mod.ModVersion;

        // ----- UI ------------------------------------------------------------

        [SettingsUISection(kAboutTab, kUiGroup)]
        public bool AutoOpenPanelForRoadTools
        {
            get; set;
        }

        // ----- KEYBINDINGS ----------------------------------------------------
        // Default: Shift+Z; user can rebind in Options → Mods and Keybindings.

        [SettingsUISection(kAboutTab, kBindingsGroup)]
        [SettingsUIKeyboardBinding(BindingKeyboard.Z, Mod.kTogglePanelActionName, shift: true)]
        public ProxyBinding TogglePanelBinding
        {
            get; set;
        }
    }
}
