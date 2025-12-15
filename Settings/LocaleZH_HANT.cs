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
                // Options title (single source of truth from Mod.cs)
                { m_Setting.GetSettingsLocaleID(), Mod.ModName + " " + Mod.ModTag },

                // Tabs
                { m_Setting.GetOptionTabLocaleID(Setting.kAboutTab), "關於" },

                // Groups
                { m_Setting.GetOptionGroupLocaleID(Setting.kAboutGroup),    "關於" },
                { m_Setting.GetOptionGroupLocaleID(Setting.kBindingsGroup), "按鍵綁定" },

                // About fields
                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.ModName)),    "模組名稱" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.ModName)),     "此模組的顯示名稱。" },

                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.ModVersion)), "版本" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.ModVersion)),  "目前的 Zone Tools 版本。" },

                // Keybinding option (Options → Mods)
                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.TogglePanelBinding)), "切換面板" },
                {
                    m_Setting.GetOptionDescLocaleID(nameof(Setting.TogglePanelBinding)),
                    "用於顯示或隱藏 Zone Tools 面板的快捷鍵（與點擊左上角選單圖示相同）。"
                },

                // Keybinding name (Options → Keybindings)
                { m_Setting.GetBindingKeyLocaleID(Mod.kTogglePanelActionName), "Zone Tools – 切換面板" },

                // -----------------------------------------------------------------
                // UI strings (React panel)
                // -----------------------------------------------------------------
                { "ZoneTools.UI.UpdateRoad", "更新道路" },

                {
                    "ZoneTools.UI.Tooltip.UpdateRoad",
                    "切換更新工具（用於既有道路）。有已分區建築的道路會被略過。"
                },
                { "ZoneTools.UI.Tooltip.ModeDefault", "預設（兩側）" },
                { "ZoneTools.UI.Tooltip.ModeLeft", "左側" },
                { "ZoneTools.UI.Tooltip.ModeRight", "右側" },
                { "ZoneTools.UI.Tooltip.ModeNone", "無" }

            };

            return d;
        }

        public void Unload()
        {
            // Nothing to clean up; CS2 manages locale life cycle.
        }
    }
}
