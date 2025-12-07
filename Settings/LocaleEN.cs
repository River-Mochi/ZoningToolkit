// Settings/LocaleEN.cs
// English (en-US) localization for ZoneTools settings.

using System.Collections.Generic;
using Colossal;
using Game.Settings;
using Game.UI;
using Game.UI.Widgets;

namespace ZoningToolkit
{
    public class LocaleEN : IDictionarySource
    {
        private readonly Setting m_Setting;

        public LocaleEN(Setting setting)
        {
            m_Setting = setting;
        }

        public IEnumerable<KeyValuePair<string, string>> ReadEntries(
            IList<IDictionaryEntryError> errors,
            Dictionary<string, int> indexCounts)
        {
            return new Dictionary<string, string>
            {
                { m_Setting.GetSettingsLocaleID(), "ZoneTools settings" },
                { m_Setting.GetOptionTabLocaleID(Setting.kSection), "Main" },

                { m_Setting.GetOptionGroupLocaleID(Setting.kButtonGroup), "Buttons" },
                { m_Setting.GetOptionGroupLocaleID(Setting.kToggleGroup), "Toggle" },
                { m_Setting.GetOptionGroupLocaleID(Setting.kSliderGroup), "Sliders" },
                { m_Setting.GetOptionGroupLocaleID(Setting.kDropdownGroup), "Dropdowns" },

                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.Button)), "Button" },
                {
                    m_Setting.GetOptionDescLocaleID(nameof(Setting.Button)),
                    $"Simple single button. It should be a bool property with only a setter or use [{nameof(SettingsUIButtonAttribute)}] to make a button from a bool property with setter and getter."
                },

                {
                    m_Setting.GetOptionLabelLocaleID(nameof(Setting.ButtonWithConfirmation)),
                    "Button with confirmation"
                },
                {
                    m_Setting.GetOptionDescLocaleID(nameof(Setting.ButtonWithConfirmation)),
                    $"Button can show a confirmation message. Use [{nameof(SettingsUIConfirmationAttribute)}]."
                },
                {
                    m_Setting.GetOptionWarningLocaleID(nameof(Setting.ButtonWithConfirmation)),
                    "Is this the confirmation text you want to show here?"
                },

                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.Toggle)), "Toggle" },
                {
                    m_Setting.GetOptionDescLocaleID(nameof(Setting.Toggle)),
                    "Use a bool property with setter and getter to get a toggle option."
                },

                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.IntSlider)), "Int slider" },
                {
                    m_Setting.GetOptionDescLocaleID(nameof(Setting.IntSlider)),
                    $"Use an int property with getter and setter and [{nameof(SettingsUISliderAttribute)}] to get an int slider."
                },

                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.IntDropdown)), "Int dropdown" },
                {
                    m_Setting.GetOptionDescLocaleID(nameof(Setting.IntDropdown)),
                    $"Use an int property with getter and setter and [{nameof(SettingsUIDropdownAttribute)}(typeof(SomeType), nameof(SomeMethod))] to get an int dropdown: Method must be static or an instance method of your setting class with 0 parameters and return {typeof(DropdownItem<int>).Name}."
                },

                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.EnumDropdown)), "Simple enum dropdown" },
                {
                    m_Setting.GetOptionDescLocaleID(nameof(Setting.EnumDropdown)),
                    "Use any enum property with getter and setter to get an enum dropdown."
                },

                { m_Setting.GetEnumValueLocaleID(Setting.SomeEnum.Value1), "Value 1" },
                { m_Setting.GetEnumValueLocaleID(Setting.SomeEnum.Value2), "Value 2" },
                { m_Setting.GetEnumValueLocaleID(Setting.SomeEnum.Value3), "Value 3" },
            };
        }

        public void Unload()
        {
        }
    }
}
