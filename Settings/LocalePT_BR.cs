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
                { m_Setting.GetSettingsLocaleID(), Mod.ModName + " " + Mod.ModTag },

                // Tabs
                { m_Setting.GetOptionTabLocaleID(Setting.kActionsTab), "Ações" },
                { m_Setting.GetOptionTabLocaleID(Setting.kAboutTab),   "Sobre" },

                // Groups
                { m_Setting.GetOptionGroupLocaleID(Setting.kActionsGroup),     "Ações" },
                { m_Setting.GetOptionGroupLocaleID(Setting.kBindingsGroup),    "Atalhos" },
                { m_Setting.GetOptionGroupLocaleID(Setting.kAboutGroup),       "" },
                { m_Setting.GetOptionGroupLocaleID(Setting.kAboutLinksGroup),  "Links" },

                // About fields
                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.ModName)),    "Nome do mod" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.ModName)),     "Nome exibido deste mod." },

                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.ModVersion)), "Versão" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.ModVersion)),  "Versão atual do Zone Tools." },

                // About links
                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.OpenParadox)), "Paradox Mods" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.OpenParadox)),  "Abrir a página do autor no Paradox Mods." },

                // Actions toggles
                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.AutoOpenPanelForRoadTools)), "Abrir Zone Tools automaticamente com ferramentas de estrada." },
                {
                    m_Setting.GetOptionDescLocaleID(nameof(Setting.AutoOpenPanelForRoadTools)),
                    "Quando ativado, o painel do Zone Tools abre automaticamente ao selecionar uma estrada com zoneamento.\n" +
                    "Desative para abrir o painel manualmente."
                },

                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.ProtectOccupiedCells)), "Proteger células ocupadas (com prédios)" },
                {
                    m_Setting.GetOptionDescLocaleID(nameof(Setting.ProtectOccupiedCells)),
                    "Quando ativado, o Zone Tools não muda a profundidade/área de zoneamento em células que já têm um prédio."
                },

                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.ProtectZonedCells)), "Proteger células já zoneadas (vazias)" },
                {
                    m_Setting.GetOptionDescLocaleID(nameof(Setting.ProtectZonedCells)),
                    "Quando ativado, o Zone Tools não muda a profundidade/área de zoneamento em células já zoneadas (mesmo vazias)."
                },

                // Keybinding option
                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.TogglePanelBinding)), "Alternar painel" },
                {
                    m_Setting.GetOptionDescLocaleID(nameof(Setting.TogglePanelBinding)),
                    "Atalho para mostrar/ocultar o painel do Zone Tools (igual ao ícone no canto superior esquerdo)."
                },

                // Keybinding name
                { m_Setting.GetBindingKeyLocaleID(Mod.kTogglePanelActionName), "Zone Tools – Alternar painel" },

                // React panel
                { "ZoneTools.UI.UpdateRoad", "Atualizar estrada" },
                { "ZoneTools.UI.Tooltip.UpdateRoad", "Alternar ferramenta de atualização (estradas existentes)." },

                { "ZoneTools.UI.Tooltip.ModeDefault", "Ambos (padrão)" },
                { "ZoneTools.UI.Tooltip.ModeLeft",    "Esquerda" },
                { "ZoneTools.UI.Tooltip.ModeRight",   "Direita" },
                { "ZoneTools.UI.Tooltip.ModeNone",    "Nenhum" }
            };

            return d;
        }

        public void Unload()
        {
        }
    }
}
