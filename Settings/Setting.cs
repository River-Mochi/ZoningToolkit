// Settings/Setting.cs
// Options UI for ZoneTools – About info + keyboard shortcut for the update tool.

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
    public sealed class Setting : ModSetting
    {
        // Tabs
        public const string kAboutTab = "About";

        // Groups
        public const string kAboutGroup = "About";
        public const string kBindingsGroup = "Bindings";

        public Setting(IMod mod)
            : base(mod)
        {
        }

        public override void SetDefaults()
        {
            // Nothing special to reset yet.
        }

        // ----- ABOUT TAB ------------------------------------------------------

        [SettingsUISection(kAboutTab, kAboutGroup)]
        public string ModName => Mod.ModName;

        [SettingsUISection(kAboutTab, kAboutGroup)]
        public string ModVersion => Mod.ModVersion;

        // ----- KEYBINDINGS ----------------------------------------------------
        // Rebindable shortcut that toggles the ZoneTools update tool (existing roads).
        // Default: Shift+G

        [SettingsUISection(kAboutTab, kBindingsGroup)]
        [SettingsUIKeyboardBinding(BindingKeyboard.G, Mod.kToggleUpdateToolBindingName, shift: true)]
        public ProxyBinding ToggleUpdateToolBinding { get; set; }
    }
}
