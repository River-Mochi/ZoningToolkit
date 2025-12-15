// Settings/LocaleIT.cs
// Italian it-IT for Zone Tools.

namespace ZoningToolkit
{
    using System.Collections.Generic;
    using Colossal;

    public sealed class LocaleIT : IDictionarySource
    {
        private readonly Setting m_Setting;

        public LocaleIT(Setting setting)
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
                { m_Setting.GetOptionTabLocaleID(Setting.kAboutTab), "Informazioni" },

                // Groups
                { m_Setting.GetOptionGroupLocaleID(Setting.kAboutGroup),    "Informazioni" },
                { m_Setting.GetOptionGroupLocaleID(Setting.kBindingsGroup), "Scorciatoie da tastiera" },

                // About fields
                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.ModName)),    "Nome della mod" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.ModName)),     "Nome visualizzato di questa mod." },

                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.ModVersion)), "Versione" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.ModVersion)),  "Versione attuale di Zone Tools." },

                // Keybinding option (Options → Mods)
                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.TogglePanelBinding)), "Mostra/nascondi pannello" },
                {
                    m_Setting.GetOptionDescLocaleID(nameof(Setting.TogglePanelBinding)),
                    "Scorciatoia da tastiera per mostrare o nascondere il pannello Zone Tools (come fare clic sull’icona del menu in alto a sinistra)."
                },

                // Keybinding name (Options → Keybindings)
                { m_Setting.GetBindingKeyLocaleID(Mod.kTogglePanelActionName), "Zone Tools – Mostra/nascondi pannello" },

                // -----------------------------------------------------------------
                // UI strings (React panel)
                // -----------------------------------------------------------------
                { "ZoneTools.UI.UpdateRoad", "Aggiorna strada" },

                {
                    "ZoneTools.UI.Tooltip.UpdateRoad",
                    "Attiva/disattiva lo strumento di aggiornamento (per strade esistenti). Le strade con edifici zonizzati vengono ignorate."
                },
                { "ZoneTools.UI.Tooltip.ModeDefault", "Predefinito (entrambi)" },
                { "ZoneTools.UI.Tooltip.ModeLeft", "Sinistra" },
                { "ZoneTools.UI.Tooltip.ModeRight", "Destra" },
                { "ZoneTools.UI.Tooltip.ModeNone", "Nessuno" }

            };

            return d;
        }

        public void Unload()
        {
            // Nothing to clean up; CS2 manages locale life cycle.
        }
    }
}
