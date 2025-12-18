// Settings/LocaleZH_HANT.cs
// Traditional Chinese zh-HANT for Zone Tools.

namespace ZoningToolkit
{
    using System.Collections.Generic;
    using Colossal;

    public sealed class LocaleZH_HANT : IDictionarySource
    {
        private readonly Setting m_Setting;

        public LocaleZH_HANT(Setting setting)
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
                { m_Setting.GetOptionTabLocaleID(Setting.kActionsTab), "操作" },
                { m_Setting.GetOptionTabLocaleID(Setting.kAboutTab),   "關於" },

                // Groups
                { m_Setting.GetOptionGroupLocaleID(Setting.kActionsGroup),     "操作" },
                { m_Setting.GetOptionGroupLocaleID(Setting.kBindingsGroup),    "按鍵綁定" },
                { m_Setting.GetOptionGroupLocaleID(Setting.kAboutGroup),       "" },
                { m_Setting.GetOptionGroupLocaleID(Setting.kAboutLinksGroup),  "連結" },

                // About fields
                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.ModName)),    "模組名稱" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.ModName)),     "此模組的顯示名稱。" },

                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.VersionText)), "版本" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.VersionText)),  "Zone Tools 目前版本。" },

                // About links
                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.OpenParadox)), "Paradox Mods" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.OpenParadox)),  "開啟作者的 Paradox Mods 頁面。" },

                // Actions toggles
                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.AutoOpenPanelForRoadTools)), "開啟道路工具時自動開啟 Zone Tools" },
                {
                    m_Setting.GetOptionDescLocaleID(nameof(Setting.AutoOpenPanelForRoadTools)),
                    "啟用後，開啟可分區的道路工具時會自動開啟 Zone Tools 面板。\n" +
                    "關閉則需手動開啟。"
                },

                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.ProtectOccupiedCells)), "保護已佔用格（有建築）" },
                {
                    m_Setting.GetOptionDescLocaleID(nameof(Setting.ProtectOccupiedCells)),
                    "啟用後，Zone Tools 不會變更已有建築的格子的分區深度/範圍。"
                },

                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.ProtectZonedCells)), "不變更已塗分區格子" },
                {
                    m_Setting.GetOptionDescLocaleID(nameof(Setting.ProtectZonedCells)),
                    "啟用後，Zone Tools 不會變更已塗分區的格子（即使是空的）。"
                },

                // Keybinding option
                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.TogglePanelBinding)), "切換面板" },
                {
                    m_Setting.GetOptionDescLocaleID(nameof(Setting.TogglePanelBinding)),
                    "顯示/隱藏 Zone Tools 面板的快捷鍵（與點選左上角圖示相同）。"
                },

                // Keybinding name
                { m_Setting.GetBindingKeyLocaleID(Mod.kTogglePanelActionName), "Zone Tools – 切換面板" },

                // React panel
                { "ZoneTools.UI.UpdateRoad", "更新道路" },
                { "ZoneTools.UI.Tooltip.UpdateRoad", "切換更新工具（用於既有道路）。" },

                { "ZoneTools.UI.Tooltip.ModeDefault", "兩側（預設）" },
                { "ZoneTools.UI.Tooltip.ModeLeft",    "左側" },
                { "ZoneTools.UI.Tooltip.ModeRight",   "右側" },
                { "ZoneTools.UI.Tooltip.ModeNone",    "無" }
            };

            return d;
        }

        public void Unload()
        {
        }
    }
}
