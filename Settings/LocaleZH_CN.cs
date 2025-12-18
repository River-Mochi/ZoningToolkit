// Settings/LocaleZH_CN.cs
// Simplified Chinese zh-HANS for Zone Tools.

namespace ZoningToolkit
{
    using System.Collections.Generic;
    using Colossal;

    public sealed class LocaleZH_CN : IDictionarySource
    {
        private readonly Setting m_Setting;

        public LocaleZH_CN(Setting setting)
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
                { m_Setting.GetOptionTabLocaleID(Setting.kAboutTab),   "关于" },

                // Groups
                { m_Setting.GetOptionGroupLocaleID(Setting.kActionsGroup),     "操作" },
                { m_Setting.GetOptionGroupLocaleID(Setting.kBindingsGroup),    "按键绑定" },
                { m_Setting.GetOptionGroupLocaleID(Setting.kAboutGroup),       "" },
                { m_Setting.GetOptionGroupLocaleID(Setting.kAboutLinksGroup),  "链接" },

                // About fields
                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.ModName)),    "模组名称" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.ModName)),     "此模组的显示名称。" },

                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.VersionText)), "版本" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.VersionText)),  "Zone Tools 当前版本。" },

                // About links
                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.OpenParadox)), "Paradox Mods" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.OpenParadox)),  "打开作者的 Paradox Mods 页面。" },

                // Actions toggles
                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.AutoOpenPanelForRoadTools)), "打开道路工具时自动打开 Zone Tools" },
                {
                    m_Setting.GetOptionDescLocaleID(nameof(Setting.AutoOpenPanelForRoadTools)),
                    "启用后，打开可分区的道路工具时会自动打开 Zone Tools 面板。\n" +
                    "关闭则需要手动打开。"
                },

                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.ProtectOccupiedCells)), "保护已占用格（有建筑）" },
                {
                    m_Setting.GetOptionDescLocaleID(nameof(Setting.ProtectOccupiedCells)),
                    "启用后，Zone Tools 不会修改已有建筑的格子的分区深度/范围。"
                },

                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.ProtectZonedCells)), "不改变已涂分区格子" },
                {
                    m_Setting.GetOptionDescLocaleID(nameof(Setting.ProtectZonedCells)),
                    "启用后，Zone Tools 不会修改已涂分区的格子（即使为空）。"
                },

                // Keybinding option
                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.TogglePanelBinding)), "切换面板" },
                {
                    m_Setting.GetOptionDescLocaleID(nameof(Setting.TogglePanelBinding)),
                    "显示/隐藏 Zone Tools 面板的快捷键（相当于点击左上角图标）。"
                },

                // Keybinding name
                { m_Setting.GetBindingKeyLocaleID(Mod.kTogglePanelActionName), "Zone Tools – 切换面板" },

                // React panel
                { "ZoneTools.UI.UpdateRoad", "更新道路" },
                { "ZoneTools.UI.Tooltip.UpdateRoad", "切换更新工具（用于现有道路）。" },

                { "ZoneTools.UI.Tooltip.ModeDefault", "两侧（默认）" },
                { "ZoneTools.UI.Tooltip.ModeLeft",    "左侧" },
                { "ZoneTools.UI.Tooltip.ModeRight",   "右侧" },
                { "ZoneTools.UI.Tooltip.ModeNone",    "无" }
            };

            return d;
        }

        public void Unload()
        {
        }
    }
}
