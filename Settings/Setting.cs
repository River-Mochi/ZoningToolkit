// Settings/Setting.cs
// Options UI for Zone Tools – About info + keyboard shortcut for the Zone Tools panel.

namespace ZoningToolkit
{
    using Colossal.IO.AssetDatabase;
    using Game.Input;
    using Game.Modding;
    using Game.Settings;

    [FileLocation("ModsSettings/ZoneTools/ZoneTools")]
    [SettingsUITabOrder(kAboutTab)]
    [SettingsUIGroupOrder(kAboutGroup, kBindingsGroup)]
    [SettingsUIShowGroupName(kAboutGroup)]

    // Declare the keyboard action (CO InputManager action).
    [SettingsUIKeyboardAction(Mod.kTogglePanelActionName, ActionType.Button, usages: new[] { "Game" })]
    public sealed class Setting : ModSetting
    {
        // Tabs
        public const string kAboutTab = "About";

        // Groups
        public const string kAboutGroup = "About";
        public const string kBindingsGroup = "Key bindings";

        public Setting(IMod mod)
            : base(mod)
        {
        }

        public override void SetDefaults()
        {
            // No saved options yet.
        }

        // ----- ABOUT TAB ------------------------------------------------------

        [SettingsUISection(kAboutTab, kAboutGroup)]
        public string ModName => Mod.ModName;

        [SettingsUISection(kAboutTab, kAboutGroup)]
        public string ModVersion => Mod.ModVersion;

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
