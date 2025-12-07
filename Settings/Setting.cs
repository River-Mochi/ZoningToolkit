// Settings/Setting.cs
// Options UI for ZoneTools – About tab only (name, version, description).

using Colossal.IO.AssetDatabase; // [FileLocation]
using Game.Modding;
using Game.Settings;

namespace ZoningToolkit
{
    [FileLocation("ModsSettings/ZoneTools/ZoneTools")]
    [SettingsUITabOrder(kAboutTab)]
    [SettingsUIGroupOrder(kAboutInfoGroup)]
    [SettingsUIShowGroupName(kAboutInfoGroup)]
    public sealed class Setting : ModSetting
    {
        // Tabs
        public const string kAboutTab = "About";

        // Groups
        public const string kAboutInfoGroup = "Info";

        public Setting(IMod mod)
            : base(mod)
        {
        }

        public override void SetDefaults()
        {
            // Nothing to reset yet – all fields are read-only display values.
        }

        // ====== ABOUT TAB =====================================================

        [SettingsUISection(kAboutTab, kAboutInfoGroup)]
        public string ModName => Mod.ModName;

        [SettingsUISection(kAboutTab, kAboutInfoGroup)]
        public string ModVersion => Mod.ModVersion;

        [SettingsUISection(kAboutTab, kAboutInfoGroup)]
        public string Description =>
            "ZoneTools lets you enable or disable zoning on each side of roads (left, right, both, or none).";
    }
}
