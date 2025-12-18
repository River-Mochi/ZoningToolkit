// Settings/LocaleDE.cs
// German de-DE for Zone Tools.

namespace ZoningToolkit
{
    using System.Collections.Generic;
    using Colossal;

    public sealed class LocaleDE : IDictionarySource
    {
        private readonly Setting m_Setting;

        public LocaleDE(Setting setting)
        {
            m_Setting = setting;
        }

        public IEnumerable<KeyValuePair<string, string>> ReadEntries(
            IList<IDictionaryEntryError> errors,
            Dictionary<string, int> indexCounts)
        {
            var d = new Dictionary<string, string>
            {
                { m_Setting.GetSettingsLocaleID(), Mod.ModName + " " + Mod.ModTag },

                // Tabs
                { m_Setting.GetOptionTabLocaleID(Setting.kActionsTab), "Aktionen" },
                { m_Setting.GetOptionTabLocaleID(Setting.kAboutTab),   "Info" },

                // Groups
                { m_Setting.GetOptionGroupLocaleID(Setting.kActionsGroup),     "Aktionen" },
                { m_Setting.GetOptionGroupLocaleID(Setting.kBindingsGroup),    "Tastenbelegung" },
                { m_Setting.GetOptionGroupLocaleID(Setting.kAboutGroup),       "" },
                { m_Setting.GetOptionGroupLocaleID(Setting.kAboutLinksGroup),  "Links" },

                // About fields
                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.ModName)),    "Mod-Name" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.ModName)),     "Anzeigename dieses Mods." },

                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.VersionText)), "Version" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.VersionText)),  "Aktuelle Version von Zone Tools." },

                // About links
                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.OpenParadox)), "Paradox Mods" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.OpenParadox)),  "Autorenseite auf Paradox Mods öffnen." },

                // Actions toggles
                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.AutoOpenPanelForRoadTools)), "Zone Tools mit Straßentools automatisch öffnen." },
                {
                    m_Setting.GetOptionDescLocaleID(nameof(Setting.AutoOpenPanelForRoadTools)),
                    "Wenn aktiviert, öffnet sich das Zone-Tools-Panel automatisch bei zonierbaren Straßentools.\n" +
                    "Deaktivieren, um das Panel manuell zu öffnen."
                },

                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.ProtectOccupiedCells)), "Belegte Zellen schützen (Gebäude)" },
                {
                    m_Setting.GetOptionDescLocaleID(nameof(Setting.ProtectOccupiedCells)),
                    "Wenn aktiviert, ändert Zone Tools keine Zonierungstiefe/-fläche bei Zellen, die bereits ein Gebäude haben."
                },

                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.ProtectZonedCells)), "Zonierte, aber leere Zellen schützen" },
                {
                    m_Setting.GetOptionDescLocaleID(nameof(Setting.ProtectZonedCells)),
                    "Wenn aktiviert, ändert Zone Tools keine Zonierungstiefe/-fläche bei bereits zonierten Zellen (auch wenn leer)."
                },

                // Keybinding option
                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.TogglePanelBinding)), "Panel umschalten" },
                {
                    m_Setting.GetOptionDescLocaleID(nameof(Setting.TogglePanelBinding)),
                    "Tastenkürzel zum Ein-/Ausblenden des Zone-Tools-Panels (wie das Symbol oben links)."
                },

                // Keybinding name
                { m_Setting.GetBindingKeyLocaleID(Mod.kTogglePanelActionName), "Zone Tools – Panel umschalten" },

                // React panel
                { "ZoneTools.UI.UpdateRoad", "Straße aktualisieren" },
                { "ZoneTools.UI.Tooltip.UpdateRoad", "Update-Tool umschalten (bestehende Straßen)." },

                { "ZoneTools.UI.Tooltip.ModeDefault", "Beide (Standard)" },
                { "ZoneTools.UI.Tooltip.ModeLeft",    "Links" },
                { "ZoneTools.UI.Tooltip.ModeRight",   "Rechts" },
                { "ZoneTools.UI.Tooltip.ModeNone",    "Keine" }
            };

            return d;
        }

        public void Unload()
        {
        }
    }
}
