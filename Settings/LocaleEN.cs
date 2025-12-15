// Settings/LocaleEN.cs
// English en-US for Zone Tools.

namespace ZoningToolkit
{
    using System.Collections.Generic;
    using Colossal;

    public sealed class LocaleEN : IDictionarySource
    {
        private readonly Setting m_Setting;

        public LocaleEN(Setting setting)
        {
            m_Setting = setting;
        }

        public IEnumerable<KeyValuePair<string, string>> ReadEntries(
            IList<IDictionaryEntryError> errors,
            Dictionary<string, int> indexCounts)
        {
            var d = new Dictionary<string, string>
            {
                // Options title (single source of truth from Mod.cs)
                { m_Setting.GetSettingsLocaleID(), Mod.ModName + " " + Mod.ModTag },

                // Tabs
                { m_Setting.GetOptionTabLocaleID(Setting.kAboutTab), "About" },

                // Groups
                { m_Setting.GetOptionGroupLocaleID(Setting.kAboutGroup),    "About"        },
                { m_Setting.GetOptionGroupLocaleID(Setting.kBindingsGroup), "Key bindings" },

                // About fields
                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.ModName)),    "Mod name" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.ModName)),     "Display name of this mod." },

                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.ModVersion)), "Version" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.ModVersion)),  "Current Zone Tools version." },

                // Keybinding option (Options → Mods)
                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.TogglePanelBinding)), "Toggle panel" },
                {
                    m_Setting.GetOptionDescLocaleID(nameof(Setting.TogglePanelBinding)),
                    "Keyboard shortcut to show or hide the Zone Tools panel (same as clicking the top-left menu icon)."
                },

                // Keybinding name (Options → Keybindings)
                { m_Setting.GetBindingKeyLocaleID(Mod.kTogglePanelActionName), "Zone Tools – Toggle panel" },

                { m_Setting.GetOptionGroupLocaleID(Setting.kUiGroup), "UI" },

                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.AutoOpenPanelForRoadTools)), "Auto-open panel for road tools" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.AutoOpenPanelForRoadTools)),
                  "When enabled, the Zone Tools panel opens automatically when you select a zonable road tool.\n" +
                  "Turn off to open it manually."
                },

                // -----------------------------------------------------------------
                // UI strings (React panel)
                // -----------------------------------------------------------------
                { "ZoneTools.UI.UpdateRoad", "Update Road" },

                { "ZoneTools.UI.Tooltip.UpdateRoad",
                  "Toggle update tool (for existing roads). Roads with zoned buildings are skipped." },
                { "ZoneTools.UI.Tooltip.ModeDefault", "Default (both)" },
                { "ZoneTools.UI.Tooltip.ModeLeft", "Left" },
                { "ZoneTools.UI.Tooltip.ModeRight", "Right" },
                { "ZoneTools.UI.Tooltip.ModeNone", "None" }
            };

            return d;
        }

        public void Unload()
        {
            // Nothing to clean up; CS2 manages locale life cycle.
        }
    }
}
