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
                // Options title (single source of truth from Mod.cs)
                { m_Setting.GetSettingsLocaleID(), Mod.ModName + " " + Mod.ModTag },

                // Tabs
                { m_Setting.GetOptionTabLocaleID(Setting.kAboutTab), "Info" },

                // Groups
                { m_Setting.GetOptionGroupLocaleID(Setting.kAboutGroup),    "Info" },
                { m_Setting.GetOptionGroupLocaleID(Setting.kBindingsGroup), "Tastenbelegung" },

                // About fields
                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.ModName)),    "Mod-Name" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.ModName)),     "Anzeigename dieser Mod." },

                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.ModVersion)), "Version" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.ModVersion)),  "Aktuelle Version von Zone Tools." },

                // Keybinding option (Options → Mods)
                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.TogglePanelBinding)), "Panel umschalten" },
                {
                    m_Setting.GetOptionDescLocaleID(nameof(Setting.TogglePanelBinding)),
                    "Tastenkürzel zum Ein- oder Ausblenden des Zone Tools-Panels (entspricht dem Klick auf das Menü-Icon oben links)."
                },

                // Keybinding name (Options → Keybindings)
                { m_Setting.GetBindingKeyLocaleID(Mod.kTogglePanelActionName), "Zone Tools – Panel umschalten" },
            };

            return d;
        }

        public void Unload()
        {
            // Nothing to clean up; CS2 manages locale life cycle.
        }
    }
}
