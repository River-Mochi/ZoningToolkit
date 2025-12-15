// Settings/LocalePT_BR.cs
// Portuguese (Brazil) pt-BR for Zone Tools.

namespace ZoningToolkit
{
    using System.Collections.Generic;
    using Colossal;

    public sealed class LocalePT_BR : IDictionarySource
    {
        private readonly Setting m_Setting;

        public LocalePT_BR(Setting setting)
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
                { m_Setting.GetOptionTabLocaleID(Setting.kAboutTab), "Sobre" },

                // Groups
                { m_Setting.GetOptionGroupLocaleID(Setting.kAboutGroup),    "Sobre" },
                { m_Setting.GetOptionGroupLocaleID(Setting.kBindingsGroup), "Atalhos do teclado" },

                // About fields
                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.ModName)),    "Nome do mod" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.ModName)),     "Nome exibido deste mod." },

                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.ModVersion)), "Versão" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.ModVersion)),  "Versão atual do Zone Tools." },

                // Keybinding option (Options → Mods)
                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.TogglePanelBinding)), "Alternar painel" },
                {
                    m_Setting.GetOptionDescLocaleID(nameof(Setting.TogglePanelBinding)),
                    "Atalho do teclado para mostrar ou ocultar o painel do Zone Tools (igual a clicar no ícone do menu no canto superior esquerdo)."
                },

                // Keybinding name (Options → Keybindings)
                { m_Setting.GetBindingKeyLocaleID(Mod.kTogglePanelActionName), "Zone Tools – Alternar painel" },

                // -----------------------------------------------------------------
                // UI strings (React panel)
                // -----------------------------------------------------------------
                { "ZoneTools.UI.UpdateRoad", "Atualizar via" },

                {
                    "ZoneTools.UI.Tooltip.UpdateRoad",
                    "Ativar/desativar a ferramenta de atualização (para vias existentes). Vias com prédios zoneados são ignoradas."
                },
                { "ZoneTools.UI.Tooltip.ModeDefault", "Padrão (ambos)" },
                { "ZoneTools.UI.Tooltip.ModeLeft", "Esquerda" },
                { "ZoneTools.UI.Tooltip.ModeRight", "Direita" },
                { "ZoneTools.UI.Tooltip.ModeNone", "Nenhum" }

            };

            return d;
        }

        public void Unload()
        {
            // Nothing to clean up; CS2 manages locale life cycle.
        }
    }
}
