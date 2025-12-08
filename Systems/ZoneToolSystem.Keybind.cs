// Systems/ZoneToolSystem.Keybind.cs
// Shift+Z (rebindable) → toggle Zone Tools panel visibility (same as GameTopLeft icon).

namespace ZoningToolkit.Systems
{
    using Game;
    using Game.Input;
    using Unity.Entities;

    internal sealed partial class ZoneToolKeybindSystem : GameSystemBase
    {
        private ZoningToolkitModUISystem m_UISystem = null!;
        private ProxyAction? m_Toggle;

        protected override void OnCreate()
        {
            base.OnCreate();

            // Grab the UI system that owns TogglePanelFromHotkey()
            m_UISystem = World.GetOrCreateSystemManaged<ZoningToolkitModUISystem>();

            // Initial reference to our ProxyAction (defined via Setting + Mod.kTogglePanelActionName)
            m_Toggle = Mod.TogglePanelAction;
        }

        protected override void OnUpdate()
        {
            // In case keybindings were reloaded, refresh a null reference.
            m_Toggle ??= Mod.TogglePanelAction;

            ProxyAction? toggle = m_Toggle;
            if (toggle == null)
            {
                return;
            }

            // This is the ONLY thing we do every frame: check for “edge” press.
            if (!toggle.WasPressedThisFrame())
            {
                return;
            }

            // Behaves exactly like clicking the GameTopLeft Zone Tools button.
            m_UISystem.TogglePanelFromHotkey();
        }
    }
}
