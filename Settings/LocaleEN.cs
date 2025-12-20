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
                { m_Setting.GetOptionTabLocaleID(Setting.kActionsTab), "Actions" },
                { m_Setting.GetOptionTabLocaleID(Setting.kAboutTab),   "About"   },

                // Groups
                { m_Setting.GetOptionGroupLocaleID(Setting.kActionsGroup),     "Actions"      },
                { m_Setting.GetOptionGroupLocaleID(Setting.kBindingsGroup),    "Key bindings" },
                { m_Setting.GetOptionGroupLocaleID(Setting.kAboutGroup),       "About"        },
                { m_Setting.GetOptionGroupLocaleID(Setting.kAboutLinksGroup),  "Links"        },

                // About fields
                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.ModName)),    "Mod name" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.ModName)),     "Display name of this mod." },

                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.VersionText)), "Version" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.VersionText)),  "Current Zone Tools version." },

                // About links
                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.OpenParadox)), "Paradox Mods" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.OpenParadox)),  "Open the author's Paradox Mods page." },

                // Actions toggles
                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.AutoOpenPanelForRoadTools)), "Auto-open Zone Tools with road tools." },
                {
                    m_Setting.GetOptionDescLocaleID(nameof(Setting.AutoOpenPanelForRoadTools)),
                    "**[ ✓ ] enabled**, the **Zone Tools panel** automatically opens/closes when you open/close a zonable road tool.\n\n" +
                    "**[   ] disabled**, panel is opened/closed manually."
                },

                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.ProtectOccupiedCells)), "Protect occupied cells (has buildings)" },
                {
                    m_Setting.GetOptionDescLocaleID(nameof(Setting.ProtectOccupiedCells)),
                    "**[ ✓ ] enabled**, Zone Tools does not change zoning depth/area on cells that already have a building.\n" +
                    "**[   ] disabled**, buildings could be condemned when changing the zoning under them."

                },

                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.ProtectZonedCells)), "Protect zoned-but-empty cells" },
                {
                    m_Setting.GetOptionDescLocaleID(nameof(Setting.ProtectZonedCells)),
                    "**[ ✓ ] enabled**, Zone Tools does not change zoning depth/area on cells that are already zoned (even if empty).\n" +
                    "**[   ] disabled**, already zoned cells (painted RCIO) could be overwritten when using the Zone Tools."
                },

                // Keybinding option (Options → Mods)
                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.TogglePanelBinding)), "Toggle panel" },
                {
                    m_Setting.GetOptionDescLocaleID(nameof(Setting.TogglePanelBinding)),
                    "**Keyboard** shortcut to show or hide the Zone Tools panel (same as clicking the top-left menu icon)."
                },

                // Keybinding name (Options → Keybindings)
                { m_Setting.GetBindingKeyLocaleID(Mod.kTogglePanelActionName), "Zone Tools – Toggle panel" },

                // -----------------------------------------------------------------
                // UI strings (React panel)
                // -----------------------------------------------------------------
                { "ZoneTools.UI.UpdateRoad", "Update Road" },

                { "ZoneTools.UI.Tooltip.UpdateRoad",
                  "Toggle update tool (for existing roads).\n\n" +
                  "If it won’t enable, open any road build tool once." },

                { "ZoneTools.UI.Tooltip.ModeDefault", "Both (default)" },
                { "ZoneTools.UI.Tooltip.ModeLeft",    "Left"          },
                { "ZoneTools.UI.Tooltip.ModeRight",   "Right"         },
                { "ZoneTools.UI.Tooltip.ModeNone",    "None"          }
            };

            return d;
        }

        public void Unload()
        {
            // Nothing to clean up; CS2 manages locale life cycle.
        }
    }
}
