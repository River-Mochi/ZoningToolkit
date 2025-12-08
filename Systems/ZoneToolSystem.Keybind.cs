// Systems/ZoneToolSystem.Keybind.cs
// Handles Zone Tools keybinding (Shift+Z by default) via CO's InputManager.

namespace ZoningToolkit.Systems
{
    using Game;
    using Game.Input;   // ProxyAction
    using Unity.Entities;   // GameSystemBase

    /// <summary>
    /// Runs in ToolUpdate and listens to the CO ProxyAction registered in Setting.RegisterKeyBindings().
    /// When the action is pressed, it toggles Zone Tools UI panel (same as clicking the GameTopLeft button).
    /// </summary>
    public sealed partial class ZoneToolSystemKeybind : GameSystemBase
    {
        private ZoneToolBridgeUI m_UISystem = null!;
        private bool m_LoggedMissingAction;

        protected override void OnCreate()
        {
            base.OnCreate();

            m_UISystem = World.GetOrCreateSystemManaged<ZoneToolBridgeUI>();
            Mod.s_Log.Info("[ZT] ZoneToolSystemKeybind created.");
        }

        protected override void OnUpdate()
        {
            ProxyAction? togglePanelAction = Mod.TogglePanelAction;
            if (togglePanelAction == null)
            {
                if (!m_LoggedMissingAction)
                {
                    m_LoggedMissingAction = true;
                    Mod.s_Log.Warn("[ZT] ZoneToolSystemKeybind: TogglePanelAction is null in OnUpdate.");
                }

                return;
            }

            // This is the CO keybinding event: fires once on the frame the key is pressed.
            if (togglePanelAction.WasPressedThisFrame())
            {
                Mod.s_Log.Info("[ZT] ZoneToolSystemKeybind: togglePanelAction.WasPressedThisFrame() → toggling panel.");
                m_UISystem.TogglePanelFromHotkey();
            }
        }
    }
}
