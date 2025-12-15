// Settings/LocaleKO.cs
// Korean ko-KR for Zone Tools.

namespace ZoningToolkit
{
    using System.Collections.Generic;
    using Colossal;

    public sealed class LocaleKO : IDictionarySource
    {
        private readonly Setting m_Setting;

        public LocaleKO(Setting setting)
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
                { m_Setting.GetOptionTabLocaleID(Setting.kActionsTab), "작업" },
                { m_Setting.GetOptionTabLocaleID(Setting.kAboutTab),   "정보" },

                // Groups
                { m_Setting.GetOptionGroupLocaleID(Setting.kActionsGroup),     "작업" },
                { m_Setting.GetOptionGroupLocaleID(Setting.kBindingsGroup),    "키 바인딩" },
                { m_Setting.GetOptionGroupLocaleID(Setting.kAboutGroup),       "" },
                { m_Setting.GetOptionGroupLocaleID(Setting.kAboutLinksGroup),  "링크" },

                // About fields
                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.ModName)),    "모드 이름" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.ModName)),     "이 모드의 표시 이름입니다." },

                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.ModVersion)), "버전" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.ModVersion)),  "Zone Tools의 현재 버전입니다." },

                // About links
                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.OpenParadox)), "Paradox Mods" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.OpenParadox)),  "작성자의 Paradox Mods 페이지를 엽니다." },

                // Actions toggles
                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.AutoOpenPanelForRoadTools)), "도로 도구 열 때 Zone Tools 자동 열기" },
                {
                    m_Setting.GetOptionDescLocaleID(nameof(Setting.AutoOpenPanelForRoadTools)),
                    "활성화하면, 구역 지정이 가능한 도로 도구를 열 때 Zone Tools 패널이 자동으로 열립니다.\n" +
                    "끄면 수동으로 열 수 있습니다."
                },

                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.ProtectOccupiedCells)), "점유된 셀 보호(건물 있음)" },
                {
                    m_Setting.GetOptionDescLocaleID(nameof(Setting.ProtectOccupiedCells)),
                    "활성화하면, 이미 건물이 있는 셀의 구역 깊이/영역을 변경하지 않습니다."
                },

                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.ProtectZonedCells)), "구역 지정됐지만 비어있는 셀 보호" },
                {
                    m_Setting.GetOptionDescLocaleID(nameof(Setting.ProtectZonedCells)),
                    "활성화하면, 이미 구역이 칠해진 셀(비어 있어도)의 구역 깊이/영역을 변경하지 않습니다."
                },

                // Keybinding option
                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.TogglePanelBinding)), "패널 토글" },
                {
                    m_Setting.GetOptionDescLocaleID(nameof(Setting.TogglePanelBinding)),
                    "Zone Tools 패널을 표시/숨김 하는 단축키(좌상단 아이콘과 동일)."
                },

                // Keybinding name
                { m_Setting.GetBindingKeyLocaleID(Mod.kTogglePanelActionName), "Zone Tools – 패널 토글" },

                // React panel
                { "ZoneTools.UI.UpdateRoad", "도로 업데이트" },
                { "ZoneTools.UI.Tooltip.UpdateRoad", "업데이트 도구 토글(기존 도로)." },

                { "ZoneTools.UI.Tooltip.ModeDefault", "양쪽(기본)" },
                { "ZoneTools.UI.Tooltip.ModeLeft",    "왼쪽" },
                { "ZoneTools.UI.Tooltip.ModeRight",   "오른쪽" },
                { "ZoneTools.UI.Tooltip.ModeNone",    "없음" }
            };

            return d;
        }

        public void Unload()
        {
        }
    }
}
