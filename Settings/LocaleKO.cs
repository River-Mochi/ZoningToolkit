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
                // Options title (single source of truth from Mod.cs)
                { m_Setting.GetSettingsLocaleID(), Mod.ModName + " " + Mod.ModTag },

                // Tabs
                { m_Setting.GetOptionTabLocaleID(Setting.kAboutTab), "정보" },

                // Groups
                { m_Setting.GetOptionGroupLocaleID(Setting.kAboutGroup),    "정보" },
                { m_Setting.GetOptionGroupLocaleID(Setting.kBindingsGroup), "키 바인딩" },

                // About fields
                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.ModName)),    "모드 이름" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.ModName)),     "이 모드의 표시 이름입니다." },

                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.ModVersion)), "버전" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.ModVersion)),  "현재 Zone Tools 버전입니다." },

                // Keybinding option (Options → Mods)
                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.TogglePanelBinding)), "패널 토글" },
                {
                    m_Setting.GetOptionDescLocaleID(nameof(Setting.TogglePanelBinding)),
                    "Zone Tools 패널을 표시/숨김하는 단축키입니다(왼쪽 상단 메뉴 아이콘 클릭과 동일)."
                },

                // Keybinding name (Options → Keybindings)
                { m_Setting.GetBindingKeyLocaleID(Mod.kTogglePanelActionName), "Zone Tools – 패널 토글" },

                // -----------------------------------------------------------------
                // UI strings (React panel)
                // -----------------------------------------------------------------
                { "ZoneTools.UI.UpdateRoad", "도로 업데이트" },

                {
                    "ZoneTools.UI.Tooltip.UpdateRoad",
                    "업데이트 도구를 전환합니다(기존 도로용). 구역 지정된 건물이 있는 도로는 건너뜁니다."
                },
                { "ZoneTools.UI.Tooltip.ModeDefault", "기본(양쪽)" },
                { "ZoneTools.UI.Tooltip.ModeLeft", "왼쪽" },
                { "ZoneTools.UI.Tooltip.ModeRight", "오른쪽" },
                { "ZoneTools.UI.Tooltip.ModeNone", "없음" }

            };

            return d;
        }

        public void Unload()
        {
            // Nothing to clean up; CS2 manages locale life cycle.
        }
    }
}
