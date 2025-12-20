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
                { m_Setting.GetSettingsLocaleID(), Mod.ModName + " " + Mod.ModTag },

                // Tabs
                { m_Setting.GetOptionTabLocaleID(Setting.kActionsTab), "Azioni" },
                { m_Setting.GetOptionTabLocaleID(Setting.kAboutTab),   "Info" },

                // Groups
                { m_Setting.GetOptionGroupLocaleID(Setting.kActionsGroup),     "Azioni" },
                { m_Setting.GetOptionGroupLocaleID(Setting.kBindingsGroup),    "Scorciatoie da tastiera" },
                { m_Setting.GetOptionGroupLocaleID(Setting.kAboutGroup),       "" },
                { m_Setting.GetOptionGroupLocaleID(Setting.kAboutLinksGroup),  "Link" },

                // About fields
                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.ModName)),    "Nome mod" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.ModName)),     "Nome visualizzato di questa mod." },

                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.VersionText)), "Versione" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.VersionText)),  "Versione attuale di Zone Tools." },

                // About links
                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.OpenParadox)), "Paradox Mods" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.OpenParadox)),  "Apri la pagina dell’autore su Paradox Mods." },

                // Actions toggles
                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.AutoOpenPanelForRoadTools)), "Apri Zone Tools con gli strumenti strade." },
                {
                    m_Setting.GetOptionDescLocaleID(nameof(Setting.AutoOpenPanelForRoadTools)),
                    "Se attivo, il pannello Zone Tools si apre automaticamente con uno strumento strade zonabile.\n" +
                    "Disattiva per aprire il pannello manualmente."
                },

                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.ProtectOccupiedCells)), "Proteggi celle occupate (con edifici)" },
                {
                    m_Setting.GetOptionDescLocaleID(nameof(Setting.ProtectOccupiedCells)),
                    "Se attivo, Zone Tools non cambia profondità/area della zona su celle che hanno già un edificio."
                },

                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.ProtectZonedCells)), "Proteggi celle già zonate ma vuote" },
                {
                    m_Setting.GetOptionDescLocaleID(nameof(Setting.ProtectZonedCells)),
                    "Se attivo, Zone Tools non cambia profondità/area della zona su celle già zonate (anche se vuote)."
                },

                // Keybinding option
                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.TogglePanelBinding)), "Mostra/nascondi pannello" },
                {
                    m_Setting.GetOptionDescLocaleID(nameof(Setting.TogglePanelBinding)),
                    "Scorciatoia da tastiera per mostrare o nascondere il pannello Zone Tools (come l’icona in alto a sinistra)."
                },

                // Keybinding name
                { m_Setting.GetBindingKeyLocaleID(Mod.kTogglePanelActionName), "Zone Tools – Mostra/nascondi pannello" },

                // React panel
                { "ZoneTools.UI.UpdateRoad", "Aggiorna strada" },
                { "ZoneTools.UI.Tooltip.UpdateRoad", "Attiva lo strumento di aggiornamento (strade esistenti)." },

                { "ZoneTools.UI.Tooltip.ModeDefault", "Entrambi (predefinito)" },
                { "ZoneTools.UI.Tooltip.ModeLeft",    "Sinistra" },
                { "ZoneTools.UI.Tooltip.ModeRight",   "Destra" },
                { "ZoneTools.UI.Tooltip.ModeNone",    "Nessuno" }
            };

            return d;
        }

        public void Unload()
        {
        }
    }
}
